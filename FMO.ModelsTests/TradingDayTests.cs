namespace FMO.Models.Tests;



[TestClass()]
public class TradingDayTests
{
    [TestMethod()]
    public void GatherTest()
    {
        for (int i = 0; i < 100; i++)
        {

            var start = DateOnly.FromDayNumber(Random.Shared.Next(1000, 10000));
            var end = DateOnly.FromDayNumber(Random.Shared.Next(start.DayNumber, 10000));

            int old = TradingDay.CountBetween(start, end);

            int nd = Days.CountTradingDays(start, end);

            Assert.AreEqual(old, nd);
        }



    }



    [TestMethod]
    public void TestMethod1()
    {
        var path = @"G:\周报\tt.csv";


        List<DateOnly> dates = new List<DateOnly>(20000);
        List<DayFlag> flags = new List<DayFlag>(20000);

        using var sr = new StreamReader(path);
        sr.ReadLine();
        while (!sr.EndOfStream)
        {
            var str = sr.ReadLine().Split(',');
            if (str[1].Length < 4) continue;

            var date = DateOnly.Parse(str[1]);

            var flag = str[3] switch { "非交易日" => DayFlag.Holiday, _ => DayFlag.Trade };
            if (date.DayOfWeek switch { DayOfWeek.Sunday or DayOfWeek.Saturday => true, _ => false } && flags[^1] != DayFlag.Holiday)
                flag = DayFlag.Weekend;


            dates.Add(date);
            flags.Add(flag);
        }


        using (var fs = new StreamWriter("test.csv"))
        {
            for (int i = 0; i < dates.Count; i++)
            {
                if (dates[i].Year > 1991)
                    fs.WriteLine($"{dates[i]},{flags[i]}");
            }
            fs.Flush();
        }

    }

}