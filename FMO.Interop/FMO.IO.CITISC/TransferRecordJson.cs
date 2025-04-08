using FMO.Models;

namespace FMO.IO.Trustee.CITISC.Json.TransferRecord;


#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
public class Data
{
    public int startRow { get; set; }
    public List<int> navigatepageNums { get; set; }
    public int prePage { get; set; }
    public bool hasNextPage { get; set; }
    public int nextPage { get; set; }
    public int pageSize { get; set; }
    public int endRow { get; set; }
    public List<List> list { get; set; }
    public int pageNum { get; set; }
    public int navigatePages { get; set; }
    public int total { get; set; }
    public int navigateFirstPage { get; set; }
    public int pages { get; set; }
    public int size { get; set; }
    public bool isLastPage { get; set; }
    public bool hasPreviousPage { get; set; }
    public int navigateLastPage { get; set; }
    public bool isFirstPage { get; set; }
}

public class List
{
    public object causeCn { get; set; }
    public string oriConfirmDate { get; set; }
    public string totalFare { get; set; }
    public string docTypeCn { get; set; }
    public string chargeType { get; set; }
    public string agencyFare { get; set; }
    public string profitBalance { get; set; }
    public int? unShares { get; set; }
    public string sharetypeCn { get; set; }
    public string oriAgio { get; set; }
    public string chargeTypeCn { get; set; }
    public object oriCserialNo { get; set; }
    public string taFlag { get; set; }
    public string remainshares { get; set; }
    public string fejb { get; set; }
    public object qingSuanId { get; set; }
    public object bzsm { get; set; }
    public string nameInBank { get; set; }
    public bool? detailsFlag { get; set; }
    public object forceredemptiontype { get; set; }
    public string manageFee { get; set; }
    public string custTypeCn { get; set; }
    public double? netValue { get; set; }
    public object qingSuanStepName { get; set; }
    public string agencyNo { get; set; }
    public string docNo { get; set; }
    public string exceedFlag { get; set; }
    public string fundAcco { get; set; }
    public string registFare { get; set; }
    public string custType { get; set; }
    public string status { get; set; }
    public string netValueDate { get; set; }
    public string pdCode { get; set; }
    public string exceedFlagCn { get; set; }
    public object shareRatio { get; set; }
    public string tradeAcco { get; set; }
    public decimal? netConfirmBalance { get; set; }
    public string currentShares { get; set; }
    public string cserialNo { get; set; }
    public string businFlagCn { get; set; }
    public string bankName { get; set; }
    public string interestShare { get; set; }
    public string bankAcco { get; set; }
    public object adjustCause { get; set; }
    public decimal? shares { get; set; }
    public string taFare { get; set; }
    public string businFlag { get; set; }
    public string interest { get; set; }
    public object bonustypeCn { get; set; }
    public string requestDate { get; set; }
    public string pdName { get; set; }
    public string fundFare { get; set; }
    public object earlyDate { get; set; }
    public string agio { get; set; }
    public object realBalance { get; set; }
    public object otherCode { get; set; }
    public string statusCn { get; set; }
    public string docType { get; set; }
    public decimal? requestBalance { get; set; }
    public object netBalance { get; set; }
    public string tradeFare { get; set; }
    public string breachFare { get; set; }
    public string custName { get; set; }
    public string requestNo { get; set; }
    public decimal? totalConfirmBalance { get; set; }
    public string agencyName { get; set; }
    public string pdShotName { get; set; }
    public string requestTime { get; set; }
    public string confirmDate { get; set; }
    public int? definedFee { get; set; }
    public object profitdiscount { get; set; }
    public decimal? confirmShares { get; set; }
    public decimal? ljjz { get; set; }
    public decimal? confirmBalance { get; set; }

    internal Models.TransferRecord ToObject(string identifier)
    {
        decimal.TryParse(profitBalance, out decimal pf);
        decimal.TryParse(totalFare, out var fee);

        return new Models.TransferRecord
        { 
            CustomerName = custName,
            CustomerIdentity = docNo,
            FundName = pdName,
            FundCode = pdCode,
            Agency = agencyName,
            Type = businFlagCn switch { "申购" => TARecordType.Purchase, "赎回" => TARecordType.Redemption, "强制调增" => TARecordType.Increase, "强制调减" => TARecordType.Decrease, "基金成立"=> TARecordType.Subscription, _ => TARecordType.UNK },
            RequestDate = DateOnly.Parse(requestDate),
            RequestShare = shares ?? 0,
            RequestAmount = requestBalance ?? 0,
            CreateDate = DateOnly.Parse(confirmDate),
            ConfirmedDate = DateOnly.Parse(confirmDate),
            ConfirmedShare = confirmShares ?? 0,
            ConfirmedAmount = confirmBalance ?? 0,
            ConfirmedNetAmount = netConfirmBalance ?? 0,
            ExternalId = cserialNo,
            ExternalRequestId = requestNo,
            Source = identifier,
            Fee = fee - pf,
            PerformanceFee = pf,
        };
    }
}

public class Root
{
    public int code { get; set; }
    public Data data { get; set; }
    public bool success { get; set; }
    public object message { get; set; }
    public object token { get; set; }
}



#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
