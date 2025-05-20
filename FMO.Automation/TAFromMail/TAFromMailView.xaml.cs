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
using CommunityToolkit.Mvvm.ComponentModel;

namespace FMO.Schedule;

/// <summary>
/// TAFromMailView.xaml 的交互逻辑
/// </summary>
public partial class TAFromMailView : UserControl
{
    public TAFromMailView()
    {
        InitializeComponent();
    }
}



public partial class TAFromMailViewModel : MissionViewModel<TAFromMailMission>
{
    [ObservableProperty]
    public partial string? Mail { get; set; }


    [ObservableProperty]
    public partial int? Interval { get; set; }

    public TAFromMailViewModel(TAFromMailMission m) : base(m)
    {
        Title = "TA更新";

        Mail = m.Mail;
        Interval = m.Interval == 0 ? null : m.Interval;
    }
}