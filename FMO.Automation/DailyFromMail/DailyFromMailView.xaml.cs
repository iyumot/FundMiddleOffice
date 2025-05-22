using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FMO.Schedule;

/// <summary>
/// DailyFromMailView.xaml 的交互逻辑
/// </summary>
public partial class DailyFromMailView : UserControl
{
    public DailyFromMailView()
    {
        InitializeComponent();
    }
}





[MissionTitle("净值更新")]
public partial class DailyFromMailViewModel : MissionViewModel<TAFromMailMission>
{
    [ObservableProperty]
    public partial string? Mail { get; set; }


    [ObservableProperty]
    public partial int? Interval { get; set; }

    public DailyFromMailViewModel(TAFromMailMission m) : base(m)
    {
        Title = "净值更新";

        Mail = m.Mail;
        Interval = m.Interval == 0 ? null : m.Interval;
    }
}