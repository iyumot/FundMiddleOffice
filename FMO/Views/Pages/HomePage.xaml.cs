﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.IO.AMAC;
using FMO.Models;
using FMO.Schedule;
using FMO.Trustee;
using FMO.Utilities;
using LiveCharts;
using Serilog;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// HomePage.xaml 的交互逻辑
/// </summary>
public partial class HomePage : UserControl
{
    public HomePage()
    {
        InitializeComponent();
    }

    private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.F5 && DataContext is HomePageViewModel vm)
            Task.Run(() => vm.InitPlot());
    }
}



public partial class HomePageViewModel : ObservableObject, IRecipient<FundTipMessage>, IRecipient<TrusteeWorkResult>
{
    /// <summary>
    /// 是否正在同步
    /// </summary>
    [ObservableProperty]
    public partial bool IsSynchronizing { get; set; }


    /// <summary>
    /// 是否正在自检
    /// </summary>
    [ObservableProperty]
    public partial bool IsSelfTesting { get; set; }

    public HomePlotViewModel FlotContext { get; } = new();


    [ObservableProperty]
    public partial double BackupProcess { get; set; }
    [ObservableProperty]
    public partial double BackupProcess2 { get; set; }

    [ObservableProperty]
    public partial IList<string>? FundTips { get; set; }


    /// <summary>
    /// 募集户余额报警
    /// </summary> 
    public RaisingAccountWarning RaisingAccountTip { get; } = new();


    public DateTime DailyUpdateTime { get; set; }

    private Timer _dailyTimer;

    public HomePageViewModel()
    {
        WeakReferenceMessenger.Default.RegisterAll(this);

        //启动api
        TrusteeGallay.Initialize();

        Task.Run(() =>
        {

            DatabaseAssist.Miggrate();

            ///数据库自检等操作
            DatabaseAssist.SystemValidation();

            MissionSchedule.Init();

            DataSelfTest();

            // 加载托管消息
            LoadTrusteeMessages();

            OnNewDate();
        });

        // 每小时运行一次，判断是不是新的一天
        _dailyTimer = new Timer(x => OnNewDate(), null, 60000, 1000 * 60 * 60);
    }

