using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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




public partial class OpenRuleViewModel:ObservableObject
{
    public static FundOpenType[] Types { get; } = [FundOpenType.Yearly, FundOpenType.SemiAnnually, FundOpenType.Quarterly, FundOpenType.Monthly, FundOpenType.Weekly, FundOpenType.Daily];

    public static Month[] Months { get; } = (Month[])Enum.GetValues(typeof(Month));

    [ObservableProperty]
    public partial int SelectedYear { get; set; } = DateTime.Now.Year;


    partial void OnSelectedYearChanged(int value)
    {
        var days = TradingDay.DaysByYear(value);

        



    }



}



public partial class DayIsOpenViewModel: ObservableObject
{
    public DateOnly Day { get; set; }

    [ObservableProperty]
    public partial bool IsOpen { get; set; }
}