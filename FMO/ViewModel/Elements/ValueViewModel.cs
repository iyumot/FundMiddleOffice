using CommunityToolkit.Mvvm.ComponentModel;

namespace FMO;


public interface IValueViewModel
{
    bool IsChanged { get; }

    public void Apply();
}



public partial class ValueViewModel<T> : ObservableObject, IValueViewModel
{
    [ObservableProperty]
    public partial T? Old { get; set; }


    [ObservableProperty]
    public partial T? New { get; set; }

    /// <summary>
    /// 有变化
    /// </summary>
    public bool IsChanged => New is not null && !New.Equals(Old);


    /// <summary>
    /// 值被应用
    /// </summary>
    public void Apply() => Old = New;
}
