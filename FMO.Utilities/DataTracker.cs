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
        // 验证最新的净值中份额与share是否一致
        using var db = DbHelper.Base();

        foreach (var fund in funds)
        {
            var last = db.GetDailyCollection(fund.Id).Find(x => x.NetValue > 0).MaxBy(x => x.Date);
            if (last is null) continue;

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
                continue;
            }

            var sh = c.OrderBy(x => x.Date).LastOrDefault(x => x.Date < last.Date);

            if (sh?.Share != last.Share)
            {
                FundTips.Add(new FundTip(fund.Id, fund.Name, TipType.FundShareNotPair, "基金份额与估值表不一致"));
                continue;
            }

            FundTips.Remove(x => x.Type == TipType.FundShareNotPair);
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
        // 验证最新的净值中份额与share是否一致
        using var db = DbHelper.Base();
        var cur = DateOnly.FromDateTime(DateTime.Today);

        var coll = db.GetCollection<FundElements>();
        foreach (var fund in funds.Where(x=>x.Status == FundStatus.Normal || x.Status == FundStatus.StartLiquidation))
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