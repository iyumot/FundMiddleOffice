using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using LiteDB;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace FMO.Trustee;

internal record FundTrusteePair(int FundId, ITrustee trustee);


/// <summary>
/// 执行结果
/// </summary>
/// <param name="Method"></param>
/// <param name="Returns"></param>
public record TrusteeWorkResult(string Method, IList<TrusteeWorker.WorkReturn> Returns);


//public record TrusteeWorkRecord(string Identifier, string Method, DateOnly Begin, DateOnly End, int Count);

/// <summary>
/// 产品对应api
/// </summary>
/// <param name="FundId"></param>
/// <param name="Identifier"></param>
public record TrusteeApiMap(int FundId, string Identifier);




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
    private WorkConfig TransferRequestConfig { get; set; }
    private WorkConfig RaisingAccountTransctionConfig { get; set; }



    private WorkConfig NetValueConfig { get; set; }



    (WorkConfig Config, IAsyncRelayCommand Command)[] tasks;

    public TrusteeWorker(ITrustee[] trustees)
    {
        WeakReferenceMessenger.Default.RegisterAll(this);

        timer = new Timer(OnTimer, null, Timeout.Infinite, 1000);

        WorkConfig[] cfg;
        TrusteeApiMap[] maps;
        using (var db = DbHelper.Platform())
        {
            cfg = db.GetCollection<WorkConfig>().FindAll().ToArray();
            maps = db.GetCollection<TrusteeApiMap>().FindAll().ToArray();
        }
        RaisingBalanceConfig = cfg.FirstOrDefault(x => x.Id == nameof(ITrustee.QueryRaisingBalance)) ?? new(nameof(ITrustee.QueryRaisingBalance));
        TransferRecordConfig = cfg.FirstOrDefault(x => x.Id == nameof(ITrustee.QueryTransferRecords)) ?? new(nameof(ITrustee.QueryTransferRecords)) { Interval = 60 }; // 每6个小时
        TransferRequestConfig = cfg.FirstOrDefault(x => x.Id == nameof(ITrustee.QueryTransferRequests)) ?? new(nameof(ITrustee.QueryTransferRequests));
        DailyFeeConfig = cfg.FirstOrDefault(x => x.Id == nameof(ITrustee.QueryFundDailyFee)) ?? new(nameof(ITrustee.QueryFundDailyFee)) { Interval = 60 * 12 }; // 每天一次
        RaisingAccountTransctionConfig = cfg.FirstOrDefault(x => x.Id == nameof(ITrustee.QueryRaisingAccountTransction)) ?? new(nameof(ITrustee.QueryRaisingAccountTransction));
        NetValueConfig = cfg.FirstOrDefault(x => x.Id == nameof(ITrustee.QueryNetValue)) ?? new(nameof(ITrustee.QueryNetValue)) { Interval = 60 }; // 每6个小时


        // 基金映射
        using (var db = DbHelper.Base())
        {
            var funds = db.GetCollection<Fund>().Query().Select(x => new { x.Id, x.Trustee }).ToList();
            var unk = funds.ExceptBy(maps.Select(x => x.FundId), x => x.Id);
            foreach (var item in unk)
            {
                if (trustees.FirstOrDefault(x => x.IsSuit(item.Trustee)) is ITrustee trustee)
                    Maps.Add(new(item.Id, trustee));
            }
            foreach (var item in maps)
            {
                if (trustees.FirstOrDefault(x => x.IsSuit(item.Identifier)) is ITrustee trustee)
                    Maps.Add(new(item.FundId, trustee));
            }
        }

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
        tasks = [
                // 募集余额查询任务：
                // 使用 RaisingBalanceConfig 配置（如执行间隔、上次运行时间等）
                // 触发 QueryRaisingBalanceOnceCommand 命令执行单次查询
                (Config: RaisingBalanceConfig, Command: QueryRaisingBalanceOnceCommand),

                // 募集户流水查询任务：
                // 使用 RaisingAccountTransctionConfig 配置
                // 触发 QueryRaisingAccountTransctionOnceCommand 命令执行单次查询
                (Config: RaisingAccountTransctionConfig, Command: QueryRaisingAccountTransctionOnceCommand),

                // 交易申请查询任务：
                // 使用 TransferRequestConfig 配置
                // 触发 QueryTransferRequestOnceCommand 命令执行单次查询
                (Config: TransferRequestConfig, Command: QueryTransferRequestOnceCommand),

                // 交易确认查询任务：
                // 使用 TransferRecordConfig 配置
                // 触发 QueryTransferRecordOnceCommand 命令执行单次查询
                (Config: TransferRecordConfig, Command: QueryTransferRecordOnceCommand),

                // 日常费用查询任务：
                // 使用 DailyFeeConfig 配置
                // 触发 QueryDailyFeeOnceCommand 命令执行单次查询
                (Config: DailyFeeConfig, Command: QueryDailyFeeOnceCommand),

                (Config: NetValueConfig, Command: QueryNetValueOnceCommand),
             ];
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
                Log.Error($"QueryRaisingBalanceOnce {e}");
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
    /// 获取交易申请记录
    /// 
    /// 中信不返回当日数据
    /// 
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task QueryTransferRequestOnce()
    {
        List<WorkReturn> ret = new();
        // 保存数据库
        using var db = DbHelper.Base();
        var funds = db.GetCollection<Fund>().FindAll().ToArray();
        var manager = db.GetCollection<Manager>().FindById(1);
        var StartDateOfAny = StartOfAnyWork();
        using var pdb = DbHelper.Platform();
        var ranges = pdb.GetCollection<TrusteeMethodShotRange>().FindAll().ToArray();
        var method = nameof(ITrustee.QueryTransferRequests);

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
                    var rc = await tr.QueryTransferRequests(begin, end);

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
                            else Log.Error($"QueryTransferRequests 发现未知的产品{r.FundName} {r.FundCode}");

                            // 子份额 在各api中完成

                        }

                        var customers = db.GetCollection<Investor>().FindAll().ToList();
                        foreach (var r in rc.Data)
                        {
                            // 此项可能存在重复Id的bug，不用name是因为名字中有（）-等，在不同情景下，全角半角不一样
                            var c = customers.FirstOrDefault(x => /*x.Name == r.CustomerName &&*/ x.Identity?.Id == r.CustomerIdentity);
                            if (c is null)
                            {
                                c = new Investor { Name = r.CustomerName, Identity = new Identity { Id = r.CustomerIdentity } };
                                db.GetCollection<Investor>().Insert(c);
                            }


                            // 添加数据 
                            r.CustomerId = c.Id;
                        }

                        // 对齐id 
                        var olds = db.GetCollection<TransferRequest>().Find(x => x.RequestDate >= rc.Data.Min(x => x.RequestDate));
                        foreach (var r in rc.Data)
                        {
                            // 同日同名
                            var exi = olds.Where(x => x.ExternalId == r.ExternalId || (x.CustomerName == r.CustomerName && x.CustomerIdentity == r.CustomerIdentity && x.RequestDate == r.RequestDate)).ToList();

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
                                db.GetCollection<TransferRequest>().DeleteMany(item => item.Source == "manual" || item.ExternalId == r.ExternalId);

                        }

                        db.GetCollection<TransferRequest>().Upsert(rc.Data);


                        // 统一更新处理
                        DataTracker.OnBatchTransferRequest(rc.Data);
                    }


                    // 如果有unset，表示数据异常，不保存进度
                    if (rc.Data?.Any(x => x.CustomerName == "unset" || x.FundName == "unset" || x.CustomerIdentity == "unset") ?? false)
                    {
                        ret.Add(new(tr.Title, ReturnCode.DataIsNotWellFormed, rc.Data));
                        WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Error, $"{tr.Title} 获取的交易申请记录数据异常"));
                        break;
                    }
                    // 更新进度
                    if (range is null) range = new(tr.Identifier + nameof(tr.QueryTransferRequests), begin, end);
                    else range.Merge(begin, end);
                    pdb.GetCollection<TrusteeMethodShotRange>().Upsert(range);

                    // 合并记录
                    ret.Add(new(tr.Title, rc.Code, rc.Data));

                    // 向前一年
                    end = range.Begin.AddDays(-1);
                    if (end.Year < 1970) break;
                    begin = end.AddYears(-1);
                } while (begin > StartDateOfAny);

            }
            catch (Exception e)
            {
                ret.Add(new(tr.Title, ReturnCode.Unknown));

                Log.Error($"QueryTransferRequestOnce {e}");
            }
        }

        // 保存ret，程序加载时恢复，并生成消息
        //db.DropCollection(TableRaisingBalance);
        //db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

        WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(method, ret));
        TransferRequestConfig.Last = DateTime.Now;
        Save(TransferRequestConfig);
    }



    /// <summary>
    /// 获取交易确认记录
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task QueryTransferRecordOnce()
    {
        List<WorkReturn> ret = new();
        // 保存数据库
        using var db = DbHelper.Base();
        var funds = db.GetCollection<Fund>().FindAll().ToArray();
        var manager = db.GetCollection<Manager>().FindById(1);
        var StartDateOfAny = StartOfAnyWork();

        using var pdb = DbHelper.Platform();
        var ranges = pdb.GetCollection<TrusteeMethodShotRange>().FindAll().ToArray();
        var method = nameof(ITrustee.QueryTransferRecords);


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
                            else Log.Error($"QueryTransferRequests 发现未知的产品{r.FundName} {r.FundCode}");

                            // 子份额 在各api中完成
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


                        DataTracker.OnBatchTransferRecord(rc.Data);
                    }



                    // 如果有unset，表示数据异常，不保存进度
                    if (rc.Data?.Any(x => x.CustomerName == "unset" || x.FundName == "unset" || x.CustomerIdentity == "unset") ?? false)
                        break;

                    // 更新进度
                    if (range is null) range = new(tr.Identifier + method, begin, end);
                    else range.Merge(begin, end);
                    pdb.GetCollection<TrusteeMethodShotRange>().Upsert(range);

                    // 合并记录
                    ret.Add(new(tr.Title, rc.Code, rc.Data));

                    // 向前一年
                    end = range.Begin.AddDays(-1);
                    if (end.Year < 1970) break;
                    begin = end.AddYears(-1);
                } while (begin > StartDateOfAny);
            }
            catch (Exception e)
            {
                ret.Add(new(tr.Title, ReturnCode.Unknown));

                Log.Error($"QueryTransferRecordOnce {e}");
            }
        }

        // 保存ret，程序加载时恢复，并生成消息
        //db.DropCollection(TableRaisingBalance);
        //db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

        WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(method, ret));
        TransferRecordConfig.Last = DateTime.Now;
        Save(TransferRecordConfig);
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
        var StartDateOfAny = StartOfAnyWork();
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
                    else range.Merge(begin, end);
                    pdb.GetCollection<TrusteeMethodShotRange>().Upsert(range);

                    // 合并记录
                    ret.Add(new(tr.Title, rc.Code, rc.Data));

                    // 向前一年
                    end = range.Begin.AddDays(-1);
                    if (end.Year < 1970) break;
                    begin = end.AddYears(-1);
                } while (begin > StartDateOfAny);
            }
            catch (Exception e)
            {
                ret.Add(new(tr.Title, ReturnCode.Unknown));
                Log.Error($"QueryDailyFeeOnce {e}");
            }
        }

        // 保存ret，程序加载时恢复，并生成消息
        //db.DropCollection(TableRaisingBalance);
        //db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

        WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(method, ret));
        DailyFeeConfig.Last = DateTime.Now;
        Save(DailyFeeConfig);

    }



    /// <summary>
    /// 获取募集户流水
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task QueryRaisingAccountTransctionOnce()
    {
        List<WorkReturn> ret = new();
        // 保存数据库
        using var db = DbHelper.Base();
        var funds = db.GetCollection<Fund>().FindAll().ToArray();
        var StartDateOfAny = StartOfAnyWork();
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
                var range = ranges.FirstOrDefault(x => x.Id == tr.Identifier + nameof(tr.QueryRaisingAccountTransction));

                DateOnly begin = range?.End ?? new DateOnly(DateTime.Today.Year, 1, 1), end = DateOnly.FromDateTime(DateTime.Now);
                do
                {
                    var rc = await tr.QueryRaisingAccountTransction(begin, end);

                    ///
                    // 保存数据库 
                    if (rc.Data is not null)
                    {
                        // 对齐数据   

                        db.GetCollection<RaisingBankTransaction>().Upsert(rc.Data);
                    }

                    if (range is null) range = new(tr.Identifier + nameof(tr.QueryRaisingAccountTransction), begin, end);
                    else range.Merge(begin, end);
                    pdb.GetCollection<TrusteeMethodShotRange>().Upsert(range);

                    // 合并记录
                    ret.Add(new(tr.Title, rc.Code, rc.Data));

                    // 向前一年
                    end = range.Begin.AddDays(-1);
                    if (end.Year < 1970) break;
                    begin = end.AddYears(-1);
                } while (begin > StartDateOfAny);
            }
            catch (Exception e)
            {
                ret.Add(new(tr.Title, ReturnCode.Unknown));
                Log.Error($"QueryRaisingAccountTransctionOnce {e}");
            }
        }

        // 保存ret，程序加载时恢复，并生成消息
        //db.DropCollection(TableRaisingBalance);
        //db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

        WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(nameof(ITrustee.QueryRaisingAccountTransction), ret));
        RaisingAccountTransctionConfig.Last = DateTime.Now;
        Save(RaisingAccountTransctionConfig);
    }

    /// <summary>
    /// 查询净值
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task QueryNetValueOnce()
    {
        List<WorkReturn> ret = new();
        // 保存数据库
        using var db = DbHelper.Base();
        var funds = db.GetCollection<Fund>().FindAll().ToArray();
        var StartDateOfAny = StartOfAnyWork();
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
                var range = ranges.FirstOrDefault(x => x.Id == tr.Identifier + nameof(tr.QueryNetValue));

                DateOnly begin = range?.End ?? new DateOnly(DateTime.Today.Year, 1, 1), end = DateOnly.FromDateTime(DateTime.Now);
                if (begin == end) begin = end.AddDays(-10);
                do
                {
                    var rc = await tr.QueryNetValue(begin, end);

                    ///
                    // 保存数据库 
                    if (rc.Data is not null)
                    {
                        foreach (var item in rc.Data.GroupBy(x => (x.FundId, x.Class)))
                        {
                            var fid = item.Key.FundId;
                            var c = item.Key.Class;
                            db.GetDailyCollection(fid, c).Upsert(item);
                        }
                    }

                    if (range is null) range = new(tr.Identifier + nameof(tr.QueryNetValue), begin, end);
                    else range.Merge(begin, end);
                    pdb.GetCollection<TrusteeMethodShotRange>().Upsert(range);

                    // 合并记录
                    ret.Add(new(tr.Title, rc.Code, rc.Data));

                    // 向前一年
                    end = range.Begin.AddDays(-1);
                    if (end.Year < 1970) break;
                    begin = end.AddYears(-1);
                } while (begin > StartDateOfAny);
            }
            catch (Exception e)
            {
                ret.Add(new(tr.Title, ReturnCode.Unknown));
                Log.Error($"QueryNetValueOnce {e}");
            }
        }

        // 保存ret，程序加载时恢复，并生成消息
        //db.DropCollection(TableRaisingBalance);
        //db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

        WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(nameof(ITrustee.QueryNetValue), ret));
        NetValueConfig.Last = DateTime.Now;
        Save(NetValueConfig);
    }

    public async Task QueryNetValueOnce(int fundId, string code, DateOnly begin, DateOnly end)
    {
        if (Maps.LastOrDefault(x => x.FundId == fundId) is FundTrusteePair pair)
        {
            var rc = await pair.trustee.QueryNetValue(begin, end, code);

            ///
            // 保存数据库 
            if (rc.Data is not null)
            {
                using var db = DbHelper.Base();
                foreach (var item in rc.Data.GroupBy(x => (x.FundId, x.Class)))
                {
                    var fid = item.Key.FundId;
                    var c = item.Key.Class;
                    db.GetDailyCollection(fid, c).Upsert(item);
                }
            }
        }
        else WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Warning, "未发现对应的API"));
    }


    [RelayCommand]
    public void Rebuild(string method)
    {
        using (var pdf = DbHelper.Platform()) //删除记录
            pdf.GetCollection("TrusteeMethodShotRange").Delete(method);
    }




    private async void OnTimer2(object? state)
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
                    await QueryRaisingBalanceOnceCommand.ExecuteAsync(null);
            }
            catch { }
            finally { RaisingBalanceConfig.Semaphore.Release(); }
        }

        // 募集户流水
        if (minute % RaisingAccountTransctionConfig.Interval == 0)
        {
            await RaisingAccountTransctionConfig.Semaphore.WaitAsync();
            try
            {
                if (minute / RaisingAccountTransctionConfig.Interval != RaisingAccountTransctionConfig.GetLastRunIndex())
                    await QueryRaisingAccountTransctionOnceCommand.ExecuteAsync(null);
            }
            catch { }
            finally { RaisingAccountTransctionConfig.Semaphore.Release(); }
        }

        // 交易申请 
        if (minute % TransferRequestConfig.Interval == 0)
        {
            await TransferRequestConfig.Semaphore.WaitAsync();
            try
            {
                if (minute / TransferRequestConfig.Interval != TransferRequestConfig.GetLastRunIndex())
                    await QueryTransferRequestOnceCommand.ExecuteAsync(null);
            }
            catch { }
            finally { TransferRequestConfig.Semaphore.Release(); }
        }

        // 交易确认
        if (minute % TransferRecordConfig.Interval == 0)
        {
            await TransferRecordConfig.Semaphore.WaitAsync();
            try
            {
                if (minute / TransferRecordConfig.Interval != TransferRecordConfig.GetLastRunIndex())
                    await QueryTransferRecordOnceCommand.ExecuteAsync(null);
            }
            catch { }
            finally { TransferRecordConfig.Semaphore.Release(); }
        }


        // 费用
        if (minute % DailyFeeConfig.Interval == 0)
        {
            await DailyFeeConfig.Semaphore.WaitAsync();
            try
            {
                if (minute / DailyFeeConfig.Interval != DailyFeeConfig.GetLastRunIndex())
                    await QueryDailyFeeOnceCommand.ExecuteAsync(null);
            }
            catch { }
            finally { DailyFeeConfig.Semaphore.Release(); }
        }
    }

    private async void OnTimer(object? state)
    {
        var now = DateTime.Now;
        var minuteIndex = now.Ticks / TimeSpan.TicksPerMinute;



        // 是否非工作时间 8-19点
        bool offwork = (now.Hour < 8 || now.Hour >= 19);


        foreach (var (Config, Command) in tasks)
        {
            var interval = offwork && 60 > Config.Interval ? 60 : Config.Interval;

            if (minuteIndex % interval == 0)
            {
                await Config.Semaphore.WaitAsync();
                try
                {
                    if (minuteIndex / Config.Interval != Config.GetLastRunIndex())
                    {
                        // 调度到 UI 线程执行
                        await Application.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            if (Command.CanExecute(null))
                                await Command.ExecuteAsync(null);
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Trustee Worker Error executing command: {ex.Message}");
                }
                finally
                {
                    Config.Semaphore.Release();
                }
            }
        }
    }

    internal void Start() => timer.Change(0, 15000);

    private void Save(WorkConfig workConfig)
    {
        using var db = DbHelper.Platform();
        db.GetCollection<WorkConfig>().Upsert(workConfig);
    }


    private DateOnly StartOfAnyWork()
    {
        using var db = DbHelper.Base();
        var dates = db.GetCollection<Fund>().Query().Select(x => x.SetupDate).ToList();
        dates.Add(db.GetCollection<Manager>().FindById(1).SetupDate);

        return dates.Where(x => x != default).Min();
    }
}
