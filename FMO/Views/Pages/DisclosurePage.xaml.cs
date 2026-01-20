using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Logging;
using FMO.Models;
using FMO.Utilities;
using System.IO;
using System.IO.Compression;
using System.Windows.Controls;
using System.Windows.Data;

namespace FMO;

/// <summary>
/// DisclosurePage.xaml 的交互逻辑
/// </summary>
public partial class DisclosurePage : UserControl
{
    public DisclosurePage()
    {
        InitializeComponent();
    }
}


public partial class DisclosurePageViewModel : ObservableObject
{
    public DisclosurePageViewModel()
    {
        using var db = DbHelper.Base();
        var begin = db.GetCollection<Fund>().Find(x => x.SetupDate != default).Min(x => x.SetupDate).Year;
        var end = DateTime.Now.Year;
        Years = Enumerable.Range(begin, end - begin + 1).Reverse().ToArray();

        Months = Enumerable.Range(1, 12).Reverse().ToArray();

        debouncer = new Debouncer(() => Update());
    }

    public int[] Years { get; set; }

    [ObservableProperty]
    public partial int[] Months { get; set; }

    [ObservableProperty]
    public partial int? SelectedYear { get; set; }


    [ObservableProperty]
    public partial int? SelectedMonth { get; set; }


    public CollectionViewSource MonthlySource { get; } = new();

    public CollectionViewSource QuarterlySource { get; } = new();


    public CollectionViewSource SemiAnnualSource { get; } = new();

    public CollectionViewSource AnnualSource { get; } = new();



    private Debouncer debouncer;

    partial void OnSelectedYearChanged(int? value)
    {
        if (value is null) Months = [];
        else if (value == DateTime.Now.Year)
            Months = Enumerable.Range(1, DateTime.Now.Month).Reverse().ToArray();
        else
            Months = Enumerable.Range(1, 12).Reverse().ToArray();

        SelectedMonth = Months.FirstOrDefault();
        debouncer.Invoke();
    }

    partial void OnSelectedMonthChanged(int? value)
    {
        debouncer.Invoke();
    }


    private void Update()
    {
        using var db = DbHelper.Base();
        var reports = db.GetCollection<FundPeriodicReport>().Find(x => x.PeriodEnd.Year == SelectedYear && x.PeriodEnd.Month == SelectedMonth).ToArray();
        var dic = db.GetCollection<Fund>().Query().Select(x => new { x.Code, x.Name }).ToArray().ToDictionary(x => x.Code!, x => x.Name);


        var vm = reports.Select(x => new FundPeriodicReportViewModel(x) { FundName = dic[x.FundCode!] });

        App.Current.Dispatcher.InvokeAsync(() =>
        {
            MonthlySource.Source = vm.Where(x => x.Type == PeriodicReportType.MonthlyReport);
            QuarterlySource.Source = vm.Where(x => x.Type == PeriodicReportType.QuarterlyReport);
            SemiAnnualSource.Source = vm.Where(x => x.Type == PeriodicReportType.SemiAnnualReport);
            AnnualSource.Source = vm.Where(x => x.Type == PeriodicReportType.AnnualReport);
        });

    }




    [RelayCommand]
    public async Task UploadQuarterly()
    {
        var items = QuarterlySource?.View?.OfType<FundPeriodicReportViewModel>().ToList();

        if (items is null) return;

        foreach (var v in items)
        {
            // 启动上传（不 await，让它在后台运行）
            _ = Task.Run(async () =>
            {
                try
                {
                    await v.Upload();
                    // 可选：成功后记录日志
                }
                catch (Exception ex)
                {
                    // 建议记录异常，避免静默失败
                    LogEx.Error($"Upload failed for {v.FundName}: {ex}");
                }
            });

            // 等待 5 秒后再启动下一个
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }


    [RelayCommand]
    public void GenerateMeishiAnnounces()
    {    // Validate inputs
        if (SelectedMonth is null || SelectedYear <= 0 || SelectedMonth < 1 || SelectedMonth > 12)
            return;

        int quarterIndex = (SelectedMonth.Value - 1) / 3; // 0, 1, 2, or 3
        string[] quarters = ["一", "二", "三", "四"];
        string q = quarters[quarterIndex];

        using var ms = new FileStream(@$"temp\{SelectedYear}{q}季报.zip", FileMode.Create);
        using ZipArchive archive = new ZipArchive(ms, ZipArchiveMode.Create);

        foreach (var item in QuarterlySource.View)
        {
            if (item is FundPeriodicReportViewModel v)
            {
                if (v.Pdf?.Meta is not null)
                    archive.CreateEntryFromFile(@$"files\hardlink\{v.Pdf.Meta.Id}", $"{v.FundName}-{SelectedYear}{q}季度报告-{DateTime.Now:yyyyMMdd}.pdf");
            }
        }

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
        {
            FileName = "temp",
            UseShellExecute = true
        });
    }
}