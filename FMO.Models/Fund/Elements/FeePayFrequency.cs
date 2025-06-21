using System.ComponentModel;

namespace FMO.Models;




[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum FeePayFrequency
{
    [Description("月")]Month,

    [Description("季")]Quarter,

    [Description("其它")] Other
}


public class FeePayInfo
{
    public FeePayFrequency Type { get; set; }

    public string? Other { get; set; }

}