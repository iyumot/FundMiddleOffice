﻿namespace FMO.Models;



public class AccountEvent
{
    public required string Name { get; set; }


}


/// <summary>
/// 开户事件
/// </summary>
public class OpenAccountEvent : AccountEvent
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


    /// <summary>
    /// 经服
    /// </summary>
    public FileStorageInfo? ServiceAgreement { get; set; }

    /// <summary>
    /// 开户信息
    /// </summary>
    public FileStorageInfo? AccountLetter { get; set; }
}


/// <summary>
/// 股票账户
/// </summary>
public class StockAccount
{
    public int Id { get; set; }

    public int FundId { get; set; }

    /// <summary>
    /// 对应股卡组
    /// </summary>
    public int Group { get; set; }

    public string? Company { get; set; }

    /// <summary>
    /// 基本户
    /// </summary>
    public OpenAccountEvent? Common { get; set; }

    /// <summary>
    /// 信用户
    /// </summary>
    public OpenAccountEvent? Credit { get; set; }


    public List<AccountEvent>? Events { get; set; }

}

public class FutureAccount
{
    public int Id { get; set; }

    public int FundId { get; set; }

    public string? Company { get; set; }

    /// <summary>
    /// 基本户
    /// </summary>
    public OpenAccountEvent? Common { get; set; }



    public List<AccountEvent>? Events { get; set; }

}






/// <summary>
/// 每个基金只有一个的账户
/// </summary>
public class FundSingletonAccounts
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



/// <summary>
/// 募集、托管户等
/// </summary>
public class FundBankAccount : BankAccount
{
    public int FundId { get; set; }

    public string? FundCode { get; set; }

    /// <summary>
    /// 销户
    /// </summary>
    public bool IsCanceled { get; set; }


}