using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using LiteDB;
using Serilog;
using System.Collections;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace FMO.Utilities;


public record class FundTip(int FundId, string FundName, TipType Type, string? Tip);

public record UniformTip(TipType Type, object? Tip);

public record FundTipMessage(int FundId);

public record FundsTipCountMessage(int Count);

public record EntityIndentify(Type Type, object Id, int FundId);


public enum ValidationMethod { }
public class DataValidationInfo
{
    public ValidationMethod Method { get; set; }

    /// <summary>
    /// 关联的类实例
    /// </summary>
    public EntityIndentify[] Related { get; set; } = [];





}







/// <summary>
/// 数据校验
/// </summary>
public static partial class DataTracker
{

    public static FundTipList FundTips { get; } = new();

    public static ConcurrentDictionary<TipType, string?> UniformTips { get; } = new();


    public static ThreadSafeList<DataValidationInfo> validationInfos { get; } = new();

    static DataTracker()
    {
        FundTips.CollectionChanged += () => WeakReferenceMessenger.Default.Send(new FundsTipCountMessage(FundTips.Count));
    }


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
                var di = dis.FirstOrDefault(x => x.Name.StartsWith($"{f.Id}.")) ?? dis.FirstOrDefault(x => x.Name.StartsWith(f.Code));

                var name = $"{f.Id}.{f.Code}.{f.Name}";

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


    //public static void CheckShareIsPair(IEnumerable<Fund> funds)
    //{
    //    // 验证最新净值中的份额（Share）与 FundShareRecord 中的份额是否一致
    //    using var db = DbHelper.Base();

    //    // 遍历所有的基金（funds）
    //    foreach (var fund in funds)
    //    {
    //        // 获取该基金的每日净值数据中，净值大于0的记录，并找到最新日期的记录
    //        var last = db.GetDailyCollection(fund.Id).Find(x => x.NetValue > 0).MaxBy(x => x.Date);

    //        // 如果没有找到有效的净值记录，则跳过当前基金
    //        if (last is null) continue;

    //        // 获取基金的 FundShareRecord 记录和 TransferRecord 记录
    //        var c = db.GetCollection<FundShareRecord>().Find(x => x.FundId == fund.Id);
    //        var ta = db.GetCollection<TransferRecord>().Find(x => x.FundId == fund.Id);

    //        // 如果没有 TA 记录或 FundShareRecord 数据为空，则添加提示：没有TA数据
    //        if (!ta.Any())
    //        {
    //            FundTips.Add(new FundTip(fund.Id, fund.Name, TipType.FundNoTARecord, "没有TA数据"));
    //            continue;
    //        }
    //        else if (FundTips.FirstOrDefault(x => x.FundId == fund.Id && x.Type == TipType.FundNoTARecord) is FundTip tip) // 清除错误信息
    //        {
    //            FundTips.Remove(tip);
    //            WeakReferenceMessenger.Default.Send(new FundTipMessage(fund.Id));
    //        }

    //        // 获取 TA 记录中确认日期（ConfirmedDate）最大的日期
    //        var lta = ta.Max(x => x.ConfirmedDate);

    //        // 如果 FundShareRecord 为空，或者最新记录的日期早于 TA 的最新确认日期，则重建 FundShareRecord
    //        if (c is null || !c.Any() || c.Max(x => x.Date) < lta)
    //        {
    //            db.RebuildFundShareRecord(fund.Id); // 重建该基金的份额记录
    //            c = db.GetCollection<FundShareRecord>().Find(x => x.FundId == fund.Id); // 重新获取份额记录
    //        }

    //        // 找到在 FundShareRecord 中，日期不晚于最新净值日期的最后一条记录
    //        var sh = c.OrderBy(x => x.Date).LastOrDefault(x => x.Date <= last.Date);

    //        // 检查份额是否一致，如果不一致则添加提示：基金份额与估值表不一致
    //        if (sh!.Share != last.Share)
    //        {
    //            FundTips.Add(new FundTip(fund.Id, fund.Name, TipType.FundShareNotPair, "基金份额与估值表不一致"));

    //            // 修改 trustee中的work日期
    //            using var pd = DbHelper.Platform();
    //            var rs = pd.GetCollection<TrusteeMethodShotRange>().Find(x => x.Id.EndsWith(nameof(ITrustee.QueryTransferRecords))).ToArray();
    //            foreach (var item in rs)
    //            {
    //                if (item.End > sh.Date)
    //                    pd.GetCollection<TrusteeMethodShotRange>().Update(new TrusteeMethodShotRange(item.Id, item.Begin, sh.Date));
    //            }

    //            // 发送基金提示消息（例如用于界面通知）
    //            WeakReferenceMessenger.Default.Send(new FundTipMessage(fund.Id));
    //            continue;
    //        }
    //        else if (FundTips.FirstOrDefault(x => x.FundId == fund.Id && x.Type == TipType.FundShareNotPair) is FundTip tip) // 清除错误信息
    //        {
    //            FundTips.Remove(tip);
    //            WeakReferenceMessenger.Default.Send(new FundTipMessage(fund.Id));
    //        }

