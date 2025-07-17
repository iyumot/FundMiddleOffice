using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using Serilog;
using System.Collections;

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
    OverDue,
}

public record class FundTip(int FundId, string FundName, TipType Type, string? Tip);

public record FundTipMessage(int FundId);

public record FundsTipCountMessage(int Count);


/// <summary>
/// 数据校验
/// </summary>
public static class DataTracker
{

    public static ThreadSafeList<FundTip> FundTips { get; } = new();

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


    public static void CheckShareIsPair(IEnumerable<Fund> funds)
    {
        // 验证最新净值中的份额（Share）与 FundShareRecord 中的份额是否一致
        using var db = DbHelper.Base();

        // 遍历所有的基金（funds）
        foreach (var fund in funds)
        {
            // 获取该基金的每日净值数据中，净值大于0的记录，并找到最新日期的记录
            var last = db.GetDailyCollection(fund.Id).Find(x => x.NetValue > 0).MaxBy(x => x.Date);

            // 如果没有找到有效的净值记录，则跳过当前基金
            if (last is null) continue;

            // 获取基金的 FundShareRecord 记录和 TransferRecord 记录
            var c = db.GetCollection<FundShareRecord>().Find(x => x.FundId == fund.Id);
            var ta = db.GetCollection<TransferRecord>().Find(x => x.FundId == fund.Id);

            // 如果没有 TA 记录或 FundShareRecord 数据为空，则添加提示：没有TA数据
            if (c is null || !c.Any() || !ta.Any())
            {
                FundTips.Add(new FundTip(fund.Id, fund.Name, TipType.FundNoTARecord, "没有TA数据"));
                continue;
            }

            // 获取 TA 记录中确认日期（ConfirmedDate）最大的日期
            var lta = ta.Max(x => x.ConfirmedDate);

            // 如果 FundShareRecord 为空，或者最新记录的日期早于 TA 的最新确认日期，则重建 FundShareRecord
            if (c is null || !c.Any() || c.Max(x => x.Date) < lta)
            {
                db.BuildFundShareRecord(fund.Id); // 重建该基金的份额记录
                c = db.GetCollection<FundShareRecord>().Find(x => x.FundId == fund.Id); // 重新获取份额记录
            }

            // 找到在 FundShareRecord 中，日期不晚于最新净值日期的最后一条记录
            var sh = c.OrderBy(x => x.Date).LastOrDefault(x => x.Date <= last.Date);

            // 检查份额是否一致，如果不一致则添加提示：基金份额与估值表不一致
            if (sh?.Share != last.Share)
            {
                FundTips.Add(new FundTip(fund.Id, fund.Name, TipType.FundShareNotPair, "基金份额与估值表不一致"));
                // 发送基金提示消息（例如用于界面通知）
                WeakReferenceMessenger.Default.Send(new FundTipMessage(fund.Id));
                continue;
            }

        }

    }
    public static void CheckShareIsPair(int fid)
    {
        // 验证最新的净值中份额与share是否一致
        using var db = DbHelper.Base();
        var fund = db.GetCollection<Fund>().FindById(fid);

        var last = db.GetDailyCollection(fund.Id).Find(x => x.NetValue > 0).MaxBy(x => x.Date);
        if (last is null)
        {
            WeakReferenceMessenger.Default.Send(new FundTipMessage(fid));
            return;
        }
        var c = db.GetCollection<FundShareRecord>().Find(x => x.FundId == fund.Id);
        var lta = db.GetCollection<TransferRecord>().Find(x => x.FundId == fund.Id).Max(x => x.ConfirmedDate);
        if (c is null || !c.Any() || c.Max(x => x.Date < lta))
        {
            db.BuildFundShareRecord(fund.Id);
            c = db.GetCollection<FundShareRecord>().Find(x => x.FundId == fund.Id);
        }

        if (c is null || !c.Any())
        {
            FundTips.Add(new FundTip(fund.Id, fund.Name, TipType.FundNoTARecord, "没有TA数据"));
            WeakReferenceMessenger.Default.Send(new FundTipMessage(fid));
            return;
        }

        var sh = c.LastOrDefault(x => x.Date < last.Date);

        if (sh?.Share != last.Share)
        {
            FundTips.Add(new FundTip(fund.Id, fund.Name, TipType.FundShareNotPair, "基金份额与估值表不一致"));
            WeakReferenceMessenger.Default.Send(new FundTipMessage(fid));
            return;
        }

        FundTips.Remove(x => x.Type == TipType.FundShareNotPair);
        WeakReferenceMessenger.Default.Send(new FundTipMessage(fid));
    }


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


    public static void OnFundCleared(Fund f)
    {
        foreach (var m in FundTips.Where(x => x.FundId == f.Id && x.Type == TipType.OverDue))
            FundTips.Remove(m);

        WeakReferenceMessenger.Default.Send(new FundTipMessage(f.Id));
    }


    public static void OnNewTransferRecord(TransferRecord r)
    {

    }




    public static void OnBatchTransferRecord(IEnumerable<TransferRecord> records)
    {
        using var db = DbHelper.Base();

        // 对齐id 
        var olds = db.GetCollection<TransferRecord>().Find(x => x.ConfirmedDate >= records.Min(x => x.ConfirmedDate));
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

        var dc = db.GetCollection<TransferRecord>();
        // 分类
        var da = records.GroupBy(x => (x.CustomerId, x.FundId)).Select(x => x.Key);
        foreach (var (c, f) in da)
        {
            var tf = dc.Find(x => x.CustomerId == c && x.FundId == f);
            var share = tf.Sum(x => x.ShareChange());
            var deposit = tf.Where(x => x.Type switch { TransferRecordType.Subscription or TransferRecordType.Purchase or TransferRecordType.MoveIn or TransferRecordType.SwitchIn or TransferRecordType.TransferIn => true, _ => false }).Sum(x => x.ConfirmedNetAmount);
            var withdraw = tf.Where(x => x.Type switch { TransferRecordType.Redemption or TransferRecordType.Redemption or TransferRecordType.MoveOut or TransferRecordType.SwitchOut or TransferRecordType.TransferOut or TransferRecordType.Distribution => true, _ => false }).Sum(x => x.ConfirmedNetAmount);

            db.GetCollection<InvestorBalance>().Upsert(new InvestorBalance { FundId = f, InvestorId = c, Share = share, Deposit = deposit, Withdraw = withdraw, Date = tf.Max(x => x.ConfirmedDate) });
        }

    }

}
public class ThreadSafeList<T> : IEnumerable<T>
{
    private readonly List<T> _innerList = new List<T>();
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    public delegate void CollectionChangedHandler();

    public CollectionChangedHandler? CollectionChanged;

    public void Add(T item)
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