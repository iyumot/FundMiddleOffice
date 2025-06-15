using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using HandyControl.Tools.Extension;
using System.Collections.ObjectModel;
using System.Windows;

namespace FMO;

/// <summary>
/// AddOrModifyShareHolderWindow.xaml 的交互逻辑
/// </summary>
public partial class AddOrModifyShareHolderWindow : Window
{
    public AddOrModifyShareHolderWindow()
    {
        InitializeComponent();
    }
}



public partial class AddOrModifyShareHolderWindowViewModel : ObservableObject
{
    public AddOrModifyShareHolderWindowViewModel()
    {
        using var db = DbHelper.Base();
        var per = db.GetCollection<IEntity>().FindAll().ToList();
        per.Insert(0, db.GetCollection<Manager>().FindById(1));
        Entities = new(per);

        Institutions = new(per.Where(x => x is Institution).OfType<Institution>());
        Entities.CollectionChanged += (s, e) => OnPropertyChanged(nameof(Candidates));
    }


    public ObservableCollection<Institution> Institutions { get; }


    public ObservableCollection<IEntity> Entities { get; }


    [ObservableProperty]
    public partial bool ShowAddEntity { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    public partial IEntity? Holder { get; set; }


    [ObservableProperty]
    public partial string? HolderName { get; set; }

    public IEnumerable<IEntity>? Candidates => Institution is null ? Entities : Entities.Except([Institution]);


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Candidates))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    public partial Institution? Institution { get; set; }


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    public partial decimal? ShareAmount { get; set; }

    public bool CanConfirm => Institution is not null && Holder is not null && (ShareAmount ?? 0) > 0;

     
    partial void OnHolderNameChanged(string? value)
    {

        ShowAddEntity = !string.IsNullOrWhiteSpace(value) && (Holder is null || Holder.Name != value);

        if (string.IsNullOrWhiteSpace(value)) Holder = null;
    }

    [RelayCommand]
    public void AddInstitution()
    {
        Institution obj = new Institution { Name = HolderName! };
        Institutions.Add(obj);
        Entities.Add(obj);
        Holder = obj;
        ShowAddEntity = false;
    }

    [RelayCommand]
    public void AddPerson()
    {
        Person obj = new Person { Name = HolderName! };
        Entities.Add(obj);
        Holder = obj;
        ShowAddEntity = false;
    }


    [RelayCommand(CanExecute = nameof(CanConfirm))]
    public void Confirm(Window wnd)
    {
        if (Holder is null || Institution is null || ShareAmount is null) return;

        using var db = DbHelper.Base();

        // 检测是否有相同的数据
        var old = db.GetCollection<ShareHolderRelation>().FindOne(x => x.HolderId == Holder.Id && x.InstitutionId == Institution.Id);
        if(old is not null)
        {
            WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Warning, "存在相同的记录"));
            return;
        }

        if (Holder.Id == 0)
            db.GetCollection<IEntity>().Insert(Holder);

        db.GetCollection<ShareHolderRelation>().Insert(new ShareHolderRelation { HolderId = Holder.Id, InstitutionId = Institution.Id, Share = ShareAmount.Value });

        App.Current.Dispatcher.BeginInvoke(()=> wnd.Close());
    }

}