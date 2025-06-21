using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Data;

namespace FMO.LearnAssist;

public partial class LearnInfo : ObservableObject
{
    public LearnInfo()
    {
        ApplyClass = new();

        ApplyClass.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(ClassApply.IsSelected), System.ComponentModel.ListSortDirection.Descending));
        ApplyClass.SortDescriptions.Add(new System.ComponentModel.SortDescription("Class.Name", System.ComponentModel.ListSortDirection.Descending));

        ApplyClass.Filter += ApplyClass_Filter;
    }

    private void ApplyClass_Filter(object sender, FilterEventArgs e)
    {
        e.Accepted = string.IsNullOrWhiteSpace(SearchKey) ? true : (e.Item as ClassApply)!.Class.Name.Contains(SearchKey);
    }

    public required string Name { get; set; }

    public required string IdType { get; set; }

    public required string IdNumber { get; set; }

    public decimal Apply { get; set; }

    public decimal Learned { get; set; }

    /// <summary>
    /// 今年已报
    /// </summary>
    [ObservableProperty]
    public partial decimal AppledThisYear { get; set; }


    public string? Url { get; set; }

    [ObservableProperty]
    public partial ClassLearnInfo[]? Record { get; set; }

    [JsonIgnore]
    public CollectionViewSource ApplyClass { get; }


    [JsonIgnore]
    [ObservableProperty]
    public partial ClassApply[]? ApplyInfo { get; set; }


    [ObservableProperty]
    public partial string? SearchKey { get; set; }

    public decimal? Choosed => ApplyInfo?.Where(x => x.IsSelected).Sum(x => x.Class.Hour);

    partial void OnApplyInfoChanged(ClassApply[]? value)
    {
        if (value is null) return;

        foreach (var v in value)
        {
            v.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(ClassApply.IsSelected)) OnPropertyChanged(nameof(Choosed)); };
        }
    }

    partial void OnRecordChanged(ClassLearnInfo[]? value)
    {
        AppledThisYear = value?.Where(x => x.PayTime.Year == DateTime.Today.Year).Sum(x => x.Hour) ?? 0; 
    }

    partial void OnSearchKeyChanged(string? value)
    {
        ApplyClass.View?.Refresh();
    }
}
