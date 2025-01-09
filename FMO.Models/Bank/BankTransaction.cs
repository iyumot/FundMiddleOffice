using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.Models;

/// <summary>
/// 银行流水
/// </summary>
public class BankTransaction
{
    public required string Id { get; set; }

    /// <summary>
    /// 付款账号
    /// </summary>
    public required string PayNo { get; set; }

    public required string PayName { get; set; }

    public required string PayBank { get; set; }


    /// <summary>
    /// 收款账号
    /// id 为BankAccount id
    /// </summary>
    public required string ReceiveNo { get; set; }

    public required string ReceiveName { get; set; }

    public required string ReceiveBank { get; set; }


    public DateTime Time { get; set; }


    public decimal Balance { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 来源
    /// </summary>
    public string? Origin { get; set; }


    public required string TransactionId { get; set; }
}
