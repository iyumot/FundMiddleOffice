using CommunityToolkit.Mvvm.ComponentModel;

namespace FMO;


public interface IValueViewModel
{
    bool IsChanged { get; }

    bool IsSetted { get; }

    public void Apply();
}



public partial class ValueViewModel<T> : ObservableObject, IValueViewModel where T : struct
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
}

public partial class RefrenceViewModel<T> : ObservableObject, IValueViewModel where T : class
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
}
