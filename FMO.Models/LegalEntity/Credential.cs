using System.ComponentModel;

namespace FMO.Models;

/// <summary>
/// 枚举表示不同的证件类型。
/// </summary>


[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum IDType
{
    [Description("未知")]Unknown,

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

    /// <summary>
    /// 组织机构代码证
    /// </summary>
    [Description("组织机构代码证")]
    OrganizationCodeCertificate,


    /// <summary>
    /// 营业执照
    /// </summary>
    [Description("营业执照")]
    BusinessLicense,

    /// <summary>
    /// 行政机关
    /// </summary>
    [Description("行政机关")]
    AdministrativeAgency,

    /// <summary>
    /// 社会团体
    /// </summary>
    [Description("社会团体")]
    SocialGroup,

    /// <summary>
    /// 军队
    /// </summary>
    [Description("军队")]
    Military,

    /// <summary>
    /// 武警
    /// </summary>
    [Description("武警")]
    ArmedPolice,

    /// <summary>
    /// 下属机构（具有主管单位批文号）
    /// </summary>
    [Description("下属机构（具有主管单位批文号）")]
    SubordinateOrganization,

    /// <summary>
    /// 基金会
    /// </summary>
    [Description("基金会")]
    Foundation,


    /// <summary>
    /// 登记证书
    /// </summary>
    [Description("登记证书")]
    RegistrationCertificate,

    /// <summary>
    /// 批文
    /// </summary>
    [Description("批文")]
    ApprovalDocument,


    /// <summary>
    /// 其他证件。
    /// </summary>
    [Description("其他")]
    Other,
}


/// <summary>
/// 证件
/// </summary>
public record struct Identity
{
    /// <summary>
    /// 证件类型
    /// </summary>
    public IDType Type { get; set; }

    /// <summary>
    /// 证件号码
    /// </summary>
    public required string Id { get; set; }

    public override int GetHashCode()
    {
        return Type.GetHashCode() ^ Id.GetHashCode();
    }
}


