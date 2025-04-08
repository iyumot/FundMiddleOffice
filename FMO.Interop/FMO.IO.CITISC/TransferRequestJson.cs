using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.IO.Trustee.CITISC.Json.TransferRequest;


#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
public class JsonRootDto
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("traceId")]
    public string TraceId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("data")]
    public Data Data { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 操作成功！
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }
}

public class Data
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("startRow")]
    public int StartRow { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("navigatepageNums")]
    public List<int> NavigatepageNums { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("prePage")]
    public int PrePage { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("nextPage")]
    public int NextPage { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("endRow")]
    public int EndRow { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("list")]
    public List<DataItem> List { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("pageNum")]
    public int PageNum { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("navigatePages")]
    public int NavigatePages { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("navigateFirstPage")]
    public int NavigateFirstPage { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("pages")]
    public int Pages { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("size")]
    public int Size { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("isLastPage")]
    public bool IsLastPage { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("navigateLastPage")]
    public int NavigateLastPage { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("isFirstPage")]
    public bool IsFirstPage { get; set; }
}

public class DataItem
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("pdCode")]
    public string PdCode { get; set; }

    /// <summary>
    /// 取消
    /// </summary>
    [JsonPropertyName("exceedFlagCn")]
    public string ExceedFlagCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("netNo")]
    public string NetNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("tradeAcco")]
    public string TradeAcco { get; set; }

    /// <summary>
    /// 营业执照
    /// </summary>
    [JsonPropertyName("docTypeCn")]
    public string DocTypeCn { get; set; }

    /// <summary>
    /// 人民币
    /// </summary>
    [JsonPropertyName("cmoneytypeCn")]
    public string CmoneytypeCn { get; set; }

    /// <summary>
    /// 赎回
    /// </summary>
    [JsonPropertyName("businFlagCn")]
    public string BusinFlagCn { get; set; }

    /// <summary>
    /// 上海浦东发展银行股份有限公司宁波海曙支行
    /// </summary>
    [JsonPropertyName("bankName")]
    public string BankName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("shareLevelCn")]
    public string ShareLevelCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("bankAcco")]
    public string BankAcco { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("forecastSettleFee")]
    public string ForecastSettleFee { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("targPrdCode")]
    public string TargPrdCode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("shouldDeliverDate")]
    public DateOnly? ShouldDeliverDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("shares")]
    public decimal Shares { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("shjsr")]
    public string Shjsr { get; set; }

    /// <summary>
    /// 都按TA计算费用
    /// </summary>
    [JsonPropertyName("chargeTypeCn")]
    public string ChargeTypeCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("taFlag")]
    public string TaFlag { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("businFlag")]
    public string BusinFlag { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("balance")]
    public decimal Balance { get; set; }

    /// <summary>
    /// 人民币
    /// </summary>
    [JsonPropertyName("cMoneytypeCn")]
    public string CMoneytypeCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("requestDate")]
    public DateOnly RequestDate { get; set; }

    /// <summary>
    /// 鑫享世宸1号私募证券投资基金
    /// </summary>
    [JsonPropertyName("pdName")]
    public string PdName { get; set; }

    /// <summary>
    /// 荣理(宁波)信息科技有限公司
    /// </summary>
    [JsonPropertyName("nameInBank")]
    public string NameInBank { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("agio")]
    public string Agio { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("tzrlx")]
    public string Tzrlx { get; set; }

    /// <summary>
    /// 确认成功
    /// </summary>
    [JsonPropertyName("statusCn")]
    public string StatusCn { get; set; }

    /// <summary>
    /// 机构
    /// </summary>
    [JsonPropertyName("custTypeCn")]
    public string CustTypeCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("docType")]
    public string DocType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("netValue")]
    public string NetValue { get; set; }

    /// <summary>
    /// 荣理(宁波)信息科技有限公司
    /// </summary>
    [JsonPropertyName("custName")]
    public string CustName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("agencyNo")]
    public string AgencyNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("requestNo")]
    public string RequestNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("docNo")]
    public string DocNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("outBusinCode")]
    public string OutBusinCode { get; set; }

    /// <summary>
    /// 直销
    /// </summary>
    [JsonPropertyName("agencyName")]
    public string AgencyName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("requestTime")]
    public string RequestTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("sgjsr")]
    public string Sgjsr { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("exceedFlag")]
    public string ExceedFlag { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("definedFee")]
    public int DefinedFee { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("fundAcco")]
    public string FundAcco { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("custType")]
    public string CustType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("bookingDate")]
    public string BookingDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("shouldConfirmDate")]
    public DateOnly? ShouldConfirmDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; }

    public  RequestType Type
    {
        get
        {
            switch (BusinFlagCn.Trim())
            {
                case "认购":
                    return RequestType.Subscription;
                case "申购":
                    return RequestType.Purchase;
                case "赎回":
                    return RequestType.Redemption;
                case "强制赎回":
                    return RequestType.ForceRedemption;
                case "调减":
                case "强制调减":
                case "份额调减":
                    return RequestType.Decrease;
                case "调增":
                case "强制调增":
                case "份额调增":
                    return RequestType.Increase;
                default:
                    return RequestType.UNK;
            }
        }
    }

    public FMO.Models.TransferRequest ToObject(string pid)
    {
        return new Models.TransferRequest
        {
            CustomerName = CustName,
            CustomerIdentity = DocNo,
            FundName = PdName,
            FundCode = PdCode,
            Agency = AgencyName,
            RequestType = Type,
            RequestDate = RequestDate,
            RequestShare = Shares,
            CreateDate = RequestDate,
            ExternalId = RequestNo,
            Source = pid,
            FeeDiscount = DefinedFee,
            RequestAmount = Balance,
            LargeRedemptionFlag = ExceedFlag switch { "0" => LargeRedemptionFlag.CancelRemaining, "1" => LargeRedemptionFlag.RollOver, _ => LargeRedemptionFlag.Unk }
        };
    }
}





#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。