using System.Diagnostics;
using FMO.Models;

namespace FMO;


public enum SequenceOrder { Ascend, Descend }

public class OpenRule
{
    static string[] array = ["一", "二", "三", "四", "五",];
    public FundOpenType Type { get; set; }

    /// <summary>
    /// 选择季
    /// Year 1-4
    /// 其它 忽略
    /// </summary>
    public int[]? Quarters { get; set; }

    /// <summary>
    /// 选择月
    /// Year 否则1-12
    /// QuarterFlag 则1-3
    /// Month Week 忽略
    /// </summary>
    public int[]? Months { get; set; }

    /// <summary>
    /// 选择周
    /// Year 否则1-54
    /// QuarterFlag 则1-14
    /// Month 1-5
    /// Week 忽略
    /// </summary>
    public int[]? Weeks { get; set; }

    public SequenceOrder WeekOrder { get; set; }

    /// <summary>
    /// 选择周
    /// Year 否则1-54
    /// QuarterFlag 则1-14
    /// Month 1-5
    /// Week 忽略
    /// </summary>
    public int[]? Dates { get; set; }

    /// <summary>
    /// 选择天
    /// Year 1-365
    /// QuarterFlag 1-92
    /// Month 1-31
    /// Week 7
    /// </summary>
    public SequenceOrder DayOrder { get; set; }

    public bool TradeOrNatural { get; set; }

    /// <summary>
    /// 是否顺延
    /// </summary>
    public bool Postpone‌ { get; set; }

    /// <summary>
    /// 顺延是否跨周
    /// </summary>
    public bool CrossWeek { get; set; }


    private string WeekStr()
    {
        var days = Dates?.Where(x => x < 5);
        if (days is null || !days.Any()) return "";

        if (DayOrder == SequenceOrder.Ascend)
        {
            if (TradeOrNatural)
                return $"第{string.Join('、', days!.Select(x => x))}个交易日开放";
            else
                return $"周{string.Join('、', days!.Select(x => array[x - 1]))}开放";
        }
        else
        {
            if (TradeOrNatural)
                return $"倒数第{string.Join('、', days!.Select(x => x))}个交易日开放";
            else
                return $"倒数第{string.Join('、', days!.Select(x => x))}个自然日开放";
        }
    }


    private string MonthStr()
    {
        if (Dates is null || Dates.Length == 0) return "";

        if (Weeks?.Length > 0)
            return $"{(WeekOrder == SequenceOrder.Ascend ? "" : "倒数")}第{string.Join('、', Weeks.Select(x => x))}个的{WeekStr()}";
        else
            return $"{(DayOrder == SequenceOrder.Ascend ? "" : "倒数")}第{string.Join('、', Dates.Select(x => x))}个{(TradeOrNatural ? "交易" : "自然")}日开放";
    }

    private string QuarterStr()
    {
        if (Dates is null || Dates.Length == 0) return "";

        if (Months?.Length > 0)
            return $"第{string.Join('、', Months.Select(x => x))}月的{MonthStr()}";
        else if (Weeks?.Length > 0)
            return $"{(WeekOrder == SequenceOrder.Ascend ? "" : "倒数")}第{string.Join('、', Weeks.Select(x => x))}个的{WeekStr()}";
        else
            return $"{(DayOrder == SequenceOrder.Ascend ? "" : "倒数")}第{string.Join('、', Dates.Select(x => x))}个{(TradeOrNatural ? "交易" : "自然")}日开放";
    }

