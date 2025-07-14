using System.ComponentModel;

namespace FMO.Models;

[TypeConverter(nameof(EnumDescriptionTypeConverter))]
public enum TransferOrderType
{
    [Description("买入")] Buy,

    [Description("份额赎回")] Share,

    [Description("按金额赎回")] Amount,

    [Description("赎回至指定金额")] RemainAmout,
}

[TypeConverter(nameof(EnumDescriptionTypeConverter))]
public enum TransferSellType
{
    [Description("份额赎回")] Share,

    [Description("按金额赎回")] Amount,

    [Description("赎回至指定金额")]RemainAmout,
}




/// <summary>
/// 交易订单
/// </summary>
public class TransferOrder
{
    public int Id { get; set; }
  

    public int InvestorId { get; set; }

    public int FundId { get; set; }

    public DateOnly Date { get; set; }

    public TransferOrderType Type { get; set; }

    public decimal Number { get; set; }

    /// <summary>
    /// 合同
    /// </summary>
    public FileStorageInfo? Contact { get; set; }

    /// <summary>
    /// 风险揭示
    /// </summary>
    public FileStorageInfo? RiskDiscloure { get; set; }

    /// <summary>
    /// 认申购、赎回单
    /// </summary>
    public FileStorageInfo? OrderSheet { get; set; }

    /// <summary>
    /// 风险匹配
    /// </summary>
    public FileStorageInfo? RiskPair { get; set; }

    /// <summary>
    /// 双录
    /// </summary>
    public FileStorageInfo? Videotape { get; set; }


    /// <summary>
    /// 回访
    /// </summary>
    public FileStorageInfo? Review { get; set; }

}
