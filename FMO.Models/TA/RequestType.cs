using System.ComponentModel;

namespace FMO.Models;

/// <summary>
/// 交易申请业务类型
/// </summary>
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum RequestType
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


    /// <summary>
    /// 二次或多次清算产生的金额
    /// </summary>
    [Description("清算")] Clear,



}