    //    }

    //}
    //public static void CheckShareIsPair(int fid)
    //{
    //    // 验证最新的净值中份额与share是否一致
    //    using var db = DbHelper.Base();
    //    var fund = db.GetCollection<Fund>().FindById(fid);

    //    var last = db.GetDailyCollection(fund.Id).Find(x => x.NetValue > 0).MaxBy(x => x.Date);
    //    if (last is null)
    //    {
    //        WeakReferenceMessenger.Default.Send(new FundTipMessage(fid));
    //        return;
    //    }
    //    var c = db.GetCollection<FundShareRecord>().Find(x => x.FundId == fund.Id);
    //    var lta = db.GetCollection<TransferRecord>().Find(x => x.FundId == fund.Id).Max(x => x.ConfirmedDate);
    //    if (c is null || !c.Any() || c.Max(x => x.Date < lta))
    //    {
    //        db.RebuildFundShareRecord(fund.Id);
    //        c = db.GetCollection<FundShareRecord>().Find(x => x.FundId == fund.Id);
    //    }

    //    if (c is null || !c.Any())
    //    {
    //        FundTips.Add(new FundTip(fund.Id, fund.Name, TipType.FundNoTARecord, "没有TA数据"));
    //        WeakReferenceMessenger.Default.Send(new FundTipMessage(fid));
    //        return;
    //    }

    //    var sh = c.LastOrDefault(x => x.Date < last.Date);

    //    if (sh?.Share != last.Share)
    //    {
    //        FundTips.Add(new FundTip(fund.Id, fund.Name, TipType.FundShareNotPair, "基金份额与估值表不一致"));
    //        WeakReferenceMessenger.Default.Send(new FundTipMessage(fid));
    //        return;
    //    }

    //    FundTips.Remove(x => x.Type == TipType.FundShareNotPair);
    //    WeakReferenceMessenger.Default.Send(new FundTipMessage(fid));

    //    DataValidationInfo validationInfo = new DataValidationInfo
    //    {
    //        Related = [new(typeof(DailyValue), 0, fid), new(typeof(TransferRecord), 0, fid)]
    //    };
    //}


    public static void CheckIsExpired(IEnumerable<Fund> funds)
    {
        using var db = DbHelper.Base();
        var cur = DateOnly.FromDateTime(DateTime.Today);

        var coll = db.GetCollection<FundElements>();
        foreach (var fund in funds.Where(x => x.Status == FundStatus.Normal || x.Status == FundStatus.StartLiquidation))
        {
            var ele = coll.FindById(fund.Id);
            if (ele is not null)
            {
                var expire = ele.ExpirationDate.Value;

                if (expire != default && cur > expire)
                {
                    FundTips.Add(new FundTip(fund.Id, fund.Name, TipType.OverDue, $"基金已超期{cur.DayNumber - expire.DayNumber}天"));
                    WeakReferenceMessenger.Default.Send(new FundTipMessage(fund.Id));
                }
            }
        }
    }

    public static void CheckInvestorBalance()
    {
        using var db = DbHelper.Base();
        var coll = db.GetCollection<InvestorBalance>().FindAll().OrderBy(x => x.Id).ToList();
        var tas = db.GetCollection<TransferRecord>().FindAll().ToList();
        IEnumerable<(long key, DateOnly date)> ta = tas.Select(x => new { id = InvestorBalance.MakeId(x.CustomerId, x.FundId), date = x.ConfirmedDate }).
            GroupBy(x => x.id).Select(x => (key: x.Key, date: x.Max(y => y.date))).OrderBy(x => x.key);

        foreach (var item in ta)
        {
            var o = coll.FirstOrDefault(x => x.Id == item.key);
            if (o is null || o.Date != item.date)
            {
                var (c, f) = InvestorBalance.ParseId(item.key);
                var tf = tas.Where(x => x.CustomerId == c && x.FundId == f);
                var share = tf.Sum(x => x.ShareChange());
                var deposit = tf.Where(x => x.Type switch { TransferRecordType.Subscription or TransferRecordType.Purchase or TransferRecordType.MoveIn or TransferRecordType.SwitchIn or TransferRecordType.TransferIn => true, _ => false }).Sum(x => x.ConfirmedNetAmount);
                var withdraw = tf.Where(x => x.Type switch { TransferRecordType.Redemption or TransferRecordType.Redemption or TransferRecordType.MoveOut or TransferRecordType.SwitchOut or TransferRecordType.TransferOut or TransferRecordType.Distribution => true, _ => false }).Sum(x => x.ConfirmedNetAmount);

                db.GetCollection<InvestorBalance>().Upsert(new InvestorBalance { FundId = f, InvestorId = c, Share = share, Deposit = deposit, Withdraw = withdraw, Date = tf.Max(x => x.ConfirmedDate) });
            }
        }


    }

