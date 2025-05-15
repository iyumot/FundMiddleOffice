using System.ComponentModel;

namespace FMO.Models;






/// <summary>
/// 管理类型
/// </summary>
[TypeConverter(nameof(EnumDescriptionTypeConverter))]
public enum ManageType
{

    Unk,

    [Description("受托管理")]
    Fiduciary,


    [Description("顾问管理")]
    Advisory,




}

public enum FundAccountType
{
    None,

    Collection,

    Custody
}

/// <summary>
/// 开放日类型
/// </summary>
[TypeConverter(nameof(EnumDescriptionTypeConverter))]
public enum FundOpenType
{
    [Description("不开放")] Closed,

    [Description("每年")]
    Yearly,

    [Description("每季度")]
    Quarterly,

    [Description("每月")]
    Monthly,

    [Description("每周")]
    Weekly,

    [Description("每天")]
    Daily
}