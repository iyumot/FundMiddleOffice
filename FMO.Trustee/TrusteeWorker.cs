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
/// ִ�н��
/// </summary>
/// <param name="Method"></param>
/// <param name="Returns"></param>
public record TrusteeWorkResult(string Method, IList<TrusteeWorker.WorkReturn> Returns);


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

    private Timer timer;

    ITrustee[] Trustees { get; }

    private WorkConfig RaisingBalanceConfig { get; set; }

    private WorkConfig TransferRecordConfig { get; set; }

    private WorkConfig DailyFeeConfig { get; set; }
    private WorkConfig TransferRequestConfig { get; set; }
    private WorkConfig RaisingAccountTransctionConfig { get; set; }



    public TrusteeWorker(ITrustee[] trustees)
    {
        WeakReferenceMessenger.Default.RegisterAll(this);

        timer = new Timer(OnTimer, null, Timeout.Infinite, 1000);

        WorkConfig[] cfg;
        using (var db = DbHelper.Platform())
            cfg = db.GetCollection<WorkConfig>().FindAll().ToArray();


        RaisingBalanceConfig = cfg.FirstOrDefault(x => x.Id == nameof(RaisingBalanceConfig)) ?? new(nameof(RaisingBalanceConfig));
        TransferRecordConfig = cfg.FirstOrDefault(x => x.Id == nameof(TransferRecordConfig)) ?? new(nameof(TransferRecordConfig)) { Interval = 60 * 6 }; // ÿ6��Сʱ
        TransferRequestConfig = cfg.FirstOrDefault(x => x.Id == nameof(TransferRequestConfig)) ?? new(nameof(TransferRequestConfig));
        DailyFeeConfig = cfg.FirstOrDefault(x => x.Id == nameof(DailyFeeConfig)) ?? new(nameof(DailyFeeConfig)) { Interval = 60 * 24 }; // ÿ��һ��
        RaisingAccountTransctionConfig = cfg.FirstOrDefault(x => x.Id == nameof(RaisingAccountTransctionConfig)) ?? new(nameof(RaisingAccountTransctionConfig));


        Trustees = trustees;
        foreach (var t in trustees)
            t.Prepare();


        // ���������api��ӳ��
        //Fund[] funds;
        //using (var db = DbHelper.Base())
        //    funds = db.GetCollection<Fund>().FindAll().ToArray();

        //foreach (var f in funds)
        //{
        //    f.Trustee
        //}


    }


    /// <summary>
    /// ��ȡļ�������
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task QueryRaisingBalanceOnce()
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
            }
        }

        // ����ret���������ʱ�ָ�����������Ϣ
        db.DropCollection(TableRaisingBalance);
        db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

        WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(nameof(ITrustee.QueryRaisingBalance), ret));
        RaisingBalanceConfig.Last = DateTime.Now;
        Save(RaisingBalanceConfig);
    }



    /// <summary>
    /// ��ȡ���������¼
    /// 
    /// ���Ų����ص�������
    /// 
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task QueryTransferRequestOnce()
    {
        List<WorkReturn> ret = new();
        // �������ݿ�
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
                // ��ȡ��ʷ����
                var range = ranges.FirstOrDefault(x => x.Id == tr.Identifier + method);

                DateOnly begin = range?.End ?? new DateOnly(DateTime.Today.Year, 1, 1), end = DateOnly.FromDateTime(DateTime.Now);
                do
                {
                    var rc = await tr.QueryTransferRequests(begin, end);

                    ///
                    // �������ݿ� 
                    if (rc.Data is not null)
                    {
                        // ��������   
                        foreach (var r in rc.Data)
                        {
                            if (r.Agency == manager.Name)
                                r.Agency = "ֱ��";


                            // code ƥ��
                            var f = funds.FirstOrDefault(x => x.Code == r.FundCode);
                            if (f is not null)
                            {
                                r.FundId = f.Id;
                                r.FundName = f.Name;
                                continue;
                            }
                            else Log.Error($"QueryTransferRequests ����δ֪�Ĳ�Ʒ{r.FundName} {r.FundCode}");

                            // �ӷݶ� �ڸ�api�����

                        }

                        var customers = db.GetCollection<Investor>().FindAll().ToList();
                        foreach (var r in rc.Data)
                        {
                            // ������ܴ����ظ�Id��bug������name����Ϊ�������У���-�ȣ��ڲ�ͬ�龰�£�ȫ�ǰ�ǲ�һ��
                            var c = customers.FirstOrDefault(x => /*x.Name == r.CustomerName &&*/ x.Identity?.Id == r.CustomerIdentity);
                            if (c is null)
                            {
                                c = new Investor { Name = r.CustomerName, Identity = new Identity { Id = r.CustomerIdentity } };
                                db.GetCollection<Investor>().Insert(c);
                            }


                            // ������� 
                            r.CustomerId = c.Id;
                        }

                        // ����id 
                        var olds = db.GetCollection<TransferRequest>().Find(x => x.RequestDate >= rc.Data.Min(x => x.RequestDate));
                        foreach (var r in rc.Data)
                        {
                            // ͬ��ͬ��
                            var exi = olds.Where(x => x.ExternalId == r.ExternalId || (x.CustomerName == r.CustomerName && x.CustomerIdentity == r.CustomerIdentity && x.RequestDate == r.RequestDate)).ToList();

                            // ֻ��һ�����滻
                            if (exi.Count == 1 && (exi[0].Source != "api" || exi[0].ExternalId == r.ExternalId))
                            {
                                r.Id = exi[0].Id;
                                continue;
                            }

                            // > 1��
                            // ����ͬex id���滻
                            var old = exi.Where(x => x.ExternalId == r.ExternalId);
                            if (old.Any())
                                r.Id = old.First().Id;

                            // ��������ֶ�¼��ģ�Ҳɾ��
                            foreach (var item in exi)
                                db.GetCollection<TransferRequest>().DeleteMany(item => item.Source == "manual" || item.ExternalId == r.ExternalId);

                        }

                        db.GetCollection<TransferRequest>().Upsert(rc.Data);

                        // ֪ͨ
                        try
                        {
                            foreach (var item in rc.Data)
                                WeakReferenceMessenger.Default.Send(item);
                        }
                        catch { }
                    }


                    // �����unset����ʾ�����쳣�����������
                    if (rc.Data?.Any(x => x.CustomerName == "unset" || x.FundName == "unset" || x.CustomerIdentity == "unset") ?? false)
                    {
                        ret.Add(new(tr.Title, ReturnCode.DataIsNotWellFormed, rc.Data));
                        WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Error, $"{tr.Title} ��ȡ�Ľ��������¼�����쳣"));
                        break;
                    }
                    // ���½���
                    if (range is null) range = new(tr.Identifier + nameof(tr.QueryTransferRequests), begin, end);
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
            }
        }

        // ����ret���������ʱ�ָ�����������Ϣ
        //db.DropCollection(TableRaisingBalance);
        //db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

        WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(method, ret));
        TransferRequestConfig.Last = DateTime.Now;
        Save(TransferRequestConfig);
    }



    /// <summary>
    /// ��ȡ����ȷ�ϼ�¼
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task QueryTransferRecordOnce()
    {
        List<WorkReturn> ret = new();
        // �������ݿ�
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
                // ��ȡ��ʷ����
                var range = ranges.FirstOrDefault(x => x.Id == tr.Identifier + method);

                DateOnly begin = range?.End ?? new DateOnly(DateTime.Today.Year, 1, 1), end = DateOnly.FromDateTime(DateTime.Now);

                do
                {
                    var rc = await tr.QueryTransferRecords(begin, end);

                    ///
                    // �������ݿ� 
                    if (rc.Data is not null)
                    {
                        // ��������   
                        foreach (var r in rc.Data)
                        {
                            if (r.Agency == manager.Name)
                                r.Agency = "ֱ��";


                            // code ƥ��
                            var f = funds.FirstOrDefault(x => x.Code == r.FundCode);
                            if (f is not null)
                            {
                                r.FundId = f.Id;
                                r.FundName = f.Name;
                                continue;
                            }
                            else Log.Error($"QueryTransferRequests ����δ֪�Ĳ�Ʒ{r.FundName} {r.FundCode}");

                            // �ӷݶ� �ڸ�api�����
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
                            else // �������
                            {
                                c = new Investor { Name = r.CustomerName, Identity = new Identity { Id = r.CustomerIdentity } };
                                db.GetCollection<Investor>().Insert(c);
                                r.CustomerId = c.Id;
                            }
                        }


                        DataTracker.OnBatchTransferRecord(rc.Data);
                    }



                    // �����unset����ʾ�����쳣�����������
                    if (rc.Data?.Any(x => x.CustomerName == "unset" || x.FundName == "unset" || x.CustomerIdentity == "unset") ?? false)
                        break;

                    // ���½���
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
            }
        }

        // ����ret���������ʱ�ָ�����������Ϣ
        //db.DropCollection(TableRaisingBalance);
        //db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

        WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(method, ret));
        TransferRecordConfig.Last = DateTime.Now;
        Save(TransferRecordConfig);
    }

    /// <summary>
    /// ��ȡÿ�շ�����ϸ
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task QueryDailyFeeOnce()
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
            }
        }

        // ����ret���������ʱ�ָ�����������Ϣ
        //db.DropCollection(TableRaisingBalance);
        //db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

        WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(method, ret));
        DailyFeeConfig.Last = DateTime.Now;
        Save(DailyFeeConfig);

    }



    /// <summary>
    /// ��ȡļ������ˮ
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task QueryRaisingAccountTransctionOnce()
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

                        db.GetCollection<BankTransaction>().Upsert(rc.Data);
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
            }
        }

        // ����ret���������ʱ�ָ�����������Ϣ
        //db.DropCollection(TableRaisingBalance);
        //db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

        WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(nameof(ITrustee.QueryRaisingAccountTransction), ret));
        RaisingAccountTransctionConfig.Last = DateTime.Now;
        Save(RaisingAccountTransctionConfig);
    }




    [RelayCommand]
    public void Rebuild(string method)
    {
        using (var pdf = DbHelper.Platform()) //ɾ����¼
            pdf.GetCollection("TrusteeMethodShotRange").Delete(method);
    }




    private async void OnTimer2(object? state)
    {
        var t = DateTime.Now;

        // ����λ
        var minute = t.Ticks / TimeSpan.TicksPerMinute; //  new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, 0);
        if (minute % RaisingBalanceConfig.Interval == 0)
        {
            await RaisingBalanceConfig.Semaphore.WaitAsync();
            try
            {
                // �����Ƿ����ϴ�����ʱ�䲻һ��
                //if (t.Hour != RaisingBalanceConfig.Last.Hour || t.Minute != RaisingBalanceConfig.Last.Minute)
                if (minute / RaisingBalanceConfig.Interval != RaisingBalanceConfig.GetLastRunIndex())
                    await QueryRaisingBalanceOnceCommand.ExecuteAsync(null);
            }
            catch { }
            finally { RaisingBalanceConfig.Semaphore.Release(); }
        }

        // ļ������ˮ
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

        // �������� 
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

        // ����ȷ��
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


        // ����
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

        (WorkConfig Config, IAsyncRelayCommand Command)[] tasks = [
                                                                    // ļ������ѯ����
                                                                    // ʹ�� RaisingBalanceConfig ���ã���ִ�м�����ϴ�����ʱ��ȣ�
                                                                    // ���� QueryRaisingBalanceOnceCommand ����ִ�е��β�ѯ
                                                                    (Config: RaisingBalanceConfig, Command: QueryRaisingBalanceOnceCommand),

                                                                    // ļ������ˮ��ѯ����
                                                                    // ʹ�� RaisingAccountTransctionConfig ����
                                                                    // ���� QueryRaisingAccountTransctionOnceCommand ����ִ�е��β�ѯ
                                                                    (Config: RaisingAccountTransctionConfig, Command: QueryRaisingAccountTransctionOnceCommand),

                                                                    // ���������ѯ����
                                                                    // ʹ�� TransferRequestConfig ����
                                                                    // ���� QueryTransferRequestOnceCommand ����ִ�е��β�ѯ
                                                                    (Config: TransferRequestConfig, Command: QueryTransferRequestOnceCommand),

                                                                    // ����ȷ�ϲ�ѯ����
                                                                    // ʹ�� TransferRecordConfig ����
                                                                    // ���� QueryTransferRecordOnceCommand ����ִ�е��β�ѯ
                                                                    (Config: TransferRecordConfig, Command: QueryTransferRecordOnceCommand),

                                                                    // �ճ����ò�ѯ����
                                                                    // ʹ�� DailyFeeConfig ����
                                                                    // ���� QueryDailyFeeOnceCommand ����ִ�е��β�ѯ
                                                                    (Config: DailyFeeConfig, Command: QueryDailyFeeOnceCommand)
        ];


        foreach (var (Config, Command) in tasks)
        {
            if (minuteIndex % Config.Interval == 0)
            {
                await Config.Semaphore.WaitAsync();
                try
                {
                    if (minuteIndex / Config.Interval != Config.GetLastRunIndex())
                    {
                        // ���ȵ� UI �߳�ִ��
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
