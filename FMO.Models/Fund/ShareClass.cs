namespace FMO.Models;

/// <summary>
/// 有不同份额安排
/// </summary>
public class ShareClass
{
    /// <summary>
    /// 份额名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 要求
    /// </summary>
    public string? Requirement { get; set; }
}


/// <summary>
/// 与份额相关的要素
/// </summary>
public class PortionFactors
{
    public ShareClass? Class { get; set; }


    public Mutable<string> MyProperty { get; set; }

}


public class Factors
{
    public required PortionFactors[] Data { get; set; }
}



public class FactorValue<T>
{
    public string? ShareClass { get; set; }

    public T? Data { get; set; }
}


public class PortionFactor<T>
{
    public FactorValue<T>[]? Value { get; set; }
}