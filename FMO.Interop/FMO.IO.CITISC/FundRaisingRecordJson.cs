using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FMO.IO.Trustee.CITISC.Json.FundRasing;


#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
internal class Root
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("pdCode")]
    public string PdCode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("note")]
    public string Note { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("curType")]
    public string CurType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("checkItem")]
    public string CheckItem { get; set; }

    /// <summary>
    /// 中国工商银行股份有限公司海盐支行
    /// </summary>
    [JsonPropertyName("recBankName")]
    public string RecBankName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("pdType")]
    public string PdType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("refundType")]
    public string RefundType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("zhzt")]
    public string Zhzt { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("balance")]
    public string Balance { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("bankJour")]
    public string BankJour { get; set; }

    /// <summary>
    /// 鑫享世宸1号私募证券投资基金
    /// </summary>
    [JsonPropertyName("pdName")]
    public string PdName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("pdId")]
    public string PdId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("tradeTm")]
    public DateTime TradeTm { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("btnEnable")]
    public string BtnEnable { get; set; }

    /// <summary>
    /// 鑫享世宸1号私募证券投资基金募集专户
    /// </summary>
    [JsonPropertyName("payAccName")]
    public string PayAccName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("direction")]
    public string Direction { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("payAccNo")]
    public string PayAccNo { get; set; }

    /// <summary>
    /// 中信银行北京瑞城中心支行
    /// </summary>
    [JsonPropertyName("payBankName")]
    public string PayBankName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("occurDate")]
    public string OccurDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("acctBal")]
    public string AcctBal { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("bankSlipNo")]
    public string BankSlipNo { get; set; }

    /// <summary>
    /// 汤雪明
    /// </summary>
    [JsonPropertyName("recAccName")]
    public string RecAccName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("flowStatus")]
    public string FlowStatus { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("recAccNo")]
    public string RecAccNo { get; set; }
}

internal class Data
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
    public List<Root> List { get; set; }

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

internal class JsonRootDto
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
    /// 
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; }
}


#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。