    /// <summary>
    /// 检查TA中的fundid,investor id是不是0
    /// </summary>
    public static void CheckTAMissOwner()
    {
        using var db = DbHelper.Base();
        var e1 = db.GetCollection<TransferRequest>().Count(x => x.FundId == 0 || x.CustomerId == 0);
        var e2 = db.GetCollection<TransferRecord>().Count(x => x.FundId == 0 || x.CustomerId == 0);

        if (e1 + e2 > 0)
            UniformTips.AddOrUpdate(TipType.TANoOwner, $"发现未关联Request {e1}个，Record {e2}个", (x, y) => $"发现未关联Request {e1}个，Record {e2}个");
        else UniformTips.AddOrUpdate(TipType.TANoOwner, x => null, (x, y) => null);

        WeakReferenceMessenger.Default.Send(new TipChangeMessage(TipType.TANoOwner));
        //WeakReferenceMessenger.Default.Send(new UniformTip(TipType.TANoOwner, $"发现未关联Request {e1}个，Record {e2}个"));
    }

    public static void CheckTransferRequestMissing()
    {
        using var db = DbHelper.Base();
        if (db.GetCollection<TransferMapping>().Count(x => x.RecordId != 0 && x.RequestId == 0) is int cc)
            UniformTips.AddOrUpdate(TipType.TANoOwner, $"发现未关联Request {cc}个", (x, y) => $"发现未关联Request {cc}个");
        else UniformTips.AddOrUpdate(TipType.TANoOwner, x => null, (x, y) => null);

        WeakReferenceMessenger.Default.Send(new TipChangeMessage(TipType.TransferRequestMissing));
    }


    //public static Debouncer CheckTAMissOwnerInvoker = new(CheckTAMissOwner, 500);

    //public static Debouncer CheckTransferRequestMissingInvoker = new(CheckTransferRequestMissing, 500);

    /// <summary>
    /// 检查并匹配订单
    /// </summary>
    /// <param name="db"></param>
    public static void CheckPairOrder(BaseDatabase db)
    {
        var orders = db.GetCollection<TransferOrder>().FindAll().ToArray();
        var tas = db.GetCollection<TransferRecord>().FindAll().Where(x => TransferRecord.RequireOrder(x.Type)).ToArray();

        // 检查缺失request
        var miss = db.GetCollection<TransferMapping>().Find(x => x.RequestId == 0 && x.RecordId != 0).ToList();


        // 清除不存在的order
        var bad = tas.Where(x => x.OrderId != 0).ExceptBy(orders.Select(x => x.Id), x => x.OrderId).ToArray();
        foreach (var item in bad)
            item.OrderId = 0;

        // 清理对不上的
        bad = tas.Where(x => x.OrderId != 0).Join(orders, x => x.OrderId, x => x.Id, (x, y) => (x, y)).Where(x => x.x.FundId != x.y.FundId || x.x.CustomerId != x.y.InvestorId).Select(x => x.x).ToArray();
        foreach (var item in bad)
            item.OrderId = 0;

        db.GetCollection<TransferRecord>().Update(bad);

        List<TransferRecord> changed = new();
        // 未匹配
        var un = orders.ExceptBy(tas.Select(x => x.OrderId), x => x.Id).ToArray();
        foreach (var od in un)
        {
            // 找当日或后的第一个ta
            DateOnly date = tas.Where(x => x.FundId == od.FundId && x.CustomerId == od.InvestorId).OrderBy(x => x.RequestDate).FirstOrDefault(x => x.RequestDate >= od.Date)?.RequestDate ?? default;

            // 可能有同日多ta
            var list = tas.Where(x => x.FundId == od.FundId && x.CustomerId == od.InvestorId && x.RequestDate == date);

            // 单独匹配
            foreach (var record in list)
            {
                if (od.Date > record.RequestDate) continue;
                switch (od.Type)
                {
                    case TransferOrderType.FirstTrade:
                    case TransferOrderType.Buy:
                        if (od.Number != record.RequestAmount)
                            continue;
                        break;
                    case TransferOrderType.Share:
                        if (od.Number != record.RequestShare)
                            continue;
                        break;
                    case TransferOrderType.Amount:
                        if (od.Number != record.RequestAmount)
                            continue;
                        break;
                    case TransferOrderType.RemainAmout:
                        if ((od.Number / record.RequestAmount) switch { < 0.99m => true, > 1.01m => true, _ => false })
                            continue;
                        break;
                    default:
                        break;
                }

                record.OrderId = od.Id;
                changed.Add(record);
                break;

            }

            // 合并匹配
            switch (od.Type)
            {
                case TransferOrderType.FirstTrade:
                case TransferOrderType.Buy:
                    if (od.Number != list.Sum(x => x.RequestAmount))
                        continue;
                    break;
                case TransferOrderType.Share:
                    if (od.Number != list.Sum(x => x.RequestShare))
                        continue;
                    break;
                case TransferOrderType.Amount:
                    if (od.Number != list.Sum(x => x.RequestAmount))
                        continue;
                    break;
                case TransferOrderType.RemainAmout:
                    if ((od.Number / list.Sum(x => x.ConfirmedAmount)) switch { < 0.99m => true, > 1.01m => true, _ => false })
                        continue;
                    break;
            }

            foreach (var item in list)
            {
                item.OrderId = od.Id;
                changed.Add(item);
            }

        }
        db.GetCollection<TransferRecord>().Update(changed);
    }

