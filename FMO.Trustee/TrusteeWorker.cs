using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using LiteDB;
using System.Diagnostics.CodeAnalysis;

namespace FMO.Trustee;

internal record FundTrusteePair(int FundId, ITrustee trustee);


/// <summary>
/// 执行结果
/// </summary>
/// <param name="Method"></param>
/// <param name="Returns"></param>
public record TrusteeWorkResult(string Method, IList<TrusteeWorker.WorkReturn> Returns);


/// <summary>
/// 已经获取的数据区间
/// </summary>
/// <param name="Id">Identifier+Method</param>
/// <param name="Begin"></param>
/// <param name="End"></param>
public record TrusteeMethodShotRange(string Id, DateOnly Begin, DateOnly End);

//public record TrusteeWorkRecord(string Identifier, string Method, DateOnly Begin, DateOnly End, int Count);


public partial class TrusteeWorker : ObservableObject
{
    public class WorkConfig
    {
        public WorkConfig()
        {
        }

        [SetsRequiredMembers]
        public WorkConfig(string id) => Id = id;

        public required string Id { get; set; }

        /// <summary>
        /// 间隔时间 分
        /// </summary>
        public int Interval { get; set; } = 15;

        /// <summary>
        /// 上次执行时间
        /// </summary>
        public DateTime Last { get; set; }


        [BsonIgnore]
        public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);

