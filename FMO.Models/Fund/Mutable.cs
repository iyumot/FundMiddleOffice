using System.Diagnostics.CodeAnalysis;

namespace FMO.Models;

public class Mutable<T>
{
    /// <summary>
    /// 名字
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }


    public T? InitalValue { get; set; }


    private  SortedDictionary<DateTime, T> _changes { get; } = new ();


    /// <summary>
    /// 值
    /// </summary>
    public IReadOnlyDictionary<DateTime, T> Changes => _changes;

    public T? Value => Changes.Count == 0 ? InitalValue : Changes.LastOrDefault().Value;

    [SetsRequiredMembers]
    public Mutable(string name, T value, string? description = null)
    {
        Name = name;
        Description = description;
        InitalValue = value;
    }

    public void SetValue(T value, DateTime time)
    {
        _changes[time] = value;
    }


}
