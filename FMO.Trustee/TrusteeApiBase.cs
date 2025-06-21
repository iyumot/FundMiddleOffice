
using FMO.Models;
using FMO.Utilities;
using LiteDB;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace FMO.Trustee;


public abstract class TrusteeApiBase : ITrustee
{
    /// <summary>
    /// Ψһ��ʶ
    /// </summary>
    public abstract string Identifier { get; }

    public abstract string Title { get; }

    public abstract string TestDomain { get; }

    public abstract string Domain { get; }


    public bool IsValid { get; private set; } = true;


    /// <summary>
    /// �����������
    /// �������5�Σ�Ӧ������invalid
    /// </summary>
    protected int ConsecutiveErrorCount { get; set; }


    /// <summary>
    /// ����API ͳһclient�������л��Ƿ���proxy : TrusteeApiBase.SetProxy
    /// </summary>
    protected static HttpClient _client { get; private set; } = new();

    private static ILiteDatabase _db { get; } = new LiteDatabase(@$"FileName=data\platformlog.db;Connection=Shared");


    public abstract Task<BankTransaction[]?> GetCustodyAccountRecords(DateOnly begin, DateOnly end);

    public abstract Task<BankTransaction[]?> GetRaisingAccountRecords(DateOnly begin, DateOnly end);


    public abstract Task<ReturnWrap<Investor>> SyncInvestors();



    public abstract Task<TransferRequest[]?> GetTransferRequests(DateOnly begin, DateOnly end);



    /// <summary>
    /// �в�
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <param name="param">object �� Dictionary<string, object></param>
    /// <returns></returns>
    protected async Task<ReturnWrap<T>> SyncWork<T>(Func<Dictionary<string, object>, Task<ReturnWrap<T>>> func, object? param)
    {
        // У��
        if (CheckBreforeSync() is ReturnCode rc && rc != ReturnCode.Success) return new(rc, null);

        // ��dict ת��dict �����޸�page
        Dictionary<string, object> fp;
        if (param is null) fp = new();
        else if (param is Dictionary<string, object> pp) // (param is not null && (param.GetType() is not Type t || !t.IsGenericType || t.GetGenericTypeDefinition() != typeof(Dictionary<,>)))
            fp = pp;
        else
            fp = GenerateParams(param);

        var result = await func(fp);
        List<T> list = new();

        // У�鷵�� 
        switch (result.Code)
        {
            case ReturnCode.Success:
                if (result.Data is not null) list.AddRange(result.Data);
                ConsecutiveErrorCount = 0;
                break;

            case ReturnCode.NotFinished: // ��������
                if (result.Data is not null) list.AddRange(result.Data);
                result = await func(fp);
                ConsecutiveErrorCount = 0;
                break;

            default: // �д���
                ++ConsecutiveErrorCount;
                if (ConsecutiveErrorCount > 6) SetDisabled();
                return new(result.Code, null);
        }

        return new(result.Code, list.ToArray());
    }

    /// <summary>
    /// ӳ���ӻ����ϵ
    /// </summary>
    /// <returns></returns>
    public abstract Task<ReturnWrap<SubjectFundMapping>> SyncSubjectFundMappings();

    /// <summary>
    /// ͬ������ȷ��
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public abstract Task<ReturnWrap<TransferRecord>> QueryTransferRecords(DateOnly begin, DateOnly end);






    public abstract Task<ReturnWrap<FundDailyFee>> QueryFundFeeDetail(DateOnly begin, DateOnly end);




    public abstract Task<ReturnWrap<BankTransaction>> QueryTrusteeAccountTransction(DateOnly begin, DateOnly end);
    public abstract Task<ReturnWrap<BankTransaction>> QueryRaisingAccountTransction(DateOnly begin, DateOnly end);







    public abstract Task<bool> Prepare();

    /// <summary>
    /// ����ǰ��֤
    /// </summary>
    /// <returns></returns>
    protected abstract ReturnCode CheckBreforeSync();





    public bool LoadConfig()
    {
        using var db = DbHelper.Platform();
        var config = db.GetCollection<IAPIConfig>().FindById(Identifier);
        return LoadConfigOverride(config);
    }

    protected abstract bool LoadConfigOverride(IAPIConfig config);

    protected abstract IAPIConfig SaveConfigOverride();

    public void SaveConfig()
    {
        var config = SaveConfigOverride();
        using var db = DbHelper.Platform();
        db.GetCollection<IAPIConfig>().Upsert(config);
    }


    protected void Log(string message)
    {
        _db.GetCollection<LogInfo>().Insert(new LogInfo { Identifier = Identifier, Log = message, Time = DateTime.Now });
    }
    protected void Log(string part, string? json, string? message)
    {
        _db.GetCollection<LogInfo>().Insert(new LogInfo { Identifier = Identifier, Log = message, Part = part, Content = json, Time = DateTime.Now });
    }


    protected virtual Dictionary<string, object> GenerateParams(object? obj)
    {
        Dictionary<string, object>? dic = new();
        if (obj is not null)
        {
            var ps = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead);
            foreach (var item in ps)
                dic.Add(item.Name, item.GetValue(obj)!);
        }
        return dic;
    }

    protected void Success(string part) => ConsecutiveErrorCount = 0;

    protected void Failed(string part)
    {
        ++ConsecutiveErrorCount;
        if (ConsecutiveErrorCount > 5)
            SetDisabled();
    }


    public static void SetProxy(WebProxy? proxy)
    {
        if (_client is not null) _client.Dispose();

        _client = new HttpClient(new HttpClientHandler
        {
            UseProxy = proxy is not null,
            Proxy = proxy,
            UseDefaultCredentials = proxy is null,
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
        });
    }


    /// <summary>
    /// domain https://xxx.com not /
    /// part /xxx
    /// </summary>
    /// <param name="part"></param>
    /// <returns></returns>
    protected string? GetUrl(string part)
    {

        return Domain + part;

#if DEBUG
        return TestDomain + part;
#else
        return Domain + part;
#endif
    }



    /// <summary>
    /// ���ò�����
    /// </summary>
    protected void SetDisabled()
    {
        IsValid = false;
    }

    protected class LogInfo
    {
        public int Id { get; set; }

        public DateTime Time { get; set; }


        public required string Identifier { get; set; }

        /// <summary>
        /// url endpoint
        /// </summary>
        public string? Part { get; set; }

        /// <summary>
        /// ���صı���
        /// </summary>
        public string? Content { get; set; }


        public string? Log { get; set; }
    }
}

public record ReturnWrap<T>(ReturnCode Code, T[]? Data);
