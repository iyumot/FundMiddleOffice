using FMO.Models;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Web;

namespace FMO.Trustee;

public partial class CMS : TrusteeApiBase
{
    public override string Identifier => "trustee_cms";

    public override string Title => "招商证券";

    public override string TestDomain => "http://210.21.237.135:27011/v1/request";

    public override string Domain => "https://tgjjapi.newone.com.cn/v1/request";



    public string? CompanyId { get; set; }

    public int? ServerType { get; set; }

    public string? LicenceKey { get; set; }

    public string? UserNo { get; set; }

    public byte[]? PFX { get; set; }

    public string? Password { get; set; }

    public X509Certificate2? Certificate { get; set; }




    public override Task<BankTransaction[]?> GetCustodyAccountRecords(DateOnly begin, DateOnly end)
    {
        throw new NotImplementedException();
    }

    public override Task<BankTransaction[]?> GetRaisingAccountRecords(DateOnly begin, DateOnly end)
    {
        throw new NotImplementedException();
    }



    public override Task<TransferRequest[]?> GetTransferRequests(DateOnly begin, DateOnly end)
    {
        throw new NotImplementedException();
    }








    public override async Task<ReturnWrap<SubjectFundMapping>> SyncSubjectFundMappings()
    {
        var data = await SyncWork<SubjectFundMapping, SubjectFundMappingJson>(1018, null, x => x.ToObject());
        return data;
    }

    public override async Task<ReturnWrap<TransferRecord>> QueryTransferRecords(DateOnly begin, DateOnly end)
    {
        var data = await SyncWork<TransferRecord, TransferRecordJson>(1006, new { beginDate = $"{begin:yyyyMMdd}", endDate = $"{end:yyyyMMdd}" }, x => x.ToObject());
        return data;
    }


    public override async  Task<ReturnWrap<FundDailyFee>> QueryFundFeeDetail(DateOnly begin, DateOnly end)
    {
        var data = await SyncWork<FundDailyFee, FundDailyFeeJson>(1020, new { beginDate = $"{begin:yyyyMMdd}", endDate = $"{end:yyyyMMdd}" }, x => x.ToObject());
        return data;
    }


    public override async Task<ReturnWrap<BankTransaction>> QueryRaisingAccountTransction(DateOnly begin, DateOnly end)
    {
        var data = await SyncWork<BankTransaction, BankTransactionJson>(1002, new { beginDate = $"{begin:yyyyMMdd}", endDate = $"{end:yyyyMMdd}" }, x => x.ToObject());
        return data;
    }


     

    public override Task<ReturnWrap<BankTransaction>> QueryTrusteeAccountTransction(DateOnly begin, DateOnly end)
    {
        throw new NotImplementedException();
    }




    //////////////////////////////////////////////////////////////////////////////////////////


    public override async Task<bool> Prepare()
    {

        InitCertificate();

        if (string.IsNullOrWhiteSpace(CompanyId) || string.IsNullOrWhiteSpace(LicenceKey) || string.IsNullOrWhiteSpace(UserNo) || ServerType is null || Certificate is null)
            SetDisabled();

        return await Task.FromResult(true);
    }

    public override Task<ReturnWrap<Investor>> SyncInvestors()
    {
        throw new NotImplementedException();
    }

    protected override bool LoadConfigOverride(IAPIConfig config)
    {
        if (config is not APIConfig c) return false;

        CompanyId = c.CompanyId;
        LicenceKey = c.LicenceKey;
        ServerType = c.ServerType;
        UserNo = c.UserNo;
        PFX = c.PFX;
        Password = c.Password;

        return true;
    }

    protected override IAPIConfig SaveConfigOverride()
    {
        return new APIConfig { CompanyId = CompanyId, LicenceKey = LicenceKey, ServerType = ServerType, UserNo = UserNo, PFX = PFX, Password = Password };
    }



    public void InitCertificate()
    {
        if (PFX is not null && !string.IsNullOrWhiteSpace(Password))
        {
            try
            {
                Certificate = X509CertificateLoader.LoadPkcs12(PFX, Password, X509KeyStorageFlags.Exportable);
            }
            catch { }
        }
    }



