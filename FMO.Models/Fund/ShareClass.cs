namespace FMO.Models;

/// <summary>
/// 有不同份额安排
/// </summary>
public class ShareClass
{
    /// <summary>
    /// 份额名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 要求
    /// </summary>
    public string? Requirement { get; set; }
}