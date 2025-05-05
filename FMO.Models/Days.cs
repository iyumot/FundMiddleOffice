using System.Reflection;

namespace FMO.Models;



public enum DayFlag
{
    None,

    Weekend = 0x1,

    Holiday = 0x2,

    Trade = 0x4
}

public record DayInfo(DateOnly Date, DayFlag Flag);


public static class Days
{
    private static List<DateOnly> Dates = new();
    private static List<DayInfo> Data = new();


    /// <summary>
    /// 获取指定年份的所有日期
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>
    public static DateOnly[] DaysByYear(int year)
    {
        int s = Dates.BinarySearch(new DateOnly(year, 1, 1));
        int e = Dates.BinarySearch(new DateOnly(year, 12, 31));

        s = s < 0 ? ~s : s;
        e = e < 0 ? ~e : e + 1;

        return Dates[s..e].ToArray();
    }

    public static DayInfo[] DayInfosByYear(int year)
    {
        int s = Dates.BinarySearch(new DateOnly(year, 1, 1));
        int e = Dates.BinarySearch(new DateOnly(year, 12, 31));

        s = s < 0 ? ~s : s;
        e = e < 0 ? ~e : e + 1;

        return Data[s..e].ToArray();
    }


    /// <summary>
    /// 获取指定年份的所有交易日
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>
    public static DateOnly[] TradeDaysByYear(int year)
    {
        int s = Dates.BinarySearch(new DateOnly(year, 1, 1));
        int e = Dates.BinarySearch(new DateOnly(year, 12, 31));

        s = s < 0 ? ~s : s;
        e = e < 0 ? ~e : e + 1;

        List<DateOnly> list = new(366);
        for (int j = s; j < e; j++)
        {
            if (Data[j].Flag.HasFlag(DayFlag.Trade))
                list.Add(Dates[j]);
        }
        return list.ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static DateOnly NextTradingDay(DateOnly date)
    {
        int s = Dates.BinarySearch(date);
        s = s < 0 ? ~s : s;

        for (int i = s + 1; i < Dates.Count; i++)
        {
            if (Data[i].Flag.HasFlag(DayFlag.Trade))
                return Dates[i];
        }
        return default;
    }

    public static DateOnly NextTradingDay(DateTime date) => NextTradingDay(DateOnly.FromDateTime(date));

    public static int CountTradingDays(DateOnly start, DateOnly end)
    {
        if (start > end) return 0;

        int s = Dates.BinarySearch(start);
        int e = Dates.BinarySearch(end);

        s = s < 0 ? ~s : s;
        e = e < 0 ? ~e : e;

        return Data[s..e].Count(x => x.Flag.HasFlag(DayFlag.Trade));
    }

    static Days() => Init();

    private static void Init()
    {
        try
        {
            var fi = new FileInfo(@"config\day.csv");
            if (fi.Exists)
            {
                using var sr = new StreamReader(@"config\day.csv");
                string[] strings = sr.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var d = strings.Select(x => x.Split(','));
                Dates = d.Select(x => x[0]).Select(x => DateOnly.Parse(x)).ToList();
                Data = d.Select(x => new DayInfo(DateOnly.Parse(x[0]), (DayFlag)Enum.Parse(typeof(DayFlag), x[1]))).ToList();
            }
            else InitFromAssembly();
        }
        catch
        {
            InitFromAssembly();
        }
    }

    private static void InitFromAssembly()
    {
        using var ri = Assembly.GetExecutingAssembly().GetManifestResourceStream("FMO.Models.day.csv");
        using var sr = new StreamReader(ri);
        string[] strings = sr.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var d = strings.Select(x => x.Split(','));
        Dates = d.Select(x => x[0]).Select(x => DateOnly.Parse(x)).ToList();
        Data = d.Select(x => new DayInfo(DateOnly.Parse(x[0]), (DayFlag)Enum.Parse(typeof(DayFlag), x[1]))).ToList();
    }
}
