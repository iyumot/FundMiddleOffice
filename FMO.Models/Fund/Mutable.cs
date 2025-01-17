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



    public SortedDictionary<int, T> Changes { get; set; } = new();

    public T? Value => Changes.LastOrDefault().Value;

    [SetsRequiredMembers]
    public Mutable(string name, T value, string? description = null)
    {
        Name = name;
        Description = description;
        //_changes = new();

        //_changes.Add(-1, value);
    }

    [SetsRequiredMembers]
    public Mutable(string name, string? description = null)
    {
        Name = name;
        Description = description;
        //_changes = new();
    }

    public void SetValue(T value, int flowid)
    {
        Changes[flowid] = value;
    }

    //public T? GetValue(int flowid, bool exact = false)
    //{

    //    return exact ? (Changes.ContainsKey(flowid) ? Changes[flowid] : default) : Changes.LastOrDefault(x => x.Key <= flowid).Value;
    //}

    public (int FlowId, T? Value) GetValue(int flowid)
    {
        foreach (var x in Changes.Reverse())
        {
            if (x.Key <= flowid)
            {
                return (x.Key, x.Value);
            }
        }
        return (-1, default);
    }



    public static implicit operator T?(Mutable<T>? mutable)
    {
        return mutable is null ? default : mutable.Value;
    }

    public override string? ToString()
    {
        return Value?.ToString() ?? base.ToString();
    }
}