    public static bool IsPair(TransferOrder order, TransferRecord record)
    {
        if (order.FundId != record.FundId) return false;
        if (order.InvestorId != record.CustomerId) return false;

        if (order.Date > record.RequestDate) return false;

        var orderbuy = order.Type switch { TransferOrderType.FirstTrade or TransferOrderType.Buy => true, _ => false };
        if (orderbuy && !TransferRecord.RequireBuyOrder(record.Type)) return false;
        if (!orderbuy && !TransferRecord.RequireSellOrder(record.Type)) return false;

        if (orderbuy && order.Number != record.RequestAmount) return false;
        if (!orderbuy)
        {
            switch (order.Type)
            {
                case TransferOrderType.FirstTrade:
                case TransferOrderType.Buy:
                    if (order.Number != record.RequestAmount)
                        return false;
                    break;
                case TransferOrderType.Share:
                    if (order.Number != record.RequestShare)
                        return false;
                    break;
                case TransferOrderType.Amount:
                    if (order.Number != record.RequestAmount)
                        return false;
                    break;
                case TransferOrderType.RemainAmout:
                    if ((order.Number / record.ConfirmedAmount) switch { < 0.99m => true, > 1.01m => true, _ => false })
                        return false;
                    break;
                default:
                    break;
            }
        }
        return true;
    }


    public static string? GetUniformTip(TipType tip) => UniformTips.TryGetValue(tip, out var text) ? text : null;


    public static void OnFundCleared(Fund f)
    {
        foreach (var m in FundTips.Where(x => x.FundId == f.Id && x.Type == TipType.OverDue))
            FundTips.Remove(m);

        WeakReferenceMessenger.Default.Send(new FundTipMessage(f.Id));
    }


    //public static void OnNewTransferRecord(TransferRecord r)
    //{

    //}


    public static void OnDailyValue(IEnumerable<DailyValue> dailyValues)
    {
        dailyValues = dailyValues.Where(x => x.NetValue != 0);
        using var db = DbHelper.Base();
        foreach (var g in dailyValues.GroupBy(x => (x.FundId, x.Class)))
        {
            var table = db.GetDailyCollection(g.Key.FundId, g.Key.Class);

            // 如果是api，只更新非sheet
            var dic = table.Query().Select(x => new { x.Id, x.SheetPath }).ToEnumerable().ToDictionary(x => x.Id);

            foreach (var item in g)
            {
                if (dic.TryGetValue(item.Id, out var value))
                    item.SheetPath = value.SheetPath;
            }
            table.Upsert(g);
        }

        // 更新管理规模
        var dates = dailyValues.Select(x => x.Date).Distinct().ToList();
        UpdateManageSacle(dates);

        // 修改 FundShareRecord
        // 一个版本是用dv算，当份额与前一日不同时，录入
        // 一个版本是用ta算， 
        var comparer = Comparer<DailyValue>.Create((a, b) => a.Date.CompareTo(b.Date));
        foreach (var g in dailyValues.GroupBy(x => x.FundId))
        {
            var fid = g.Key;
            List<FundShareRecordByDaily> add = new();
            var col = db.GetCollection<FundShareRecordByDaily>();

            // 检查是否与前一天share不一样
            var sd = g.Min(x => x.Date);

            // 与历史记录比较，补充缺失的部分
            var last = col.Find(x => x.Date < sd).LastOrDefault();
            var lastd = last?.Date ?? default;
            var middle = db.GetDailyCollection(g.Key).Find(x => x.Date >= lastd && x.Date <= sd).OrderBy(x => x.Date).ToList();
            if (middle.Count > 0 && middle[0].Date is DateOnly datem && col.FindOne(x => x.FundId == fid && x.Date == datem) == null)
                add.Add(new FundShareRecordByDaily(g.Key, middle[0].Date, middle[0].Share));

            for (int i = 1; i < middle.Count; i++)
            {
                if (middle[i - 1].Share != middle[i].Share)
                    add.Add(new FundShareRecordByDaily(g.Key, middle[i].Date, middle[i].Share));
            }

            if (sd.DayNumber > 20) sd = sd.AddDays(-20);
            var ed = g.Max(x => x.Date);
            var ds = db.GetDailyCollection(g.Key).Find(x => x.Date > sd && x.Date < ed).OrderBy(x => x.Date).ToList();
            foreach (var dy in g)
            {
                // 使用二分查找找到前一日记录
                int index = ds.BinarySearch(dy, comparer);
                index = index < 0 ? ~index - 1 : index - 1;

                DailyValue? previous = index < 0 || index >= ds.Count ? null : ds[index];

                // 检查份额变化
                if (previous != null && previous.Share != dy.Share && dy.Share != 0)
                    add.Add(new FundShareRecordByDaily(fid, dy.Date, dy.Share));

                //var pre = ds.FirstOrDefault(x => x.Date < dy.Date);
                //if (pre != null && pre.Share != dy.Share)
                //    add.Add(new(g.Key, dy.Date, dy.Share));
            }



            col.Upsert(add);
            var ee = col.Find(x => x.FundId == fid).OrderBy(x => x.Date).ToList();

            // 删除连续相同的值
            for (int i = 1; i < ee.Count; i++)
            {
                if (ee[i].Share == ee[i - 1].Share)
                    col.Delete(ee[i].Id);
            }

            OnFundShareRecord(add);

        }


        // 通知
        Parallel.ForEach(dailyValues, x =>
        {
            WeakReferenceMessenger.Default.Send(new FundDailyUpdateMessage(x.FundId, x));
        });


        // 检验
        VerifyRules.OnEntityArrival(dailyValues);


    }

