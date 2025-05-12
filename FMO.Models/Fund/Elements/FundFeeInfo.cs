namespace FMO.Models;

public class FundFeeInfo
{
    public FundFeeType Type { get; set; }


    //public bool HasFee { get; set; }

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
