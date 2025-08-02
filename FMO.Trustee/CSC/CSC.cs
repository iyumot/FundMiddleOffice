using FMO.Models;
using FMO.Utilities;
using LiteDB;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using static FMO.Trustee.SM4;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace FMO.Trustee;

/// <summary>
/// ���Ž�Ͷ֤ȯ
/// </summary>
public partial class CSC : TrusteeApiBase
{
    public const string _Identifier = "trustee_csc";

    private readonly int SizePerPage = 1000;

    public override string Identifier => "trustee_csc";

    public override string Title => "���Ž�Ͷ֤ȯ";

    public override string TestDomain { get; } = "https://zsjg.csc108.com:443/rest";

    public override string Domain { get; } = "https://zsjg.csc108.com:443/rest";

    public string? APIKey { get; set; }

    public string? APISecret { get; set; }

    public string? EncryptKey { get; set; }


    private SM4? Encoder { get; set; }

    private SM4? Decoder { get; set; }


    // ��Ʒ�б�
    private List<SubjectFundMapping> FundsInfo { get; set; } = new();



    public override async Task<ReturnWrap<TransferRequest>> QueryTransferRequests(DateOnly begin, DateOnly end)
    {
        var part = "/institution/tgpt/erp/product/query/findAckTransList";
        var data = await SyncWork<TransferRequest, TransferRequestJson>(part, new { beginDate = begin.ToString("yyyyMMdd"), endDate = end.ToString("yyyyMMdd") }, x => x.ToObject());


        // �Ӳ�Ʒ ӳ��
        if (FundsInfo is null)
            await QuerySubjectFundMappings();

        if (data.Code == ReturnCode.Success && data.Data is not null)
        {
            foreach (var item in data.Data)
            {
                if (FundsInfo?.FirstOrDefault(x => x.FundCode == item.FundCode) is SubjectFundMapping sfm && sfm.MasterCode is not null)
                {
                    item.FundCode = sfm.MasterCode;
                    item.FundName = sfm.MasterName!;
                    if (!string.IsNullOrWhiteSpace(sfm.ShareClass))
                        item.ShareClass = sfm.ShareClass;
                }
            }
        }
        return data;
    }



    public override async Task<ReturnWrap<SubjectFundMapping>> QuerySubjectFundMappings()
    {
        var part = "/institution/tgpt/erp/file/productFile/findProductList";
        var result = await SyncWork<SubjectFundMapping, SubjectFundMappingJson>(part, null, x => x.ToObject());

        if (result.Data?.Length > 0)
            FundsInfo = [.. result.Data];

        return result;
    }


    public override async Task<ReturnWrap<TransferRecord>> QueryTransferRecords(DateOnly begin, DateOnly end)
    {
        var part = "/institution/tgpt/erp/product/query/findAckTransList";
        var data = await SyncWork<TransferRecord, TransferRecordJson>(part, new { beginDate = begin.ToString("yyyyMMdd"), endDate = end.ToString("yyyyMMdd") }, x => x.ToObject());


        // �Ӳ�Ʒ ӳ��
        if (FundsInfo is null)
            await QuerySubjectFundMappings();

        if (data.Code == ReturnCode.Success && data.Data is not null)
        {
            foreach (var item in data.Data)
            {
                if (FundsInfo?.FirstOrDefault(x => x.FundCode == item.FundCode) is SubjectFundMapping sfm && sfm.MasterCode is not null)
                {
                    item.FundCode = sfm.MasterCode;
                    item.FundName = sfm.MasterName!;
                    if (!string.IsNullOrWhiteSpace(sfm.ShareClass))
                        item.ShareClass = sfm.ShareClass;
                }
            }
        }
        return data;

    }


    public override Task<ReturnWrap<Investor>> QueryInvestors()
    {
        throw new NotImplementedException();
    }


