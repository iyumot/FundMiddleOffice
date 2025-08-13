using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.IO.DS.MeiShi.Json.OrderDetail;

/// <summary>
/// 根级JSON响应实体类
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// 响应状态码（1008表示请求成功）
    /// </summary>
    public int code { get; set; }

    /// <summary>
    /// 响应数据主体
    /// </summary>
    public ResponseData data { get; set; }

    /// <summary>
    /// 响应消息（如"请求成功"）
    /// </summary>
    public string message { get; set; }

    /// <summary>
    /// 总记录数（此处为0，可能为预留字段）
    /// </summary>
    public int total { get; set; }
}

/// <summary>
/// 响应数据主体类（包含核心业务数据）
/// </summary>
public class ResponseData
{
    public string? InvestorName { get; set; }

    public string? productName { get; set; }

    public string? cardNumber { get; set; }

    public string? openDay { get; set; }

    public string? bankName { get; set; }
    public string? subbranch { get; set; }
    public string? accountNumber { get; set; }

    public int signType { get; set; }


    public decimal? redemptionMoney { get; set; }

    public int? redemptionType { get; set; }

    public decimal? tradeMoney { get; set; }
    
    public decimal? tradeShare { get; set; }

    public string? doubleRecordingUrl { get; set; }

    public List<SealedDocument>? sealedDocuments { get; set; }
}

public class SealedDocument
{
    public string? documentName { get; set; }

    public string? sealedUrl { get; set; }

    public string? sealTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int? codeType { get; set; }
}