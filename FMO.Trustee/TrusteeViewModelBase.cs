using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FMO.Trustee;

public abstract partial class TrusteeViewModelBase : ObservableObject
{
    public string? Title { get; protected set; }



    [ObservableProperty]
    public partial bool ShowConfigSetting { get; set; }


    public TrusteeViewModelBase()
    {
    }



    public bool CanSave
    {
        get
        {
            try { return CanSaveOverride(); } catch { return false; }
        }
    }


    protected abstract bool CanSaveOverride();

    protected abstract void SaveConfigOverride();



    [RelayCommand(CanExecute = nameof(CanSave))]
    public void SaveConfig()
    {
        SaveConfigOverride();
        ShowConfigSetting = false;
    }
}


public interface ITrusteeViewModel
{
    ITrustee Assist { get; }
}

public abstract partial class TrusteeViewModelBase<T> : TrusteeViewModelBase, ITrusteeViewModel where T : TrusteeApiBase, ITrustee, new()
{
    protected TrusteeViewModelBase()
    {
        Assist = new T();
        Assist.LoadConfig();

        Title = Assist.Title;
    }


    public T Assist { get; }

    ITrustee ITrusteeViewModel.Assist => Assist;
}