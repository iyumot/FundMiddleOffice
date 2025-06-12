using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.IO.AMAC;
using FMO.Models;
using FMO.Schedule;
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



public partial class HomePageViewModel : ObservableObject
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


    public HomePageViewModel()
    {

        Task.Run(() =>
        {
            MissionSchedule.Init();

            DataSelfTest();

            InitPlot();
        });
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
        ];

        Task.WaitAll(t);
        IsSelfTesting = false;
    }

    private static async Task SyncFundsFromAmac(Fund[] c)
    {
        var ned = c.Where(x => x.PublicDisclosureSynchronizeTime == default).ToArray();
        if (ned.Length > 0)
        {
            try
            {
                var db = DbHelper.Base();
                HandyControl.Controls.Growl.Warning($"发现{ned.Length}个基金未同步公示信息");

                using HttpClient client = new HttpClient();
                foreach (var (i, set) in ned.Chunk(50).Index())
                {
                    foreach (var (j, f) in set.Index())
                    {
                        await AmacAssist.SyncFundInfoAsync(f, client);
                        DataTracker.CheckFundFolder([f]);

                        db.GetCollection<Fund>().Update(f);
                        WeakReferenceMessenger.Default.Send(f);

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











    public partial class HomePlotViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial SeriesCollection? Series { get; set; }
        [ObservableProperty]
        public partial string[]? Labels { get; set; }
        [ObservableProperty]
        public partial Func<double, string>? YFormatter { get; set; }
    }


}