    public static void UpdateManageSacle(IEnumerable<DateOnly> dates)
    {
        using var db = DbHelper.Base();
        // 获取所有名称包含"fv_开关"的集合（表）名称
        var fvCollections = db.GetCollectionNames().Where(c => Regex.IsMatch(c, @"fv_\d+$")).ToList();

        //IEnumerable<BsonValue> array = dates.Select(x => BsonMapper.Global.ToDocument(x));
        var array = dates.Select(x => new BsonValue(x.DayNumber));

        Dictionary<DateOnly, decimal> assets = new();

        // 遍历每个符合条件的集合，执行查询并转换结果
        foreach (var collectionName in fvCollections)
        {
            var collection = db.GetCollection(collectionName);

            // 1. 构建查询条件：Date在指定日期列表中
            var query = Query.In("Date.DayNumber", dates.Select(d => new BsonValue(d.DayNumber)).ToArray());

            // 2. 执行查询获取文档（仅包含需要的字段以提高效率）
            var results = collection.Find(query).Select(x => new { Date = BsonMapper.Global.ToObject<DateOnly>(x["Date"].AsDocument), NetAsset = x["NetAsset"].AsDecimal }).ToList();

            foreach (var item in results)
            {
                if (!assets.ContainsKey(item.Date))
                    assets[item.Date] = 0;

                assets[item.Date] += item.NetAsset;
            }
        }

        db.GetCollection<DailyManageSacle>().InsertBulk(assets.Select(x => new DailyManageSacle(x.Key, x.Value)));
    }


    private static void OnFundShareRecord(List<FundShareRecordByDaily> add)
    {
        VerifyRules.OnEntityArrival(add);
    }

    private static void OnFundShareRecord(List<FundShareRecordByTransfer> add)
    {
        VerifyRules.OnEntityArrival(add);
    }
    public static void OnEntityChanged(EntityChanged<Fund, DateOnly> changed)
    {
        if (changed.PropertyName == nameof(Fund.ClearDate) && changed.New != default)
            VerifyRules.OnEntityArrival([changed]);
    }
    public static void OnEntityChanged(EntityChanged<DateOnly> changed)
    {
        VerifyRules.OnEntityArrival([changed]);
    }

    /// <summary>
    ///  关联订单
    /// </summary>
    /// <param name="same"></param>
    public static void LinkOrder(params TransferRecord[] same)
    {
        foreach (var item in same)
            WeakReferenceMessenger.Default.Send(new TransferRecordLinkOrderMessage(item.Id, item.OrderId));
    }

