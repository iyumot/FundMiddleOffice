using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace FMO;

/// <summary>
/// OpenRuleEditor.xaml 的交互逻辑
/// </summary>
public partial class OpenRuleEditor : UserControl
{
    public OpenRuleEditor()
    {
        InitializeComponent();
    }
}


public enum SequenceOrder { Ascend, Descend }

public class OpenRule
{
    static string[] array = ["一", "二", "三", "四", "五",];
    public FundOpenType Type { get; set; }

    /// <summary>
    /// 选择季
    /// Year 否则1-4
    /// 其它 忽略
    /// </summary>
    public int[]? Quarters { get; set; }

    /// <summary>
    /// 选择月
    /// Year 否则1-12
    /// QuarterFlag 则1-3
    /// Month Week 忽略
    /// </summary>
    public int[]? Months { get; set; }

    /// <summary>
    /// 选择周
    /// Year 否则1-54
    /// QuarterFlag 则1-14
    /// Month 1-5
    /// Week 忽略
    /// </summary>
    public int[]? Weeks { get; set; }

    public SequenceOrder WeekOrder { get; set; }

    /// <summary>
    /// 选择周
    /// Year 否则1-54
    /// QuarterFlag 则1-14
    /// Month 1-5
    /// Week 忽略
    /// </summary>
    public int[]? Dates { get; set; }

    /// <summary>
    /// 选择天
    /// Year 1-365
    /// QuarterFlag 1-92
    /// Month 1-31
    /// Week 7
    /// </summary>
    public SequenceOrder DayOrder { get; set; }

    public bool TradeOrNatural { get; set; }

    /// <summary>
    /// 是否顺延
    /// </summary>
    public bool Postpone‌ { get; set; }

    /// <summary>
    /// 顺延是否跨周
    /// </summary>
    public bool CrossWeek { get; set; }


    private string WeekStr()
    {
        var days = Dates?.Where(x => x < 5);
        if (days is null || !days.Any()) return "";

        if (DayOrder == SequenceOrder.Ascend)
        {
            if (TradeOrNatural)
                return $"第{string.Join('、', days!.Select(x => x + 1))}个交易日开放";
            else
                return $"周{string.Join('、', days!.Select(x => array[x]))}开放";
        }
        else
        {
            if (TradeOrNatural)
                return $"倒数第{string.Join('、', days!.Select(x => x + 1))}个交易日开放";
            else
                return $"倒数第{string.Join('、', days!.Select(x => x + 1))}个自然日开放";
        }
    }


    private string MonthStr()
    {
        if (Dates is null || Dates.Length == 0) return "";

        if (Weeks?.Length > 0)
            return $"{(WeekOrder == SequenceOrder.Ascend ? "" : "倒数")}第{string.Join('、', Weeks.Select(x => x + 1))}周的{WeekStr()}";
        else
            return $"{(DayOrder == SequenceOrder.Ascend ? "" : "倒数")}第{string.Join('、', Dates.Select(x => x + 1))}个{(TradeOrNatural ? "交易" : "自然")}日开放";
    }

    private string QuarterStr()
    {
        if (Dates is null || Dates.Length == 0) return "";

        if (Months?.Length > 0)
            return $"第{string.Join('、', Months.Select(x => x + 1))}月的{MonthStr()}";
        else if (Weeks?.Length > 0)
            return $"{(WeekOrder == SequenceOrder.Ascend ? "" : "倒数")}第{string.Join('、', Weeks.Select(x => x + 1))}周的{WeekStr()}";
        else
            return $"{(DayOrder == SequenceOrder.Ascend ? "" : "倒数")}第{string.Join('、', Dates.Select(x => x + 1))}个{(TradeOrNatural ? "交易" : "自然")}日开放";
    }

