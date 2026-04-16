using FMO.Models;

namespace FMO.Disclosure;

public interface IDisclosureReport
{
    public int Id { get; set; }

    public DisclosureType Type { get; set; }

    public int FundId { get; set; }

    /// <summary>
    /// 期望发布日期
    /// </summary>
    public DateOnly PublishDate { get; set; }

    /// <summary>
    /// 报告名称
    /// </summary>
    public string Name { get; set; }

}


/// <summary>
/// 定期报告
/// 月报、季报、半年报、年报等
/// </summary>
public class PeriodicalDisclosureReport : IDisclosureReport
{
    public int Id { get; set; }
    
    public DisclosureType Type { get; set; }
    
    public int FundId { get; set; }
    
    public DateOnly PublishDate { get; set; }

    public required string Name { get; set; }




    public SimpleFile? Word { get; set; }

    public SimpleFile? Excel { get; set; }

    public SimpleFile? Xbrl { get; set; }

    public SimpleFile? Pdf { get; set; }

    public SimpleFile? Sealed { get; set; }


}

/// <summary>
/// 临时报告
/// </summary>
public class TemporaryDisclosureReport : IDisclosureReport
{
    public int Id { get; set; }
    
    public DisclosureType Type { get; set; }
    
    public int FundId { get; set; }
    
    public DateOnly PublishDate { get; set; }
    
    public required string Name { get; set; }

    public SimpleFile? File { get; set; }
}