    public static void OnBatchTransferRecord(IList<TransferRecord> records)
    {
        if (records.Count == 0) return;

        using var db = DbHelper.Base();

        // 对齐id 
        var rmin = records.Min(x => x.ConfirmedDate);
        var olds = db.GetCollection<TransferRecord>().Find(x => x.ConfirmedDate >= rmin);
        foreach (var r in records)
        {
            // 同日同名
            var exi = olds.Where(x => x.ExternalId == r.ExternalId || (x.CustomerName == r.CustomerName && x.CustomerIdentity == r.CustomerIdentity && x.ConfirmedDate == r.ConfirmedDate)).ToList();

            // 只有一个，替换
            if (exi.Count == 1 && (exi[0].Source != "api" || exi[0].ExternalId == r.ExternalId))
            {
                r.Id = exi[0].Id;
                r.OrderId = exi[0].OrderId;
                r.RequestId = exi[0].RequestId;
                continue;
            }

            // > 1个
            // 存在同ex id，替换
            var old = exi.Where(x => x.ExternalId == r.ExternalId);
            if (old.Any())
            {
                r.Id = old.First().Id;
                r.OrderId = old.First().OrderId;
                r.RequestId = old.First().RequestId;
            }
            // 如果存在手动录入的，也删除
            foreach (var item in exi)
                db.GetCollection<TransferRecord>().DeleteMany(item => item.Source == "manual" || item.ExternalId == r.ExternalId);

        }

        db.GetCollection<TransferRecord>().Upsert(records);

        // 通知
        try
        {
            foreach (var item in records)
                WeakReferenceMessenger.Default.Send(item);
        }
        catch { }

        var tableRecord = db.GetCollection<TransferRecord>();
        var tableBalance = db.GetCollection<InvestorBalance>();

        // 更新投资人平衡表 
        foreach (var g in records.GroupBy(x => (x.CustomerId, x.FundId)))
            UpdateInvestorBalance(tableRecord, tableBalance, g.Key.CustomerId, g.Key.FundId, g.Min(x => x.ConfirmedDate));

        // 更新基金份额平衡表
        var t2 = db.GetCollection<FundShareRecordByTransfer>();
        foreach (var g in records.GroupBy(x => x.FundId))
            UpdateFundShareRecordByTA(tableRecord, t2, g.Key, g.Min(x => x.ConfirmedDate));


        var ids = records.Select(x => x.FundId).Distinct().ToList();
        var funds = db.GetCollection<Fund>().Find(x => ids.Contains(x.Id)).ToList();
        //DataTracker.CheckShareIsPair(funds);

        //VerifyRules.OnEntityArrival(records);


        // 检查基金是否份额是否为0，如果是这样，最后的ta设为清盘
        var fsr = db.GetCollection<FundShareRecordByTransfer>();
        foreach (var id in ids)
        {
            var r = fsr.Find(x => x.FundId == id).OrderByDescending(x => x.Date).ToList();
            if (r.Count > 1 && r[0].Share == 0) //清盘
            {
                var last = r[0].Date;
                db.BeginTrans();
                foreach (var item in db.GetCollection<TransferRecord>().Find(x => x.FundId == id && x.ConfirmedDate == last).ToList())
                {
                    if (item.Type != TransferRecordType.Redemption && item.Type != TransferRecordType.ForceRedemption)
                    {
                        db.Rollback();
                        break;
                    }

                    item.IsLiquidating = true;
                    db.GetCollection<TransferRecord>().Update(item);
                }
                db.Commit();
            }
        }


        CheckTAMissOwnerInvoker.Invoke();
        CheckTransferRequestMissingInvoker.Invoke();
    }

    private static void OnUpdateValidation(IEnumerable<TransferRecord> records)
    {
        foreach (var item in validationInfos)
        {
            //if(item.Related.FirstOrDefault(x=>x.Type == typeof(TransferRecord)) is EntityIndentify idf && idf.Id == )
        }
    }

    public static void OnBatchTransferRequest(TransferRequest[] data)
    {
        // 匹配订单
        using var db = DbHelper.Base();
        // 找已知的map
        var rids = data.Select(x => x.Id).ToList();
        var mapTable = db.GetCollection<TransferMapping>();
        var exists = mapTable.Find(x => rids.Contains(x.Id)).ToList();

        foreach (var req in data)
        {
            var old = exists.FirstOrDefault(x => x.RecordId == req.Id) ?? new() { RequestId = req.Id };

            // 未匹配order
            if (old.OrderId == 0)
            {
                // 找RequestDate前最后一个order
                if (db.GetCollection<TransferOrder>().Find(x => x.FundId == req.FundId && x.InvestorId == req.CustomerId && x.Date <= req.RequestDate).MaxBy(x => x.Date) is TransferOrder order)
                {
                    // 验证order 没有匹配
                    if (mapTable.FindOne(x => x.OrderId == order.Id) is TransferMapping conflict)
                    {
                        if (conflict.RequestId != req.Id)
                            Log.Error($"Order {conflict.Id} 匹配冲突，已匹配 {conflict.RequestId}，待匹配 {req.Id}");
                        else old.Merge(conflict);
                    }
                }
            }

            // 未匹配record
            if (old.RecordId == 0)
            {
                if (db.GetCollection<TransferRecord>().Find(x => x.ExternalRequestId == req.ExternalId) is TransferRecord record)
                {
                    old.RecordId = record.Id;
                }
            }

            // 未匹配流水
            if (string.IsNullOrWhiteSpace(old.TransactionId))
            {
                bool isbuy = req.IsBuy();
                var banks = db.GetCollection<InvestorBankAccount>().Find(x => x.OwnerId == req.CustomerId).Select(x => x.Number);
                if (isbuy)
                {
                    var limit = new DateTime(req.RequestDate, TimeOnly.MaxValue);
                    foreach (var t in db.GetCollection<RaisingBankTransaction>().Find(x => x.FundId == req.FundId && x.Direction == TransctionDirection.Receive
                            && x.Time <= limit && (banks.Contains(x.CounterNo) || x.CounterName == req.CustomerName)).OrderByDescending(x => x.Time))
                    {
                        if (t.Amount == req.RequestAmount)
                            old.TransactionId = t.Id;

                    }
                }


            }

            mapTable.Update(old);
        }


        //CheckTAMissOwnerInvoker.Invoke();
        //CheckTransferRequestMissingInvoker.Invoke();


        // 通知
        try
        {
            foreach (var item in data)
                WeakReferenceMessenger.Default.Send(item);
        }
        catch { }
    }


