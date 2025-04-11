﻿using System.ComponentModel;

namespace FMO.Models;

/// <summary>
/// TA 记录
/// </summary>
[Description("TA记录")]
public class TransferRecord : IEquatable<TransferRecord>
{
    public int Id { get; set; }

    public int FundId { get; set; }

    public string? FundName { get; set; }

    public string? FundCode { get; set; }


    public string? ExternalId { get; set; }

    public string? ExternalRequestId { get; set; }

    public int RequestId { get; set; }

    /// <summary>
    /// 份额类型
    /// </summary>
    public string? ShareClass { get; set; }


    /// <summary>
    /// 内部id
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// 客户Id
    /// </summary>
    [Description("证件号码")]
    public required string CustomerIdentity { get; set; }

    [Description("客户名称")]
    public required string CustomerName { get; set; }


    public DateOnly CreateDate { get; set; }

    /// <summary>
    /// 申请时间
    /// </summary>
    [Description("申请日期")]
    public DateOnly RequestDate { get; set; }

    /// <summary>
    /// 确认时间
    /// </summary>
    [Description("确认日期")]
    public DateOnly ConfirmedDate { get; set; }

    /// <summary>
    /// 交易类型
    /// </summary> 
    [Description("业务类型")]
    public TARecordType Type { get; set; }

    /// <summary>
    /// 申请份额
    /// </summary>
    [Description("申请份额")]
    public decimal RequestShare { get; set; }

    /// <summary>
    /// 申请金额
    /// </summary>
    [Description("申请金额")]
    public decimal RequestAmount { get; set; }

    /// <summary>
    /// 确认份额
    /// </summary>
    [Description("确认份额")]
    public decimal ConfirmedShare { get; set; }

    /// <summary>
    /// 确认金额
    /// </summary>
    [Description("确认金额")]
    public decimal ConfirmedAmount { get; set; }

    /// <summary>
    /// 确认净额（扣费）
    /// </summary>
    [Description("确认净额")]
    public decimal ConfirmedNetAmount { get; set; }

    /// <summary>
    /// 手续费用
    /// </summary>
    [Description("交易费用")]
    public decimal Fee { get; set; }

    /// <summary>
    /// 业绩报酬
    /// </summary>
    [Description("业绩报酬")]
    public decimal PerformanceFee { get; set; }

    [Description("销售机构")]
    public string? Agency { get; set; }

    /// <summary>
    /// 数据来源
    /// </summary>
    public string? Source { get; set; }

    public override int GetHashCode()
    {
        return FundId ^ CustomerName.GetHashCode() ^ RequestDate.GetHashCode() ^ Type.GetHashCode();
    }
    public bool Equals(TransferRecord? other)
    {
        if (other is null) return false;

        return FundId == other.FundId && CustomerIdentity == other.CustomerIdentity && RequestDate == other.RequestDate && Type == other.Type;
    }

    public decimal ShareChange()
    {
        switch (Type)
        {
            case TARecordType.Subscription:
            case TARecordType.Purchase:
            case TARecordType.Increase:
            case TARecordType.Distribution:
                return ConfirmedShare;
            case TARecordType.Redemption:
            case TARecordType.ForceRedemption:
            case TARecordType.Decrease:
                return -ConfirmedShare; 
            default:
                return 0;
        }
    }
    public decimal AmountChange()
    {
        switch (Type)
        {
            case TARecordType.Subscription:
            case TARecordType.Purchase:
            case TARecordType.Increase:
            case TARecordType.Distribution:
                return -ConfirmedAmount;
            case TARecordType.Redemption:
            case TARecordType.ForceRedemption:
            case TARecordType.Decrease:
                return ConfirmedAmount;
            default:
                return 0;
        }
    }
}

