using System.Text.RegularExpressions;

namespace FMO.Models;

public class BankAccount
{
    public int OwnerId { get; set; }



    /// <summary>
    /// 户名
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string? Number { get; set; }

    /// <summary>
    /// 银行
    /// </summary>
    public string? Bank { get; set; }

    /// <summary>
    /// 银行-支行
    /// </summary>
    public string? Branch { get; set; }

    /// <summary>
    /// 开户行
    /// </summary>
    public string? BankOfDeposit { get => Bank + Branch; set => SetDeposit(value); }

    public string? LargePayNo { get; set; }

    /// <summary>
    /// swift
    /// </summary>
    public string? SwiftCode { get; set; }

    /// <summary>
    /// 银行地址
    /// </summary>
    public string? BankAddress { get; set; }



    private void SetDeposit(string? str)
    {
        if (string.IsNullOrWhiteSpace(str)) return;
        var m = Regex.Match(str, @"(\w+银行)(?:.*公司)?(\w+)");
        if (!m.Success) return;

        Bank = m.Groups[1].Value;
        Branch = m.Groups[2].Value;
    }

    public static BankAccount? FromString(string str)
    {
        BankAccount account = new BankAccount();
        var m = Regex.Match(str, @"账号\s*[:：]*(\d+)");
        if (!m.Success) return null;
        account.Number = m.Groups[1].Value;

        m = Regex.Match(str, @"账户名称?\s*[:：]*(\w+)");
        if (!m.Success) return null;
        account.Name = m.Groups[1].Value;

        m = Regex.Match(str, @"(\w+银行)(?:.*公司)?(\w+)");
        if (!m.Success) return null;
        account.BankOfDeposit = m.Value;


        m = Regex.Match(str, @"大额支付号?\s*[:：]*(\w+)");
        if (m.Success) account.LargePayNo = m.Groups[1].Value;

        return account;
    }


    public void FillFrom(string? str)
    {
        if (string.IsNullOrWhiteSpace(str)) return;

        var m = Regex.Match(str, @"账号\s*[:：]*(\d+)");
        if (m.Success)
            Number = m.Groups[1].Value;

        m = Regex.Match(str, @"账户名称?\s*[:：]*(\w+)");
        if (m.Success)
            Name = m.Groups[1].Value;

        m = Regex.Match(str, @"(\w+银行)(?:.*公司)?(\w+)");
        if (m.Success)
            BankOfDeposit = m.Value;

        m = Regex.Match(str, @"大额支付号?\s*[:：]*(\w+)");
        if (m.Success) LargePayNo = m.Groups[1].Value;
    }
}