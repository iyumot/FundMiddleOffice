using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.Models;

public enum SecurityCardType
{
    Unk,

    ShangHai,

    ShenZhen
}

/// <summary>
/// 股东卡
/// </summary>
public class SecurityCard
{
    public int Id { get; set; }

    public int FundId { get; set; }

    /// <summary>
    /// 流水号
    /// </summary>
    public required string SerialNo { get; set; }

    /// <summary>
    /// 子账户号
    /// </summary>
    public required string CardNo { get; set; }

    /// <summary>
    /// 一码通
    /// </summary>
    public required string UniversalNo { get; set; }

    public required string Name { get; set; }

    public string? FundCode { get; set; }

    /// <summary>
    /// 沪、深
    /// </summary>
    public SecurityCardType Type { get; set; }

    /// <summary>
    /// 申请日期
    /// </summary>
    public DateOnly Date { get; set; }


}



public class AccountEvent
{
    public required string Name { get; set; }

    
}


public class BasicAccountEvent:AccountEvent
{
    /// <summary>
    /// 资金账号
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// 交易密码
    /// </summary>
    public string? TradePassword { get; set; }

    /// <summary>
    /// 资金密码
    /// </summary>
    public string? CapitalPassword { get; set; }

    /// <summary>
    /// 银证、银期等
    /// </summary>
    public FileStorageInfo? BankLetter { get; set; }
}


/// <summary>
/// 股票账户
/// </summary>
public class StockAccount
{
    public int Id { get; set; }

    public int FundId { get; set; }

    public string? Company { get; set; }

    /// <summary>
    /// 基本户
    /// </summary>
    public BasicAccountEvent? Common { get; set; }

    /// <summary>
    /// 信用户
    /// </summary>
    public BasicAccountEvent? Credit { get; set; }


    public List<AccountEvent>? Events { get; set; }

}




/// <summary>
/// 
/// </summary>
public class FundAccounts
{
    public int Id { get; set; }

    /// <summary>
    /// 一码通
    /// </summary>
    public string? UniversalNo { get; set; }

    /// <summary>
    /// 统一开户编码
    /// </summary>
    public string? FutureNo { get; set; }

    /// <summary>
    /// 中债登债券账户
    /// </summary>
    public string? CCDCBondAccount { get; set; }

    /// <summary>
    /// 上清所债券账户
    /// </summary>
    public string? SHCBondAccount { get; set; }







}
