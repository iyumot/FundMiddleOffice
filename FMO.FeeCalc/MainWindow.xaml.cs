using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.IO.Trustee;
using FMO.Models;
using FMO.Utilities;
using LiteDB;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;

namespace FMO.FeeCalc;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum MonthQuarter
{
    [Description("1月")] January = 1,
    [Description("2月")] February,
    [Description("3月")] March,
    [Description("4月")] April,
    [Description("5月")] May,
    [Description("6月")] June,
    [Description("7月")] July,
    [Description("8月")] August,
    [Description("9月")] September,
    [Description("10月")] October,
    [Description("11月")] November,
    [Description("12月")] December,
    [Description("一季度")] Q1,
    [Description("二季度")] Q2,
    [Description("三季度")] Q3,
    [Description("四季度")] Q4
}

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    public partial FundInfo[]? Funds { get; set; }

    public MonthQuarter[] MonthQuarters { get; } = (MonthQuarter[])Enum.GetValues(typeof(MonthQuarter));

    [ObservableProperty]
    public partial DateTime? Begin { get; set; }

    [ObservableProperty]
    public partial DateTime? End { get; set; }

    [ObservableProperty]
    public partial bool IsWorking { get; set; }


    public bool CanCalc => Funds?.Any(x => x.IsChoosed) ?? false;

    public List<ITrusteeAssist> Trustees { get; private set; } = new();

    Debouncer debouncer;


    public MainWindowViewModel()
    {
        debouncer = new Debouncer(() => Update());


        var files = new DirectoryInfo("plugins").GetFiles("*.dll");


        foreach (var file in files)
        {
            try
            {
                var assembly = Assembly.LoadFile(file.FullName);
                TryAddTrustee(assembly);
            }
            catch (Exception e)
            {

            }
        }
    }
    void TryAddTrustee(Assembly assembly)
    {
        var type = assembly.GetTypes().FirstOrDefault(x => x.GetInterface(typeof(ITrusteeAssist).FullName!) is not null);
        if (type is null) return;

        ITrusteeAssist trusteeAssist = (ITrusteeAssist)Activator.CreateInstance(type)!;

        Trustees.Add(trusteeAssist);
    }

    partial void OnBeginChanged(DateTime? value)
    {
        debouncer.Invoke();
    }

    partial void OnEndChanged(DateTime? value)
    {
        debouncer.Invoke();
    }


    private void Update()
    {
        using var db = DbHelper.Base();
        Funds = db.GetCollection<Fund>().FindAll().Where(x => x.Status <= FundStatus.StartLiquidation || Begin switch { DateTime d => x.ClearDate > DateOnly.FromDateTime(d), _ => true }).Select(x => new FundInfo { Fund = x }).ToArray();

        foreach (var f in Funds)
            f.PropertyChanged += (s, e) => Application.Current.Dispatcher.BeginInvoke(()=> CalcCommand.NotifyCanExecuteChanged());// OnPropertyChanged(nameof(CanCalc));

    }

    [RelayCommand]
    public void SetDateRange(MonthQuarter d)
    {
        switch (d)
        {
            case MonthQuarter.January:
                Begin = Begin.HasValue ? new DateTime(Begin.Value.Year, 1, 1) : new DateTime(DateTime.Today.Year, 1, 1);
                End = End.HasValue ? new DateTime(Begin.Value.Year, 1, 31) : new DateTime(DateTime.Today.Year, 1, 31);
                break;
            case MonthQuarter.February:
                Begin = Begin.HasValue ? new DateTime(Begin.Value.Year, 2, 1) : new DateTime(DateTime.Today.Year, 2, 1);
                End = End.HasValue ? new DateTime(Begin.Value.Year, 3, 1).AddDays(-1) : new DateTime(DateTime.Today.Year, 3, 1).AddDays(-1);
                break;
            case MonthQuarter.March:
            case MonthQuarter.May:
            case MonthQuarter.July:
            case MonthQuarter.August:
            case MonthQuarter.October:
            case MonthQuarter.December:
                Begin = Begin.HasValue ? new DateTime(Begin.Value.Year, (int)d, 1) : new DateTime(DateTime.Today.Year, (int)d, 1);
                End = End.HasValue ? new DateTime(Begin.Value.Year, (int)d, 31) : new DateTime(DateTime.Today.Year, (int)d, 31);
                break;
            case MonthQuarter.April:
            case MonthQuarter.June:
            case MonthQuarter.September:
            case MonthQuarter.November:
                Begin = Begin.HasValue ? new DateTime(Begin.Value.Year, (int)d, 1) : new DateTime(DateTime.Today.Year, (int)d, 1);
                End = End.HasValue ? new DateTime(Begin.Value.Year, (int)d, 30) : new DateTime(DateTime.Today.Year, (int)d, 30);
                break;
            case MonthQuarter.Q1:
                Begin = Begin.HasValue ? new DateTime(Begin.Value.Year, 1, 1) : new DateTime(DateTime.Today.Year, 1, 1);
                End = End.HasValue ? new DateTime(Begin.Value.Year, 3, 31) : new DateTime(DateTime.Today.Year, 3, 31);
                break;
            case MonthQuarter.Q2:
                Begin = Begin.HasValue ? new DateTime(Begin.Value.Year, 4, 1) : new DateTime(DateTime.Today.Year, 4, 1);
                End = End.HasValue ? new DateTime(Begin.Value.Year, 6, 30) : new DateTime(DateTime.Today.Year, 6, 30);
                break;
            case MonthQuarter.Q3:
                Begin = Begin.HasValue ? new DateTime(Begin.Value.Year, 7, 1) : new DateTime(DateTime.Today.Year, 7, 1);
                End = End.HasValue ? new DateTime(Begin.Value.Year, 9, 30) : new DateTime(DateTime.Today.Year, 9, 30);
                break;
            case MonthQuarter.Q4:
                Begin = Begin.HasValue ? new DateTime(Begin.Value.Year, 10, 1) : new DateTime(DateTime.Today.Year, 10, 1);
                End = End.HasValue ? new DateTime(Begin.Value.Year, 12, 31) : new DateTime(DateTime.Today.Year, 12, 31);
                break;
            default:
                break;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCalc))]
    public void Calc()
    {
        if (Funds is null) return;

        IsWorking = true;

        // 生成日期列表
        var end = DateOnly.FromDateTime(End!.Value);
        List<DateOnly> dates = new List<DateOnly>();
        dates.Add(DateOnly.FromDateTime(Begin!.Value));
        while (dates[^1] < end)
            dates.Add(dates[^1].AddDays(1));

        Task.Run(async () =>
        {
            var sel = Funds.Where(x => x.IsChoosed).GroupBy(x => x.Fund.Trustee);


            Parallel.ForEach(sel, async f =>
            {
                await Calc(f!, dates);
            });


            await Task.Delay(10000);
            IsWorking = false;
        });

    }

    private async Task Calc(IGrouping<string, FundInfo> col, List<DateOnly> dates)
    {
        foreach (var f in col)
        {
            f.IsWorking = true;
            using var db = new LiteDatabase(@"FileName=data\feecalc.db;Connection=Shared");
            var begin = dates[0];
            var end = dates[^1];
            var fees = db.GetCollection<ManageFeeDetail>($"f{f.Fund.Id}").Find(x => x.Date >= begin && x.Date <= end).OrderBy(x=>x.Date).ToList();
            var fdate = fees.Select(x => x.Date).ToArray();

            // 核验日期
            if (!dates.SequenceEqual(fdate))
            {
                // 重新下载数据
                if (col.Key is null)
                {
                    f.Error = "托管未设置";
                    f.IsWorking = false;
                    continue;
                }

                var assist = Trustees.FirstOrDefault(x => x.Name == col.Key);

                if (assist is null)
                {
                    f.Error = $"未找到{col.Key}托管平台插件";
                    f.IsWorking = false;
                    continue;
                }

                //获取数据
                var mfd = await assist.GetManageFeeDetails(begin, end);
                if (mfd.Count() == 0)
                {
                    f.IsWorking = false;
                    continue;
                }

                using var db2 = DbHelper.Base();
                var dict = db2.GetCollection<Fund>().FindAll().Select(x => new { x.Id, x.Code });

                // 保存数据
                foreach (var item in mfd)
                {
                    var id = dict.FirstOrDefault(x => x.Code == item.Code);
                    if (id is null)
                    {
                        f.Error = $"未找到{col.Key}托管平台插件";
                        continue;
                    }

                    db.GetCollection<ManageFeeDetail>($"f{id.Id}").EnsureIndex(x => x.Date, true);
                    db.GetCollection<ManageFeeDetail>($"f{id.Id}").Upsert(item.Fee);
                }

                // 重新加载
                fees = db.GetCollection<ManageFeeDetail>($"f{f.Fund.Id}").Find(x => x.Date >= begin && x.Date <= end).OrderBy(x => x.Date).ToList();
                fdate = fees.Select(x => x.Date).ToArray();
            }

            // 再次核验日期
            if (!dates.SequenceEqual(fdate))
            {
                f.Error = "没有完整的费用数据";
                f.IsWorking = false;
                continue;
            }

            Calc(f, fees, dates);
        }
    }

    private void Calc(FundInfo f, List<ManageFeeDetail> fees, List<DateOnly> dates)
    {
        var (dd, ids, names, array)  = FundHelper.GenerateShareSheet(f.Fund.Id, dates[0], dates[^1]);


        using (var workbook = new XLWorkbook())
        {
            var sheet = workbook.Worksheets.Add("份额明细表");

            // 客户名
            for (int i = 0; i < ids.Count; i++) 
                sheet.Cell(1, i + 2).Value = names[i]; 

            for (var i = 0;i < dd.Count; i++)
            {
                sheet.Cell(2 + i, 1).Value = dd[i].ToString("yyyy-MM-dd");

                for (var j = 0; j < ids.Count; j++)
                    sheet.Cell(i + 2, j + 2).Value = array[i, j];
            } 

            string path = $"files/fee/{f.Fund.ShortName}_{dates[0]:yyyy.MM.dd}-{dates[^1]:yyyy.MM.dd}.xlsx";
            workbook.SaveAs(path);
        }

    }
}


public partial class FundInfo : ObservableObject
{
    public required Fund Fund { get; set; }

    [ObservableProperty]
    public partial bool IsChoosed { get; set; }

    [ObservableProperty]
    public partial bool IsWorking { get; set; }

    [ObservableProperty]
    public partial string? Error { get; internal set; }
}