    public override string ToString()
    {
        switch (Type)
        {
            case FundOpenType.Closed:
                return "不开放";
            case FundOpenType.Yearly:
                if (Dates is null || Dates.Length == 0) return "无效的设置";
                if (Quarters?.Length > 0)
                    return $"每年第{string.Join('、', Quarters.Select(x => x))}季度的{QuarterStr()}";
                else if (Months?.Length > 0)
                    return $"每年第{string.Join('、', Months.Select(x => x))}月的{MonthStr()}";
                else if (Weeks?.Length > 0)
                    return $"每年{(WeekOrder == SequenceOrder.Ascend ? "" : "倒数")}第{string.Join('、', Weeks.Select(x => x + 1))}周的{WeekStr()}";
                else
                    return $"每年{(DayOrder == SequenceOrder.Ascend ? "" : "倒数")}第{string.Join('、', Dates.Select(x => x + 1))}个{(TradeOrNatural ? "交易" : "自然")}日开放";
            case FundOpenType.Quarterly:
                if (Dates is null || Dates.Length == 0) return "无效的设置";
                return "每季" + QuarterStr();
            case FundOpenType.Monthly:
                if (Dates is null || Dates.Length == 0) return "无效的设置";
                return "每月" + MonthStr();
            case FundOpenType.Weekly:
                if (Dates is null || Dates.Length == 0) return "无效的设置";

                return "每周" + WeekStr();
            case FundOpenType.Daily:
                return "每日开放";
            default:
                return "-";
        }
    }



    private void FilterQuarter(DateEx[] dates)
    {

    }


    public void Filter2(DateMeta[] dates)
    {
        var ex = dates.Select(x => new DateEx { Date = x.Date, Flag = x.Flag, WeekOfMonth = 1, WeekOfYear = 1 }).ToArray();

        // 计算周序号
        for (int i = 1; i < ex.Length; i++)
        {
            var last = WeekOrder == SequenceOrder.Ascend ? ex[i - 1] : ex[^i];
            var cur = WeekOrder == SequenceOrder.Ascend ? ex[i] : ex[^(i + 1)];
            if (WeekOrder == SequenceOrder.Ascend ? cur.Date.DayOfWeek == DayOfWeek.Monday : cur.Date.DayOfWeek == DayOfWeek.Sunday)
                cur.WeekOfYear = last.WeekOfYear + 1;
            else cur.WeekOfYear = last.WeekOfYear;
        }

        // 计算月内周序号
        var p = ex.AsEnumerable();
        foreach (var m in (WeekOrder == SequenceOrder.Ascend ? p : p.Reverse()).GroupBy(x => x.Date.Month))
        {
            foreach (var w in m.GroupBy(x => x.WeekOfYear).Index())
            {
                foreach (var n in w.Item)
                {
                    n.WeekOfMonth = w.Index + 1;
                }
            }
        }

        // 计算季内周序号 
        foreach (var m in (WeekOrder == SequenceOrder.Ascend ? p : p.Reverse()).GroupBy(x => QuarterOfDay(x.Date)))
        {
            foreach (var w in m.GroupBy(x => x.WeekOfYear).Index())
            {
                foreach (var n in w.Item)
                {
                    n.WeekOfQuarter = w.Index + 1;
                }
            }
        }

        switch (Type)
        {
            case FundOpenType.Closed:
                break;
            case FundOpenType.Yearly:
                if (Quarters?.Length > 0)
                    FilterQuarter(ex);



                break;
            case FundOpenType.Quarterly:
                break;
            case FundOpenType.Monthly:
                break;
            case FundOpenType.Weekly:
                break;
            case FundOpenType.Daily:
                break;
            default:
                break;
        }



        var ok = ex.Where(x => x.Pair == Pair.Ok).ToList();

        foreach (var x in ok)
            Debug.WriteLine($"{x.Date:MM-dd}     {x.WeekOfYear,2} | {x.Date.DayOfWeek}");
        //FilterByWeek(dates.Select(x => new DateEx { Date = x, Select = !x.Flag.HasFlag(DayFlag.Weekend) }).ToArray());
    }



    /// <summary>
    /// 获取对应序号的date
    /// </summary>
    /// <param name="dates"></param>
    /// <param name="ids">id+1</param>
    /// <returns></returns>
    private IEnumerable<DateMeta> GetTradeDates(IEnumerable<DateMeta> dates, int[] ids, bool reverse = false)
    {
        var obj = dates.Where(x => x.Flag.HasFlag(DayFlag.Trade));
        if (reverse) obj = obj.Reverse();
        foreach (var d in obj.Index())
        {
            if (ids.Contains(d.Index + 1))
                yield return d.Item;
        }
    }




