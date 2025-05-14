using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;

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
    public bool YearFlag { get; set; }

    public bool QuarterFlag { get; set; }

    public bool MonthFlag { get; set; }

    public bool WeekFlag { get; set; }

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
    public int[]? Days { get; set; }

    /// <summary>
    /// 选择天
    /// Year 1-365
    /// QuarterFlag 1-92
    /// Month 1-31
    /// Week 7
    /// </summary>
    public SequenceOrder DaykOrder { get; set; }

    public bool TradeOrNatural { get; set; }
}

public partial class OpenRuleViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowQuarterList))]
    [NotifyPropertyChangedFor(nameof(ShowMonthList))]
    [NotifyPropertyChangedFor(nameof(ShowWeekList))]
    public partial bool YearFlag { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowQuarterList))]
    [NotifyPropertyChangedFor(nameof(ShowMonthList))]
    [NotifyPropertyChangedFor(nameof(ShowWeekList))]
    public partial bool QuarterFlag { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowQuarterList))]
    [NotifyPropertyChangedFor(nameof(ShowMonthList))]
    [NotifyPropertyChangedFor(nameof(ShowWeekList))]
    public partial bool MonthFlag { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowQuarterList))]
    [NotifyPropertyChangedFor(nameof(ShowMonthList))]
    [NotifyPropertyChangedFor(nameof(ShowWeekList))]
    public partial bool WeekFlag { get; set; }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowQuarterList))]
    [NotifyPropertyChangedFor(nameof(ShowMonthList))]
    [NotifyPropertyChangedFor(nameof(ShowWeekList))]
    public partial bool DayFlag { get; set; }

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
    public DayInfo[]? Days { get; set; }
    public CollectionViewSource DaySource { get; } = new();

    /// <summary>
    /// 选择天
    /// Year 1-365
    /// QuarterFlag 1-92
    /// Month 1-31
    /// Week 7
    /// </summary>
    [ObservableProperty]
    public partial SequenceOrder DaykOrder { get; set; }


    public static FundOpenType[] Types { get; } = [FundOpenType.Yearly, FundOpenType.SemiAnnually, FundOpenType.Quarterly, FundOpenType.Monthly, FundOpenType.Weekly, FundOpenType.Daily];



    //public static DayOfWeek[] DayOfWeeks { get; } = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];

    public static int[] DayIndex { get; } = [1, 2, 3, 4, 5];

    /// <summary>
    /// 倒序
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OrderString))]
    public partial bool Descende { get; set; }

    /// <summary>
    /// 自然日或交易日
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DayTypeString))]
    public partial bool TradeOrNatural { get; set; }


    public bool ShowQuarterList => !DayFlag && !WeekFlag && !MonthFlag && !QuarterFlag && YearFlag;

    public bool ShowMonthList => !DayFlag && !WeekFlag && !MonthFlag && (QuarterFlag || YearFlag);

    public bool ShowWeekList => !DayFlag && !WeekFlag && (MonthFlag || QuarterFlag || YearFlag);


    partial void OnQuarterFlagChanged(bool value)
    {
        MonthSource.View.Refresh();
        WeekSource.View.Refresh();
        DaySource.View.Refresh();
    }
    partial void OnMonthFlagChanged(bool value)
    {
        WeekSource.View.Refresh();
        DaySource.View.Refresh();
    }

    partial void OnWeekFlagChanged(bool value)
    {
        DaySource.View.Refresh();
    }

    partial void OnYearFlagChanged(bool value)
    {
        WeekSource.View.Refresh();
        DaySource.View.Refresh();
    }

    public OpenRuleViewModel()
    {
        SelectedYear = DateTime.Today.Year;


        Quarters = [new(0), new(1), new(2), new(3)];
        foreach (var item in Quarters)
            item.PropertyChanged += QuarterChanged;


        Months = Enumerable.Range(0, 12).Select(x => new MonthInfo(x)).ToArray();
        MonthSource.Source = Months;
        MonthSource.Filter += (s, e) => e.Accepted = e.Item switch { MonthInfo m => QuarterFlag ? m.Value < 3 : true, _ => true };

        foreach (var item in Months)
            item.PropertyChanged += MonthChanged;

        DayOfWeeks = [new(DayOfWeek.Monday), new(DayOfWeek.Tuesday), new(DayOfWeek.Wednesday), new(DayOfWeek.Thursday), new(DayOfWeek.Friday)];
        foreach (var item in DayOfWeeks)
            item.PropertyChanged += (s, e) => UpdateByDayOfWeekChoose();


        Weeks = Enumerable.Range(0, 54).Select(x => new WeekInfo(x)).ToArray();
        
        foreach (var item in Weeks)
            item.PropertyChanged += WeekChanged;
        WeekSource.Source = Weeks;
        WeekSource.Filter += WeekSource_Filter; ;


        Days = Enumerable.Range(0, 365).Select(x => new DayInfo(x)).ToArray();
        DaySource.Source = Days;
        DaySource.Filter += DaySource_Filter; ; ;
    }

    private void WeekChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        DaySource.View.Refresh();
    }

    private void DaySource_Filter(object sender, FilterEventArgs e)
    {
        if (WeekFlag || Weeks.Any(x=>x.IsSelected))
            e.Accepted = (e.Item as DayInfo)!.Value < 5;
        else if (MonthFlag)
            e.Accepted = (e.Item as DayInfo)!.Value < 31;
        else if (QuarterFlag)
            e.Accepted = (e.Item as DayInfo)!.Value < 92;
        else if (YearFlag)
            e.Accepted = true;
    }

    private void WeekSource_Filter(object sender, FilterEventArgs e)
    {
        if (MonthFlag)
            e.Accepted = (e.Item as WeekInfo)!.Value < 6;
        else if (QuarterFlag)
            e.Accepted = (e.Item as WeekInfo)!.Value < 26;
        else if (YearFlag)
            e.Accepted = true;
    }

    private void QuarterChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is QuarterInfo q && !q.IsSelected) return;

        if (Quarters.Any(x => x.IsSelected))
            foreach (var m in Months)
                m.IsSelected = false;
    }
    private void MonthChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is MonthInfo q && !q.IsSelected) return;

        if (Months.Any(x => x.IsSelected))
            foreach (var m in Quarters)
                m.IsSelected = false;
    }


    public string OrderString => Descende ? "倒序" : "顺序";
    public string DayTypeString => TradeOrNatural ? "交易日" : "自然日";


    public int[] Years { get; set; } = Enumerable.Range(DateTime.Today.Year - 10, 11).ToArray();

    [ObservableProperty]
    public partial int SelectedYear { get; set; } //= DateTime.Now.Year;


    [ObservableProperty]
    public partial FundOpenType SelectedType { get; set; }


    [ObservableProperty]
    public partial DayIsOpenViewModel[]? AllDays { get; set; }

    [ObservableProperty]
    public partial DayIsOpenViewModel[]? Selected { get; set; }


    [ObservableProperty]
    public partial MonthInfo[]? MonthOfYear { get; set; }

    public Calendar[] Calendars { get; } = Enumerable.Range(1, 12).Select(x => new Calendar { SelectionMode = CalendarSelectionMode.MultipleRange, LayoutTransform = new ScaleTransform(0.7, 0.7) }).ToArray();


    [ObservableProperty]
    public partial DayOfWeek[]? SelectedDayOfWeek { get; set; }

    public DayOfWeekInfo[] DayOfWeeks { get; }


    [ObservableProperty]
    public partial bool ShowMonth { get; set; }



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

    partial void OnSelectedChanged(DayIsOpenViewModel[]? value)
    {
        Debug.WriteLine(value?.Length);
    }

    partial void OnSelectedTypeChanged(FundOpenType value)
    {
        //ShowMonth = value switch { FundOpenType.Yearly or FundOpenType.SemiAnnually => true, _ => false };

        //if (value == FundOpenType.Daily)
        //{
        //    var days = Days.TradeDaysByYear(SelectedYear);
        //    foreach (var c in Calendars)
        //    {
        //        foreach (var d in days.Where(x => x.Month == c.DisplayDateEnd!.Value.Month))
        //            c.SelectedDates.Add(new DateTime(d, default));
        //    }
        //}
        //else
        //{
        //    foreach (var c in Calendars)
        //        c.SelectedDates.Clear();
        //}
    }

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
            Name = i switch { 0 => "一季度", 1 => "二季度", 2 => "三季度", 3 => "四季度", _ => "Error" };
        }
    }

    public partial class MonthInfo : ObservableObject
    {
        static string[] array = ["一", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二"];

        [SetsRequiredMembers]
        public MonthInfo(int month)
        {
            Value = month;
            Name = $"{array[month]}月";
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
        public WeekInfo(int month)
        {
            Value = month;
            Name = $"第{month + 1}周";
        }


        public required string Name { get; set; }

        public int Value { get; set; }

        [ObservableProperty]
        public partial bool IsSelected { get; set; }
    }


    public partial class DayInfo : ObservableObject
    {

        [SetsRequiredMembers]
        public DayInfo(int month)
        {
            Value = month;
            Name = $"第{month + 1}日";
        }


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