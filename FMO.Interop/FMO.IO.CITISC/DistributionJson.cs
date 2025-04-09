using FMO.Models;

namespace FMO.IO.Trustee.CITISC.Json.Distribution;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
public class Data
{
    public int? startRow { get; set; }
    public List<int?> navigatepageNums { get; set; }
    public int? prePage { get; set; }
    public bool? hasNextPage { get; set; }
    public int? nextPage { get; set; }
    public int? pageSize { get; set; }
    public int? endRow { get; set; }
    public List<Item> list { get; set; }
    public int? pageNum { get; set; }
    public int? navigatePages { get; set; }
    public int? total { get; set; }
    public int? navigateFirstPage { get; set; }
    public int? pages { get; set; }
    public int? size { get; set; }
    public bool? isLastPage { get; set; }
    public bool? hasPreviousPage { get; set; }
    public int? navigateLastPage { get; set; }
    public bool? isFirstPage { get; set; }
}

public class Item
{
    public string date { get; set; }
    public string pdCode { get; set; }
    public decimal? fare { get; set; }
    public decimal? realShares { get; set; }
    public string flag { get; set; }
    public decimal? totalProfit { get; set; }
    public string tradeAcco { get; set; }
    public string docTypeCn { get; set; }
    public string cserialNo { get; set; }
    public string regDate { get; set; }
    public decimal? deductbalance { get; set; }
    public string unitProfit { get; set; }
    public string shareType { get; set; }
    public string dateNew { get; set; }
    public decimal? frozenShares { get; set; }
    public string flagCn { get; set; }
    public string pdName { get; set; }
    public string lastDate { get; set; }
    public decimal? realBalance { get; set; }
    public decimal? managerFee { get; set; }
    public string custTypeCn { get; set; }
    public string netValue { get; set; }
    public string docType { get; set; }
    public string exDividendDate { get; set; }
    public decimal? reinvestBalance { get; set; }
    public decimal? frozenBalance { get; set; }
    public decimal? tax { get; set; }
    public string custName { get; set; }
    public string agencyNo { get; set; }
    public string docNo { get; set; }
    public string agencyName { get; set; }
    public string pdShotName { get; set; }
    public string confirmDate { get; set; }
    public string fundAcco { get; set; }
    public string custType { get; set; }
    public decimal? totalShare { get; set; }

    internal Models.TransferRecord ToObject(string identifier)
    {
        if (!DateOnly.TryParse(confirmDate, out var con))
            DateOnly.TryParseExact(confirmDate, "yyyyMMdd", out con);

        return new Models.TransferRecord
        {
            CustomerName = custName,
            CustomerIdentity = docNo,
            FundName = pdName,
            FundCode = pdCode,
            Agency = agencyName,
            Type = TARecordType.Distribution,
            RequestDate = DateOnly.Parse(regDate),
            RequestShare = totalShare ?? 0,
            RequestAmount = 0,
            CreateDate = con,
            ConfirmedDate = con,
            ConfirmedShare = realShares ?? 0,
            ConfirmedAmount = totalProfit ?? 0,
            ConfirmedNetAmount = realBalance ?? 0,
            ExternalId = cserialNo,
            Source = identifier,
            Fee = 0,
            PerformanceFee = managerFee ?? 0,
        };
    }
}

public class Root
{
    public decimal? code { get; set; }
    public Data data { get; set; }
    public bool? success { get; set; }
    public object message { get; set; }
    public object token { get; set; }
}


#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。