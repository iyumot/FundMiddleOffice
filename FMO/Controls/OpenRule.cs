using FMO.Models;
using FMO.Shared;
using System.Diagnostics;

namespace FMO;


public enum SequenceOrder { Ascend, Descend }

public class OpenRule
{
    static string[] weekhead = ["一", "二", "三", "四", "五",];
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
                return $"周{string.Join('、', days!.Select(x => weekhead[x - 1]))}开放";
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
            return $"{(DayOrder == SequenceOrder.Ascend ? "" : "倒数")}{(TradeOrNatural ? "第" : "")}{string.Join('、', Dates.Select(x => x))}{(TradeOrNatural ? "个交易" : "")}日开放";
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
                    return $"每年第{string.Join('、', Quarters.Select(x => x))}季度的{QuarterStr()}" + (Postpone ? "，非交易日顺延" : "");
                else if (Months?.Length > 0)
                    return $"每年第{string.Join('、', Months.Select(x => x))}月的{MonthStr()}" + (Postpone ? "，非交易日顺延" : "");
                else if (Weeks?.Length > 0)
                    return $"每年{(WeekOrder == SequenceOrder.Ascend ? "" : "倒数")}第{string.Join('、', Weeks.Select(x => x + 1))}周的{WeekStr()}" + (Postpone ? "，非交易日顺延" : "");
                else
                    return $"每年{(DayOrder == SequenceOrder.Ascend ? "" : "倒数")}第{string.Join('、', Dates.Select(x => x + 1))}个{(TradeOrNatural ? "交易" : "自然")}日开放" + (Postpone ? "，非交易日顺延" : "");
            case FundOpenType.Quarterly:
                if (Dates is null || Dates.Length == 0) return "无效的设置";
                return "每季" + QuarterStr() + (Postpone ? "，非交易日顺延" : "");
            case FundOpenType.Monthly:
                if (Dates is null || Dates.Length == 0) return "无效的设置";
                return "每月" + MonthStr() + (Postpone ? "，非交易日顺延" : "");
            case FundOpenType.Weekly:
                if (Dates is null || Dates.Length == 0) return "无效的设置";

