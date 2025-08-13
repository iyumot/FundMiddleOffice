
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using LiteDB;
using System.Net;
using System.Reflection;

namespace FMO.Trustee;


public interface IAPIConfig
{
    public string Id { get; }
}

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




    public abstract Task<ReturnWrap<Investor>> QueryInvestors();



    public abstract Task<ReturnWrap<TransferRequest>> QueryTransferRequests(DateOnly begin, DateOnly end);



    /// <summary>
    /// ӳ���ӻ����ϵ
    /// </summary>
    /// <returns></returns>
    public abstract Task<ReturnWrap<SubjectFundMapping>> QuerySubjectFundMappings();

    /// <summary>
    /// ͬ������ȷ��
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public abstract Task<ReturnWrap<TransferRecord>> QueryTransferRecords(DateOnly begin, DateOnly end);






    public abstract Task<ReturnWrap<FundDailyFee>> QueryFundDailyFee(DateOnly begin, DateOnly end);




    public abstract Task<ReturnWrap<BankTransaction>> QueryCustodialAccountTransction(DateOnly begin, DateOnly end = default);


    public abstract Task<ReturnWrap<RaisingBankTransaction>> QueryRaisingAccountTransction(DateOnly begin, DateOnly end);



    public abstract Task<ReturnWrap<FundBankBalance>> QueryRaisingBalance();


    public abstract Task<ReturnWrap<DailyValue>> QueryNetValue(DateOnly begin, DateOnly end, string? fundCode = null);



    public abstract bool Prepare();

    /// <summary>
    /// ����ǰ��֤
    /// </summary>
    /// <returns></returns>
    protected abstract ReturnCode CheckBreforeSync();





    public bool LoadConfig()
    {
        using var db = DbHelper.Platform();
        try { if (db.GetCollection<IAPIConfig>().FindById(Identifier) is IAPIConfig config) return LoadConfigOverride(config); }
        catch/*(Exception e) */{ WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Error, $"����{Title}�������ļ�����")); }
        return false;
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
    protected void Log(string? caller, string? json, string? message)
    {
        _db.GetCollection<LogInfo>().Insert(new LogInfo { Identifier = Identifier, Log = message, Method = caller, Content = json, Time = DateTime.Now });
    }

    protected void CacheJson<T>(string? caller, IEnumerable<T> list)
    {
        using (var db = DbHelper.Platform())
            db.GetCollection<T>($"{Identifier}_{caller}").Insert(list);
    }


    protected void LogRun(string? caller, Dictionary<string, object> formatedParams, string? json)
    {
        _db.GetCollection<TrusteeCallHistory>().Insert(new TrusteeCallHistory(Identifier, caller ?? "unknown", DateTime.Now, System.Text.Json.JsonSerializer.Serialize(formatedParams), json));
    }

    public static LogInfo[]? GetLogs()
    {
        try
        {
            var dic = _db.GetCollection<LogInfo>().Query().OrderByDescending(x => x.Time).Limit(1000).ToList().GroupBy(x => x.Identifier).ToDictionary(x => x.Key, x => x.GroupBy(y => y.Method ?? "").ToDictionary(y => y.Key, y => y.Take(5)));
            return dic.SelectMany(x => x.Value.SelectMany(y => y.Value)).OrderByDescending(x => x.Time).ToArray();
        }
        catch (Exception ex)
        {
            Serilog.Log.Error($"Can Not Load Log {ex.Message}");
            return [];
        }
        // return _db.GetCollection<LogInfo>().FindAll().GroupBy(x=>x.Identifier).Select(x=> (x.Key, x.GroupBy(x=>x.Method).Select(y=> (y.Key, y.Take(5))))).ToArray();
    }


    /// <summary>
    /// ����ʾʶ���json ����
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="method"></param>
    /// <param name="info"></param>
    //public static void ReportJsonUnexpected(string identifier, string method, string info)
    //{
    //    _db.GetCollection<LogInfo>().Insert(new LogInfo { Identifier = identifier, Log = info, Method = method, Content = "�����쳣", Time = DateTime.Now });
    //}


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

        //#if DEBUG
        //        return TestDomain + part;
        //#else
        //        return Domain + part;
        //#endif
    }


#if DEBUG
    protected static string? GetCache(string Identifier, string Method, object Params)
    {
        return _db.GetCollection<APIDebugCache>().Find(x => x.Identifier == Identifier && x.Method == Method && x.Params == Params).LastOrDefault()?.Json;
    }

    protected static void SetCache(string Identifier, string Method, object Params, string json)
    {
        _db.GetCollection<APIDebugCache>().Insert(new APIDebugCache(Identifier, Method, Params, json));
    } 
#endif


    protected static decimal ParseDecimal(string value)
    {
        if (string.IsNullOrEmpty(value))
            return 0;

        if (decimal.TryParse(value, out var result))
            return result;

        throw new FormatException($"�޷��� '{value}' ����Ϊdecimal����");
    }

    /// <summary>
    /// ����������days���ָ�
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <param name="days"></param>
    /// <returns></returns>
    protected static (DateOnly b, DateOnly e)[] Split(DateOnly begin, DateOnly end, int days)
    {
        if (begin == end) return [(begin, end)];
        if (begin > end) return [];

        int total = end.DayNumber - begin.DayNumber;
        int cnt = (int)Math.Ceiling((double)total / days);
        int unit = total / cnt;

        var tmp = begin.AddDays(unit);
        List<(DateOnly b, DateOnly e)> list = new();

        do
        {
            if (tmp > end) tmp = end;
            list.Add((begin, tmp));

            begin = tmp.AddDays(1);
            tmp = begin.AddDays(unit);
        } while (begin <= end);

        return list.ToArray();
    }

    /// <summary>
    /// ���ò�����
    /// </summary>
    protected void SetDisabled()
    {
        IsValid = false;
    }

    public abstract bool IsSuit(string? comapny);

    public class LogInfo
    {
        public int Id { get; set; }

        public DateTime Time { get; set; }


        public required string Identifier { get; set; }

        /// <summary>
        /// url endpoint
        /// </summary>
        public string? Method { get; set; }

        /// <summary>
        /// ���صı���
        /// </summary>
        public string? Content { get; set; }


        public string? Log { get; set; }
    }
}


#if DEBUG

public record APIDebugCache(string Identifier, string Method, object Params, string Json);
#endif