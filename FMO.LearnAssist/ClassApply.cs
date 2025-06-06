using CommunityToolkit.Mvvm.ComponentModel;

namespace FMO.LearnAssist;

public partial class ClassApply : ObservableObject
{
    public required ClassInfo Class { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}