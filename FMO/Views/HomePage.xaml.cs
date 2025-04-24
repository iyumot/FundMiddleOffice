using System.IO;
using System.Net.Http;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.IO.AMAC;
using FMO.Models;
using FMO.Schedule;
using FMO.Utilities;
using LiveCharts;
using Serilog;

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


        Task[] t = [Task.Run(async() => db = await SyncFundsFromAmac(db, c) ),
                    Task.Run(() => DataTracker.CheckFundFolder(c)),
                    Task.Run(()=> DataTracker.CheckShareIsPair(c))];

        Task.WaitAll(t);
        IsSelfTesting = false;
    }

    private static async Task<BaseDatabase> SyncFundsFromAmac(BaseDatabase db, Fund[] c)
    {
        var ned = c.Where(x => x.Status >= FundStatus.Normal && x.PublicDisclosureSynchronizeTime == default).ToArray();
        if (ned.Length > 0)
        {
            try
            {
                HandyControl.Controls.Growl.Warning($"发现{ned.Length}个基金未同步公示信息");

                using HttpClient client = new HttpClient();
                foreach (var (i, set) in ned.Chunk(50).Index())
                {
                    foreach (var (j, f) in set.Index())
                    {
                        await AmacAssist.SyncFundInfoAsync(f, client);

                        WeakReferenceMessenger.Default.Send(f);

                        await Task.Delay(200);
                    }

                    int finished = i * 50 + set.Length;
                    if (finished < ned.Length)
                        HandyControl.Controls.Growl.Success($"已同步{finished}个基金，剩余{ned.Length - finished}个");

                    db = DbHelper.Base();
                    db.GetCollection<Fund>().Update(set);
                    db.Dispose();
                }

                HandyControl.Controls.Growl.Success($"基金同步公示信息完成");
            }
            catch (Exception ex)
            {
                Log.Error($"同步公示信息失败：{ex.Message}");
                HandyControl.Controls.Growl.Error($"同步基金公示信息失败");
            }
        }

        return db;
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