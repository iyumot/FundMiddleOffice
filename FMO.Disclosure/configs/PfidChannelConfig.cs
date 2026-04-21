namespace FMO.Disclosure;

/// <summary>
/// PFID通道配置
/// </summary>
public class PfidChannelConfig : IDisclosureChannelConfig
{
    public string ChannelCode { get; set; } = DisclosureChannelCode.Pfid;

    /// <summary>
    /// API地址
    /// </summary>
    public required string ApiUrl { get; set; }

    /// <summary>
    /// API密钥
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// 机构ID
    /// </summary>
    public required string InstitutionId { get; set; }

    /// <summary>
    /// 产品代码
    /// </summary>
    public string? ProductCode { get; set; }
}
