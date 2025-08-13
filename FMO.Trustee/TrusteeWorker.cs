using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using LiteDB;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FMO.Trustee;

internal record FundTrusteePair(int FundId, ITrustee trustee);


/// <summary>
/// ִ�н��
/// </summary>
/// <param name="Method"></param>
/// <param name="Returns"></param>
public record TrusteeWorkResult(string Method, IList<TrusteeWorker.WorkReturn> Returns);


//public record TrusteeWorkRecord(string Identifier, string Method, DateOnly Begin, DateOnly End, int Count);

/// <summary>
/// ��Ʒ��Ӧapi
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
        /// ���ʱ�� ��
        /// </summary>
        public int Interval { get; set; } = 15;

        /// <summary>
        /// �ϴ�ִ��ʱ��
        /// </summary>
        public DateTime Last { get; set; }


        [BsonIgnore]
        public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);

        public int GetLastRunIndex() => (int)(Last.Ticks / TimeSpan.TicksPerMinute / Interval);
    }



    public record WorkReturn(string Name, ReturnCode Code, object? Data = null);

    public const string TableRaisingBalance = "api_raising_balance";



    internal List<FundTrusteePair> Maps { get; } = new();

    private Dictionary<string, TrusteeMethodShotRange> _workRange = new();

    private DateOnly StartOfAny { get; }

    //private PeriodicTimer periodicTimer;

    ITrustee[] Trustees { get; }

    private WorkConfig RaisingBalanceConfig { get; set; }

    private WorkConfig TransferRecordConfig { get; set; }

    private WorkConfig DailyFeeConfig { get; set; }
    private WorkConfig TransferRequestConfig { get; set; }
    private WorkConfig RaisingAccountTransctionConfig { get; set; }



    private WorkConfig NetValueConfig { get; set; }



    (WorkConfig Config, Func<Task> Command)[] tasks;

    public TrusteeWorker(ITrustee[] trustees)
    {
        WeakReferenceMessenger.Default.RegisterAll(this);


        WorkConfig[] cfg;
        TrusteeApiMap[] maps;
        using (var db = DbHelper.Platform())
        {
            cfg = db.GetCollection<WorkConfig>().FindAll().ToArray();
            maps = db.GetCollection<TrusteeApiMap>().FindAll().ToArray();
            _workRange = db.GetCollection<TrusteeMethodShotRange>().FindAll().ToDictionary(x => x.Id);
        }
        RaisingBalanceConfig = cfg.FirstOrDefault(x => x.Id == nameof(ITrustee.QueryRaisingBalance)) ?? new(nameof(ITrustee.QueryRaisingBalance));
        TransferRecordConfig = cfg.FirstOrDefault(x => x.Id == nameof(ITrustee.QueryTransferRecords)) ?? new(nameof(ITrustee.QueryTransferRecords)) { Interval = 60 }; // ÿ6��Сʱ
        TransferRequestConfig = cfg.FirstOrDefault(x => x.Id == nameof(ITrustee.QueryTransferRequests)) ?? new(nameof(ITrustee.QueryTransferRequests));
        DailyFeeConfig = cfg.FirstOrDefault(x => x.Id == nameof(ITrustee.QueryFundDailyFee)) ?? new(nameof(ITrustee.QueryFundDailyFee)) { Interval = 60 * 12 }; // ÿ��һ��
        RaisingAccountTransctionConfig = cfg.FirstOrDefault(x => x.Id == nameof(ITrustee.QueryRaisingAccountTransction)) ?? new(nameof(ITrustee.QueryRaisingAccountTransction));
        NetValueConfig = cfg.FirstOrDefault(x => x.Id == nameof(ITrustee.QueryNetValue)) ?? new(nameof(ITrustee.QueryNetValue)) { Interval = 60 }; // ÿ6��Сʱ


        RaisingBalanceConfig.Interval = 15;
        TransferRecordConfig.Interval = 60;
        TransferRequestConfig.Interval = 60;
        DailyFeeConfig.Interval = 60 * 12;
        RaisingAccountTransctionConfig.Interval = 15;
        NetValueConfig.Interval = 60;


        // ����ӳ��
        using (var db = DbHelper.Base())
        {
            var funds = db.GetCollection<Fund>().Query().Select(x => new { x.Id, x.SetupDate, x.Trustee }).ToList();
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

            // �����������С����
            StartOfAny = db.GetCollection<Manager>().Query().First().SetupDate;
            // ��ֹ�ռ���
            try { StartOfAny = funds.Where(x => x.SetupDate.Year > 1970).Min(x => x.SetupDate); } catch { }

        }

        Trustees = trustees;
        foreach (var t in trustees)
            t.Prepare();


        tasks = [
                // ļ������ѯ����
                // ʹ�� RaisingBalanceConfig ���ã���ִ�м�����ϴ�����ʱ��ȣ�
                // ���� QueryRaisingBalanceOnceCommand ����ִ�е��β�ѯ
                (Config: RaisingBalanceConfig, Command: QueryRaisingBalanceOnce),

                // ļ������ˮ��ѯ����
                // ʹ�� RaisingAccountTransctionConfig ����
                // ���� QueryRaisingAccountTransctionOnceCommand ����ִ�е��β�ѯ
                (Config: RaisingAccountTransctionConfig, Command: QueryRaisingAccountTransctionOnce),

                // ���������ѯ����
                // ʹ�� TransferRequestConfig ����
                // ���� QueryTransferRequestOnceCommand ����ִ�е��β�ѯ
                (Config: TransferRequestConfig, Command: QueryTransferRequestOnce),

                // ����ȷ�ϲ�ѯ����
                // ʹ�� TransferRecordConfig ����
                // ���� QueryTransferRecordOnceCommand ����ִ�е��β�ѯ
                (Config: TransferRecordConfig, Command: QueryTransferRecordOnce),

                // �ճ����ò�ѯ����
                // ʹ�� DailyFeeConfig ����
                // ���� QueryDailyFeeOnceCommand ����ִ�е��β�ѯ
                (Config: DailyFeeConfig, Command: QueryDailyFeeOnce),

                (Config: NetValueConfig, Command: QueryNetValueOnce),
             ];
    }

    #region Impl

    /// <summary>
    /// ��ȡļ�������
    /// </summary>
    /// <returns></returns>
    private async Task QueryRaisingBalanceImpl()
    {
        try
        {
            List<WorkReturn> ret = new();
            // �������ݿ�
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
                    // �������ݿ� 
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

            // ����ret���������ʱ�ָ�����������Ϣ
            db.DropCollection(TableRaisingBalance);
            db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

            WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(nameof(ITrustee.QueryRaisingBalance), ret));
            RaisingBalanceConfig.Last = DateTime.Now;
            Save(RaisingBalanceConfig);
        }
        catch (Exception e)
        {
            Log.Error($"{nameof(QueryRaisingBalanceOnce)} {e.Message}");
        }
    }



    /// <summary>
    /// ��ȡ���������¼
    /// 
    /// ���Ų����ص�������
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task QueryTransferRequestImpl()
    {
        try
        {
            List<WorkReturn> ret = new();
            // �������ݿ�
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
                    // ��ȡ��ʷ����
                    var range = GetWorkedRange(tr.Identifier, method);

                    DateOnly begin = range.End, end = DateOnly.FromDateTime(DateTime.Now);

                    var rc = await tr.QueryTransferRequests(begin, end);
                    if (rc.Code != ReturnCode.Success && rc.Code != ReturnCode.TrafficLimit)
                    {
                        WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Error, $"{tr.Title} ��ȡ�Ľ��������¼�����쳣"));
                    }
                    ///
                    // �������ݿ⣬���ֳɹ���Ҳ����
                    if (rc.Data?.Count > 0)
                    {
                        // ���������ʧ�ܵģ�����end
                        if (rc.Code != ReturnCode.Success)
                            end = rc.Data.Max(x => x.RequestDate);

                        // ͳһ���´���
                        DataTracker.OnBatchTransferRequest(rc.Data);

                        // �����unset����ʾ�����쳣�����������
                        if (rc.Data?.Any(x => x.InvestorName == "unset" || x.FundName == "unset" || x.InvestorIdentity == "unset") ?? false)
                        {
                            ret.Add(new(tr.Title, ReturnCode.DataIsNotWellFormed, rc.Data));
                            WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Error, $"{tr.Title} ��ȡ�Ľ��������¼�����쳣"));
                            break;
                        }
                    }

                    // ���½���
                    range.Merge(begin, end);
                    using var pdb = DbHelper.Platform();
                    pdb.GetCollection<TrusteeMethodShotRange>().Upsert(range);

                }
                catch (Exception e)
                {
                    ret.Add(new(tr.Title, ReturnCode.Unknown));

                    Log.Error($"QueryTransferRequestOnce {e}");
                }
            }

            // ����ret���������ʱ�ָ�����������Ϣ
            //db.DropCollection(TableRaisingBalance);
            //db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

            WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(method, ret));
            TransferRequestConfig.Last = DateTime.Now;
            Save(TransferRequestConfig);
        }
        catch (Exception e)
        {
            Log.Error($"{nameof(QueryTransferRequestOnce)} {e.Message}");
        }
    }



    /// <summary>
    /// ��ȡ����ȷ�ϼ�¼
    /// </summary>
    /// <returns></returns>
    private async Task QueryTransferRecordImpl()
    {
        try
        {
            List<WorkReturn> ret = new();
            // �������ݿ�
            using var db = DbHelper.Base();
            var funds = db.GetCollection<Fund>().FindAll().ToArray(); 
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
                    // ��ȡ��ʷ����
                    var range = GetWorkedRange(tr.Identifier, method);

                    DateOnly begin = range.End, end = DateOnly.FromDateTime(DateTime.Now);

                    var rc = await tr.QueryTransferRecords(begin, end);

                    ///
                    // �������ݿ� 
                    if (rc.Data?.Count > 0)
                    {
                        DataTracker.OnBatchTransferRecord(rc.Data);
                    }

                    // �����unset����ʾ�����쳣�����������
                    if (rc.Data?.Any(x => x.InvestorName == "unset" || x.FundName == "unset" || x.InvestorIdentity == "unset") ?? false)
                        break;

                    // ���½���
                    range.Merge(begin, end);
                    using var pdb = DbHelper.Platform();
                    pdb.GetCollection<TrusteeMethodShotRange>().Upsert(range);

                    // �ϲ���¼
                    ret.Add(new(tr.Title, rc.Code, rc.Data));

                }
                catch (Exception e)
                {
                    ret.Add(new(tr.Title, ReturnCode.Unknown));

                    Log.Error($"QueryTransferRecordOnce {e}");
                }
            }

            // ����ret���������ʱ�ָ�����������Ϣ
            //db.DropCollection(TableRaisingBalance);
            //db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

            WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(method, ret));
            TransferRecordConfig.Last = DateTime.Now;
            Save(TransferRecordConfig);
        }
        catch (Exception e)
        {
            Log.Error($"{nameof(QueryTransferRecordOnce)} {e.Message}");
        }
    }

    /// <summary>
    /// ��ȡÿ�շ�����ϸ
    /// </summary>
    /// <returns></returns>
    private async Task QueryDailyFeeImpl()
    {
        try
        {
            List<WorkReturn> ret = new();
            // �������ݿ�
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
                    // ��ȡ��ʷ����
                    var range = ranges.FirstOrDefault(x => x.Id == tr.Identifier + method);

                    DateOnly begin = range?.End ?? new DateOnly(DateTime.Today.Year, 1, 1), end = DateOnly.FromDateTime(DateTime.Now);

                    do
                    {
                        var rc = await tr.QueryFundDailyFee(begin, end);

                        ///
                        // �������ݿ� 
                        if (rc.Data is not null)
                        {
                            // ����Fund
                            foreach (var f in rc.Data)
                            {
                                f.FundId = funds.FirstOrDefault(x => x.Code == f.FundCode)?.Id ?? 0;
                            }

                            db.GetCollection<FundDailyFee>().Upsert(rc.Data);
                        }

                        if (range is null) range = new(tr.Identifier + method, begin, end);
                        else range.Merge(begin, end);
                        pdb.GetCollection<TrusteeMethodShotRange>().Upsert(range);

                        // �ϲ���¼
                        ret.Add(new(tr.Title, rc.Code, rc.Data));

                        // ��ǰһ��
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

            // ����ret���������ʱ�ָ�����������Ϣ
            //db.DropCollection(TableRaisingBalance);
            //db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

            WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(method, ret));
            DailyFeeConfig.Last = DateTime.Now;
            Save(DailyFeeConfig);
        }
        catch (Exception e)
        {
            Log.Error($"{nameof(QueryDailyFeeOnce)} {e.Message}");
        }
    }


    /// <summary>
    /// ��ȡļ������ˮ
    /// </summary>
    /// <returns></returns>
    private async Task QueryRaisingAccountTransctionImpl()
    {
        List<WorkReturn> ret = new();
        // �������ݿ�
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
                // ��ȡ��ʷ����
                var range = ranges.FirstOrDefault(x => x.Id == tr.Identifier + nameof(tr.QueryRaisingAccountTransction));

                DateOnly begin = range?.End ?? new DateOnly(DateTime.Today.Year, 1, 1), end = DateOnly.FromDateTime(DateTime.Now);
                do
                {
                    var rc = await tr.QueryRaisingAccountTransction(begin, end);

                    ///
                    // �������ݿ� 
                    if (rc.Data is not null)
                    {
                        // ��������   

                        db.GetCollection<RaisingBankTransaction>().Upsert(rc.Data);
                    }

                    if (range is null) range = new(tr.Identifier + nameof(tr.QueryRaisingAccountTransction), begin, end);
                    else range.Merge(begin, end);
                    pdb.GetCollection<TrusteeMethodShotRange>().Upsert(range);

                    // �ϲ���¼
                    ret.Add(new(tr.Title, rc.Code, rc.Data));

                    // ��ǰһ��
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

        // ����ret���������ʱ�ָ�����������Ϣ
        //db.DropCollection(TableRaisingBalance);
        //db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

        WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(nameof(ITrustee.QueryRaisingAccountTransction), ret));
        RaisingAccountTransctionConfig.Last = DateTime.Now;
        Save(RaisingAccountTransctionConfig);
    }



    /// <summary>
    /// ��ѯ��ֵ
    /// </summary>
    /// <returns></returns>

    private async Task QueryNetValueImpl()
    {
        using var db = DbHelper.Base();
        var funds = db.GetCollection<Fund>().Query().Select(x => new { x.Id, x.Code, x.SetupDate, x.ClearDate, x.LastUpdate, x.Status }).ToList();

        // �����̵�
        foreach (var fund in funds.Where(x => x.Status > FundStatus.StartLiquidation))
        {

        }
    }
    #endregion

    /// <summary>
    /// ��ȡļ������ˮ
    /// </summary>
    /// <returns></returns>
    public async Task QueryRaisingAccountTransctionOnce() => await RunTask(QueryRaisingAccountTransctionImpl());

    /// <summary>
    /// ��ȡ����ȷ�ϼ�¼
    /// </summary>
    /// <returns></returns>
    public async Task QueryTransferRecordOnce() => await RunTask(QueryTransferRecordImpl());

    /// <summary>
    /// ��ȡÿ�շ�����ϸ
    /// </summary>
    /// <returns></returns>
    public async Task QueryDailyFeeOnce() => await RunTask(QueryDailyFeeImpl());

    /// <summary>
    /// ��ȡ���������¼
    /// 
    /// ���Ų����ص�������
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task QueryTransferRequestOnce() => await RunTask(QueryTransferRequestImpl());

    /// <summary>
    /// ��ȡļ�������
    /// </summary>
    /// <returns></returns>
    public async Task QueryRaisingBalanceOnce() => await RunTask(QueryRaisingBalanceImpl());


    /// <summary>
    /// ��ѯ��ֵ
    /// </summary>
    /// <returns></returns>
    public async Task QueryNetValueOnce() => await RunTask(QueryNetValueImpl());

    public async Task QueryNetValueOnce(int fundId, string code, DateOnly begin, DateOnly end)
    {
        if (Maps.LastOrDefault(x => x.FundId == fundId) is FundTrusteePair pair)
        {
            var rc = await pair.trustee.QueryNetValue(begin, end, code);

            ///
            // �������ݿ� 
            if (rc.Data is not null)
            {
                DataTracker.OnDailyValue(rc.Data);
            }
        }
        else WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Warning, "δ���ֶ�Ӧ��API"));
    }


    private async Task RunTask(Task task, [CallerMemberName] string name = "")
    {
        WeakReferenceMessenger.Default.Send(new TrusteeRunMessage(name, true));

        try { await task; } catch (Exception e) { Log.Error($"{name} {e.Message}"); }

        WeakReferenceMessenger.Default.Send(new TrusteeRunMessage(name, false));
    }


    [RelayCommand]
    public void Rebuild(string method)
    {
        using (var pdf = DbHelper.Platform()) //ɾ����¼
            pdf.GetCollection("TrusteeMethodShotRange").Delete(method);
    }


    private TrusteeMethodShotRange GetWorkedRange(string idf, string method)
    {
        string key = $"{idf}.{method}";
        return _workRange.TryGetValue(key, out var range) ? range : new(key, StartOfAny, StartOfAny);
    }


    //private async void OnTimer2(object? state)
    //{
    //    var t = DateTime.Now;

    //    // ����λ
    //    var minute = t.Ticks / TimeSpan.TicksPerMinute; //  new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, 0);
    //    if (minute % RaisingBalanceConfig.Interval == 0)
    //    {
    //        await RaisingBalanceConfig.Semaphore.WaitAsync();
    //        try
    //        {
    //            // �����Ƿ����ϴ�����ʱ�䲻һ��
    //            //if (t.Hour != RaisingBalanceConfig.Last.Hour || t.Minute != RaisingBalanceConfig.Last.Minute)
    //            if (minute / RaisingBalanceConfig.Interval != RaisingBalanceConfig.GetLastRunIndex())
    //                await QueryRaisingBalanceOnceCommand.ExecuteAsync(null);
    //        }
    //        catch { }
    //        finally { RaisingBalanceConfig.Semaphore.Release(); }
    //    }

    //    // ļ������ˮ
    //    if (minute % RaisingAccountTransctionConfig.Interval == 0)
    //    {
    //        await RaisingAccountTransctionConfig.Semaphore.WaitAsync();
    //        try
    //        {
    //            if (minute / RaisingAccountTransctionConfig.Interval != RaisingAccountTransctionConfig.GetLastRunIndex())
    //                await QueryRaisingAccountTransctionOnceCommand.ExecuteAsync(null);
    //        }
    //        catch { }
    //        finally { RaisingAccountTransctionConfig.Semaphore.Release(); }
    //    }

    //    // �������� 
    //    if (minute % TransferRequestConfig.Interval == 0)
    //    {
    //        await TransferRequestConfig.Semaphore.WaitAsync();
    //        try
    //        {
    //            if (minute / TransferRequestConfig.Interval != TransferRequestConfig.GetLastRunIndex())
    //                await QueryTransferRequestOnceCommand.ExecuteAsync(null);
    //        }
    //        catch { }
    //        finally { TransferRequestConfig.Semaphore.Release(); }
    //    }

    //    // ����ȷ��
    //    if (minute % TransferRecordConfig.Interval == 0)
    //    {
    //        await TransferRecordConfig.Semaphore.WaitAsync();
    //        try
    //        {
    //            if (minute / TransferRecordConfig.Interval != TransferRecordConfig.GetLastRunIndex())
    //                await QueryTransferRecordOnceCommand.ExecuteAsync(null);
    //        }
    //        catch { }
    //        finally { TransferRecordConfig.Semaphore.Release(); }
    //    }


    //    // ����
    //    if (minute % DailyFeeConfig.Interval == 0)
    //    {
    //        await DailyFeeConfig.Semaphore.WaitAsync();
    //        try
    //        {
    //            if (minute / DailyFeeConfig.Interval != DailyFeeConfig.GetLastRunIndex())
    //                await QueryDailyFeeOnceCommand.ExecuteAsync(null);
    //        }
    //        catch { }
    //        finally { DailyFeeConfig.Semaphore.Release(); }
    //    }
    //}

    private async void LoopOnce()
    {
        var now = DateTime.Now;
        var minuteIndex = now.Ticks / TimeSpan.TicksPerMinute;

        // �Ƿ�ǹ���ʱ�� 8-19��
        bool offwork = (now.Hour < 8 || now.Hour >= 19);


        foreach (var (Config, Task) in tasks)
        {
            var interval = offwork && 60 > Config.Interval ? 60 : Config.Interval;

            if (minuteIndex % interval == 0)
            {
                await Config.Semaphore.WaitAsync();
                try
                {
                    if (minuteIndex / Config.Interval != Config.GetLastRunIndex())
                    {
                        await Task();
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

    internal void Start()
    {
        Task.Run(async () =>
        {
            using var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            while (await periodicTimer.WaitForNextTickAsync())
                LoopOnce();
        });
    }
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