    public override string ToString()
    {
        switch (Type)
        {
            case FundOpenType.Closed:
                return "不开放";
            case FundOpenType.Yearly:
                if (Dates is null || Dates.Length == 0) return "无效的设置";
                if (Quarters?.Length > 0)
                    return $"每年第{string.Join('、', Quarters.Select(x => x))}季度的{QuarterStr()}";
                else if (Months?.Length > 0)
                    return $"每年第{string.Join('、', Months.Select(x => x))}月的{MonthStr()}";
                else if (Weeks?.Length > 0)
                    return $"每年{(WeekOrder == SequenceOrder.Ascend ? "" : "倒数")}第{string.Join('、', Weeks.Select(x => x + 1))}周的{WeekStr()}";
                else
                    return $"每年{(DayOrder == SequenceOrder.Ascend ? "" : "倒数")}第{string.Join('、', Dates.Select(x => x + 1))}个{(TradeOrNatural ? "交易" : "自然")}日开放";
            case FundOpenType.Quarterly:
                if (Dates is null || Dates.Length == 0) return "无效的设置";
                return "每季" + QuarterStr();
            case FundOpenType.Monthly:
                if (Dates is null || Dates.Length == 0) return "无效的设置";
                return "每月" + MonthStr();
            case FundOpenType.Weekly:
                if (Dates is null || Dates.Length == 0) return "无效的设置";

                return "每周" + WeekStr();
            case FundOpenType.Daily:
                return "每日开放";
            default:
                return "-";
        }
    }


    public IEnumerable<DateMeta> FilterByWeek2(IEnumerable<DateMeta> dates)
    {
        foreach (var item in dates)
        {


            yield return item;
        }
    }
    public IEnumerable<DateMeta> FilterByWeek(IEnumerable<DateMeta> dates)
    {
        var days = Dates?.Where(x => x <= 5);
        if (days is null || !days.Any())
            return Array.Empty<DateMeta>();

        if (DayOrder == SequenceOrder.Ascend)
        {
            if (TradeOrNatural)
                return dates.Where(x => x.Flag.HasFlag(DayFlag.Trade)).GroupBy(x => x.Week).SelectMany(x => ) && days.Contains((int)x.Date.DayOfWeek));
            else
                return dates.Where(x => days.Contains((int)x.Date.DayOfWeek));
        }
        else
        {
            if (TradeOrNatural)
                return $"倒数第{string.Join('、', days!.Select(x => x + 1))}个交易日开放";
            else
                return $"倒数第{string.Join('、', days!.Select(x => x + 1))}个自然日开放";
        }
    }


    //public DateOnly[] FilterDays(int year)
    //{
    //    IEnumerable<DateMeta> dates = Days.DayInfosByYear(year);

    //    switch (Type)
    //    {
    //        case FundOpenType.Closed:
    //            return Array.Empty<DateOnly>();
    //        case FundOpenType.Yearly:
    //            if (Quarters?.Length > 0)
    //                dates = dates.Where(x => Quarters.Contains(QuarterOfDay(x)));
    //            if (Months?.Length > 0)
    //                dates = dates.Where(x => Months.Contains(x.Month));
    //            else if (Weeks?.Length > 0)
    //                return $"每年{(WeekOrder == SequenceOrder.Ascend ? "" : "倒数")}第{string.Join('、', Weeks.Select(x => x + 1))}周的{WeekStr()}";
    //            else
    //                break;
    //        case FundOpenType.Quarterly:
    //            break;
    //        case FundOpenType.Monthly:
    //            break;
    //        case FundOpenType.Weekly:
    //            break;
    //        case FundOpenType.Daily:
    //            return dates.Where(x=>x.Flag.HasFlag(DayFlag.Trade)).Select(x=>x.Date).ToArray();
    //        default:
    //            break;
    //    }




    //}


    private static int QuarterOfDay(DateOnly d) => (d.Month - 1) / 3;
}

public partial class OpenRuleViewModel : ObservableObject
{

    /// <summary>
    /// 选择季
    /// Year 否则1-4
    /// 其它 忽略
    /// </summary>
    public QuarterInfo[] Quarters { get; }

    /// <summary>
    /// 选择月
    /// Year 否则1-12
    /// QuarterFlag 则1-3
    /// Month Week 忽略
    /// </summary>
    public MonthInfo[] Months { get; }

    public CollectionViewSource MonthSource { get; } = new();

