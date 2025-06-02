using System.ComponentModel;

namespace FMO.Models;

/// <summary>
/// 枚举表示不同的证件类型。
/// </summary>


[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum IDType
{
    [Description("未知")] Unknown,

    /// <summary>
    /// 居民身份证。
    /// </summary>
    [Description("居民身份证")]
    IdentityCard,

    /// <summary>
    /// 中国护照。
    /// </summary>
    [Description("中国护照")]
    PassportChina,

    /// <summary>
    /// 军官证。
    /// </summary>
    [Description("军官证")]
    OfficerID,

    /// <summary>
    /// 士兵证。
    /// </summary>
    [Description("士兵证")]
    SoldierID,

    /// <summary>
    /// 港澳居民来往内地通行证。
    /// </summary>
    [Description("港澳居民来往内地通行证")]
    HongKongMacauPass,

    /// <summary>
    /// 户口本。
    /// </summary>
    [Description("户口本")]
    HouseholdRegister,

    /// <summary>
    /// 外国护照。
    /// </summary>
    [Description("外国护照")]
    PassportForeign,

    /// <summary>
    /// 文职证。
    /// </summary>
    [Description("文职证")]
    CivilianID,

    /// <summary>
    /// 警官证。
    /// </summary>
    [Description("警官证")]
    PoliceID,

    /// <summary>
    /// 台胞证。
    /// </summary>
    [Description("台胞证")]
    TaiwanCompatriotsID,

    /// <summary>
    /// 外国人永久居留身份证。
    /// </summary>
    [Description("外国人永久居留身份证")]
    ForeignPermanentResidentID,

    Institusion,

    [Description("统一社会信用代码")]
    UnifiedSocialCreditCode,

    [Description("组织机构代码证")]
    OrganizationCodeCertificate,

    [Description("营业执照号")]
    BusinessLicenseNumber,

    [Description("注册号")]
    RegistrationNumber,

    [Description("管理人登记编码")]
    ManagerRegistrationCode,

    [Description("产品备案编码")]
    ProductFilingCode,

    [Description("证券业务许可证")]
    SecuritiesBusinessLicense,


    [Description("产品登记编码")]
    ProductRegistrationCode,

    [Description("港澳台居民居住证")]
    ResidencePermitForHongKongMacaoAndTaiwanResidents,

    [Description("信托登记系统产品编码")]
    TrustRegistrationSystemProductCode,


    /// <summary>
    /// 其他证件。
    /// </summary>
    [Description("其他")]
    Other,
}


/// <summary>
/// 证件
/// </summary>
public record class Identity 
{
    /// <summary>
    /// 证件号码
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// 证件类型
    /// </summary>
    public IDType Type { get; set; }


    public string? Other { get; set; }

     

    public override int GetHashCode()
    {
        return Type.GetHashCode() ^ (Id ?? "").GetHashCode();
    }
}


