using System.ComponentModel;
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
    [NotifyPropertyChangedFor(nameof(AllowDayOrder))]
    public partial FundOpenType SelectedType { get; set; }

    public static FundOpenType[] Types { get; } = [FundOpenType.Daily, FundOpenType.Weekly, FundOpenType.Monthly, FundOpenType.Quarterly, FundOpenType.Yearly,];


    public bool AllowDayOrder => SelectedType != FundOpenType.Weekly || TradeOrNatrual;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AllowDayOrder))]
    [NotifyPropertyChangedFor(nameof(Statement))]
    public partial bool TradeOrNatrual { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Statement))] 
    public partial bool WeekDescend { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Statement))]
    public partial bool DayDescend { get; set; }


    public bool ShowQuarterList => SelectedType switch { FundOpenType.Yearly => true, _ => false };

    public bool ShowMonthList => SelectedType switch { FundOpenType.Yearly or FundOpenType.Quarterly => true, _ => false };

    public bool ShowWeekList => SelectedType switch { FundOpenType.Yearly or FundOpenType.Quarterly or FundOpenType.Monthly => true, _ => false };

    public bool ShowDayList => SelectedType switch { FundOpenType.Daily or FundOpenType.Closed => false, _ => true };


    public string? Statement => Rule.ToString();

    public OpenRule Rule => Build();

    partial void OnTradeOrNatrualChanged(bool value)
    {
        if (!value && SelectedType == FundOpenType.Weekly)
            DayDescend = false;
    }

    partial void OnSelectedTypeChanged(FundOpenType value)
    {
        MonthSource.View.Refresh();
        WeekSource.View.Refresh();
        DaySource.View.Refresh();

        OnPropertyChanged(nameof(Statement)); 
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

        
        OnPropertyChanged(nameof(Statement));
    }

    private void MonthChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        WeekSource.View.Refresh();
        DaySource.View.Refresh();

        OnPropertyChanged(nameof(Statement));
    }

    private void WeekChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        DaySource.View.Refresh();


        OnPropertyChanged(nameof(Statement));
    }

    private void DayChanged(object? sender, PropertyChangedEventArgs e)
    { 
        OnPropertyChanged(nameof(Statement));
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
            DayOrder = /*Days[0].IsSelected*/ DayDescend ? SequenceOrder.Descend : SequenceOrder.Ascend,
            TradeOrNatural = TradeOrNatrual//Days[1].IsSelected
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