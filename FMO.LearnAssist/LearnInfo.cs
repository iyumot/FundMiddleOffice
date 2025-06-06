using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Data;

namespace FMO.LearnAssist;

public partial class LearnInfo : ObservableObject
{
    public LearnInfo()
    {
        ApplyClass = new();

        ApplyClass.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(ClassApply.IsSelected), System.ComponentModel.ListSortDirection.Descending));
        ApplyClass.SortDescriptions.Add(new System.ComponentModel.SortDescription("Class.Name", System.ComponentModel.ListSortDirection.Descending));
    }

    public required string Name { get; set; }

    public required string IdType { get; set; }

    public required string IdNumber { get; set; }

    public decimal Apply { get; set; }

    public decimal Learned { get; set; }

    public string? Url { get; set; }

    [ObservableProperty]
    public partial ClassLearnInfo[]? Record { get; set; }


    public CollectionViewSource ApplyClass { get; } 


    [ObservableProperty]
    public partial ClassApply[]? ApplyInfo { get; set; }



}
