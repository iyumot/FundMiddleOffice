using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FMO.Shared;

public abstract partial class ChangeableEntityViewModel : ObservableObject
{
    public int Id { get; protected set; }

    [ObservableProperty]
    public partial bool IsReadOnly { get; set; } = true;


    protected abstract void DeleteOverride(IPropertyModifier unit);
    protected abstract void ResetOverride(IPropertyModifier unit);
    protected abstract void ModifyOverride(IPropertyModifier unit);
    protected abstract void SaveOverride();

    [RelayCommand]
    public void Delete(IPropertyModifier unit) => DeleteOverride(unit);


    [RelayCommand]
    public void Reset(IPropertyModifier unit) => ResetOverride(unit);



    [RelayCommand]
    public void Modify(IPropertyModifier unit) => ModifyOverride(unit);


    [RelayCommand]
    public void Save() => SaveOverride();
}