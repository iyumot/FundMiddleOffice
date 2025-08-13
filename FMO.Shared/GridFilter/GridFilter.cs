using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Data;

namespace FMO.Shared;


public partial class GridFilterItem : ObservableObject
{
    public required string Title { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }


    public Func<object, bool> FilterFunc { get; set; } = x => true;


}

public partial class GridFilter : ObservableObject
{
    public GridFilter(CollectionViewSource source)
    {
        Debouncer = new Debouncer(Update);
        Source = new WeakReference<CollectionViewSource>(source);

        source.Filter += (s, e) => e.Accepted = e.Accepted && Filter(e.Item);

        FilterSource.Filter += (s, e) => e.Accepted = string.IsNullOrWhiteSpace(SearchKey) ? true : e.Item switch { GridFilterItem f => f.Title.Contains(SearchKey), _ => true };
    }


    [ObservableProperty]
    public partial IEnumerable<GridFilterItem>? Filters { get; set; }

    [ObservableProperty]
    public partial string? SearchKey { get; set; }


    public CollectionViewSource FilterSource { get; } = new();


    private Debouncer Debouncer { get; }

    [ObservableProperty]
    public partial bool IsActive { get; set; }

    public WeakReference<CollectionViewSource> Source { get; }

    public bool Filter(object obj) => !IsActive ? true : Filters?.Any(x => x.IsSelected && x.FilterFunc(obj)) ?? false;


    private void Update()
    {
        IsActive = Filters?.Any(x => x.IsSelected) ?? false;
        if (Source.TryGetTarget(out var obj))
            Application.Current.Dispatcher.BeginInvoke(() => obj.View.Refresh());
    }
    partial void OnFiltersChanged(IEnumerable<GridFilterItem>? value)
    {
        Application.Current.Dispatcher.BeginInvoke(() => FilterSource.Source = value);
        if (value is null) return;

        foreach (var item in value)
        {
            item.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(GridFilterItem.IsSelected))
                    Debouncer.Invoke();
            };
        }
    }

    partial void OnSearchKeyChanged(string? value) => FilterSource.View.Refresh();


    [RelayCommand]
    public void Clear()
    {
        if (Filters is null) return;
        foreach (var item in Filters)
        {
            item.IsSelected = false;
        }
    }
}