    /// <summary>
    /// 选择周
    /// Year 否则1-54
    /// QuarterFlag 则1-14
    /// Month 1-5
    /// Week 忽略
    /// </summary>
    public WeekInfo[] Weeks { get; set; }
    public CollectionViewSource WeekSource { get; } = new();



    [ObservableProperty]
    public partial SequenceOrder WeekOrder { get; set; }

    /// <summary>
    /// 选择周
    /// Year 否则1-54
    /// QuarterFlag 则1-14
    /// Month 1-5
    /// Week 忽略
    /// </summary>
    public DateInfo[] Days { get; set; }
    public CollectionViewSource DaySource { get; } = new();

    /// <summary>
    /// 选择天
    /// Year 1-365
    /// QuarterFlag 1-92
    /// Month 1-31
    /// Week 7
    /// </summary>
    [ObservableProperty]
    public partial SequenceOrder DayOrder { get; set; }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowMonthList))]
    [NotifyPropertyChangedFor(nameof(ShowQuarterList))]
    [NotifyPropertyChangedFor(nameof(ShowWeekList))]
    [NotifyPropertyChangedFor(nameof(ShowDayList))]
    public partial FundOpenType SelectedType { get; set; }

    public static FundOpenType[] Types { get; } = [FundOpenType.Daily, FundOpenType.Weekly, FundOpenType.Monthly, FundOpenType.Quarterly, FundOpenType.Yearly,];




    public bool ShowQuarterList => SelectedType switch { FundOpenType.Yearly => true, _ => false };

    public bool ShowMonthList => SelectedType switch { FundOpenType.Yearly or FundOpenType.Quarterly => true, _ => false };

    public bool ShowWeekList => SelectedType switch { FundOpenType.Yearly or FundOpenType.Quarterly or FundOpenType.Monthly => true, _ => false };

    public bool ShowDayList => SelectedType switch { FundOpenType.Daily or FundOpenType.Closed => false, _ => true };


    [ObservableProperty]
    public partial string? Statement { get; set; }

    public OpenRule Rule => Build();


    partial void OnSelectedTypeChanged(FundOpenType value)
    {
        MonthSource.View.Refresh();
        WeekSource.View.Refresh();
        DaySource.View.Refresh();

        Statement = Rule.ToString();
    }



    public OpenRuleViewModel()
    {
        SelectedYear = DateTime.Today.Year;


        Quarters = Enumerable.Range(1, 4).Select(x => new QuarterInfo(x)).ToArray();
        foreach (var item in Quarters)
            item.PropertyChanged += QuarterChanged;


        Months = Enumerable.Range(1, 12).Select(x => new MonthInfo(x)).ToArray();
        MonthSource.Source = Months;
        MonthSource.Filter += MonthSource_Filter;

        foreach (var item in Months)
            item.PropertyChanged += MonthChanged;

        Weeks = [new WeekInfo { Value = -1, Name = "倒序" }, .. Enumerable.Range(1, 54).Select(x => new WeekInfo(x))];

        foreach (var item in Weeks)
            item.PropertyChanged += WeekChanged;
        WeekSource.Source = Weeks;
        WeekSource.Filter += WeekSource_Filter;


        Days = [new DateInfo { Value = -1, Name = "倒序" }, new DateInfo { Value = -1, Name = "交易日" }, .. Enumerable.Range(1, 366).Select(x => new DateInfo(x))];
        foreach (var item in Days)
            item.PropertyChanged += DayChanged;
        DaySource.Source = Days;
        DaySource.Filter += DaySource_Filter;

        DayOfWeeks = [new(DayOfWeek.Monday), new(DayOfWeek.Tuesday), new(DayOfWeek.Wednesday), new(DayOfWeek.Thursday), new(DayOfWeek.Friday)];
        foreach (var item in DayOfWeeks)
            item.PropertyChanged += (s, e) => UpdateByDayOfWeekChoose();

    }

    private void MonthSource_Filter(object sender, FilterEventArgs e)
    {
        if (SelectedType == FundOpenType.Quarterly || Quarters.Any(x => x.IsSelected))
            e.Accepted = (e.Item as MonthInfo)!.Value <= 3;
        else if (SelectedType == FundOpenType.Yearly)
            e.Accepted = true;
    }

    private void DaySource_Filter(object sender, FilterEventArgs e)
    {
        if (SelectedType == FundOpenType.Weekly || (Weeks.Any(x => x.IsSelected && x.Value != -1)))
            e.Accepted = (e.Item as DateInfo)!.Value <= 5;
        else if (SelectedType == FundOpenType.Monthly || Months.Any(x => x.IsSelected))
            e.Accepted = (e.Item as DateInfo)!.Value <= 31;
        else if (SelectedType == FundOpenType.Quarterly || Quarters.Any(x => x.IsSelected))
            e.Accepted = (e.Item as DateInfo)!.Value <= 92;
        else if (SelectedType == FundOpenType.Yearly)
            e.Accepted = true;

        if (!e.Accepted) (e.Item as DateInfo)!.IsSelected = false;
    }

    private void WeekSource_Filter(object sender, FilterEventArgs e)
    {
        if (SelectedType == FundOpenType.Monthly || Months.Any(x => x.IsSelected))
            e.Accepted = (e.Item as WeekInfo)!.Value <= 6;
        else if (SelectedType == FundOpenType.Quarterly || Quarters.Any(x => x.IsSelected))
            e.Accepted = (e.Item as WeekInfo)!.Value <= 26;
        else if (SelectedType == FundOpenType.Yearly)
            e.Accepted = true;
    }

    private void QuarterChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        MonthSource.View.Refresh();
        WeekSource.View.Refresh();
        DaySource.View.Refresh();

        Statement = Rule.ToString();
    }

    private void MonthChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        WeekSource.View.Refresh();
        DaySource.View.Refresh();

        Statement = Rule.ToString();
    }

    private void WeekChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        DaySource.View.Refresh();


        Statement = Rule.ToString();
    }

    private void DayChanged(object? sender, PropertyChangedEventArgs e)
    {
        Statement = Rule.ToString();
    }




    public OpenRule Build()
    {
        return new OpenRule
        {
            Type = SelectedType,
            Quarters = Quarters.Where(x => x.IsSelected).Select(x => x.Value).ToArray(),
            Months = Months.Where(x => x.IsSelected).Select(x => x.Value).ToArray(),
            Weeks = Weeks.Skip(1).Where(x => x.IsSelected).Select(x => x.Value).ToArray(),
            WeekOrder = Weeks[0].IsSelected ? SequenceOrder.Descend : SequenceOrder.Ascend,
            Dates = Days.Skip(2).Where(x => x.IsSelected).Select(x => x.Value).ToArray(),
            DayOrder = Days[0].IsSelected ? SequenceOrder.Descend : SequenceOrder.Ascend,
            TradeOrNatural = Days[1].IsSelected
        };
    }




    public int[] Years { get; set; } = Enumerable.Range(DateTime.Today.Year - 10, 11).ToArray();

    [ObservableProperty]
    public partial int SelectedYear { get; set; } //= DateTime.Now.Year;




    public Calendar[] Calendars { get; } = Enumerable.Range(1, 12).Select(x => new Calendar { SelectionMode = CalendarSelectionMode.MultipleRange, LayoutTransform = new ScaleTransform(0.7, 0.7) }).ToArray();



    public DayOfWeekInfo[] DayOfWeeks { get; }



    partial void OnSelectedYearChanged(int value)
    {
        //var days = Days.DayInfosByYear(value);


        //AllDays = days.Select(x => new DayIsOpenViewModel { Day = x.Date, }).ToArray();

        //MonthOfYear = Enumerable.Range(1, 12).Select(x => new MonthInfo { Month = (Month)x, Start = new DateTime(SelectedYear, x, 1), End = new DateTime(SelectedYear, x, 1).AddMonths(1).AddDays(-1) }).ToArray();

        //for (int i = 0; i < 12; i++)
        //{
        //    Calendars[i].SelectedDates.Clear();
        //    Calendars[i].DisplayDateEnd = new DateTime(SelectedYear, i + 1, 1).AddMonths(1).AddDays(-1);
        //    Calendars[i].DisplayDateStart = new DateTime(SelectedYear, i + 1, 1);
        //    Calendars[i].BlackoutDates.Clear();

        //    var black = days.Where(x => x.Flag.HasFlag(DayFlag.Weekend) || x.Flag.HasFlag(DayFlag.Holiday)).Select(x => new DateTime(x.Date, default));

        //    foreach (var blackdate in black)
        //    {
        //        Calendars[i].BlackoutDates.Add(new CalendarDateRange(blackdate));
        //    }

        //}
    }


    //partial void OnSelectedTypeChanged(FundOpenType value)
    //{
    //    //ShowMonth = value switch { FundOpenType.Yearly or FundOpenType.SemiAnnually => true, _ => false };

    //    //if (value == FundOpenType.Daily)
    //    //{
    //    //    var days = Days.TradeDaysByYear(SelectedYear);
    //    //    foreach (var c in Calendars)
    //    //    {
    //    //        foreach (var d in days.Where(x => x.Month == c.DisplayDateEnd!.Value.Month))
    //    //            c.SelectedDates.Add(new DateTime(d, default));
    //    //    }
    //    //}
    //    //else
    //    //{
    //    //    foreach (var c in Calendars)
    //    //        c.SelectedDates.Clear();
    //    //}
    //}

    private void UpdateByDayOfWeekChoose()
    {
        var value = DayOfWeeks.Where(x => x.IsSelected).Select(x => x.Value).ToArray();


        foreach (var c in Calendars)
            c.SelectedDates.Clear();


        //var days = Days.TradeDaysByYear(SelectedYear);

        //foreach (var c in Calendars)
        //{
        //    foreach (var d in days.Where(x => x.Month == c.DisplayDateEnd!.Value.Month && value.Contains(x.DayOfWeek)))
        //        c.SelectedDates.Add(new DateTime(d, default));
        //}
    }




    public partial class QuarterInfo : ObservableObject
    {
        public required string Name { get; set; }

        public int Value { get; set; }


        [ObservableProperty]
        public partial bool IsSelected { get; set; }

        [SetsRequiredMembers]
        public QuarterInfo(int i)
        {
            Value = i;
            Name = i switch { 1 => "一季度", 2 => "二季度", 3 => "三季度", 4 => "四季度", _ => "Error" };
        }
    }

    public partial class MonthInfo : ObservableObject
    {
        static string[] array = ["一", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二"];

        [SetsRequiredMembers]
        public MonthInfo(int month)
        {
            Value = month;
            Name = $"{array[month - 1]}月";
        }


        public required string Name { get; set; }

        public int Value { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        [ObservableProperty]
        public partial bool IsSelected { get; set; }


    }



    public partial class WeekInfo : ObservableObject
    {

        [SetsRequiredMembers]
        public WeekInfo(int w)
        {
            Value = w;
            Name = $"第{w}周";
        }

        public WeekInfo() { }


        public required string Name { get; set; }

        public int Value { get; set; }

        [ObservableProperty]
        public partial bool IsSelected { get; set; }
    }


    public partial class DateInfo : ObservableObject
    {

        [SetsRequiredMembers]
        public DateInfo(int day)
        {
            Value = day;
            Name = $"第{day}日";
        }

        public DateInfo() { }


        public required string Name { get; set; }

        public int Value { get; set; }

        [ObservableProperty]
        public partial bool IsSelected { get; set; }
    }
}









public partial class DayIsOpenViewModel : ObservableObject
{
    public DateOnly Day { get; set; }

    [ObservableProperty]
    public partial bool IsOpen { get; set; }
}

public partial class DayOfWeekInfo : ObservableObject
{
    static string[] array = ["一", "二", "三", "四", "五"];

    [SetsRequiredMembers]
    public DayOfWeekInfo(DayOfWeek value)
    {
        Value = value;
        Name = array[(int)value - 1];
    }

    public required string Name { get; set; }

    public DayOfWeek Value { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}