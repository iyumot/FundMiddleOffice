namespace FMO.Trustee;

/// <summary>
/// ͳһ������
/// </summary>
public class TrusteeUnifiedConfig
{
    public int Id { get; set; }

    public bool UseProxy { get; set; }

    public string? ProxyUrl { get; set; }

    public string? ProxyUser { get; set; }

    public string? ProxyPassword { get; set; }
}
