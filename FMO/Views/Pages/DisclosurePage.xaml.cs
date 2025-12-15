using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using FMO.Utilities;
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

    public int[] Months { get; set; }

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






}