                return "每周" + WeekStr() + (Postpone ? "，非交易日顺延" : "");
            case FundOpenType.Daily:
                return "每日开放";
            default:
                return "-";
        }
    }



    public OpenDateSheet[] Apply(int year)
    {
        if (Type == FundOpenType.Closed)
            return Days.DayInfosByYear(year).Select(x => new DateEx { Date = x.Date, Flag = x.Flag, WeekOfMonth = 1, WeekOfYear = 1, Type = OpenType.None }).ToArray();

        if (Type == FundOpenType.Daily)
            return Days.DayInfosByYear(year).Select(x => new DateEx { Date = x.Date, Flag = x.Flag, WeekOfMonth = 1, WeekOfYear = 1, Type = x.Flag.HasFlag(DayFlag.Trade) ? OpenType.Fixed : OpenType.None }).ToArray();

        var result = Days.DayInfosByYear(year).Select(x => new DateEx { Date = x.Date, Flag = x.Flag, WeekOfMonth = 1, WeekOfYear = 1, Type = OpenType.None }).ToList();

        // 计算周序号
        for (int i = 1; i < result.Count; i++)
        {
            var last = result[i - 1];
            var cur = result[i];
            if (cur.Date.DayOfWeek == DayOfWeek.Monday)
                cur.WeekOfYear = last.WeekOfYear + 1;
            else cur.WeekOfYear = last.WeekOfYear;
        }

        ////////////////////////////////////////////////////////
        // 季度
        if (Type == FundOpenType.Yearly && Quarters?.Length > 0)
        {
            // 排除不符合的季度
            // 计算
            var f = result.Where(x => Array.BinarySearch(Quarters, (x.Date.Month - 1) / 3 + 1) < 0);

            foreach (var x in f)
                x.IsExclude = true;
        }

        // 月
        if (Type switch { FundOpenType.Yearly or FundOpenType.Quarterly => true, _ => false })
        {
            if (Months?.Length > 0)
            {
                // 计算 
                var sq = Type == FundOpenType.Quarterly || Quarters?.Length > 0;
                var f = result.Where(x => Array.BinarySearch(Months, sq ? (x.Date.Month - 1) % 3 + 1 : x.Date.Month) < 0);

                foreach (var x in f)
                    x.IsExclude = true;
            }
        }
        ////////////////////////////////////////////////////////
        ///


        // 周
        if (Type switch { FundOpenType.Yearly or FundOpenType.Quarterly or FundOpenType.Monthly => true, _ => false })
        {
            var sm = Type == FundOpenType.Monthly || Months?.Length > 0;
            var sq = Type == FundOpenType.Quarterly || Quarters?.Length > 0;
            var sw = Weeks?.Length > 0;

            if (sm) // 按月
            {
                // 交易日
                if (TradeOrNatural)
                {
                    if (sw && Dates?.Length > 0) //n 周的第n个交易日 此项手动不可用
                    {
                        IEnumerable<DateEx> orderd = DayOrder == SequenceOrder.Ascend ? result : result.AsEnumerable().Reverse();
                        var sel = orderd.Where(x => !x.IsExclude).GroupBy(x => x.Date.Month);
                        foreach (var m in sel)
                        {
                            foreach (var w in m.GroupBy(x => x.WeekOfYear).Index().Select(x => (Index: x.Index + 1, x.Item)))
                            {
                                bool pairw = Array.BinarySearch(Weeks!, w.Index) >= 0;
                                if (!pairw) continue;

                                foreach (var s in w.Item.Index())
                                {
                                    if (Array.BinarySearch(Dates, s.Index + 1) >= 0)
                                        s.Item.Type = OpenType.Fixed;
                                }
                            }
                        }

                        // 非交易日及顺延
                        var chk = result.Where(x => !x.Flag.HasFlag(DayFlag.Trade) && x.Type == OpenType.Fixed).ToArray();
                        foreach (var item in chk)
                        {
                            item.Type = OpenType.None;

                            //if (Postpone) //顺延
                            {
                                int idx = result.IndexOf(item);
                                for (int i = idx + 1; i < result.Count; i++)
                                {
                                    if (result[i].Type != OpenType.Fixed && !result[i].IsExclude && result[i].Flag.HasFlag(DayFlag.Trade))
                                    {
                                        result[i].Type = OpenType.Fixed;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (Dates?.Length > 0) //月的第n个交易日
                    {
                        IEnumerable<DateEx> orderd = DayOrder == SequenceOrder.Ascend ? result : result.AsEnumerable().Reverse();
                        var sel = orderd.Where(x => !x.IsExclude && x.Flag.HasFlag(DayFlag.Trade)).GroupBy(x => x.Date.Month);

                        foreach (var m in sel)
                        {
                            foreach (var s in m.Index())
                            {
                                if (Array.BinarySearch(Dates, s.Index + 1) >= 0)
                                    s.Item.Type = OpenType.Fixed;
                            }
                        }
                    }
                }
                else// 自然日
                {
                    if (sw && Dates?.Length > 0) //n个周N 
                    {
                        IEnumerable<DateEx> orderd = DayOrder == SequenceOrder.Ascend ? result : result.AsEnumerable().Reverse();
                        var sel = orderd.Where(x => !x.IsExclude).GroupBy(x => (int)x.Date.DayOfWeek);// && Array.BinarySearch(Dates, (int)x.Date.DayOfWeek) >= 0).GroupBy(x => x.Date.Month);
                        foreach (var dw in sel)
                        {
                            if (Array.BinarySearch(Dates, dw.Key) < 0) // 星期几不符合
                                continue;

                            foreach (var m in dw.GroupBy(x => x.Date.Month)) // 月
                            {
                                foreach (var w in m.Index()) //第N周N
                                {
                                    bool pairw = Array.BinarySearch(Weeks!, w.Index + 1) >= 0;
                                    if (!pairw) continue;

                                    w.Item.Type = OpenType.Fixed;
                                }
                            }
                        }


                    }
                    else if (Dates?.Length > 0) //月的第n个交易日
                    {
                        IEnumerable<DateEx> orderd = DayOrder == SequenceOrder.Ascend ? result : result.AsEnumerable().Reverse();
                        var sel = orderd.Where(x => !x.IsExclude).GroupBy(x => x.Date.Month);

                        foreach (var m in sel)
                        {
                            foreach (var s in m.Index())
                            {
                                if (Array.BinarySearch(Dates, s.Index + 1) >= 0)
                                    s.Item.Type = OpenType.Fixed;
                            }
                        }
                    }

                    // 非交易日及顺延
                    var chk = result.Where(x => !x.Flag.HasFlag(DayFlag.Trade) && x.Type == OpenType.Fixed).ToArray();
                    foreach (var item in chk)
                    {
                        item.Type = OpenType.None;

                        if (Postpone) //顺延
                        {
                            int idx = result.IndexOf(item);
                            for (int i = idx + 1; i < result.Count; i++)
                            {
                                if (result[i].Type != OpenType.Fixed && !result[i].IsExclude && result[i].Flag.HasFlag(DayFlag.Trade))
                                {
                                    result[i].Type = OpenType.Fixed;
                                    break;
                                }
                            }
                        }
                    }
                }

            }
            else if (sq) //季
            {
                // 交易日
                if (TradeOrNatural)
                {
                    if (sw && Dates?.Length > 0) //n 周的第n个交易日 此项手动不可用
                    {
                        IEnumerable<DateEx> orderd = DayOrder == SequenceOrder.Ascend ? result : result.AsEnumerable().Reverse();
                        var sel = orderd.Where(x => !x.IsExclude).GroupBy(x => (x.Date.Month - 1) / 3 + 1);
                        foreach (var m in sel)
                        {
                            foreach (var w in m.GroupBy(x => x.WeekOfYear).Index().Select(x => (Index: x.Index + 1, x.Item)))
                            {
                                bool pairw = Array.BinarySearch(Weeks!, w.Index) >= 0;
                                if (!pairw) continue;

                                foreach (var s in w.Item.Index())
                                {
                                    if (Array.BinarySearch(Dates, s.Index + 1) >= 0)
                                        s.Item.Type = OpenType.Fixed;
                                }
                            }
                        }

                        // 非交易日及顺延
                        var chk = result.Where(x => !x.Flag.HasFlag(DayFlag.Trade) && x.Type == OpenType.Fixed).ToArray();
                        foreach (var item in chk)
                        {
                            item.Type = OpenType.None;

                            //if (Postpone) //顺延
                            {
                                int idx = result.IndexOf(item);
                                for (int i = idx + 1; i < result.Count; i++)
                                {
                                    if (result[i].Type != OpenType.Fixed && !result[i].IsExclude && result[i].Flag.HasFlag(DayFlag.Trade))
                                    {
                                        result[i].Type = OpenType.Fixed;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (Dates?.Length > 0) //月的第n个交易日
                    {
                        IEnumerable<DateEx> orderd = DayOrder == SequenceOrder.Ascend ? result : result.AsEnumerable().Reverse();
                        var sel = orderd.Where(x => !x.IsExclude && x.Flag.HasFlag(DayFlag.Trade)).GroupBy(x => (x.Date.Month - 1) / 3 + 1);

                        foreach (var m in sel)
                        {
                            foreach (var s in m.Index())
                            {
                                if (Array.BinarySearch(Dates, s.Index + 1) >= 0)
                                    s.Item.Type = OpenType.Fixed;
                            }
                        }
                    }
                }
                else// 自然日
                {
                    if (sw && Dates?.Length > 0) //n个周N 
                    {
                        IEnumerable<DateEx> orderd = DayOrder == SequenceOrder.Ascend ? result : result.AsEnumerable().Reverse();
                        var sel = orderd.Where(x => !x.IsExclude).GroupBy(x => (int)x.Date.DayOfWeek);// && Array.BinarySearch(Dates, (int)x.Date.DayOfWeek) >= 0).GroupBy(x => x.Date.Month);
                        foreach (var dw in sel)
                        {
                            if (Array.BinarySearch(Dates, dw.Key) < 0) // 星期几不符合
                                continue;

                            foreach (var m in dw.GroupBy(x => (x.Date.Month - 1) / 3 + 1)) // 月
                            {
                                foreach (var w in m.Index()) //第N周N
                                {
                                    bool pairw = Array.BinarySearch(Weeks!, w.Index + 1) >= 0;
                                    if (!pairw) continue;

                                    w.Item.Type = OpenType.Fixed;
                                }
                            }
                        }


                    }
                    else if (Dates?.Length > 0) //月的第n个交易日
                    {
                        IEnumerable<DateEx> orderd = DayOrder == SequenceOrder.Ascend ? result : result.AsEnumerable().Reverse();
                        var sel = orderd.Where(x => !x.IsExclude).GroupBy(x => (x.Date.Month - 1) / 3 + 1);

                        foreach (var m in sel)
                        {
                            foreach (var s in m.Index())
                            {
                                if (Array.BinarySearch(Dates, s.Index + 1) >= 0)
                                    s.Item.Type = OpenType.Fixed;
                            }
                        }
                    }

                    // 非交易日及顺延
                    var chk = result.Where(x => !x.Flag.HasFlag(DayFlag.Trade) && x.Type == OpenType.Fixed).ToArray();
                    foreach (var item in chk)
                    {
                        item.Type = OpenType.None;

                        if (Postpone) //顺延
                        {
                            int idx = result.IndexOf(item);
                            for (int i = idx + 1; i < result.Count; i++)
                            {
                                if (result[i].Type != OpenType.Fixed && !result[i].IsExclude && result[i].Flag.HasFlag(DayFlag.Trade))
                                {
                                    result[i].Type = OpenType.Fixed;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else //年
            {
                // 交易日
                if (TradeOrNatural)
                {
                    if (sw && Dates?.Length > 0) //n 周的第n个交易日 此项手动不可用
                    {
                        IEnumerable<DateEx> orderd = DayOrder == SequenceOrder.Ascend ? result : result.AsEnumerable().Reverse();
                        var sel = orderd.Where(x => !x.IsExclude).GroupBy(x => x.WeekOfYear);
                        foreach (var w in sel.Index())
                        {
                            bool pairw = Array.BinarySearch(Weeks!, w.Index) >= 0;
                            if (!pairw) continue;

                            foreach (var s in w.Item.Index())
                            {
                                if (Array.BinarySearch(Dates, s.Index + 1) >= 0)
                                    s.Item.Type = OpenType.Fixed;
                            }

                        }

                        // 非交易日及顺延
                        var chk = result.Where(x => !x.Flag.HasFlag(DayFlag.Trade) && x.Type == OpenType.Fixed).ToArray();
                        foreach (var item in chk)
                        {
                            item.Type = OpenType.None;

                            //if (Postpone) //顺延
                            {
                                int idx = result.IndexOf(item);
                                for (int i = idx + 1; i < result.Count; i++)
                                {
                                    if (result[i].Type != OpenType.Fixed && !result[i].IsExclude && result[i].Flag.HasFlag(DayFlag.Trade))
                                    {
                                        result[i].Type = OpenType.Fixed;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (Dates?.Length > 0) //的第n个交易日
                    {
                        IEnumerable<DateEx> orderd = DayOrder == SequenceOrder.Ascend ? result : result.AsEnumerable().Reverse();
                        var sel = orderd.Where(x => !x.IsExclude && x.Flag.HasFlag(DayFlag.Trade));

                        foreach (var s in sel.Index())
                        {
                            if (Array.BinarySearch(Dates, s.Index + 1) >= 0)
                                s.Item.Type = OpenType.Fixed;
                        }

                    }
                }
                else// 自然日
                {
                    if (sw && Dates?.Length > 0) //n个周N 
                    {
                        IEnumerable<DateEx> orderd = DayOrder == SequenceOrder.Ascend ? result : result.AsEnumerable().Reverse();
                        var sel = orderd.Where(x => !x.IsExclude).GroupBy(x => (int)x.Date.DayOfWeek);
                        foreach (var dw in sel)
                        {
                            if (Array.BinarySearch(Dates, dw.Key) < 0) // 星期几不符合
                                continue;

                            foreach (var w in dw.Index()) //第N周N
                            {
                                bool pairw = Array.BinarySearch(Weeks!, w.Index + 1) >= 0;
                                if (!pairw) continue;

                                w.Item.Type = OpenType.Fixed;
                            }

                        }


                    }
                    else if (Dates?.Length > 0) //月的第n个交易日
                    {
                        IEnumerable<DateEx> orderd = DayOrder == SequenceOrder.Ascend ? result : result.AsEnumerable().Reverse();
                        var sel = orderd.Where(x => !x.IsExclude).GroupBy(x => x.Date.Month);

                        foreach (var m in sel)
                        {
                            foreach (var s in m.Index())
                            {
                                if (Array.BinarySearch(Dates, s.Index + 1) >= 0)
                                    s.Item.Type = OpenType.Fixed;
                            }
                        }
                    }

                    // 非交易日及顺延
                    var chk = result.Where(x => !x.Flag.HasFlag(DayFlag.Trade) && x.Type == OpenType.Fixed).ToArray();
                    foreach (var item in chk)
                    {
                        item.Type = OpenType.None;

                        if (Postpone) //顺延
                        {
                            int idx = result.IndexOf(item);
                            for (int i = idx + 1; i < result.Count; i++)
                            {
                                if (result[i].Type != OpenType.Fixed && !result[i].IsExclude && result[i].Flag.HasFlag(DayFlag.Trade))
                                {
                                    result[i].Type = OpenType.Fixed;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        else if (Type == FundOpenType.Weekly) //每周
        {
            // 交易日
            if (TradeOrNatural)
            {
                if (Dates?.Length > 0) //每周的第n个交易日
                {
                    IEnumerable<DateEx> orderd = DayOrder == SequenceOrder.Ascend ? result : result.AsEnumerable().Reverse();
                    var sel = orderd.Where(x => !x.IsExclude && x.Flag.HasFlag(DayFlag.Trade)).GroupBy(x => x.WeekOfYear);
                    foreach (var m in sel)
                    {
                        foreach (var d in m.Index())
                        {

                            if (Array.BinarySearch(Dates, d.Index + 1) >= 0)
                                d.Item.Type = OpenType.Fixed;
                        }
                    }

                    // 非交易日及顺延
                    //var chk = result.Where(x => !x.Flag.HasFlag(DayFlag.Trade) && x.Type == OpenType.Fixed).ToArray();
                    //foreach (var item in chk)
                    //{
                    //    item.Type = OpenType.None;

                    //    //if (Postpone) //顺延
                    //    {
                    //        int idx = result.IndexOf(item);
                    //        for (int i = idx + 1; i < result.Count; i++)
                    //        {
                    //            if (result[i].Type != OpenType.Fixed && !result[i].IsExclude && result[i].Flag.HasFlag(DayFlag.Trade))
                    //            {
                    //                result[i].Type = OpenType.Fixed;
                    //                break;
                    //            }
                    //        }
                    //    }
                    //}
                }
                else if (Dates?.Length > 0) //月的第n个交易日
                {
                    IEnumerable<DateEx> orderd = DayOrder == SequenceOrder.Ascend ? result : result.AsEnumerable().Reverse();
                    var sel = orderd.Where(x => !x.IsExclude && x.Flag.HasFlag(DayFlag.Trade)).GroupBy(x => x.Date.Month);

                    foreach (var m in sel)
                    {
                        foreach (var s in m.Index())
                        {
                            if (Array.BinarySearch(Dates, s.Index + 1) >= 0)
                                s.Item.Type = OpenType.Fixed;
                        }
                    }
                }
            }
            else// 自然日
            {
                if (Dates?.Length > 0) //n个周N 
                {
                    IEnumerable<DateEx> orderd = DayOrder == SequenceOrder.Ascend ? result : result.AsEnumerable().Reverse();
                    var sel = orderd.Where(x => !x.IsExclude).GroupBy(x => (int)x.Date.DayOfWeek);
                    foreach (var dw in sel)
                    {
                        if (Array.BinarySearch(Dates, dw.Key) < 0) // 星期几不符合
                            continue;


                        foreach (var w in dw) //第N周N
                        {
                            w.Type = OpenType.Fixed;
                        }

                    }


                    // 非交易日及顺延
                    var chk = result.Where(x => !x.Flag.HasFlag(DayFlag.Trade) && x.Type == OpenType.Fixed).ToArray();
                    foreach (var item in chk)
                    {
                        item.Type = OpenType.None;

                        if (Postpone) //顺延
                        {
                            int idx = result.IndexOf(item);
                            for (int i = idx + 1; i < result.Count; i++)
                            {
                                if (result[i].Type != OpenType.Fixed && !result[i].IsExclude && result[i].Flag.HasFlag(DayFlag.Trade))
                                {
                                    result[i].Type = OpenType.Fixed;
                                    break;
                                }
                            }
                        }
                    }
                }

            }
        }
        return result.ToArray();
    }








    public bool IsPair(DateOnly date)
    {
        // 不开放
        if (Type == FundOpenType.Closed) return false;

        // 季度
        if (Type == FundOpenType.Yearly)
        {
            if (Quarters?.Length > 0)
            {
                // 计算
                int v = (date.Month - 1) / 3 + 1;

                if (Array.BinarySearch(Quarters, v) < 0)
                    return false;
            }
        }

        // 月
        if (Type < FundOpenType.Monthly)
        {
            if (Months?.Length > 0)
            {
                // 计算
                int v = Type == FundOpenType.Quarterly ? (date.Month - 1) % 3 + 1 : date.Month;

                if (Array.BinarySearch(Months, v) < 0)
                    return false;
            }
        }


        return true;
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
        public bool IsExclude { get; set; }

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

public class OpenDateSheet : IDate
{
    public required DateOnly Date { get; init; }

    public DayFlag Flag { get; set; }

    public OpenType Type { get; set; }

}