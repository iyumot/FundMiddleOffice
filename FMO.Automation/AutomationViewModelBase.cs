﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;

namespace FMO.Schedule;

public partial class AutomationViewModelBase : ObservableRecipient, IRecipient<MissionMessage> ,IRecipient<MissionProgressMessage>
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

    partial void OnIsActivatedChanged(bool value)
    {
        using var db = new MissionDatabase();
        var mission = db.GetCollection<Mission>().FindById(Id);
        if (mission is null) return;

        mission.IsEnabled = value;
        db.GetCollection<Mission>().Upsert(mission);
        NextRunTime = mission.NextRun;
    }


    [RelayCommand]
    public void DoManualSetNextRunTime(bool set)
    {
        if (set && NextRunDate is not null && NextRunTime is not null && NextRunTime.Value is DateTime t && t > DateTime.Now)
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
            NextRunTime = mission?.NextRun;
        }

        ManualSetNextRun = false;
    }

    public void Receive(MissionMessage message)
    {
        if (Id != message.Id) return;

        IsWorking = message.IsWorking;

        if (message.LastRun is not null)
            LastRunTime = message.LastRun.Value;


        if (message.NextRun is not null)
            NextRunTime = message.NextRun;
    }

    public void Receive(MissionProgressMessage message)
    {
        if (message.Id == Id) ProgressValue = message.Progress;
    }
}


public class MissionViewModel<T> : AutomationViewModelBase where T : Mission
{
    protected T Mission { get; set; }


    public MissionViewModel(T mission) : base(mission)
    {
        Mission = mission;
    }

}