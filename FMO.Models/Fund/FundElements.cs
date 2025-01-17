﻿namespace FMO.Models;







public class FundElements
{
    public int Id { get; set; }

    public required int FundId { get; set; }


    /// <summary>
    /// 名称
    /// </summary>
    public Mutable<string>? FullName { get; set; }

    /// <summary>
    /// 简称
    /// </summary>
    public Mutable<string>? ShortName { get; set; }

    /// <summary>
    /// 风险等级
    /// </summary>
    public Mutable<RiskLevel?>? RiskLevel { get; set; }

    /// <summary>
    /// 存续期
    /// </summary>
    public Mutable<int?>? DurationInMonths { get; set; }




    /// <summary>
    /// 结束日期
    /// </summary>
    public Mutable<DateOnly?>? ExpirationDate { get; set; }


    /// <summary>
    /// 主募集账户
    /// </summary>
    public Mutable<BankAccount>? CollectionAccount { get; set; }


    /// <summary>
    /// 主托管账户
    /// </summary>
    public Mutable<BankAccount>? CustodyAccount { get; set; }



    public Mutable<ShareClass[]>? ShareClasses { get; set; }




    public static FundElements Create(int fundid)
    {
        return new FundElements
        {
            FundId = fundid,

        };
    }

    public bool Init()
    {
        bool changed = false;

        if (FullName is null)
        { changed = true; FullName = new Mutable<string>(nameof(FullName), "基金全称"); }

        if (ShortName is null)
        { changed = true; ShortName = new Mutable<string>(nameof(ShortName), "基金简称"); }

        if (RiskLevel is null)
        { changed = true; RiskLevel = new Mutable<RiskLevel?>(nameof(RiskLevel), "风险等级"); }

        if (DurationInMonths is null)
        { changed = true; DurationInMonths = new Mutable<int?>(nameof(DurationInMonths), "存续期（月）"); }

        if (ExpirationDate is null)
        { changed = true; ExpirationDate = new Mutable<DateOnly?>(nameof(ExpirationDate), "到期日"); }

        if (CollectionAccount is null)
        { changed = true; CollectionAccount = new Mutable<BankAccount>(nameof(CollectionAccount), "募集账户"); }

        if (CustodyAccount is null)
        { changed = true; CustodyAccount = new Mutable<BankAccount>(nameof(CustodyAccount), "托管账户"); }


        if (ShareClasses is null)
        { changed = true; ShareClasses = new Mutable<ShareClass[]>(nameof(ShareClasses), "份额类别"); }


        return changed;
    }
}