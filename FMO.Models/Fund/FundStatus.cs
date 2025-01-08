using System.ComponentModel;

namespace FMO.Models;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum FundStatus
{
    Unk,

    [Description("发行")] Setup,

    [Description("备案")] Registration,

    [Description("正在运作")] Normal,

    [Description("提前清算")] EarlyLiquidation,

    [Description("正常清算")] Liquidation,

    [Description("延期清算")] LateLiquidation, 

    [Description("投顾协议已终止")] AdvisoryTerminated,
}