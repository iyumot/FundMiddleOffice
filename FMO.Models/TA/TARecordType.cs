using System.ComponentModel;

namespace FMO.Models;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
[Description("TA业务类型")]
public enum TARecordType
{
    [Description("未知")] UNK,
    /// <summary>
    /// 认购
    /// </summary>
    [Description("认购")] Subscription,

    /// <summary>
    /// 申购
    /// </summary>
    [Description("申购")] Purchase,

    /// <summary>
    /// 份额调增
    /// </summary>
    [Description("份额调增")] Increase,

    /// <summary>
    /// 
    /// </summary>
    [Description("转入")] MoveIn,

    /// <summary>
    /// 分红
    /// </summary>
    [Description("分红")] Distribution = 9,


    /// <summary>
    /// 赎回
    /// </summary>
    [Description("赎回")] Redemption = 11,

    /// <summary>
    /// 强制赎回
    /// </summary>
    [Description("强制赎回")] ForceRedemption,

    /// <summary>
    /// 份额调减
    /// </summary>
    [Description("份额调减")] Decrease,

    [Description("转出")] MoveOut,

    /// <summary>
    /// 二次或多次清算产生的金额
    /// </summary>
    [Description("清算")] Clear,



}

