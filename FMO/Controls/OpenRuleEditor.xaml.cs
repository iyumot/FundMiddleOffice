using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using System.Diagnostics;
using System.Windows.Controls;

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

    public static FundOpenType[] Types { get; } = [FundOpenType.Yearly, FundOpenType.SemiAnnually, FundOpenType.Quarterly, FundOpenType.Monthly, FundOpenType.Weekly, FundOpenType.Daily];
   


    public static Month[] Months { get; } = (Month[])Enum.GetValues(typeof(Month));


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

    public Calendar[] Calendars { get; } = Enumerable.Range(1, 12).Select(x => new Calendar { SelectionMode = CalendarSelectionMode.MultipleRange }).ToArray();


    [ObservableProperty]
    public partial bool ShowMonth { get; set; }


    public OpenRuleViewModel()
    {
        SelectedYear = DateTime.Today.Year;
    }

    partial void OnSelectedYearChanged(int value)
    {
        var days = Days.DayInfosByYear(value);


        AllDays = days.Select(x => new DayIsOpenViewModel { Day = x.Date, }).ToArray();

        MonthOfYear = Enumerable.Range(1, 12).Select(x => new MonthInfo { Month = (Month)x, Start = new DateTime(SelectedYear, x, 1), End = new DateTime(SelectedYear, x, 1).AddMonths(1).AddDays(-1) }).ToArray();

        for (int i = 0; i < 12; i++)
        {
            Calendars[i].SelectedDates.Clear();
            Calendars[i].DisplayDateEnd = new DateTime(SelectedYear, i + 1, 1).AddMonths(1).AddDays(-1);
            Calendars[i].DisplayDateStart = new DateTime(SelectedYear, i + 1, 1);
            Calendars[i].BlackoutDates.Clear();

            var black = days.Where(x => x.Flag.HasFlag(DayFlag.Weekend) || x.Flag.HasFlag(DayFlag.Holiday)).Select(x => new DateTime(x.Date, default));

            foreach (var blackdate in black)
            {
                Calendars[i].BlackoutDates.Add(new CalendarDateRange(blackdate));
            }

            var open = days.Where(x => x.Date.Month == i + 1 && x.Flag.HasFlag(DayFlag.Trade));
            Calendars[i].SelectedDates.Add(new DateTime(open.First().Date, default));
        }
    }

    partial void OnSelectedChanged(DayIsOpenViewModel[]? value)
    {
        Debug.WriteLine(value?.Length);
    }

    partial void OnSelectedTypeChanged(FundOpenType value)
    {
        ShowMonth = value switch { FundOpenType.Yearly or FundOpenType.SemiAnnually => true, _ => false };
    }
}


public partial class MonthInfo : ObservableObject
{
    public Month Month { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public bool IsChoosed { get; set; }
}

public partial class DayIsOpenViewModel : ObservableObject
{
    public DateOnly Day { get; set; }

    [ObservableProperty]
    public partial bool IsOpen { get; set; }
}