    public override async Task<ReturnWrap<FundDailyFee>> QueryFundDailyFee(DateOnly begin, DateOnly end)
    {
        var part = "/institution/tgpt/erp/product/query/findDailyFeeList";

        // ��Ҫ��FundCode
        if (FundsInfo.Count == 0)
            await QuerySubjectFundMappings();

        List<FundDailyFee> list = [];
        foreach (var item in FundsInfo)
        {
            var result = await SyncWork<FundDailyFee, FundDailyFeeJson>(part, new { fundCode = item.FundCode, beginDate = begin.ToString("yyyyMMdd"), endDate = end.ToString("yyyyMMdd") }, x => x.ToObject());
            if (result.Code != ReturnCode.Success)
                return result;

            if (result.Data?.Length > 0)
                list.AddRange(result.Data);
        }

        return new(ReturnCode.Success, list.ToArray());
    }

    /// <summary>
    /// ���Ž�Ͷ������ ����������������
    /// �ֶ����ݲ�Ʒ������
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public override async Task<ReturnWrap<RaisingBankTransaction>> QueryRaisingAccountTransction(DateOnly begin, DateOnly end)
    {
        var part = "/institution/tgpt/erp/raise/query/findRaiseAccountDetailList";
        var result = await SyncWork<RaisingBankTransaction, RaisingBankTransactionJson>(part, new { beginDate = begin.ToString("yyyyMMdd"), endDate = end.ToString("yyyyMMdd") }, x => x.ToObject());


        return result;
    }


    /// <summary>
    /// 02	ļ���� 04	�йܻ�
    /// </summary>
    /// <returns></returns>
    public override async Task<ReturnWrap<FundBankBalance>> QueryRaisingBalance()
    {
        var part = "/institution/tgpt/erp/product/query/findAccBalanceList";
        var result = await SyncWork<FundBankBalance, BankBalanceJson>(part, new { accType = "02" }, x => x.ToObject());


        return result;
    }





    public override Task<ReturnWrap<BankTransaction>> QueryCustodialAccountTransction(DateOnly begin, DateOnly end)
    {
        throw new NotImplementedException();
    }










    protected override ReturnCode CheckBreforeSync()
    {
        // ��������� 
        if (!IsValid) return ReturnCode.ConfigInvalid;

        if (string.IsNullOrWhiteSpace(APIKey) || string.IsNullOrWhiteSpace(APISecret) || Encoder is null || Decoder is null)
        {
            SetDisabled();
            return ReturnCode.ConfigInvalid;
        }

        return ReturnCode.Success;
    }


    #region ����
    protected override bool LoadConfigOverride(IAPIConfig config)
    {
        if (config is not APIConfig c)
            return false;

        APIKey = c.APIKey;
        APISecret = c.APISecret;
        EncryptKey = c.EncryptKey;
        return true;
    }

    public override bool Prepare()
    {
        if (string.IsNullOrWhiteSpace(EncryptKey))
            return false;

        byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptKey);

        Encoder = new SM4();
        Encoder.sm4_setkey_enc(new SM4_Context { isPadding = true }, keyBytes);

        Decoder = new SM4();
        Decoder.sm4_setkey_dec(new SM4_Context { isPadding = true }, keyBytes);



