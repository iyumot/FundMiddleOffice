namespace FMO.Disclosure;

/// <summary>
/// 信批类型
/// </summary>
public enum DisclosureType
{
    /// <summary>
    /// 月报
    /// </summary>
    Monthly = 1,

    /// <summary>
    /// 季报
    /// </summary>
    Quarterly = 2,

    /// <summary>
    /// 半年报
    /// </summary>
    SemiAnnually = 3,

    /// <summary>
    /// 年报
    /// </summary>
    Annually = 4,


    /// <summary>
    /// 临时报告
    /// </summary>
    Temporary = 99,

    /// <summary>
    /// 临时开放
    /// </summary>
    TemporaryOpen,

    /// <summary>
    /// 巨额赎回
    /// </summary>
    HugeRedemption,

    /// <summary>
    /// 基金成立
    /// </summary>
    FundSetup,

    //以下是管理人层面的公告类型，ID从100开始，区分于基金层面的公告

    ManagerLevel =100,

    /// <summary>
    /// 管理人变更公告
    /// </summary>
    MangerChange ,

    /// <summary>
    /// 管理人办公地址变更公告
    /// </summary>
    OfficeAddressChange = 102,
    

}

/// <summary>
/// 报告格式
/// </summary>
[Flags]
public enum DisclosureFormat
{
    None = 0,
    Excel = 1 << 0,
    Pdf = 1 << 1,
    Word = 1 << 2,
    Xbrl = 1 << 3,
    Sealed = 1 << 4,  // 用印PDF（年报专用）
}

/// <summary>
/// 信批状态
/// </summary>
public enum DisclosureStatus
{
    Pending,       // 待提交
    Canceled,
    Processing,
    Submitted ,     // 已提交
    Verified ,      // 审核通过
    Rejected ,     // 审核驳回
    Published ,     // 已发布
    Failed ,       // 失败
}

/// <summary>
/// 信批通道代码
/// </summary>
public static class DisclosureChannelCode
{
    public const string Email = "email";
    public const string Pfid = "pfid";
    public const string MeiShi = "meishi";
    public const string AMAC = "amac";      // 基金业协会
    public const string Custom = "custom";  // 自定义/其他平台
}

/// <summary>
/// 扩展方法
/// </summary>
public static class DisclosureTypeExtensions
{
    /// <summary>
    /// 获取指定信批类型支持的格式
    /// </summary>
    public static DisclosureFormat GetSupportedFormats(this DisclosureType type)
    {
        return type switch
        {
            DisclosureType.Monthly or DisclosureType.Quarterly or DisclosureType.SemiAnnually =>
                DisclosureFormat.Excel | DisclosureFormat.Pdf,
            DisclosureType.Annually =>
                DisclosureFormat.Excel | DisclosureFormat.Pdf | DisclosureFormat.Word | DisclosureFormat.Xbrl | DisclosureFormat.Sealed,
            DisclosureType.Temporary =>
                DisclosureFormat.Pdf | DisclosureFormat.Word,
            _ => DisclosureFormat.None
        };
    }

    /// <summary>
    /// 获取类型名称
    /// </summary>
    public static string GetName(this DisclosureType type)
    {
        return type switch
        {
            DisclosureType.Monthly => "月报",
            DisclosureType.Quarterly => "季报",
            DisclosureType.SemiAnnually => "半年报",
            DisclosureType.Annually => "年报",
            DisclosureType.Temporary => "临时报告",
            DisclosureType.TemporaryOpen => "临时开放",
            DisclosureType.HugeRedemption => "巨额赎回",
            DisclosureType.FundSetup => "基金成立",
            _ => type.ToString()
        };
    }
}
