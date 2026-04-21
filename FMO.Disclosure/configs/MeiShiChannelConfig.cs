namespace FMO.Disclosure;

/// <summary>
/// 美市通道配置
/// </summary>
public class MeiShiChannelConfig : IDisclosureChannelConfig
{
    public string ChannelCode { get; set; } = DisclosureChannelCode.MeiShi;

    /// <summary>
    /// API地址
    /// </summary>
    public required string ApiUrl { get; set; }

    /// <summary>
    /// API密钥
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// 是否发送通知
    /// </summary>
    public bool Notify { get; set; } = true;

    /// <summary>
    /// 是否用印
    /// </summary>
    public bool Seal { get; set; } = false;
}