    /// <summary>
    /// 更新基金份额平衡表
    /// </summary>
    /// <param name="db"></param>
    /// <param name="fundId"></param>
    public static void UpdateFundShareRecordByTA(ILiteCollection<TransferRecord> table, ILiteCollection<FundShareRecordByTransfer> tableSR, int fundId, DateOnly from = default)
    {
        var old = tableSR.Query().OrderByDescending(x => x.Date).Where(x => x.FundId == fundId && x.Date < from).FirstOrDefault();

        var data = table.Find(x => x.FundId == fundId && x.ConfirmedDate >= from).GroupBy(x => x.ConfirmedDate).OrderBy(x => x.Key);
        var list = new List<FundShareRecordByTransfer>();
        if (old is not null) list.Add(old);
        foreach (var item in data)
        {
            if (item.Sum(x => x.ShareChange()) is decimal change && change != 0)
                list.Add(new FundShareRecordByTransfer(fundId, item.First().RequestDate, item.Key, change + (list.Count > 0 ? list[^1].Share : 0)));
        }
        tableSR.DeleteMany(x => x.FundId == fundId && x.Date >= from);
        tableSR.Upsert(list);

        OnFundShareRecord(list);
    }


    /// <summary>
    ///  更新投资人平衡表
    /// </summary>
    /// <param name="table"></param>
    /// <param name="tableIB"></param>
    /// <param name="investorId"></param>
    /// <param name="fundId"></param>
    /// <param name="from"></param>
    public static void UpdateInvestorBalance(ILiteCollection<TransferRecord> table, ILiteCollection<InvestorBalance> tableIB, int investorId, int fundId, DateOnly from = default)
    {
        var old = tableIB.Query().OrderByDescending(x => x.Date).Where(x => x.Date < from).FirstOrDefault();
        var data = table.Find(x => x.FundId == fundId && x.CustomerId == investorId && x.ConfirmedDate >= from).GroupBy(x => x.ConfirmedDate).OrderBy(x => x.Key);
        var list = new List<InvestorBalance>();
        if (old is not null) list.Add(old);
        foreach (var tf in data)
        {
            var share = tf.Sum(x => x.ShareChange());
            var deposit = tf.Where(x => x.Type switch { TransferRecordType.Subscription or TransferRecordType.Purchase or TransferRecordType.MoveIn or TransferRecordType.SwitchIn or TransferRecordType.TransferIn => true, _ => false }).Sum(x => x.ConfirmedNetAmount);
            var withdraw = tf.Where(x => x.Type switch { TransferRecordType.Redemption or TransferRecordType.ForceRedemption or TransferRecordType.MoveOut or TransferRecordType.SwitchOut or TransferRecordType.TransferOut or TransferRecordType.Distribution => true, _ => false }).Sum(x => x.ConfirmedNetAmount);

            var last = list.LastOrDefault() ?? new();
            var cur = new InvestorBalance { FundId = fundId, InvestorId = investorId, Share = share + last.Share, Deposit = deposit + last.Deposit, Withdraw = withdraw + last.Withdraw, Date = tf.Key };
            list.Add(cur);
        }

        tableIB.DeleteMany(x => x.FundId == fundId && x.InvestorId == investorId && x.Date >= from);
        tableIB.Upsert(list);
    }

