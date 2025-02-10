using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Schedule;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// TaskPage.xaml 的交互逻辑
/// </summary>
public partial class TaskPage : UserControl
{
    public TaskPage()
    {
        InitializeComponent();
    }
}



public partial class TaskPageViewModel : ObservableObject
{

    public ObservableCollection<AutomationViewModelBase> Tasks { get; } = new();


    public TaskPageViewModel()
    {
        var ms = MissionSchedule.Missions;

        foreach (var m in ms)
        {
            var assembly = Assembly.GetAssembly(m.GetType());

            var vmtype = assembly!.GetType(m.GetType().ToString().Replace("Mission", "ViewModel"));
            if (vmtype is null)
                continue;

            if (!vmtype.IsAssignableTo(typeof(AutomationViewModelBase)))
                continue;

            var obj = Activator.CreateInstance(vmtype, m) as AutomationViewModelBase;

            if (obj is not null)
                Tasks.Add(obj);
        }

        if (!Tasks.Any(x => x is GatherDailyFromMailViewModel))
            Tasks.Add(new GatherDailyFromMailViewModel(new()));
        if (!Tasks.Any(x => x is SendDailyReportToWebhookViewModel))
            Tasks.Add(new SendDailyReportToWebhookViewModel(new()));
    }
}

