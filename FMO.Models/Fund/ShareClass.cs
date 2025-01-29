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


/// <summary>
/// 与份额相关的要素
/// </summary>
public class PortionElements
{
    /// <summary>
    /// 份额
    /// </summary>
    public ShareClass? Class { get; set; }

    /// <summary>
    /// 锁定期
    /// </summary>
    public Mutable<SealingRule>? LockingRule { get; set; }

}

