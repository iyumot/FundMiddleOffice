using FMO.Models;
using FMO.Utilities;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using static FMO.Trustee.SM4;

namespace FMO.Trustee;

/// <summary>
/// 中信建投证券
/// </summary>
public partial class CSC : TrusteeApiBase
{
    private readonly int SizePerPage = 1000;

    public override string Identifier => "trustee_csc";

    public override string Title => "中信建投证券";

    public override string TestDomain { get; } = "https://zsjg.csc108.com:443/rest";

    public override string Domain { get; } = "https://zsjg.csc108.com:443/rest";

    public string? APIKey { get; set; }

    public string? APISecret { get; set; }

    public string? EncryptKey { get; set; }


    private SM4? Encoder { get; set; }

    private SM4? Decoder { get; set; }

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
        var part = "/institution/tgpt/erp/file/productFile/findProductList";
        var result = await SyncWork<SubjectFundMapping, SubjectFundMapping>(part, null, x => x);


        return result;
    }


    public override async Task<ReturnWrap<TransferRecord>> QueryTransferRecords(DateOnly begin, DateOnly end)
    {
        var part = "/institution/tgpt/erp/product/query/findAckTransList";
        var result = await SyncWork<TransferRecord, TransferRecordJson>(part, new { beginDate = begin.ToString("yyyyMMdd"), endDate = end.ToString("yyyyMMdd") }, x => x.ToObject());
        return result;

        var records = result.Data!;

        // 对齐数据 
        using var db = DbHelper.Base();
        var funds = db.GetCollection<Fund>().FindAll().ToArray();
        foreach (var r in records)
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
        foreach (var r in records)
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
        var olds = db.GetCollection<TransferRecord>().FindAll().ToList();
        foreach (var r in records)
        {
            // 同日同名
            var exi = olds.Where(x => x.CustomerName == r.CustomerName && x.CustomerIdentity == r.CustomerIdentity && x.ConfirmedDate == r.ConfirmedDate).ToList();

            // 只有一个，替换
            if (exi.Count == 0)
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
                if (item.Source == "manual" || item.ExternalId == r.ExternalId)
                    db.GetCollection<TransferRecord>().Delete(item.Id);

        }

        // 保存到数据库
        db.GetCollection<TransferRecord>().Upsert(records);


        return new(ReturnCode.Success, records);
    }


    public override Task<ReturnWrap<Investor>> SyncInvestors()
    {
        throw new NotImplementedException();
    }


    public override async Task<ReturnWrap<FundDailyFee>> QueryFundFeeDetail(DateOnly begin, DateOnly end)
    {
        var part = "/institution/tgpt/erp/product/query/findDailyFeeList";
        var result = await SyncWork<FundDailyFee, FundDailyFeeJson>(part, null, x => x.ToObject());


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












    protected override ReturnCode CheckBreforeSync()
    {
        // 检验可用性 
        if (!IsValid) return ReturnCode.ConfigInvalid;

        if (string.IsNullOrWhiteSpace(APIKey) || string.IsNullOrWhiteSpace(APISecret) || Encoder is null || Decoder is null)
        {
            SetDisabled();
            return ReturnCode.ConfigInvalid;
        }

        return ReturnCode.Success;
    }


    #region 配置
    protected override bool LoadConfigOverride(IAPIConfig config)
    {
        if (config is not APIConfig c)
            return false;

        APIKey = c.APIKey;
        APISecret = c.APISecret;
        EncryptKey = c.EncryptKey;
        return true;
    }

    public override Task<bool> Prepare()
    {
        if (string.IsNullOrWhiteSpace(EncryptKey))
            return Task.FromResult(false);

        byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptKey);

        Encoder = new SM4();
        Encoder.sm4_setkey_enc(new SM4_Context { isPadding = true }, keyBytes);

        Decoder = new SM4();
        Decoder.sm4_setkey_dec(new SM4_Context { isPadding = true }, keyBytes);



        return Task.FromResult(true);
    }

    protected override IAPIConfig SaveConfigOverride()
    {
        return new APIConfig { EncryptKey = EncryptKey, APIKey = APIKey, APISecret = APISecret };
    }


    #endregion

    #region 通用处理 




    /// <summary>
    /// 有参
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <param name="param">object 或 Dictionary<string, object></param>
    /// <returns></returns>
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

        int total = 0;
        // 获取所有结果
        try
        {
            for (int i = 0; i < 99; i++) // 防止无限循环，最多99次 
            {
                var json = await Query(part, formatedParams);

                try
                {
                    if (json is null) return new(ReturnCode.EmptyResponse, null);
                    var ret = JsonSerializer.Deserialize<RetJson>(json);

                    int.TryParse(ret!.Code, out var code);

                    // 有错误
                    if (code != 0)
                    {
                        Log(part, json, ret.Msg);
                        return new(TransferReturnCode(code, ret.Msg), null);
                    }

                    // 解析实际数据
                    var data = JsonSerializer.Deserialize<RetJson<TJSON>>(json);
                    //if(data.Data.RowCount != data.Data.Data.Length)
                    //    return new(ReturnCode., null);

                    total += data!.Data.Data.Length;
                    list.AddRange(data.Data.Data);

                    // 数据获取全
                    if (data!.Data.TotalCount >= total)
                        break;

                    // 下一页
                    var page = (int)formatedParams["reqPageno"];
                    formatedParams["reqPageno"] = page + 1;
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


    protected override Dictionary<string, object> GenerateParams(object? obj)
    {
        var dic = base.GenerateParams(obj);

        if (!dic.Any(x => x.Key == "reqNum"))
            dic.Add("reqNum", SizePerPage);
        if (!dic.Any(x => x.Key == "regPageno"))
            dic.Add("regPageno", 1);

        return dic;
    }



    /// <summary>
    /// 主入口
    /// </summary>
    /// <param name="part"></param>
    /// <param name="param"></param>
    /// <returns>
    /// 响应码	响应信息
    ///10000	超时了
    ///10002	该服务不可用（说明服务没有启动）
    ///10003	签名不合法（sign签名有问题）
    ///10004	签名过期
    ///10005	无权访问
    ///10007	缺少参数apikey
    ///10019	参数body加密错误
    ///ERP响应码  响应信息
    ///0000	成功
    ///0001	业务处理失败
    ///1001	系统内部错误
    ///1002	第三方流水号为空
    ///1003	参数为空
    ///1005	参数不合法
    ///1006	业务校验不通过
    /// </returns>
    public async Task<string?> Query(string part, object? param)
    {
        // 创建request
        var request = Build(part, param);
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        //{"errcode":"10005","errmsg":"无权访问该服务:无api访问权限"}
        if (content.Contains("errcode"))
        {
            // 对齐到 RetJson
            content = content.Replace("errcode", "retCode");
            content = content.Replace("errmsg", "retMsg");
            return content;
        }

        // 解密
        var dec = Decoder!.Handle(Convert.FromBase64String(content));
        var json = Encoding.UTF8.GetString(dec!);
        return json;
    }

    /// <summary>
    /// 生成request
    /// </summary>
    /// <param name="part"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    private HttpRequestMessage Build(string part, object? param)
    {
        var url = GetUrl(part)!;

        // json
        var paramsStr = param is null ? "" : JsonSerializer.Serialize(param);

        // 加密
        byte[]? encrypted = Encoder!.Handle(Encoding.UTF8.GetBytes(paramsStr));

        var body = encrypted is null ? "" : Convert.ToBase64String(encrypted);

        // time stamp
        long ts = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;

        // url params
        var param2 = $"apikey={APIKey}gwenctype=3ts={ts}";

        var sign = CalcMD5(UrlEncoder.Default.Encode($"POST/rest{part}{param2}{APISecret}"));

        // real url
        var requrl = $"{url}?ts={ts}&apikey={APIKey}&sign={sign}&gwenctype=3";

        HttpRequestMessage request = new HttpRequestMessage();
        request.RequestUri = new Uri(requrl);
        request.Method = HttpMethod.Post;

        // 生成 msgId
        request.Headers.Add("msgId", Guid.NewGuid().ToString());

        // 设置 Content-Type 为 application/json 
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");


        return request;
    }


    private ReturnCode TransferReturnCode(int code, string message)
    {
        switch (code)
        {
            case 0000:
                return ReturnCode.Success;
            case 0001:
                return ReturnCode.CSC_BusinessFailed;
            case 1001:
                return ReturnCode.CSC_InternalServerError;
            case 1002:
                return ReturnCode.CSC_ThirdPartySerialNumberEmpty;
            case 1003:
                return ReturnCode.CSC_ParameterEmpty;
            case 1005:
                return ReturnCode.ParameterInvalid;
            case 1006:
                return ReturnCode.CSC_BusinessValidationFailed;
            case 10000:
                return ReturnCode.TimeOut;
            case 10002:
                return ReturnCode.InterfaceUnavailable;
            case 10003:
                return ReturnCode.CSC_IlligalSign;
            case 10004:
                return ReturnCode.CSC_SignExpired;
            case 10005:
                return ReturnCode.AccessDenied;
            case 10007:
                return ReturnCode.CSC_APIKEY;
            case 10019:
                return ReturnCode.CSC_BodyEncrypt;
            default:
                return ReturnCode.Unknown;
        }
    }




    /// <summary>
    /// 获取字符串的 MD5 值 和 常规的不一样
    /// sb.Append((b & 0xFF).ToString("x2"));
    /// </summary>
    private static string CalcMD5(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // 将字节数组转换为十六进制字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
                sb.Append((b & 0xFF).ToString("x2"));

            return sb.ToString();
        }
    }
    #endregion

    public class APIConfig : IAPIConfig
    {
        public string Id { get; } = "trustee_csc";

        public string? APIKey { get; set; }

        public string? APISecret { get; set; }

        public string? EncryptKey { get; set; }

    }


}
