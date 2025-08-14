using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace FMO.Trustee;

public abstract partial class TrusteeViewModelBase : ObservableObject,IRecipient<TrusteeStatus>
{
    public string Idenitifier { get; set; } = "";

    public string? Title { get; protected set; }

    [ObservableProperty]
    public partial bool IsAvaliable { get; set; }


    [ObservableProperty]
    public partial bool ShowConfigSetting { get; set; }
     


    public bool CanSave
    {
        get
        {
            try { return CanSaveOverride(); } catch { return false; }
        }
    }


    protected abstract bool CanSaveOverride();

    protected abstract void SaveConfigOverride();




    public void Receive(TrusteeStatus message)
    {
        if (message.Identifier == Idenitifier)
            IsAvaliable = message.Status;
    }
}

public record TrusteeStatus(string Identifier, bool Status);

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
        IsAvaliable = Assist.IsValid;

        Idenitifier = Assist.Identifier;
        Title = Assist.Title;
    }


    public T Assist { get; }

    ITrustee ITrusteeViewModel.Assist => Assist;


    [RelayCommand(CanExecute = nameof(CanSave))]
    public void SaveConfig()
    {
        SaveConfigOverride();
        ShowConfigSetting = false;
        Assist.Renew();
    }
}