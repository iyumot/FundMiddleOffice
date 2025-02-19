using CommunityToolkit.Mvvm.ComponentModel;

namespace FMO;


public interface IModifiableValue
{
    bool IsChanged { get; }

    bool IsSetted { get; }

    public void Apply();

    void Clear();
}



public partial class ValueViewModel<T> : ObservableObject, IModifiableValue where T : struct
{
    [ObservableProperty]
    public partial T? Old { get; set; }


    [ObservableProperty]
    public partial T? New { get; set; }

    /// <summary>
    /// 有变化
    /// </summary>
    public bool IsChanged => New is not null && !New.Equals(Old);

    public bool IsSetted => New is not null && New.Value switch { Enum e => true, _ => !New.Value.Equals(default(T)) };

    /// <summary>
    /// 值被应用
    /// </summary>
    public void Apply() => Old = New;

    public void Clear()
    {
        Old = null;
        New = null;
    }
}

public partial class RefrenceViewModel<T> : ObservableObject, IModifiableValue where T : class
{
    [ObservableProperty]
    public partial T? Old { get; set; }


    [ObservableProperty]
    public partial T? New { get; set; }

    /// <summary>
    /// 有变化
    /// </summary>
    public bool IsChanged => New switch { string s => !string.IsNullOrWhiteSpace(s), var v => v is not null } && !New!.Equals(Old);

    public bool IsSetted => New switch { string s => !string.IsNullOrWhiteSpace(s), var v => v is not null };

    /// <summary>
    /// 值被应用
    /// </summary>
    public void Apply() => Old = New;

    public void Clear()
    {
        Old = null;
        New = null;
    }
}



public partial class PropertyViewModel<T> : ObservableObject, IModifiableValue
{
    [ObservableProperty]
    public partial T? Old { get; set; }


    [ObservableProperty]
    public partial T? New { get; set; }

    /// <summary>
    /// 有变化
    /// </summary>
    public bool IsChanged => New is not null && !New.Equals(Old);

    public bool IsSetted => New is not null && New switch { Enum e => true, _ => !New.Equals(default(T)) };

    /// <summary>
    /// 值被应用
    /// </summary>
    public void Apply() => Old = New;

    public void Clear()
    {
        Old = default;
        New = default;
    }
}