    public void Filter(DateMeta[] dates)
    {
        var ex = dates.Select(x => new DateEx { Date = x.Date, Flag = x.Flag, WeekOfMonth = 1, WeekOfYear = 1 }).ToArray();

        // 计算周序号
        for (int i = 1; i < ex.Length; i++)
        {
            var last = WeekOrder == SequenceOrder.Ascend ? ex[i - 1] : ex[^i];
            var cur = WeekOrder == SequenceOrder.Ascend ? ex[i] : ex[^(i + 1)];
            if (WeekOrder == SequenceOrder.Ascend ? cur.Date.DayOfWeek == DayOfWeek.Monday : cur.Date.DayOfWeek == DayOfWeek.Sunday)
                cur.WeekOfYear = last.WeekOfYear + 1;
            else cur.WeekOfYear = last.WeekOfYear;
        }

        // 计算月内周序号
        var p = ex.AsEnumerable();
        foreach (var m in (WeekOrder == SequenceOrder.Ascend ? p : p.Reverse()).GroupBy(x => x.Date.Month))
        {
            foreach (var w in m.GroupBy(x => x.WeekOfYear).Index())
            {
                foreach (var n in w.Item)
                {
                    n.WeekOfMonth = w.Index + 1;
                }
            }
        }

        // 计算季内周序号 
        foreach (var m in (WeekOrder == SequenceOrder.Ascend ? p : p.Reverse()).GroupBy(x => QuarterOfDay(x.Date)))
        {
            foreach (var w in m.GroupBy(x => x.WeekOfYear).Index())
            {
                foreach (var n in w.Item)
                {
                    n.WeekOfQuarter = w.Index + 1;
                }
            }
        }

        switch (Type)
        {
            case FundOpenType.Closed:
                break;
            case FundOpenType.Yearly:
                if (Quarters?.Length > 0) // 选了季度
                {
                    foreach (var item in ex)
                    {
                        if (Array.BinarySearch(Quarters, QuarterOfDay(item.Date) + 1) >= 0) // 符合要求
                            item.PairTo(Pair.Quarter);
                    }
                }
                else
                    PairTo(ex, Pair.Quarter); //所有季度

                if (Quarters?.Length > 0)
                    PairToMonth(ex, FundOpenType.Quarterly);
                else
                    PairToMonth(ex, FundOpenType.Yearly);

                if (Months?.Length > 0)
                    PairToWeek(ex, FundOpenType.Monthly);
                else if (Quarters?.Length > 0)
                    PairToWeek(ex, FundOpenType.Quarterly);
                else
                    PairToWeek(ex, FundOpenType.Yearly);

                if (Weeks?.Length > 0)
                    PairToDay(ex, FundOpenType.Weekly);
                else if (Months?.Length > 0)
                    PairToDay(ex, FundOpenType.Monthly);
                else if (Quarters?.Length > 0)
                    PairToDay(ex, FundOpenType.Quarterly);
                else
                    PairToDay(ex, FundOpenType.Yearly);

                break;
            case FundOpenType.Quarterly:
                PairTo(ex, Pair.Quarter);
                PairToMonth(ex, FundOpenType.Quarterly);

                if (Months?.Length > 0)
                    PairToWeek(ex, FundOpenType.Monthly);
                else
                    PairToWeek(ex, FundOpenType.Quarterly);

                if (Weeks?.Length > 0)
                    PairToDay(ex, FundOpenType.Weekly);
                else if (Months?.Length > 0)
                    PairToDay(ex, FundOpenType.Monthly);
                else
                    PairToDay(ex, FundOpenType.Quarterly);
                break;
            case FundOpenType.Monthly:
                PairTo(ex, Pair.Quarter);
                PairTo(ex, Pair.Month);
                PairToWeek(ex, FundOpenType.Monthly);

                if (Weeks?.Length > 0)
                    PairToDay(ex, FundOpenType.Weekly);
                else
                    PairToDay(ex, FundOpenType.Monthly);
                break;
            case FundOpenType.Weekly:
                PairTo(ex, Pair.Quarter);
                PairTo(ex, Pair.Month);
                PairTo(ex, Pair.Week);
                PairToDay(ex, FundOpenType.Weekly);
                break;
            case FundOpenType.Daily:
                PairTo(ex, Pair.Quarter);
                PairTo(ex, Pair.Month);
                PairTo(ex, Pair.Week);
                PairTo(ex, Pair.Day);
                break;
            default:
                break;
        }



        var ok = ex.Where(x => x.Pair == Pair.Ok).ToList();

        foreach (var x in ok)
            Debug.WriteLine($"{x.Date:MM-dd}     {x.WeekOfYear,2} | {x.Date.DayOfWeek}");
        //FilterByWeek(dates.Select(x => new DateEx { Date = x, Select = !x.Flag.HasFlag(DayFlag.Weekend) }).ToArray());
    }

