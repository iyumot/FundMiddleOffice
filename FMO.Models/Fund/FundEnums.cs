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