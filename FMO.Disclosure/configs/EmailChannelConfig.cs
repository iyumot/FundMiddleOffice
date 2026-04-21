namespace FMO.Disclosure;

/// <summary>
/// 邮件通道配置
/// </summary>
public class EmailChannelConfig : IDisclosureChannelConfig
{
    public string ChannelCode { get; set; } = DisclosureChannelCode.Email;

    /// <summary>
    /// SMTP服务器地址
    /// </summary>
    public required string SmtpHost { get; set; }

    /// <summary>
    /// SMTP端口
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// 使用SSL
    /// </summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>
    /// 用户名
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// 发件人邮箱
    /// </summary>
    public required string FromEmail { get; set; }

    /// <summary>
    /// 发件人名称
    /// </summary>
    public string? FromName { get; set; }

}
