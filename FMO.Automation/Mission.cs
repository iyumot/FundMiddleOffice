using CommunityToolkit.Mvvm.Messaging;

namespace FMO.Schedule;

public class Mission
{
    public string Id => GetType().Name;

    public DateTime LastRun { get; set; }

    public DateTime NextRun { get; set; } = DateTime.MaxValue;

    bool _isEnabled;
    public bool IsEnabled { get => _isEnabled; set { _isEnabled = value; if (value) SetNextRun(); } }

    public bool IsWorking { get; private set; }





    public void OnTime(DateTime time)
    {
        if (!IsEnabled || IsWorking) return;

        if (time < NextRun) return;

        // 设为永不执行
        NextRun = DateTime.MaxValue;
        IsWorking = true;
        Task.Run(() => Work());
    }

    public bool Work()
    {
        IsWorking = true;
        WeakReferenceMessenger.Default.Send(new MissionMessage { Id = Id, IsWorking = true }, nameof(Mission));
        var r = false;

        try { r = WorkOverride(); LastRun = DateTime.Now; if (r) SetNextRun(); } catch { }

        using (var db = new AutoTaskDatabase())
            db.GetCollection<Mission>().Upsert(this);

        IsWorking = false;
        WeakReferenceMessenger.Default.Send(new MissionMessage { Id = Id, IsWorking = false }, nameof(Mission));
        return r;
    }


    protected virtual bool WorkOverride()
    {
        return true;
    }

    protected virtual void SetNextRun()
    {
        NextRun = DateTime.MaxValue;
    }

    public virtual void Init()
    {
        SetNextRun();
    }



}

