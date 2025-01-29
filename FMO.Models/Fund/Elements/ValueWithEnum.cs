using System.ComponentModel;

namespace FMO.Models;



/// <summary>
/// 基金费用
/// </summary>
public enum FundFeeType
{

    [Description("固定比例")] Ratio,

    [Description("固定金额")] Fix,

    [Description("其它")] Other
}





/// <summary>
/// 带枚举及值的类型
/// </summary>
/// <typeparam name="TEnum"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class ValueWithEnum<TEnum, TValue> where TEnum : struct, Enum
{
    public TEnum Type { get; set; }

    public TValue? Value { get; set; }

    public string? Extra { get; set; }
}


public class ValueWithBoolean<T>
{
    /// <summary>
    /// 是否采用
    /// </summary>
    public bool IsAdopted { get; set; }

    public T? Value { get; set; }
}