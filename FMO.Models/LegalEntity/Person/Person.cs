namespace FMO.Models;

/// <summary>
/// 自然人
/// </summary>
public class Person : LegalEntity
{
    /// <summary>
    /// 称谓
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    public string? Cellphone { get; set; }

    /// <summary>
    /// 固话
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? Email { get; set; }

    public override EntityType Type => EntityType.Natrual;
}