    public async Task<string?> Query(int interfaceId, Dictionary<string, object> param)
    {
        // 创建request
        var request = Build(interfaceId, param);
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        return content;
    }

    protected async Task<ReturnWrap<TEntity>> SyncWork<TEntity, TJSON>(int interfaceId, object? param, Func<TJSON, TEntity> transfer)
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
                var json = await Query(interfaceId, formatedParams);

                try
                {
                    if (json is null) return new(ReturnCode.EmptyResponse, null);
                    var ret = JsonSerializer.Deserialize<JsonRoot>(json);

                    var code = int.Parse(ret!.Code);

                    // 有错误
                    if (code != 10000)
                    {
                        Log($"{interfaceId}", json, ret.Msg);
                        return new(TransferReturnCode(code, ret.Msg), null);
                    }

                    // 解析实际数据
                    var data = JsonSerializer.Deserialize<TJSON[]>(ret.Data!);
                    list.AddRange(data!);

                    // 数据获取是否齐全
                    var pi = JsonSerializer.Deserialize<PaginationInfo>(ret.Page!)!;
                    if (pi.PageNumber >= pi.PageCount)
                        break;

                    // 下一页
                    var page = (int)formatedParams["pageNumber"];
                    formatedParams["pageNumber"] = page + 1;
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



    private HttpRequestMessage Build(int interfaceId, Dictionary<string, object> param)
    {
        string url = GetUrl("")!;
        var time = DateTime.Now;

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("uap-licence-key", LicenceKey);
        request.Headers.Add("uap-request-no", $"{interfaceId}");
        request.Headers.Add("uap-server-type", $"{ServerType}");
        request.Headers.Add("uap-user-no", UserNo);
        request.Headers.Add("uap-request-id", $"{time.Ticks}");

        var timestamp = time.ToString("yyyyMMddHHmmss");
        var sign = SignatureUtils.GetManagerSignature(CompanyId!, timestamp, Certificate!);


        var businessparam = JsonSerializer.Serialize(param);
        string post = $"companyid={HttpUtility.UrlEncode(CompanyId)}&timestamp={timestamp}&signature={HttpUtility.UrlEncode(sign)}&businessparam={HttpUtility.UrlEncode(businessparam)}";
        request.Content = new StringContent(post, Encoding.UTF8, "application/x-www-form-urlencoded");

        return request;
    }


    protected override ReturnCode CheckBreforeSync()
    {
        // 检验可用性 
        if (!IsValid) return ReturnCode.ConfigInvalid;

        if (string.IsNullOrWhiteSpace(LicenceKey) || string.IsNullOrWhiteSpace(UserNo) || Certificate is null || ServerType is null)
        {
            SetDisabled();
            return ReturnCode.ConfigInvalid;
        }

        return ReturnCode.Success;
    }


    private ReturnCode TransferReturnCode(int code, string message)
    {
        switch (code)
        {
            case 0000:
            case 10000:
                return ReturnCode.Success;

            case 10001://（请求参数无效） 
                return ReturnCode.ParameterInvalid;

            case 10002://（身份认证失败） 
                return ReturnCode.IdentitifyFailed;

            case 10003://（非法IP） 
                return ReturnCode.InvalidIP;

            case 10004://（接口不可用） 
                return ReturnCode.InterfaceUnavailable;
                  
            default:
                return ReturnCode.Unknown;
        }
    }

    protected override Dictionary<string, object> GenerateParams(object? obj)
    {
        var param = base.GenerateParams(obj);
        param.Add("pageNumber", 1);
        param.Add("pageSize", 300);

        return param;
    }

    public class APIConfig : IAPIConfig
    {
        public string Id { get; } = "trustee_cms";


        public string? CompanyId { get; set; }

        public int? ServerType { get; set; }

        public string? LicenceKey { get; set; }

        public string? UserNo { get; set; }

        public byte[]? PFX { get; set; }

        public string? Password { get; set; }
    }


}