    public static void RebuildTARelation()
    {
        using var db = DbHelper.Base();
        var orders = db.GetCollection<TransferOrder>().FindAll().OrderBy(x => x.Date).ToList();
        var requests = db.GetCollection<TransferRequest>().FindAll().OrderBy(x => x.RequestDate).Where(x => x.RequiredOrder()).ToList();
        var records = db.GetCollection<TransferRecord>().FindAll().OrderBy(x => x.RequestDate).Where(x => x.RequiredOrder()).ToList();

        // 对应request 和 record
        List<TransferMapping> map = new();
        var rq = requests.Join(records, x => x.ExternalId, x => x.ExternalRequestId, (q, r) => new TransferMapping { RequestId = q.Id, RecordId = r.Id });
        map.AddRange(rq);

        // 有request 无record
        rq = requests.ExceptBy(records.Select(x => x.ExternalRequestId), x => x.ExternalId).Join(records, x => x.ExternalId, x => x.ExternalId, (q, r) => new TransferMapping { RequestId = q.Id, RecordId = r.Id });
        map.AddRange(rq);

        foreach (var item in requests.ExceptBy(map.Select(x => x.RequestId), x => x.Id))
            map.Add(new TransferMapping { RequestId = item.Id });

        // 有  record 无 request
        foreach (var item in records.ExceptBy(map.Select(x => x.RecordId), x => x.Id))
            map.Add(new TransferMapping { RecordId = item.Id });

        // 对应order
        var reqDates = requests.GroupBy(x => ((long)x.FundId << 32) | (long)x.CustomerId).ToDictionary(x => x.Key);
        var recDates = records.GroupBy(x => ((long)x.FundId << 32) | (long)x.CustomerId).ToDictionary(x => x.Key);
        foreach (var o in orders)
        {
            var gid = ((long)o.FundId << 32) | (long)o.InvestorId;

            bool needtestrec = false;

            if (reqDates.ContainsKey(gid))
            {
                List<TransferRequest> req = [.. reqDates[gid]];

                // 找 o.Date 后一个日期的所有同fundId InvestorId的 request
                // 使用二分查找找到第一个符合条件的请求
                int index = req.Select(x => x.RequestDate).ToList().BinarySearch(o.Date);
                if (index < 0) index = ~index;

                var take = 1;
                for (int i = index + 1; i < req.Count; i++, take++)
                {
                    if (req[i].RequestDate != req[index].RequestDate)
                        break;
                }

                var may = req.Skip(index).Take(take);

                bool pair = false;
                switch (o.Type)
                {
                    case TransferOrderType.FirstTrade:
                    case TransferOrderType.Buy:
                    case TransferOrderType.Amount:
                    case TransferOrderType.RemainAmout:
                        pair = o.Number == may.Sum(x => x.RequestAmount);
                        break;
                    case TransferOrderType.Share:
                        pair = o.Number == may.Sum(x => x.RequestShare);
                        break;
                }

                if (pair)
                {
                    var rids = may.Select(x => x.Id);
                    foreach (var item in map.Where(x => rids.Contains(x.RequestId)).ToArray())
                    {
                        // 有冲突
                        if (item.OrderId != 0 && item.OrderId != o.Id)
                            map.Add(new TransferMapping { OrderId = o.Id, RequestId = item.RequestId, RecordId = item.RecordId });
                        else
                            item.OrderId = o.Id;
                    }
                }
                else needtestrec = true;
            }

            if (needtestrec && recDates.ContainsKey(gid))
            {
                List<TransferRecord> rec = recDates[gid].ToList();

                // 找 o.Date 后一个日期的所有同fundId InvestorId的 request
                // 使用二分查找找到第一个符合条件的请求
                int index = rec.Select(x => x.RequestDate).ToList().BinarySearch(o.Date);
                if (index < 0) index = ~index;

                var take = 1;
                for (int i = index + 1; i < rec.Count; i++, take++)
                {
                    if (rec[i].RequestDate != rec[index].RequestDate)
                        break;
                }

                var may = rec.Skip(index).Take(take);

                bool pair = false;
                switch (o.Type)
                {
                    case TransferOrderType.FirstTrade:
                    case TransferOrderType.Buy:
                    case TransferOrderType.Amount:
                    case TransferOrderType.RemainAmout:
                        pair = o.Number == may.Sum(x => x.RequestAmount);
                        break;
                    case TransferOrderType.Share:
                        pair = o.Number == may.Sum(x => x.RequestShare);
                        break;
                }

                if (pair)
                {
                    var rids = may.Select(x => x.Id);
                    foreach (var item in map.Where(x => rids.Contains(x.RecordId)).ToArray())
                    {
                        // 有冲突
                        if (item.OrderId != 0 && item.OrderId != o.Id)
                            map.Add(new TransferMapping { OrderId = o.Id, RequestId = item.RequestId, RecordId = item.RecordId });
                        else
                            item.OrderId = o.Id;
                    }
                }
            }

        }

        //if (db.GetCollection<TransferMapping>().Query().FirstOrDefault() is not null)
        var col = db.GetCollection<TransferMapping>();
        col.DeleteAll();
        col.InsertBulk(map);
    }

    public static void OnDeleteTransferRecord(int id)
    {
        throw new NotImplementedException();
    }
}


public class ThreadSafeList<T> : IEnumerable<T>
{
    protected readonly List<T> _innerList = new List<T>();
    protected readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    public delegate void CollectionChangedHandler();

    public CollectionChangedHandler? CollectionChanged;

    public virtual void Add(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            _innerList.Add(item);
        }
        finally
        {
            _lock.ExitWriteLock();
            CollectionChanged?.Invoke();
        }
    }
    public void Remove(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            _innerList.Remove(item);
        }
        finally
        {
            _lock.ExitWriteLock();
            CollectionChanged?.Invoke();
        }
    }
    public void Remove(Func<T, bool> cond)
    {
        _lock.EnterWriteLock();
        try
        {
            foreach (var item in _innerList.Where(cond).ToArray())
                _innerList.Remove(item);
        }
        finally
        {
            _lock.ExitWriteLock();
            CollectionChanged?.Invoke();
        }
    }

    public T Get(int index)
    {
        _lock.EnterReadLock();
        try
        {
            return _innerList[index];
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IEnumerator<T> GetEnumerator() => _innerList.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _innerList.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}

public class FundTipList : ThreadSafeList<FundTip>
{
    public override void Add(FundTip item)
    {
        _lock.EnterWriteLock();
        bool add = false;
        try
        {
            // 不重复添加
            if (!_innerList.Any(x => x.FundId == item.FundId && x.Type == item.Type))
            {
                _innerList.Add(item);
                add = true;
            }
        }
        finally
        {
            _lock.ExitWriteLock();
            if (add)
                CollectionChanged?.Invoke();
        }
    }
}

