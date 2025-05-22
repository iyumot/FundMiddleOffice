using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Schedule;

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



public partial class TaskPageViewModel : ObservableObject, IRecipient<RemoveMissionMessage>
{

    public ObservableCollection<AutomationViewModelBase> Tasks { get; } = new();


    public TaskTemplate[]? Templates { get; set; }


    public TaskPageViewModel()
    {
        WeakReferenceMessenger.Default.RegisterAll(this);
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

        InitTaskTpl(); return;


        if (!Tasks.Any(x => x is GatherDailyFromMailViewModel))
            Tasks.Add(new GatherDailyFromMailViewModel(new()));
        if (!Tasks.Any(x => x is SendDailyReportToWebhookViewModel))
            Tasks.Add(new SendDailyReportToWebhookViewModel(new()));

        //if (!Tasks.Any(x => x is MailCacheViewModel))
        //    Tasks.Add(new MailCacheViewModel(new()));

        if (!Tasks.Any(x => x is TAFromMailViewModel))
            Tasks.Add(new TAFromMailViewModel(new()));
    }


    public void InitTaskTpl()
    {
        var mvm = AssemblyLoadContext.Default.Assemblies.SelectMany(x => x.DefinedTypes).Where(x => x.BaseType is not null && x.BaseType.IsGenericType && x.BaseType.GetGenericTypeDefinition() == typeof(MissionViewModel<>));

        Templates = mvm.Select(x => (type: x, attr: x.GetCustomAttribute<MissionTitleAttribute>())).Where(x => x.attr is not null).Select(x => new TaskTemplate { Title = x.attr!.Title, ViewModel = x.type }).ToArray();


    }




    [RelayCommand]
    public void AddMailCache()
    {
        Tasks.Add(new MailCacheViewModel(new()));
    }

    [RelayCommand]
    public void AddTask(TaskTemplate template)
    {
        if (template.ViewModel.BaseType?.GetGenericArguments() is Type[] types) 
            try { Tasks.Add((Activator.CreateInstance(template.ViewModel, Activator.CreateInstance(types[0])) as AutomationViewModelBase)!); } catch { }
    }


    public void Receive(RemoveMissionMessage message)
    {
        Tasks.Remove(message.ViewModel);
        MissionSchedule.Unregister(message.ViewModel.Id);
    }


    public class TaskTemplate
    {
        public string? Title { get; set; }

        public required Type ViewModel { get; set; }
    }
}