    private void PairTo(DateEx[] ex, Pair flag)
    {
        foreach (var item in ex)
            item.PairTo(flag);
    }

    private void PairToWeek(DateEx[] ex, Pair flag, FundOpenType type)
    {
        foreach (var item in ex)
        {
            if (Array.BinarySearch(Weeks!, type switch { FundOpenType.Monthly => item.WeekOfMonth, FundOpenType.Quarterly => item.WeekOfQuarter, _ => item.WeekOfYear }) >= 0) // 符合要求
                item.PairTo(Pair.Week);
        }
    }
    private void PairToMonth(DateEx[] ex, FundOpenType up)
    {
        if (Months?.Length > 0) // 选了周
        {
            switch (up)
            {
                case FundOpenType.Yearly:
                    foreach (var v in (DayOrder == SequenceOrder.Ascend ? ex.AsEnumerable() : ex.AsEnumerable().Reverse()))
                        v.Calc = v.Date.Month;
                    break;
                case FundOpenType.Quarterly:
                    foreach (var q in (DayOrder == SequenceOrder.Ascend ? ex.AsEnumerable() : ex.AsEnumerable().Reverse()).GroupBy(x => QuarterOfDay(x.Date)))
                        foreach (var w in q.GroupBy(x => x.Date.Month).Index())
                            foreach (var v in w.Item)
                                v.Calc = w.Index + 1;
                    break;
                case FundOpenType.Closed:
                    return;
                default:
                    throw new NotImplementedException();
                    break;
            }

            foreach (var v in ex)
            {
                if (Array.BinarySearch(Months!, v.Calc) >= 0) // 符合要求
                    v.PairTo(Pair.Month);
            }
        }
        else PairTo(ex, Pair.Month);
    }

    private void PairToWeek(DateEx[] ex, FundOpenType up)
    {
        if (Weeks?.Length > 0) // 选了周
        {
            switch (up)
            {
                case FundOpenType.Yearly:
                    foreach (var v in (DayOrder == SequenceOrder.Ascend ? ex.AsEnumerable() : ex.AsEnumerable().Reverse()))
                        v.Calc = v.WeekOfYear;
                    break;
                case FundOpenType.Quarterly:
                    foreach (var q in (DayOrder == SequenceOrder.Ascend ? ex.AsEnumerable() : ex.AsEnumerable().Reverse()).GroupBy(x => QuarterOfDay(x.Date)))
                        foreach (var w in q.GroupBy(x => x.WeekOfYear).Index())
                            foreach (var v in w.Item)
                                v.Calc = w.Index + 1;
                    break;
                case FundOpenType.Monthly:
                    foreach (var q in (DayOrder == SequenceOrder.Ascend ? ex.AsEnumerable() : ex.AsEnumerable().Reverse()).GroupBy(x => x.Date.Month))
                        foreach (var w in q.GroupBy(x => x.WeekOfYear).Index())
                            foreach (var v in w.Item)
                                v.Calc = w.Index + 1;
                    break;
                case FundOpenType.Closed:
                    return;
                default:
                    throw new NotImplementedException();
                    break;
            }

            foreach (var v in ex)
            {
                if (Array.BinarySearch(Weeks!, v.Calc) >= 0) // 符合要求
                    v.PairTo(Pair.Week);
            }
        }
        else PairTo(ex, Pair.Week);
    }

