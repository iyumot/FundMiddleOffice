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


    public string? Cellphone { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public override EntityType Type => EntityType.Natrual;
}