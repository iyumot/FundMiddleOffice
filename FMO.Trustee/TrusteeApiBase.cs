
using FMO.Models;
using FMO.Utilities;
using LiteDB;
using System.Net;
using System.Net.Http;
using System.Reflection;
using static FMO.Trustee.CSC;

namespace FMO.Trustee;


public abstract class TrusteeApiBase : ITrustee
{
    /// <summary>
    /// 唯一标识
    /// </summary>
    public abstract string Identifier { get; }

    public abstract string Title { get; }

    public abstract string TestDomain { get; }

    public abstract string Domain { get; }


    public bool IsValid { get; private set; } = true;


    /// <summary>
    /// 连续错误次数
    /// 如果超过5次，应该设置invalid
    /// </summary>
    protected int ConsecutiveErrorCount { get; set; }


    /// <summary>
    /// 所有API 统一client，方便切换是否用proxy : TrusteeApiBase.SetProxy
    /// </summary>
    protected static HttpClient _client { get; private set; } = new();

    private static ILiteDatabase _db { get; } = new LiteDatabase(@$"FileName=data\platformlog.db;Connection=Shared");

     


    public abstract Task<ReturnWrap<Investor>> QueryInvestors();



    public abstract Task<ReturnWrap<TransferRequest>> QueryTransferRequests(DateOnly begin, DateOnly end);


 
    /// <summary>
    /// 映射子基金关系
    /// </summary>
    /// <returns></returns>
    public abstract Task<ReturnWrap<SubjectFundMapping>> QuerySubjectFundMappings();

    /// <summary>
    /// 同步交易确认
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public abstract Task<ReturnWrap<TransferRecord>> QueryTransferRecords(DateOnly begin, DateOnly end);






    public abstract Task<ReturnWrap<FundDailyFee>> QueryFundDailyFee(DateOnly begin, DateOnly end);




    public abstract Task<ReturnWrap<BankTransaction>> QueryCustodialAccountTransction(DateOnly begin, DateOnly end = default);


    public abstract Task<ReturnWrap<BankTransaction>> QueryRaisingAccountTransction(DateOnly begin, DateOnly end);



    public abstract Task<ReturnWrap<FundBankBalance>> QueryRaisingBalance();





    public abstract bool Prepare();

    /// <summary>
    /// 访问前验证
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
    protected void Log(string? caller, string? json, string? message)
    {
        _db.GetCollection<LogInfo>().Insert(new LogInfo { Identifier = Identifier, Log = message, Method = caller, Content = json, Time = DateTime.Now });
    }

    protected void LogRun(string? caller)
    {
        _db.GetCollection<TrusteeCallHistory>().Insert(new TrusteeCallHistory(Identifier, caller??"unknown", DateTime.Now));
    }

    public static LogInfo[]? GetLogs()
    {
        return _db.GetCollection<LogInfo>().FindAll().ToArray();
    }


    /// <summary>
    /// 报告示识别的json 数据
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="method"></param>
    /// <param name="info"></param>
    protected static void ReportJsonUnexpected(string identifier, string method, string info)
    {
        _db.GetCollection<LogInfo>().Insert(new LogInfo { Identifier = identifier, Log = info, Method = method, Content = "", Time = DateTime.Now });
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



    protected static decimal ParseDecimal(string value)
    {
        if (string.IsNullOrEmpty(value))
            return 0;

        if (decimal.TryParse(value, out var result))
            return result;

        throw new FormatException($"无法将 '{value}' 解析为decimal类型");
    }


    /// <summary>
    /// 设置不可用
    /// </summary>
    protected void SetDisabled()
    {
        IsValid = false;
    }

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
        /// 返回的报文
        /// </summary>
        public string? Content { get; set; }


        public string? Log { get; set; }
    }
}

public record ReturnWrap<T>(ReturnCode Code, T[]? Data);

public record TrusteeCallHistory(string Identifier, string Method, DateTime Time);