        public int GetLastRunIndex() => (int)(Last.Ticks / TimeSpan.TicksPerMinute / Interval);
    }



    public record WorkReturn(string Name, ReturnCode Code, object? Data = null);

    public const string TableRaisingBalance = "api_raising_balance";



    internal List<FundTrusteePair> Maps { get; } = new();

    private Timer timer;

    ITrustee[] Trustees { get; }

    private WorkConfig RaisingBalanceConfig { get; set; }

    private WorkConfig TransferRecordConfig { get; set; }

    private WorkConfig DailyFeeConfig { get; set; }

    public TrusteeWorker(ITrustee[] trustees)
    {
        WeakReferenceMessenger.Default.RegisterAll(this);

        timer = new Timer(OnTimer, null, Timeout.Infinite, 1000);

        WorkConfig[] cfg;
        using (var db = DbHelper.Platform())
            cfg = db.GetCollection<WorkConfig>().FindAll().ToArray();


        RaisingBalanceConfig = cfg.FirstOrDefault(x => x.Id == nameof(RaisingBalanceConfig)) ?? new(nameof(RaisingBalanceConfig));
        TransferRecordConfig = cfg.FirstOrDefault(x => x.Id == nameof(TransferRecordConfig)) ?? new(nameof(TransferRecordConfig));
        DailyFeeConfig = cfg.FirstOrDefault(x => x.Id == nameof(DailyFeeConfig)) ?? new(nameof(DailyFeeConfig)) { Interval = 60 * 24 }; // 每天一次



        Trustees = trustees;
        foreach (var t in trustees)
            t.Prepare();


        // 解析基金和api的映射
        //Fund[] funds;
        //using (var db = DbHelper.Base())
        //    funds = db.GetCollection<Fund>().FindAll().ToArray();

        //foreach (var f in funds)
        //{
        //    f.Trustee
        //}


    }


    /// <summary>
    /// 获取募集户余额
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task QueryRaisingBalanceOnce()
    {
        List<WorkReturn> ret = new();
        // 保存数据库
        using var db = DbHelper.Base();

        foreach (var tr in Trustees)
        {
            if (!tr.IsValid)
            {
                ret.Add(new(tr.Title, ReturnCode.ConfigInvalid));
                continue;
            }

            try
            {
                var rc = await tr.QueryRaisingBalance();

                ///
                // 保存数据库 
                if (rc.Data is not null)
                    db.GetCollection<FundBankBalance>().Upsert(rc.Data);

                ret.Add(new(tr.Title, rc.Code, rc.Data));
            }
            catch (Exception e)
            {
                ret.Add(new(tr.Title, ReturnCode.Unknown));
            }
        }

        // 保存ret，程序加载时恢复，并生成消息
        db.DropCollection(TableRaisingBalance);
        db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

        WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(nameof(ITrustee.QueryRaisingBalance), ret));
        RaisingBalanceConfig.Last = DateTime.Now;
        Save(RaisingBalanceConfig);
    }

    /// <summary>
    /// 获取交易确认记录
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task QueryTransferRecoredOnce()
    {
        List<WorkReturn> ret = new();
        // 保存数据库
        using var db = DbHelper.Base();
        var funds = db.GetCollection<Fund>().FindAll().ToArray();
        var manager = db.GetCollection<Manager>().FindById(1);

        using var pdb = DbHelper.Platform();
        var ranges = pdb.GetCollection<TrusteeMethodShotRange>().FindAll().ToArray();


        foreach (var tr in Trustees)
        {
            if (!tr.IsValid)
            {
                ret.Add(new(tr.Title, ReturnCode.ConfigInvalid));
                continue;
            }

            try
            {
                // 获取历史区间
                var range = ranges.FirstOrDefault(x => x.Id == tr.Identifier + nameof(tr.QueryTransferRecords));

                DateOnly begin = range?.End ?? new DateOnly(DateTime.Today.Year, 1, 1), end = DateOnly.FromDateTime(DateTime.Now);
                var rc = await tr.QueryTransferRecords(begin, end);

                ///
                // 保存数据库 
                if (rc.Data is not null)
                {
                    // 对齐数据   
                    foreach (var r in rc.Data)
                    {
                        if (r.Agency == manager.Name)
                            r.Agency = "直销";


                        // code 匹配
                        var f = funds.FirstOrDefault(x => x.Code == r.FundCode);
                        if (f is not null)
                        {
                            r.FundId = f.Id;
                            r.FundName = f.Name;
                            continue;
                        }

                        // 子份额


                        // 待完善
                    }

                    var customers = db.GetCollection<Investor>().FindAll().ToList();
                    foreach (var r in rc.Data)
                    {
                        var c = customers.FirstOrDefault(x => x.Name == r.CustomerName && x.Identity?.Id == r.CustomerIdentity);
                        if (c is not null)
                        {
                            r.CustomerId = c.Id;
                            continue;
                        }
                        else // 添加数据
                        {
                            c = new Investor { Name = r.CustomerName, Identity = new Identity { Id = r.CustomerIdentity } };
                            db.GetCollection<Investor>().Insert(c);
                            r.CustomerId = c.Id;
                        }
                    }

                    // 对齐id 
                    var olds = db.GetCollection<TransferRecord>().Find(x => x.ConfirmedDate >= rc.Data.Min(x => x.ConfirmedDate));
                    foreach (var r in rc.Data)
                    {
                        // 同日同名
                        var exi = olds.Where(x => x.CustomerName == r.CustomerName && x.CustomerIdentity == r.CustomerIdentity && x.ConfirmedDate == r.ConfirmedDate).ToList();

                        // 只有一个，替换
                        if (exi.Count == 1 && (exi[0].Source != "api" || exi[0].ExternalId == r.ExternalId))
                        {
                            r.Id = exi[0].Id;
                            continue;
                        }

                        // > 1个
                        // 存在同ex id，替换
                        var old = exi.Where(x => x.ExternalId == r.ExternalId);
                        if (old.Any())
                            r.Id = old.First().Id;

                        // 如果存在手动录入的，也删除
                        foreach (var item in exi)
                            db.GetCollection<TransferRecord>().DeleteMany(item => item.Source == "manual" || item.ExternalId == r.ExternalId);

                    }

                    db.GetCollection<TransferRecord>().Upsert(rc.Data);
                }

                if (range is null) range = new(tr.Identifier + nameof(tr.QueryTransferRecords), begin, end);
                else range = range with { End = end };


                // 如果有unset，表示数据异常，不保存进度
                if (!(rc.Data?.Any(x => x.CustomerName == "unset" || x.FundName == "unset" || x.CustomerIdentity == "unset") ?? false))
                    pdb.GetCollection<TrusteeMethodShotRange>().Upsert(range);

                // 合并记录
                ret.Add(new(tr.Title, rc.Code, rc.Data));
            }
            catch (Exception e)
            {
                ret.Add(new(tr.Title, ReturnCode.Unknown));
            }
        }

        // 保存ret，程序加载时恢复，并生成消息
        db.DropCollection(TableRaisingBalance);
        db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

        WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(nameof(ITrustee.QueryRaisingBalance), ret));
        RaisingBalanceConfig.Last = DateTime.Now;
        Save(RaisingBalanceConfig);

        DataTracker.CheckShareIsPair(funds);
    }

    /// <summary>
    /// 获取每日费用明细
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task QueryDailyFeeOnce()
    {
        List<WorkReturn> ret = new();
        // 保存数据库
        using var db = DbHelper.Base();
        var funds = db.GetCollection<Fund>().FindAll().ToArray();
        var StartDateOfAny = db.GetCollection<Fund>().FindAll().Min(x => x.SetupDate);

        using var pdb = DbHelper.Platform();
        var ranges = pdb.GetCollection<TrusteeMethodShotRange>().FindAll().ToArray();

        var method = nameof(ITrustee.QueryFundDailyFee);

        foreach (var tr in Trustees)
        {
            if (!tr.IsValid)
            {
                ret.Add(new(tr.Title, ReturnCode.ConfigInvalid));
                continue;
            }

            try
            {
                // 获取历史区间
                var range = ranges.FirstOrDefault(x => x.Id == tr.Identifier + method);

                DateOnly begin = range?.End ?? new DateOnly(DateTime.Today.Year, 1, 1), end = DateOnly.FromDateTime(DateTime.Now);

                do
                {
                    var rc = await tr.QueryFundDailyFee(begin, end);

                    ///
                    // 保存数据库 
                    if (rc.Data is not null)
                    {
                        // 对齐Fund
                        foreach (var f in rc.Data)
                        {
                            f.FundId = funds.FirstOrDefault(x => x.Code == f.FundCode)?.Id ?? 0;
                        }

                        db.GetCollection<FundDailyFee>().Upsert(rc.Data);
                    }

                    if (range is null) range = new(tr.Identifier + method, begin, end);
                    else range = range with { End = end };
                    pdb.GetCollection<TrusteeMethodShotRange>().Upsert(range);

                    // 合并记录
                    ret.Add(new(tr.Title, rc.Code, rc.Data));

                    // 向前一年
                    end = begin.AddDays(-1);
                    begin = end.AddYears(-1);
                } while (begin > StartDateOfAny);
            }
            catch (Exception e)
            {
                ret.Add(new(tr.Title, ReturnCode.Unknown));
            }
        }

        // 保存ret，程序加载时恢复，并生成消息
        db.DropCollection(TableRaisingBalance);
        db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

        WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(nameof(ITrustee.QueryRaisingBalance), ret));
        RaisingBalanceConfig.Last = DateTime.Now;
        Save(RaisingBalanceConfig);

    }




    [RelayCommand]
    public async Task QueryRaisingAccountTransctionOnce()
    {
        List<WorkReturn> ret = new();
        // 保存数据库
        using var db = DbHelper.Base();
        var funds = db.GetCollection<Fund>().FindAll().ToArray();

        using var pdb = DbHelper.Platform();
        var ranges = pdb.GetCollection<TrusteeMethodShotRange>().FindAll().ToArray();


        foreach (var tr in Trustees)
        {
            if (!tr.IsValid)
            {
                ret.Add(new(tr.Title, ReturnCode.ConfigInvalid));
                continue;
            }

            try
            {
                // 获取历史区间
                var range = ranges.FirstOrDefault(x => x.Id == tr.Identifier + nameof(tr.QueryTransferRecords));

                DateOnly begin = range?.End ?? new DateOnly(DateTime.Today.Year, 1, 1), end = DateOnly.FromDateTime(DateTime.Now);
                var rc = await tr.QueryTransferRecords(begin, end);

                ///
                // 保存数据库 
                if (rc.Data is not null)
                {
                    // 对齐数据   
                    foreach (var r in rc.Data)
                    {
                        // code 匹配
                        var f = funds.FirstOrDefault(x => x.Code == r.FundCode);
                        if (f is not null)
                        {
                            r.FundId = f.Id;
                            continue;
                        }

                        // 待完善

                    }

                    var customers = db.GetCollection<Investor>().FindAll().ToList();
                    foreach (var r in rc.Data)
                    {
                        var c = customers.FirstOrDefault(x => x.Name == r.CustomerName && x.Identity?.Id == r.CustomerIdentity);
                        if (c is not null)
                        {
                            r.CustomerId = c.Id;
                            continue;
                        }
                        else // 添加数据
                        {
                            c = new Investor { Name = r.CustomerName, Identity = new Identity { Id = r.CustomerIdentity } };
                            db.GetCollection<Investor>().Insert(c);
                            r.CustomerId = c.Id;
                        }
                    }

                    // 对齐id 
                    var olds = db.GetCollection<TransferRecord>().Find(x => x.ConfirmedDate >= rc.Data.Min(x => x.ConfirmedDate));
                    foreach (var r in rc.Data)
                    {
                        // 同日同名
                        var exi = olds.Where(x => x.CustomerName == r.CustomerName && x.CustomerIdentity == r.CustomerIdentity && x.ConfirmedDate == r.ConfirmedDate).ToList();

                        // 只有一个，替换
                        if (exi.Count == 1)
                        {
                            r.Id = exi[0].Id;
                            continue;
                        }

                        // > 1个
                        // 存在同ex id，替换
                        var old = exi.Where(x => x.ExternalId == r.ExternalId);
                        if (old.Any())
                            r.Id = old.First().Id;

                        // 如果存在手动录入的，也删除
                        foreach (var item in exi)
                            db.GetCollection<TransferRecord>().DeleteMany(item => item.Source == "manual" || item.ExternalId == r.ExternalId);

                    }

                    db.GetCollection<TransferRecord>().Upsert(rc.Data);
                }

                if (range is null) range = new(tr.Identifier + nameof(tr.QueryTransferRecords), begin, end);
                else range = range with { End = end };
                pdb.GetCollection<TrusteeMethodShotRange>().Upsert(range);

                // 合并记录
                ret.Add(new(tr.Title, rc.Code, rc.Data));
            }
            catch (Exception e)
            {
                ret.Add(new(tr.Title, ReturnCode.Unknown));
            }
        }

        // 保存ret，程序加载时恢复，并生成消息
        db.DropCollection(TableRaisingBalance);
        db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

        WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(nameof(ITrustee.QueryRaisingBalance), ret));
        RaisingBalanceConfig.Last = DateTime.Now;
        Save(RaisingBalanceConfig);

        DataTracker.CheckShareIsPair(funds);
    }











    private async void OnTimer(object? state)
    {
        var t = DateTime.Now;

        // 分钟位
        var minute = t.Ticks / TimeSpan.TicksPerMinute; //  new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, 0);
        if (minute % RaisingBalanceConfig.Interval == 0)
        {
            await RaisingBalanceConfig.Semaphore.WaitAsync();
            try
            {
                // 检验是否与上次运行时间不一样
                //if (t.Hour != RaisingBalanceConfig.Last.Hour || t.Minute != RaisingBalanceConfig.Last.Minute)
                if (minute / RaisingBalanceConfig.Interval != RaisingBalanceConfig.GetLastRunIndex())
                    await QueryRaisingBalanceOnce();
            }
            catch { }
            finally { RaisingBalanceConfig.Semaphore.Release(); }
        }
    }


    internal void Start() => timer.Change(0, 1000);

    private void Save(WorkConfig workConfig)
    {
        using var db = DbHelper.Platform();
        db.GetCollection<WorkConfig>().Upsert(workConfig);
    }

}
