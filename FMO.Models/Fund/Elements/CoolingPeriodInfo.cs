using System.ComponentModel;

namespace FMO.Models;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum CoolingPeriodType
{
    [Description("24小时")]OneDay,

    [Description("其它")] Other
}


public class CoolingPeriodInfo
{
    public CoolingPeriodType Type { get; set; }

    public string? Other { get; set; }

}