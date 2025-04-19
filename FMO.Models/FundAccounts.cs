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
