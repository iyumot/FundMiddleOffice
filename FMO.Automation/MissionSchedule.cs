namespace FMO.Schedule;

public static class MissionSchedule
{
    /// <summary>
    /// 默认每分钟一次
    /// </summary>
    private static System.Timers.Timer _taskTimer { get; } = new System.Timers.Timer(60000);

    /// <summary>
    /// 用于倒计时
    /// </summary>
    private static System.Timers.Timer _secondTimer { get; } = new System.Timers.Timer(1000);


    static HashSet<Mission> missions = new HashSet<Mission>();


    public static void Init()
    {
        using (var db = new AutoTaskDatabase())
            Register(db.GetCollection<Mission>().FindById(nameof(FillFundDailyMission)) ?? new());

        _taskTimer.Elapsed += _taskTimer_Elapsed;
        _taskTimer.Start();

        //延时执行一次
        Task.Run(async () => { await Task.Delay(8000); DoWork(DateTime.Now); });
    }

    private static void _taskTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        DoWork(e.SignalTime);
    }

    private static void DoWork(DateTime t)
    {
        foreach (var item in missions)
            item.OnTime(t);
    }


    public static void Register(Mission mission)
    {
        mission.Init();
        missions.Add(mission);
    }


}
