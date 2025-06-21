using FMO.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web; 

namespace FMO.Trustee;

public partial class CITICS : TrusteeApiBase
{
    private readonly int SizePerPage = 500;

    public override string Identifier => "trustee_citics";

    public override string Title => "中信证券";

    public override string TestDomain { get; } = "https://apitest-iservice.citicsinfo.com";

    public override string Domain { get; } = "https://api.iservice.citics.com/";

    public string? CustomerAuth { get; set; }

    public string? Token { get; set; }

    private DateTime? TokenTime { get; set; }

    public override async Task<BankTransaction[]?> GetCustodyAccountRecords(DateOnly begin, DateOnly end)
    {
        var content = await _client.GetStringAsync(GetUrl("/v1/ta/TradeConfirmationForApi "));


        return [];
    }

    public override Task<BankTransaction[]?> GetRaisingAccountRecords(DateOnly begin, DateOnly end)
    {
        throw new NotImplementedException();
    }


    public override async Task<ReturnWrap<Investor>> SyncInvestors()
    {
        var part = "/v1/ta/queryCustInfoForApi";

        return await SyncWork<Investor, InvestorJson>(part, null, x => x.ToObject());

    }


    //public override async Task<TransferRecord[]?> SyncTransferRecords(DateOnly begin, DateOnly end)
    //{
    //    var data = await Query<TransferRecordJson>("/v1/ta/TradeConfirmationForApi", [("ackBeginDate", begin.ToString("yyyyMMdd")), ("ackEndDate", end.ToString("yyyyMMdd"))]);

    //    // 检查错误
    //    if (data.Code != 0) ;


    //    // 保存
    //    using var db = DbHelper.Platform();
    //    db.GetCollection<TransferRecordJson>(Identifier).Upsert(data.Data);

    //    // 所有的基金账号
    //    var accouts = data.Data.Select(x => x.TradeAcco).Distinct().ToList();




    //    return [];

    //}

    public override async Task<ReturnWrap<FundDailyFee>> QueryFundFeeDetail(DateOnly begin, DateOnly end)
    {
        var part = "/v1/fm/queryFeeInfoForApi";

        var result = await SyncWork<FundDailyFee, FundDailyFeeJson>(part, new { ackBeginDate = $"{begin:yyyyMMdd}", ackEndDate = $"{end:yyyyMMdd}" }, x => x.ToObject());

        return result;
    }


    public override Task<TransferRequest[]?> GetTransferRequests(DateOnly begin, DateOnly end)
    {
        throw new NotImplementedException();
    }


    public override Task<ReturnWrap<SubjectFundMapping>> SyncSubjectFundMappings()
    {
        return Task.FromResult(new ReturnWrap<SubjectFundMapping>(ReturnCode.NotImplemented, null));
    }



    public override async Task<ReturnWrap<TransferRecord>> QueryTransferRecords(DateOnly begin, DateOnly end)
    {
        var part = "/v1/ta/TradeConfirmationForApi";
        var result = await SyncWork<TransferRecord, TransferRecordJson>(part, new { ackBeginDate = $"{begin:yyyyMMdd}", ackEndDate = $"{end:yyyyMMdd}" }, x => x.ToObject());

        return result;
    }




    public override Task<ReturnWrap<BankTransaction>> QueryRaisingAccountTransction(DateOnly begin, DateOnly end)
    {
        throw new NotImplementedException();
    }

    public override Task<ReturnWrap<BankTransaction>> QueryTrusteeAccountTransction(DateOnly begin, DateOnly end)
    {
        throw new NotImplementedException();
    }









    private async Task<string> Query(string part, Dictionary<string, object> param)
    {
        var request = Build(part, param);

        var respone = await _client.SendAsync(request);
        var content = await respone.Content.ReadAsStringAsync();

        return content;
    }


