namespace FMO.Models;

public class FundFeeInfo
{
    public FundFeeType Type { get; set; }


    public bool HasFee { get; set; }

    public decimal Fee { get; set; }

    public bool HasGuaranteedFee { get; set; }

    /// <summary>
    /// 保底费用/年
    /// </summary>
    public decimal GuaranteedFee { get; set; }


    /// <summary>
    /// 特殊类型
    /// </summary>
    public string? Other { get; set; }
}


public class PartRedemptionFee
{
    public int Month { get; set; }

    public bool Include { get; set; }

    public decimal Fee { get; set; }
}
public class RedemptionFeeInfo : FundFeeInfo
{
    public List<PartRedemptionFee>? Parts { get; set; }
}