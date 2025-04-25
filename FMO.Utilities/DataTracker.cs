using System.Collections.Concurrent;
using FMO.Models;
using Serilog;

namespace FMO.Utilities;

public enum TipType
{
    None,

    /// <summary>
    /// 基金份额与估值表不致
    /// </summary>
    FundShareNotPair,

    /// <summary>
    /// 没有TA数据
    /// </summary>
    FundNoTARecord,
}

public record class FundTip(int FundId, TipType Type, string? Tip);

/// <summary>
/// 数据校验
/// </summary>
public static class DataTracker
{

    public static ConcurrentBag<FundTip> FundTips { get; } = new();



    /// <summary>
    /// 检查基金存储文件夹
    /// </summary>
    /// <param name="funds"></param>
    public static void CheckFundFolder(IEnumerable<Fund> funds)
    {
        // 基金文件夹
        var dis = new DirectoryInfo(@"files\funds").GetDirectories();
        foreach (var f in funds)
        {
            if (f.Code?.Length > 4)
            {
                var di = dis.FirstOrDefault(x => x.Name.StartsWith(f.Code));

                var name = $"{f.Code}.{f.Name}";

                string folder = $"files\\funds\\{name}";

                if (di is null)
                {
                    Directory.CreateDirectory(folder);
                    continue;
                }
                if (di.Name != name)
                {
                    Directory.Move(di.FullName, folder);
                    Log.Warning($"基金 {f.Code} 名称已更新 [{di.Name}] -> [{f.Name}]");
                }

                FundHelper.Map(f, folder);
            }
        }
    }


    public static void CheckShareIsPair(IEnumerable<Fund> funds)
    {
        // 验证最新的净值中份额与share是否一致
        using var db = DbHelper.Base();

        foreach (var fund in funds)
        {
            var last = db.GetDailyCollection(fund.Id).Find(x => x.NetValue > 0).MaxBy(x => x.Date);
            if (last is null) continue;

            var c = db.GetCollection<FundShareRecord>().Find(x => x.FundId == fund.Id);

            if (c is null || !c.Any())
            {
                db.BuildFundShareRecord(fund.Id);
                c = db.GetCollection<FundShareRecord>().Find(x => x.FundId == fund.Id);
            }

            if (c is null || !c.Any())
            {
                FundTips.Add(new FundTip(fund.Id, TipType.FundNoTARecord, "没有TA数据"));
                continue;
            }

            var sh = c.LastOrDefault(x => x.Date < last.Date);

            if(sh?.Share != last.Share)
            {
                FundTips.Add(new FundTip(fund.Id, TipType.FundShareNotPair, "基金份额与估值表不一致"));
                continue;
            }
        }








    }


}
