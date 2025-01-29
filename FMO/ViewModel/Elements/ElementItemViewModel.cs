using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;

namespace FMO;

public abstract class ElementItemViewModel : ObservableObject
{
    public required string Label { get; set; }

    public abstract bool CanConfirm { get; }

    public abstract bool CanDelete { get; }

    public required string Property { get; set; }

    public abstract void UpdateEntity(FundElements elements, int fid);

    protected virtual void Init()
    {
        foreach (var p in GetType().GetProperties())
        {
            if (p.PropertyType.IsAssignableTo(typeof(ObservableObject)))
                ((ObservableObject)p.GetValue(this)!).PropertyChanged += ItemPropertyChanged;
        }
    }

    public abstract void Init(FundElements elements, int flowid);



    protected void ItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is IValueViewModel x)
        {
            OnPropertyChanged(nameof(CanConfirm));
            OnPropertyChanged(nameof(CanDelete));
        }
    }

    public virtual void Apply()
    {
        foreach (var p in GetType().GetProperties())
        {
            if (p.PropertyType.IsAssignableTo(typeof(IValueViewModel)))
                ((IValueViewModel)p.GetValue(this)!).Apply();
        }

        OnPropertyChanged(nameof(CanConfirm));
        OnPropertyChanged(nameof(CanDelete));
    }
}