    protected async Task<ReturnWrap<TEntity>> SyncWork<TEntity, TJSON>(string part, object? param, Func<TJSON, TEntity> transfer)
    {
        // 校验
        if (CheckBreforeSync() is ReturnCode rc && rc != ReturnCode.Success) return new(rc, null);

        // 非dict 转成dict 方便修改page
        Dictionary<string, object> formatedParams;
        if (param is null) formatedParams = new();
        else if (param is Dictionary<string, object> pp) formatedParams = pp;
        else formatedParams = GenerateParams(param);

        List<TJSON> list = new();

        // 获取所有结果
        try
        {
            for (int i = 0; i < 99; i++) // 防止无限循环，最多99次 
            {
                var json = await Query(part, formatedParams);

                try
                {
                    if (json is null) return new(ReturnCode.EmptyResponse, null);
                    var ret = JsonSerializer.Deserialize<RootJson>(json)!;

                    var code = ret.Code;
                    // 有错误
                    if (code != 0)
                    {
                        if (ret.Data?.ContainsKey("reason") ?? false)
                            ret.Msg = ret.Data["reason"]?.ToString();
                        
                        Log(part, json, ret.Msg);
                        return new(TransferReturnCode(code), null);
                    }

                    var data = ret.Data.Deserialize<QueryRoot<TJSON>>()!;
                    list.AddRange(data.List!);

                    var page = (int)formatedParams["pageNum"];

                    // 数据获取全 
                    if (page >= data.Pages)
                        break;

                    // 下一页
                    formatedParams["pageNum"] = page + 1;
                }
                catch
                {
                    return new(ReturnCode.JsonNotPairToEntity, null);
                }
            }
        }
        catch (Exception e)
        {
            Log(e.Message);
            return new(ReturnCode.Unknown, null);
        }


        return new(ReturnCode.Success, list.Select(x => transfer(x)).ToArray());
    }


    public async Task<string?> GetToken()
    {
        var cc = await _client.GetStringAsync("https://www.baidu.com");

        string? requestUri = GetUrl("/v1/auth/getToken");

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Add("consumerAuth", CustomerAuth);

        var response = await _client.SendAsync(request);


        var json = await response.Content.ReadAsStringAsync();

        var obj = JsonSerializer.Deserialize<ReturnJsonRoot<TokenJson>>(json);

        // 更新
        if (obj?.Data?.Token is string s)
        {
            Token = s;
            TokenTime = DateTime.Now;
            SaveConfig();
        }


        return obj?.Data?.Token;
    }

    protected override bool LoadConfigOverride(IAPIConfig config)
    {
        if (config is not APIConfig c)
            return false;

        CustomerAuth = c.CustomerAuth;
        Token = c.Token;
        TokenTime = c.TokenTime;
        return true;
    }

    public override async Task<bool> Prepare()
    {
        LoadConfig();

        // 检查token
        if (string.IsNullOrWhiteSpace(Token))
            await GetToken();
        return true;
    }

    protected override IAPIConfig SaveConfigOverride()
    {
        return new APIConfig { CustomerAuth = CustomerAuth, Token = Token };
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    private ReturnCode TransferReturnCode(int code, string? message = null)
    {
        switch (code)
        {
            case 0:
                return ReturnCode.Success;
            case 401:
                return ReturnCode.CITICS_NoTokenOrAuth;

            case 403:
                if (message?.Contains("credentials") ?? false)
                    return ReturnCode.CITICS_Credentials;
                else if (message?.Contains("consume this service") ?? false)
                    return ReturnCode.AccessDenied;
                return ReturnCode.CITICS_Token;

            case 404:
                return ReturnCode.InterfaceUnavailable;
            case 429:
                return ReturnCode.CITICS_Limited;

            default:
                return ReturnCode.Unknown;
        }
    }

    protected override Dictionary<string, object> GenerateParams(object? obj)
    {
        var dic = base.GenerateParams(obj);

        if (!dic.Any(x => x.Key == "pageSize"))
            dic.Add("pageSize", SizePerPage);
        if (!dic.Any(x => x.Key == "pageNum"))
            dic.Add("pageNum", 1);

        return dic;
    }


    protected HttpRequestMessage Build(string part, Dictionary<string, object> param)
    {
        var url = GetUrl(part)!;
        if (param.Count > 0)
            url = $"{url}?" + HttpUtility.UrlEncode(string.Join('&', param.Select(x => $"{x.Key}={x.Value}")));


        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("consumerAuth", CustomerAuth);
        if (!string.IsNullOrWhiteSpace(Token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);

        return request;
    }



    protected override ReturnCode CheckBreforeSync()
    {
        // 检验可用性 
        if (!IsValid) return ReturnCode.ConfigInvalid;

        if (string.IsNullOrWhiteSpace(CustomerAuth) || string.IsNullOrWhiteSpace(Token))
        {
            SetDisabled();
            return ReturnCode.ConfigInvalid;
        }

        return ReturnCode.Success;
    }

    public class APIConfig : IAPIConfig
    {
        public string Id { get; } = "trustee_citics";

        public string? CustomerAuth { get; set; }

        public string? Token { get; set; }

        public DateTime? TokenTime { get; set; }

    }

}
