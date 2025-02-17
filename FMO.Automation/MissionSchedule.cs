using static System.Net.Mime.MediaTypeNames;

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

    public static Mission[] Missions => missions.ToArray();


    public static void Init()
    {
        using var db = new MissionDatabase();
        var ms = db.GetCollection<Mission>().FindAll().ToArray();

        /// 如果没有，新建一个
        if (!ms.Any(x => x is FillFundDailyMission))
        {
            var fm = new FillFundDailyMission();
            db.GetCollection<Mission>().Insert(fm);
            fm.Init();
            missions.Add(fm);
        }

        foreach (var m in ms)
        {
            m.Init();
            missions.Add(m);
        }

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

    public static void SaveChanges(Mission m)
    {
        using var db = new MissionDatabase();
        db.GetCollection<Mission>().Upsert(m);
    }
}