        return true;
    }

    protected override IAPIConfig SaveConfigOverride()
    {
        return new APIConfig { EncryptKey = EncryptKey, APIKey = APIKey, APISecret = APISecret };
    }


    #endregion

    #region ͨ�ô��� 




    /// <summary>
    /// �в�
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <param name="param">object �� Dictionary<string, object></param>
    /// <returns></returns>
    protected async Task<ReturnWrap<TEntity>> SyncWork<TEntity, TJSON>(string part, object? param, Func<TJSON, TEntity> transfer, [CallerMemberName] string? caller = null)
    {
        // У��
        if (CheckBreforeSync() is ReturnCode rc && rc != ReturnCode.Success) return new(rc, null);

        // ��dict ת��dict �����޸�page
        Dictionary<string, object> formatedParams;
        if (param is null) formatedParams = new();
        if (param is Dictionary<string, object> pp) formatedParams = pp;
        else formatedParams = GenerateParams(param);

        List<TJSON> list = new();

        int total = 0;
        // ��ȡ���н��
        try
        {
            for (int i = 0; i < 19; i++) // ��ֹ����ѭ�������99�� 
            {
                var json = await Query(part, formatedParams);
                LogRun(caller, formatedParams, json);

                try
                {
                    if (json is null) return new(ReturnCode.EmptyResponse, null);
                    var ret = JsonSerializer.Deserialize<RetJson>(json);

                    int.TryParse(ret!.Code, out var code);

                    // �д���
                    if (code != 0)
                    {
                        Log(caller, json, ret.Msg);
                        return new(TransferReturnCode(code, ret.Msg), null);
                    }

                    // ����ʵ������
                    var data = JsonSerializer.Deserialize<RetJson<TJSON>>(json);
                    //if(data.Data.RowCount != data.Data.Data.Length)
                    //    return new(ReturnCode., null);

                    total += data!.Data.Data.Length;
                    list.AddRange(data.Data.Data);

                    // ��¼���ص����ͣ�����debug
                    Log(caller, data!.Data.Data);

                    // ���ݻ�ȡȫ
                    if (data!.Data.TotalCount >= total)
                        break;

                    // ��һҳ
                    var page = (int)formatedParams["reqPageno"];
                    formatedParams["reqPageno"] = page + 1;
                }
                catch
                {
                    Log(caller, json, "Json Serialize Error");
                    return new(ReturnCode.JsonNotPairToEntity, null);
                }
            }
        }
        catch (Exception e)
        {
            Log(caller, null, e.Message);
            return new(ReturnCode.Unknown, null);
        }


        Log(caller, null, "OK");
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
    /// �����
    /// </summary>
    /// <param name="part"></param>
    /// <param name="param"></param>
    /// <returns>
    /// ��Ӧ��	��Ӧ��Ϣ
    ///10000	��ʱ��
    ///10002	�÷��񲻿��ã�˵������û��������
    ///10003	ǩ�����Ϸ���signǩ�������⣩
    ///10004	ǩ������
    ///10005	��Ȩ����
    ///10007	ȱ�ٲ���apikey
    ///10019	����body���ܴ���
    ///ERP��Ӧ��  ��Ӧ��Ϣ
    ///0000	�ɹ�
    ///0001	ҵ����ʧ��
    ///1001	ϵͳ�ڲ�����
    ///1002	��������ˮ��Ϊ��
    ///1003	����Ϊ��
    ///1005	�������Ϸ�
    ///1006	ҵ��У�鲻ͨ��
    /// </returns>
    public async Task<string?> Query(string part, object? param)
    {
        // ����request
        var request = Build(part, param);
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        //{"errcode":"10005","errmsg":"��Ȩ���ʸ÷���:��api����Ȩ��"}
        if (content.Contains("errcode"))
        {
            // ���뵽 RetJson
            content = content.Replace("errcode", "retCode");
            content = content.Replace("errmsg", "retMsg");
            return content;
        }

        // ����
        var dec = Decoder!.Handle(Convert.FromBase64String(content));
        var json = Encoding.UTF8.GetString(dec!);
        return json;
    }

    /// <summary>
    /// ����request
    /// </summary>
    /// <param name="part"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    private HttpRequestMessage Build(string part, object? param)
    {
        var url = GetUrl(part)!;

        // json
        var paramsStr = param is null ? "" : JsonSerializer.Serialize(param);

        // ����
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

        // ���� msgId
        request.Headers.Add("msgId", Guid.NewGuid().ToString());

        // ���� Content-Type Ϊ application/json 
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
    /// ��ȡ�ַ����� MD5 ֵ �� ����Ĳ�һ��
    /// sb.Append((b & 0xFF).ToString("x2"));
    /// </summary>
    private static string CalcMD5(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // ���ֽ�����ת��Ϊʮ�������ַ���
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
