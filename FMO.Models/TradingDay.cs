using System.IO;
using System.Windows;

namespace FMO.Models;

public static class TradingDay
{

    public static List<DateOnly> Days { get; } = new();

    public static DateOnly Current
    {
        get
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            if (today > Days.Last()) return today;

            var idx = Days.BinarySearch(today);
            idx = idx < 0 ? ~idx - 1 : idx;
            var tradeday = Days[Math.Max(0, idx)];
            return tradeday;
        }
    }

    public static int CountBetween(DateOnly start, DateOnly end)
    {
        //if (start < end) return 0;

        int s = Days.BinarySearch(start);
        int e = Days.BinarySearch(end);

        s = s < 0 ? ~s : s;
        e = e < 0 ? ~e : e;

        return e - s + 1;
    }

    internal static DateOnly Next(DateTime date)
    {
        return Next(DateOnly.FromDateTime(date));
    }
    internal static DateOnly Next(DateOnly date)
    {
        var idx = Days.BinarySearch(date);
        idx = idx < 0 ? ~idx : idx + 1;
        return idx < Days.Count ? Days[idx] : Days.Last().AddDays(1);
    }

    internal static List<DateOnly> Gather(DateOnly start, DateOnly end)
    {
        int s = Days.BinarySearch(start);
        int e = Days.BinarySearch(end);

        s = s < 0 ? ~s : s;
        e = e < 0 ? ~e : e + 1;

        return Days[s..e].ToList();
    }

    static TradingDay()
    {
        try
        {
            using var sr = new StreamReader(@"config\tradedays.csv");
            string[] strings = sr.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Days.AddRange(strings.Select(x => DateOnly.ParseExact(x, "yyyy-MM-dd")));
        }
        catch
        {
            //var info = Application.GetResourceStream(new Uri("/res/tradedays.csv", UriKind.Relative));
            //using var sr = new StreamReader(info.Stream);
            //string[] strings = sr.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            //Days.AddRange(strings.Select(x => DateOnly.ParseExact(x, "yyyy-MM-dd")));
            //Days = Days.Distinct().ToList();
        }
    }
}
