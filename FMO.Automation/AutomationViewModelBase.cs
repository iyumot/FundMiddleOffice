using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using System.ComponentModel;
using System.Reflection;

namespace FMO.Schedule;


public record RemoveMissionMessage(AutomationViewModelBase ViewModel);

public partial class AutomationViewModelBase : ObservableRecipient, IRecipient<MissionMessage>, IRecipient<MissionProgressMessage>
{


    [ObservableProperty]
    public partial bool IsActivated { get; set; }

    [ObservableProperty]
    public partial string? Title { get; set; }

    [ObservableProperty]
    public partial DateTime? LastRunTime { get; set; }

     
    [ObservableProperty]
    public partial DateTime? NextRunDate { get; set; }


    [ObservableProperty]
    public partial DateTime? NextRunTime { get; set; }

    [ObservableProperty]
    public partial string? Description { get; set; }

    [ObservableProperty]
    public partial bool ManualSetNextRun { get; set; }

    [ObservableProperty]
    public partial bool IsWorking { get; set; }

    [ObservableProperty]
    public partial double ProgressValue { get; set; }

    public int Id { get; }

    public AutomationViewModelBase(Mission mission)
    {
        IsActive = true;

        try
        {
            LastRunTime = mission.LastRun;
            NextRunDate = mission.NextRun;
            NextRunTime = mission.NextRun;
            IsActivated = mission.IsEnabled;
        }
        catch (Exception ex) { Log.Error($"无法初始化任务ViewModel{ex.Message}"); }

        Id = mission.Id;
    }

    protected override void OnActivated()
    {
        WeakReferenceMessenger.Default.Register<MissionMessage, string>(this, nameof(Mission));
        WeakReferenceMessenger.Default.Register<MissionProgressMessage, string>(this, nameof(Mission));
    }



    [RelayCommand]
    public void DoManualSetNextRunTime(bool set)
    {
        if (set && NextRunDate is not null && NextRunTime is not null &&  NextRunDate.Value.Date.Add(NextRunTime.Value.TimeOfDay) is DateTime t && t > DateTime.Now)
        {
            using var db = new MissionDatabase();
            var mission = db.GetCollection<Mission>().FindById(Id);
            mission.NextRun = t;
            db.GetCollection<Mission>().Upsert(mission);
        }
        else
        {
            using var db = new MissionDatabase();
            var mission = db.GetCollection<Mission>().FindById(Id);
            NextRunDate = mission?.NextRun;
            NextRunTime = mission?.NextRun;
        }

        ManualSetNextRun = false;
    }

    [RelayCommand]
    public void DeleteMission(AutomationViewModelBase mission)
    {
        WeakReferenceMessenger.Default.Send(new RemoveMissionMessage(mission));
    }


    partial void OnNextRunTimeChanged(DateTime? value)
    {

    }

    public void Receive(MissionMessage message)
    {
        if (Id != message.Id) return;

        IsWorking = message.IsWorking;

        if (message.LastRun is not null)
            LastRunTime = message.LastRun.Value;


        if (message.NextRun is not null)
        {
            NextRunDate = message.NextRun;
            NextRunTime = message.NextRun;
        }
    }

    public void Receive(MissionProgressMessage message)
    {
        if (message.Id == Id) ProgressValue = message.Progress;
    }
}


public partial class MissionViewModel<T> : AutomationViewModelBase where T : Mission
{
    protected T Mission { get; set; }


    public MissionViewModel(T mission) : base(mission)
    {
        Mission = mission;
    }


    [RelayCommand]
    public void RunOnce()
    {
        Task.Run(() => Mission.Work());
    }


    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (Mission is not null)
        {
            switch (e.PropertyName)
            {
                case nameof(IsActivated):
                    if (IsActivated != Mission.IsEnabled)
                    {
                        Mission.IsEnabled = IsActivated;
                        NextRunDate = Mission.NextRun;
                        NextRunTime = Mission.NextRun;
                        using var db = new MissionDatabase();
                        db.GetCollection<Mission>().Upsert(Mission);
                    }
                    break;

                default:
                    break;
            }


            try
            {
                if (e.PropertyName is not null && Mission.GetType().GetProperty(e.PropertyName) is PropertyInfo p)
                {
                    var v = p.GetValue(Mission);
                    var vm = GetType().GetProperty(e.PropertyName)!.GetValue(this);
                    if (v != vm)
                    {
                        p.SetValue(Mission, vm);
                        MissionSchedule.SaveChanges(Mission);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Save Mission {ex}");
            }

        
        }
    }
}