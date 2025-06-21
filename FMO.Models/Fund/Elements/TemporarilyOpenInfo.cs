namespace FMO.Models;

/// <summary>
/// 临时开放
/// </summary>
public class TemporarilyOpenInfo
{
    public bool IsAllowed { get; set; }

    /// <summary>
    /// 仅在合同变更、法规
    /// </summary>
    public bool IsLimited { get; set; }

    public bool AllowPurchase { get; set; }

    public bool AllowRedemption { get; set; }
}