    private void LoadTrusteeMessages()
    {
        using var db = DbHelper.Base();
        var data = db.GetCollection<TrusteeWorker.WorkReturn>(TrusteeWorker.TableRaisingBalance).FindAll().ToArray();
        if (data.Length > 0) WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(nameof(ITrustee.QueryRaisingBalance), data));


    }



    /// <summary>
    /// 数据自检
    /// </summary>
    public void DataSelfTest()
    {
        IsSelfTesting = true;
        var db = DbHelper.Base();
        var c = db.GetCollection<Fund>().FindAll().ToArray();
        db.Dispose();


        Task[] t = [Task.Run(async() => await SyncFundsFromAmac(c) ),
                    Task.Run(() => DataTracker.CheckFundFolder(c)),
                    Task.Run(()=> DataTracker.CheckShareIsPair(c)),
                    Task.Run(()=> DataTracker.CheckIsExpired(c)),
                    Task.Run(()=> DataTracker.CheckInvestorBalance()),
                    Task.Run(()=> DataTracker.CheckPairOrder(db))
        ];

        Task.WaitAll(t);
        IsSelfTesting = false;
    }

    private static async Task SyncFundsFromAmac(Fund[] c)
    {
        // 从未同步过的、正在清算的、备案的
        var ned = c.Where(x => x.PublicDisclosureSynchronizeTime == default || x.Status switch { FundStatus.StartLiquidation or FundStatus.Registration => true, _ => false }).ToArray();
        if (ned.Length > 0)
        {
            try
            {
                var db = DbHelper.Base();
                HandyControl.Controls.Growl.Warning($"发现{ned.Length}个基金待同步公示信息");

                using HttpClient client = new HttpClient();
                foreach (var (i, set) in ned.Chunk(50).Index())
                {
                    foreach (var (j, f) in set.Index())
                    {
                        var cleared = f.Status > FundStatus.StartLiquidation;

                        await AmacAssist.SyncFundInfoAsync(f, client);
                        DataTracker.CheckFundFolder([f]);

                        f.PublicDisclosureSynchronizeTime = DateTime.Now;
                        db.GetCollection<Fund>().Update(f);
                        WeakReferenceMessenger.Default.Send(f);

                        //
                        if (!cleared && f.Status > FundStatus.StartLiquidation)
                            DataTracker.OnFundCleared(f);

                        await Task.Delay(200);
                    }

                    int finished = i * 50 + set.Length;
                    if (finished < ned.Length)
                        HandyControl.Controls.Growl.Success($"已同步{finished}个基金，剩余{ned.Length - finished}个");
                }
                db.Dispose();
                HandyControl.Controls.Growl.Success($"基金同步公示信息完成");
            }
            catch (Exception ex)
            {
                Log.Error($"同步公示信息失败：{ex}");
                HandyControl.Controls.Growl.Error($"同步基金公示信息失败");
            }
        }
    }

    public void InitPlot()
    {
        using var db = DbHelper.Base();
        var status = db.GetCollection<Fund>().FindAll().Select(x => x.Status).ToList();



        // 计算管理规模
        var fids = db.GetCollection<Fund>().FindAll().Select(x => x.Id).ToList();
        if (fids.Count > 0)
        {
            var sd = fids.Select(x => db.GetDailyCollection(x).FindAll().Where(x => x is not null).OrderBy(x => x.Date).ToList()).Where(x => x.Count > 0);
            if (!sd.Any()) return;
            var mindate = sd.Select(x => x[0].Date).Min(); var maxdate = sd.Select(x => x[^1].Date).Max();
            var tmpdate = mindate;
            var dates = new List<DateOnly>();
            dates.Add(mindate);
            while (tmpdate < maxdate)
            {
                tmpdate = tmpdate.AddDays(1);
                dates.Add(tmpdate);
            }

            var scale = new double[dates.Count];

            Parallel.ForEach(sd, f =>
            {
                // 对齐第一个
                var first = dates.IndexOf(f[0].Date);
                for (int i = 0, j = first; i < f.Count; i++)
                {
                    for (; j < dates.Count; j++)
                    {
                        if (f[i].Date == dates[j])
                        {
                            scale[j++] += (double)f[i].NetAsset;
                            break;
                        }
                    }

                }
            });

            // 计算月内最大规模
            var lastmon = dates[0];
            List<DateOnly> mons = [dates[0]];
            List<double> sc = [scale[0]];
            for (int i = 1; i < dates.Count; i++)
            {
                if (scale[i] <= 0) continue;

                if ((dates[i].Year > mons[^1].Year || dates[i].Month > mons[^1].Month))
                {
                    mons.Add(dates[i]);
                    sc.Add(scale[i]);
                }
                else if (scale[i] > sc[^1])
                {
                    mons[^1] = dates[i];
                    sc[^1] = scale[i];
                }

            }
            if (sc.Count > 1 && sc[^1] < sc[^2] / 2)
            {
                mons.RemoveAt(mons.Count - 1);
                sc.RemoveAt(sc.Count - 1);
            }

            App.Current.Dispatcher.BeginInvoke(() =>
            {
                FlotContext.Series =
            [
                new LiveCharts.Wpf.LineSeries
                {
                    Title = "管理规模",
                    PointGeometry = null,
                    Values = new ChartValues<double>(sc.Select(x=>x/10000).ToArray())
                },
            ];

                FlotContext.YFormatter = value => value.ToString("N0");
                FlotContext.Labels = mons.Select(x => x.ToString("yyyy-MM-dd")).ToArray();
            });


        }


    }

    /// <summary>
    /// 检查交易申请与流水是否匹配
    /// </summary>
    private void DailyCheckRequestIsWell()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var limit = DateTime.Today.AddDays(-2);
        using var db = DbHelper.Base();
        // 2日内的request
        var tq = db.GetCollection<TransferRequest>().Find(x => today.DayNumber - x.RequestDate.DayNumber < 2).ToArray();
        var btrans = db.GetCollection<BankTransaction>().Find(x => x.Time > limit).ToArray();

        foreach (var item in tq)
        { 
        }
    }



    [RelayCommand]
    public void CalcFee()
    {
        try
        {
            var di = new DirectoryInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName).Parent!;

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = Path.Combine(di.FullName, "FMO.FeeCalc.exe"), WorkingDirectory = Directory.GetCurrentDirectory() });
        }
        catch (Exception e)
        {

            HandyControl.Controls.Growl.Error($"无法启动计算器，{e.Message}");
        }
    }

    [RelayCommand]
    public void OpenDbViewer()
    {
        try
        {
            var di = new DirectoryInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName).Parent!;

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = Path.Combine(di.FullName, "DatabaseViewer.exe"), WorkingDirectory = Directory.GetCurrentDirectory() });
        }
        catch (Exception e)
        {

            HandyControl.Controls.Growl.Error($"无法启动数据视图，{e.Message}");
        }
    }


    [RelayCommand]
    public void OpenDataFolder()
    {
        try { System.Diagnostics.Process.Start("explorer.exe", Path.Combine(Directory.GetCurrentDirectory(), "files")); } catch { }
    }


    [RelayCommand]
    public async Task Backup()
    {
        Task task = new Task(() =>
        {
            try
            {
                BackupProcess = 0;
                // 检测备份大小
                long bytes = GetFolderSize("data") + GetFolderSize("files") + GetFolderSize("config") + GetFolderSize("manager");
                int fcnt = GetFileCount("data") + GetFileCount("config") + GetFileCount("files") + GetFileCount("manager");
                int c = 0;
                using var fs = new FileStream($"backup-{DateTime.Today:yyyy.MM.dd}.zip", FileMode.Create);

                using ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Create);
                AddDirectoryToZip(archive, "data", "data", ref c, fcnt);
                AddDirectoryToZip(archive, "config", "config", ref c, fcnt);
                AddDirectoryToZip(archive, "files", "files", ref c, fcnt);
                AddDirectoryToZip(archive, "manager", "manager", ref c, fcnt);
            }
            catch (Exception e)
            {
                Log.Error($"备份失败 {e}");
                HandyControl.Controls.Growl.Error("备份失败");
            }
        }, TaskCreationOptions.LongRunning);

        task.Start();
        await task.WaitAsync(Timeout.InfiniteTimeSpan);
    }

    private long GetFolderSize(string dir)
    {
        var di = new DirectoryInfo(dir);
        return !di.Exists ? 0 : di.GetFiles("*.*", SearchOption.AllDirectories).Sum(x => x.Length);
    }

    private int GetFileCount(string dir) => Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).Length;

    // 将文件夹及其内容添加到压缩包
    private void AddDirectoryToZip(ZipArchive archive, string sourceDir, string entryBase, ref int c, int size)
    {
        // 添加文件夹条目
        archive.CreateEntry($"{entryBase}/");

        // 添加文件
        foreach (string filePath in Directory.GetFiles(sourceDir))
        {
            string entryName = Path.Combine(entryBase, Path.GetFileName(filePath));
            archive.CreateEntryFromFile(filePath, entryName);

            BackupProcess = (double)(++c) / size * 100;
            BackupProcess2 = 100 * (BackupProcess - Math.Floor(BackupProcess));
        }

        // 递归添加子文件夹
        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string subDirName = Path.GetFileName(subDir);
            string newEntryBase = Path.Combine(entryBase, subDirName);
            AddDirectoryToZip(archive, subDir, newEntryBase, ref c, size);
        }
    }

    [RelayCommand]
    public void RefreshPlot() => InitPlot();

    public void Receive(FundTipMessage message)
    {
        App.Current.Dispatcher.BeginInvoke(() =>
          {
              FundTips = DataTracker.FundTips.Where(x => x.Tip is not null).Select(x => $"{x.FundName}  {x.Tip}").ToArray();
          });

    }

    public void Receive(TrusteeWorkResult message)
    {
        switch (message.Method)
        {
            case nameof(ITrustee.QueryRaisingBalance):
                OnHandleRaisingBalance(message.Returns);
                break;
            default:
                break;
        }

    }

    private void OnHandleRaisingBalance(IList<TrusteeWorker.WorkReturn> returns)
    {
        //
        using var db = DbHelper.Base();
        var funds = db.GetCollection<Fund>().FindAll().Where(x => x.Status >= FundStatus.ContractFinalized && x.Status <= FundStatus.StartLiquidation).ToArray();

        List<(string fc, ReturnCode rc, decimal v)> list = new();

        foreach (var r in returns)
        {
            // 失败
            if (r.Code != ReturnCode.Success)
            {
                // 获取对应的产品名



                continue;
            }

            // 成功 r.Data 可能是object[]
            if (r.Data is System.Collections.IEnumerable en && en.OfType<FundBankBalance>() is IEnumerable<FundBankBalance> bankBalances)
            {
                foreach (var b in bankBalances)
                {
                    list.Add((b.FundName!, r.Code, b.Balance));
                }
            }
        }

        // 没有记录的产品
        var failed = funds.Where(x => x.Status >= FundStatus.ContractFinalized && x.Status <= FundStatus.StartLiquidation).ExceptBy(list.Select(x => x.fc), x => x.Name).ToArray();

        RaisingAccountTip.HasFailed = failed.Length > 0;
        RaisingAccountTip.FailedFunds = failed.Select(x => x.Name).ToArray();
        RaisingAccountTip.TotalBalance = list.Sum(x => x.v);
        RaisingAccountTip.BalanceDetail = list.Where(x => x.v > 0).ToDictionary(x => x.fc, x => x.v);
    }




    private void OnNewDate()
    {
        if (DailyUpdateTime.Date == DateTime.Today) return;

        try
        {
            // 更新规模图
            InitPlot();


            DailyCheckRequestIsWell();






            WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Info, "更新每日数据完成"));
        }
        catch (Exception ex)
        {
            Log.Error($"HomePage, OnNewDate {ex}");
            WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Info, "更新每日数据失败"));
        }
        DailyUpdateTime = DateTime.Now;
    }





    public partial class HomePlotViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial SeriesCollection? Series { get; set; }
        [ObservableProperty]
        public partial string[]? Labels { get; set; }
        [ObservableProperty]
        public partial Func<double, string>? YFormatter { get; set; }
    }



    public partial class RaisingAccountWarning : ObservableObject
    {
        [ObservableProperty]
        public partial string[]? FailedFunds { get; set; }

        [ObservableProperty]
        public partial bool HasFailed { get; set; }

        [ObservableProperty]
        public partial decimal? TotalBalance { get; set; }

        [ObservableProperty]
        public partial bool HasBalance { get; set; }

        [ObservableProperty]
        public partial Dictionary<string, decimal>? BalanceDetail { get; set; }



        [RelayCommand]
        public async Task Update()
        {
            /// 在WeakReferenceMessenger接收消息
            await TrusteeGallay.Worker.QueryRaisingBalanceOnce();
        }

    }

}