    private void PairToDay(DateEx[] ex, FundOpenType up)
    {
        if (Dates?.Length > 0) // 选了日期
        {
            switch (up)
            {
                case FundOpenType.Yearly:
                    foreach (var v in (DayOrder == SequenceOrder.Ascend ? ex.AsEnumerable() : ex.AsEnumerable().Reverse()).Index())
                        v.Item.Calc = v.Index + 1;
                    break;
                case FundOpenType.Quarterly:
                    foreach (var q in (DayOrder == SequenceOrder.Ascend ? ex.AsEnumerable() : ex.AsEnumerable().Reverse()).GroupBy(x => QuarterOfDay(x.Date)))
                        foreach (var v in q.Index())
                            v.Item.Calc = v.Index + 1;
                    break;
                case FundOpenType.Monthly:
                    foreach (var q in (DayOrder == SequenceOrder.Ascend ? ex.AsEnumerable() : ex.AsEnumerable().Reverse()).GroupBy(x => x.Date.Month))
                        foreach (var v in q.Index())
                            v.Item.Calc = v.Index + 1;
                    break;
                case FundOpenType.Weekly:
                    foreach (var q in (DayOrder == SequenceOrder.Ascend ? ex.AsEnumerable() : ex.AsEnumerable().Reverse()).GroupBy(x => x.WeekOfYear))
                        foreach (var v in q.Index())
                            v.Item.Calc = v.Index + 1;
                    break;
                case FundOpenType.Daily:
                    PairTo(ex, Pair.Day);
                    return;
                case FundOpenType.Closed:
                    return;
                default:
                    throw new NotImplementedException();
                    break;
            }

            foreach (var v in ex)
            {
                if (Array.BinarySearch(Dates!, v.Calc) >= 0) // 符合要求
                    v.PairTo(Pair.Day);
            }
        }
    }

    //public DateOnly[] FilterDays(int year)
    //{
    //    IEnumerable<DateMeta> dates = Days.DayInfosByYear(year);

    //    switch (Type)
    //    {
    //        case FundOpenType.Closed:
    //            return Array.Empty<DateOnly>();
    //        case FundOpenType.Yearly:
    //            if (Quarters?.Length > 0)
    //                dates = dates.Where(x => Quarters.Contains(QuarterOfDay(x)));
    //            if (Months?.Length > 0)
    //                dates = dates.Where(x => Months.Contains(x.Month));
    //            else if (Weeks?.Length > 0)
    //                return $"每年{(WeekOrder == SequenceOrder.Ascend ? "" : "倒数")}第{string.Join('、', Weeks.Select(x => x + 1))}周的{WeekStr()}";
    //            else
    //                break;
    //        case FundOpenType.Quarterly:
    //            break;
    //        case FundOpenType.Monthly:
    //            break;
    //        case FundOpenType.Weekly:
    //            break;
    //        case FundOpenType.Daily:
    //            return dates.Where(x=>x.Flag.HasFlag(DayFlag.Trade)).Select(x=>x.Date).ToArray();
    //        default:
    //            break;
    //    }




    //}


    private static int QuarterOfDay(DateOnly d) => (d.Month - 1) / 3;


    public class DateEx : OpenDateSheet
    {
        public Pair Pair { get; set; }

        public int WeekOfYear { get; set; }

        public int WeekOfMonth { get; set; }

        public int WeekOfQuarter { get; set; }

        public int Calc { get; set; }

        public void PairTo(Pair pair) => Pair |= pair;
    }

    [Flags]
    public enum Pair
    {
        Quarter = 1,

        Month = 2,

        Week = 4,

        Day = 8,

        Ok = Quarter | Month | Week | Day,
    }

}

public enum OpenType
{
    None,

    /// <summary>
    /// 固定
    /// </summary>
    Fixed,

    /// <summary>
    /// 临时
    /// </summary>
    Temperaty,

    /// <summary>
    /// 顺延的
    /// </summary>
    Postpone
}

public class OpenDateSheet
{
    public required DateOnly Date { get; init; }

    public DayFlag Flag { get; set; }

    public OpenType Type { get; set; }

}