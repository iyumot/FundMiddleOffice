namespace FMO.Models;

/// <summary>
/// 子产品映射
/// </summary>
public class SubjectFundMapping
{
    public int Id { get; set; }

    public required string FundName { get; set; }

    public required string FundCode { get; set; }

    /// <summary>
    /// 主产品
    /// </summary>
    public string? MasterName { get; set; }

    /// <summary>
    /// 主产品
    /// </summary>
    public string? MasterCode { get; set; }

    /// <summary>
    /// 份额名
    /// </summary>
    public string? ShareClass { get; set; }
}