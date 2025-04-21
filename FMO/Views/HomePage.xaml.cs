using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.IO.AMAC;
using FMO.Models;
using FMO.Schedule;
using FMO.Utilities;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using Serilog;
using System.IO;
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

    [ObservableProperty]
    public partial PlotModel? FundCountPlot { get; set; }

    [ObservableProperty]
    public partial PlotModel? FundScalePlot { get; set; }

    public HomePageViewModel()
    {

        Task.Run(async () =>
        {
            MissionSchedule.Init();

            await Task.Delay(2000);

            await DataSelfTest();

            InitPlot();


        });
    }



    /// <summary>
    /// 数据自检
    /// </summary>
    public async Task DataSelfTest()
    {
        IsSelfTesting = true;
        var db = DbHelper.Base();
        var c = db.GetCollection<Fund>().FindAll().ToArray();
        db.Dispose();



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



        // 基金文件夹
        var dis = new DirectoryInfo(@"files\funds").GetDirectories();
        foreach (var f in c)
        {
            if (f.Code?.Length > 4)
            {
                var di = dis.FirstOrDefault(x => x.Name.StartsWith(f.Code));

                var name = $"{f.Code}.{f.Name}";

                string folder = $"files\\funds\\{name}";

                if (di is null)
                {
                    Directory.CreateDirectory(folder);
                    continue;
                }
                if (di.Name != name)
                {
                    Directory.Move(di.FullName, folder);
                    Log.Warning($"基金 {f.Code} 名称已更新 [{di.Name}] -> [{f.Name}]");
                }

                FundHelper.Map(f, folder);
            }
        }





        IsSelfTesting = false;
    }


    public void InitPlot()
    {
        using var db = DbHelper.Base();
        var status = db.GetCollection<Fund>().FindAll().Select(x => x.Status).ToList();

        FundCountPlot = new PlotModel();

        // 创建 PieSeries
        var pieSeries = new PieSeries
        {
            Stroke = OxyColors.Transparent,
            StrokeThickness = 2.0,
            InsideLabelPosition = 0.8,
            AngleSpan = 360,
            StartAngle = 0
        };

        // 添加数据点
        var cnt = status.Count(x => x < FundStatus.Setup);
        if (cnt > 0)
            pieSeries.Slices.Add(new PieSlice("发行中", cnt));

        cnt = status.Count(x => x >= FundStatus.Setup && x <= FundStatus.Registration);
        if (cnt > 0)
            pieSeries.Slices.Add(new PieSlice("成立备案", cnt));

        cnt = status.Count(x => x == FundStatus.Normal);
        pieSeries.Slices.Add(new PieSlice($"运行中 {cnt}只", cnt) { Fill = OxyColors.Pink, IsExploded = true });
        cnt = status.Count(x => x >= FundStatus.StartLiquidation);
        pieSeries.Slices.Add(new PieSlice($"已清算 {cnt}只", cnt) { Fill = OxyColors.Orange, IsExploded = true });

        // 将 PieSeries 添加到 PlotModel
        FundCountPlot.Series.Add(pieSeries);


        // 计算管理规模
        var fids = db.GetCollection<Fund>().FindAll().Select(x => x.Id).ToList();
        if (fids.Count > 0)
        {
            var sd = fids.Select(x => db.GetDailyCollection(x).FindAll().Where(x => x is not null).OrderBy(x => x.Date).ToList()).Where(x => x.Count > 0);
            var mindate = sd.Select(x => x[0].Date).Min(); var maxdate = sd.Select(x => x[0].Date).Max();
            var tmpdate = mindate;
            var dates = new List<DateOnly>();
            dates.Add(mindate);
            while (tmpdate < maxdate)
            {
                tmpdate = tmpdate.AddDays(1);
                dates.Add(tmpdate);
            }
            var scale = new double[dates.Count];

            foreach (var f in sd)
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
            }

            var lineSeries = new LineSeries
            {
                Title = "数据系列",
                StrokeThickness = 2,
                Color = OxyColors.Blue,
                TrackerKey = "Default"
            };

            // 计算月内最大规模
            var lastmon = dates[0];
            var max = 0;
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

            for (int i = 0; i < mons.Count; i++)
            {
                lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(new DateTime(mons[i], default)), sc[i] / 10000));
            }
            // 创建日期坐标轴
            var dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "yyyy-MM-dd",
            };


            DataPoint maxPoint = new DataPoint(0, double.MinValue);
            foreach (var point in lineSeries.Points)
            {
                if (point.Y > maxPoint.Y)
                    maxPoint = point;
            }

            // 添加标签
            var annotation = new TextAnnotation
            {
                Text = $"{maxPoint.Y:N0}",               
                TextPosition = new DataPoint(maxPoint.X - 5, maxPoint.Y + 0.1),
                TextColor = OxyColors.Black,
                Stroke = OxyColors.Transparent,
            };


            FundScalePlot = new PlotModel();
            FundScalePlot.Title = "管理规模";
            FundScalePlot.PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1);

            FundScalePlot.Annotations.Add(annotation);
            FundScalePlot.Series.Add(lineSeries);
            // 将日期坐标轴添加到 PlotModel
            FundScalePlot.Axes.Add(dateAxis);
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
}