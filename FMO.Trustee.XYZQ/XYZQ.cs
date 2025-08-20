using FMO.Logging;
using FMO.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FMO.Trustee;

public class XYZQ : TrusteeApiBase
{
    public const string _Identifier = "trustee_xyzq";

    public override string Identifier => _Identifier;

    public override string Title => "兴业证券";
    public override string TestDomain => "http://101.95.178.246:8089";

    public override string Domain => "https://glr.xyzq.cn";


    public string? ClientId { get; set; }


    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string? ClientSecret { get; set; }

    public string? AToken { get; private set; }
    public string? FToken { get; private set; }

    private DateTime TokenTime { get; set; }

    public override bool IsSuit(string? company) => string.IsNullOrWhiteSpace(company) ? false : Regex.IsMatch(company, $"兴业证券|兴业证券股份有限公司|{_Identifier}");


    public override bool Prepare()
    {
        if (string.IsNullOrWhiteSpace(ClientId) || string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password) || ClientSecret is null)
            SetStatus();

        return true;
    }


    public async Task<bool> GetToken()
    {
        // 有token 可以刷新
        if (!string.IsNullOrWhiteSpace(AToken) && !string.IsNullOrWhiteSpace(FToken) && await RefreshToken())
            return true;

        // 重新申请

        var url = GetUrl("/pposapi?servername=GetApiAuthCode");
        var param = new Dictionary<string, string>
        {
            ["client_id"] = ClientId!,
            ["username"] = UserName!,
            ["password"] = Password!,
            ["response_type"] = "code",
            ["redirect_uri"] = "http://101.95.178.246:8089/testppos"
        };
        var response = await _client.PostAsync(url, new FormUrlEncodedContent(param));

        /////////////////////
        var resjson = await response.Content.ReadAsStringAsync();
        var resobj = JsonNode.Parse(resjson);
        // 有有效token，直接返回
        if (resobj?["access_token"] is JsonNode ano)
        {
            AToken = ano.GetValue<string>();
            return true;
        }

        if (resobj?["code"] is not JsonNode node)
        {
            LogEx.Error($"XYZQ Auth Failed {resobj?["msg"]}");
            return false;
        }

        var auth = node.GetValue<string>();


        url = GetUrl($"/pposapi?servername=GetApiToken&client_id={ClientId}&client_secret={ClientSecret}&code={auth}&grant_type=authorization_code&redirect_uri=http://101.95.178.246:8089/testppos");

        response = await _client.PostAsync(url, new FormUrlEncodedContent([]));
        resjson = await response.Content.ReadAsStringAsync();
        resobj = JsonNode.Parse(resjson);

        var rnode = resobj?["refresh_token"];
        var tnode = resobj?["refresh_token"];
        if (tnode is null || rnode is null)
        {
            LogEx.Error($"XYZQ GetToken Failed {resobj?["msg"]}");
            return false;
        }
        AToken = tnode.GetValue<string>();
        FToken = rnode.GetValue<string>();
        TokenTime = DateTime.Now;

        return true;
    }


    public async Task<bool> RefreshToken()
    {
        var url = GetUrl("/pposapi?servername=RefreshToken");
        var param = new
        {
            client_id = ClientId,
            refresh_token = FToken,
            access_token = AToken
        };

        var json = JsonSerializer.Serialize(param);
        var response = await _client.PostAsync(url, new StringContent(json));
        var resjson = await response.Content.ReadAsStringAsync();
        var resobj = JsonNode.Parse(resjson);

        var rnode = resobj?["refreshToken"];
        var tnode = resobj?["accessToken"];
        if (tnode is null || rnode is null)
        {
            LogEx.Error($"XYZQ GetToken Failed {resobj?["msg"]}");
            return false;
        }
        AToken = tnode.GetValue<string>();
        FToken = rnode.GetValue<string>();
        TokenTime = DateTime.Now;
        return true;
    }

    protected override ReturnCode CheckBreforeSync()
    {
        // 检验可用性 
        if (!IsValid) return ReturnCode.ConfigInvalid;

        return ReturnCode.Success;
    }

    public override Task<ReturnWrap<BankTransaction>> QueryCustodialAccountTransction(DateOnly begin, DateOnly end, string? fundCode = null)
    {
        throw new NotImplementedException();
    }

    public override Task<ReturnWrap<FundDailyFee>> QueryFundDailyFee(DateOnly begin, DateOnly end)
    {
        throw new NotImplementedException();
    }

    public override Task<ReturnWrap<Investor>> QueryInvestors()
    {
        throw new NotImplementedException();
    }

    public override Task<ReturnWrap<DailyValue>> QueryNetValue(DateOnly begin, DateOnly end, string? fundCode = null)
    {
        throw new NotImplementedException();
    }

    public override Task<ReturnWrap<RaisingBankTransaction>> QueryRaisingAccountTransction(DateOnly begin, DateOnly end, string? fundCode = null)
    {
        throw new NotImplementedException();
    }

    public override Task<ReturnWrap<FundBankBalance>> QueryRaisingBalance()
    {
        throw new NotImplementedException();
    }

    public override async Task<ReturnWrap<SubjectFundMapping>> QuerySubjectFundMappings()
    {
        int pageId = 1;

        if (!await EnsureToken()) return new(ReturnCode.XYZQ_CanNotGetToken, []);

        var url = GetUrl($"/pposapi?servername=productList&access_token={AToken}&version=V1.0&currentpage={pageId}&y=5000");

        HttpRequestMessage request = new(new HttpMethod("GET"), url);
        //request.Headers.Add("Authorization", "");
        //request.Headers.Add("client_id", ClientId);

        var caller = nameof(QuerySubjectFundMappings);

        for (int i = 0; i < 19; i++) // 防止无限循环 
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var response = await _client.SendAsync(request, cts.Token);
            var json = await response.Content.ReadAsStringAsync();
            var root = JsonSerializer.Deserialize<JsonRoot>(json);
            if (root?.returnCode == "-3") // token失效
            {
                if (!await GetToken())
                {
                    SetStatus();
                    return new(ReturnCode.IdentitifyFailed, []);
                }
                continue;
            }

            // 其它原因失败
            if (root?.returnCode != "1")
            {
                LogEx.Error($"{caller} {root?.msg}");
                return new(ReturnCode.Unknown, []);
            }

            if (root.currentPage >= root.totalPage) break;
            
            url = GetUrl($"/pposapi?servername=productList&access_token={AToken}&version=V1.0&currentpage={++pageId}&y=5000");
        }

        return new(ReturnCode.Success, []);
    }


    public async Task<bool> EnsureToken()
    {
        if (string.IsNullOrWhiteSpace(AToken) && !await GetToken())
        {
            SetStatus();
            return false;
        }
        return true;
    }


    public override Task<ReturnWrap<TransferRecord>> QueryTransferRecords(DateOnly begin, DateOnly end, string? fundCode = null)
    {
        throw new NotImplementedException();
    }

    public override async Task<ReturnWrap<TransferRequest>> QueryTransferRequests(DateOnly begin, DateOnly end, string? fundCode = null)
    {

        throw new NotImplementedException();
    }


    protected override bool LoadConfigOverride(IAPIConfig config)
    {
        if (config is not APIConfig c) return false;

        this.CloneFrom(c);

        return true;
    }

    protected override IAPIConfig SaveConfigOverride()
    {
        var c = new APIConfig();
        c.CloneFrom(this);
        return c;
    }

    protected override async Task<bool> VerifyConfigOverride()
    {
        try { return (await QuerySubjectFundMappings()).Code == ReturnCode.Success; } catch { return false; }
    }
}


internal class APIConfig : IAPIConfig
{
    public string Id { get; } = XYZQ._Identifier;


    public string? ClientId { get; set; }


    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string? ClientSecret { get; set; }

    public string? Token { get; set; }


    public bool IsValid { get; set; }
}
