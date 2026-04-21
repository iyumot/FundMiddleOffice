using FMO.Models;

namespace FMO.Disclosure;

public interface IDisclosureNotice
{
    public long Id { get; }

    public DisclosureType Type { get; }


    /// <summary>
    /// 期望发布日期
    /// </summary>
    public DateOnly PublishDate { get; }

    /// <summary>
    /// 报告名称
    /// </summary>
    public string Name { get; }

}

public interface IFundDisclosureNotice : IDisclosureNotice
{
    public int FundId { get; }

    public string FundName { get; }

    public string FundCode { get; }
}


/// <summary>
/// 定期报告
/// 月报、季报、半年报、年报等
/// </summary>
public class PeriodicalDisclosureNotice : IFundDisclosureNotice
{
    public long Id =>  FundId << 10 | ((int)Type);

    public DisclosureType Type { get; set; }

    public int FundId { get; set; }

    public required string FundName { get; set; }

    public required string FundCode { get; set; }

    public DateOnly PublishDate { get; set; }

    public required string Name { get; set; }


    public DateOnly ReportDate { get; set; }


    public SimpleFile? Word { get; set; }

    public SimpleFile? Excel { get; set; }

    public SimpleFile? Xbrl { get; set; }

    public SimpleFile? Pdf { get; set; }

    public SimpleFile? Sealed { get; set; }


}

/// <summary>
/// 临时报告
/// </summary>
public class TemporaryDisclosureNotice : IFundDisclosureNotice
{
    public long Id { get; set; }

    public DisclosureType Type { get; set; }

    public int FundId { get; set; }
    public required string FundName { get; set; }

    public required string FundCode { get; set; }

    public DateOnly PublishDate { get; set; }

    public required string Name { get; set; }

    public SimpleFile? File { get; set; }
}

/// <summary>
/// 临时开放公告
/// </summary>
public class TemporaryOpenNotice : IFundDisclosureNotice
{
    public long Id { get; set; }

    public DisclosureType Type => DisclosureType.TemporaryOpen;

    public int FundId { get; set; }
    public required string FundName { get; set; }

    public required string FundCode { get; set; }

    public DateOnly PublishDate { get; set; }

    public string Name => $"{FundName} 临时开放公告";

    public DateOnly OpenDay { get; set; }

    public bool AllowPurchase { get; set; }

    public bool AllowRedemption { get; set; }



}


/// <summary>
/// 巨额赎回公告
/// </summary>
public class HugeRedemptionNotice : IFundDisclosureNotice
{
    public long Id { get; set; }

    public DisclosureType Type => DisclosureType.HugeRedemption;

    public int FundId { get; set; }

    public required string FundName { get; set; }

    public required string FundCode { get; set; }

    public DateOnly PublishDate { get; set; }

    public string Name => $"{FundName} 巨额赎回公告";

    public DateOnly OpenDay { get; set; }

    /// <summary>
    /// 赎回比例
    /// </summary>
    public decimal Ratio { get; set; }

    /// <summary>
    /// 是否全部兑付
    /// </summary>
    public bool IsFullyPaied { get; set; }
}

/// <summary>
/// 产品成立公告
/// </summary>
public class FundSetupNotice : IFundDisclosureNotice
{
    public long Id { get; set; }

    public DisclosureType Type => DisclosureType.FundSetup;

    public int FundId { get; set; }

    public required string FundName { get; set; }

    public required string FundCode { get; set; }

    public DateOnly PublishDate { get; set; }

    public string Name => $"{FundName} 产品成立公告";

    public DateOnly SetupDay { get; set; }
}







/// <summary>
/// 管理人公告
/// </summary>
public class ManagerDisclosureNotice : IDisclosureNotice
{
    public long Id { get; set; }

    public DisclosureType Type { get; set; }

    public DateOnly PublishDate { get; set; }

    public required string Name { get; set; }

    public SimpleFile? File { get; set; }
}