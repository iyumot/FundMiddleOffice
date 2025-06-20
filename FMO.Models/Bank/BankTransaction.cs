﻿namespace FMO.Models;



public enum TransctionDirection { Pay, Receive };

/// <summary>
/// 银行流水
/// </summary>
public class BankTransaction
{
    public required string Id { get; set; }

    /// <summary>
    /// 方向
    /// </summary>
    public TransctionDirection Direction { get; set; }

    /// <summary>
    /// 付款账号
    /// </summary>
    public required string AccountNo { get; set; }

    public required string AccountName { get; set; }

    public required string AccountBank { get; set; }


    /// <summary>
    /// 收款账号
    /// id 为BankAccount id
    /// </summary>
    public required string TheirNo { get; set; }

    public required string TheirName { get; set; }

    public required string TheirBank { get; set; }


    public DateTime Time { get; set; }


    /// <summary>
    /// 余额
    /// </summary>
    public decimal? Balance { get; set; }

    /// <summary>
    /// 发生金额
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 流水号
    /// </summary>
    public required string Serial { get; set; }
}
