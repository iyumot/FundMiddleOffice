using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Utilities;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// ManagerPage.xaml 的交互逻辑
/// </summary>
public partial class ManagerPage : UserControl
{
    public ManagerPage()
    {
        InitializeComponent();
    }
}

public interface IDatabaseUpdater
{
    void Update();
}

public abstract partial class DataItem<T, TEntity> : ObservableObject
{

    public required string Label { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsChanged))]
    public partial T? OldValue { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsChanged))]
    public partial T? NewValue { get; set; }
      
    public bool IsChanged => NewValue is not null && !NewValue.Equals(OldValue);

    public required Action<T?, TEntity> Updater { get; set; }

     
    public string? Format { get; set; }

    public abstract void Update();
}

public partial class ManagerDataItem<T> : DataItem<T, Manager>, IDatabaseUpdater
{

    public override void Update()
    {
        using var db = new BaseDatabase();
        var manager = db.GetCollection<Manager>().FindOne(x => x.IsMaster);
        Updater(NewValue, manager);
        db.GetCollection<Manager>().Update(manager);
    }
}


public partial class ManagerPageViewModel : ObservableObject
{
    /// <summary>
    /// 管理人名称
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<string> ManagerName { get; set; }

    /// <summary>
    /// 实控人
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<string> ArtificialPerson { get; set; }
     

    public string? RegisterNo { get; set; }

    [ObservableProperty]
    public partial ManagerDataItem<decimal> RegisterCapital { get; set; }


    [ObservableProperty]
    public partial bool IsReadOnly { get; set; } = true;


    public ManagerPageViewModel()
    {
        using var db = new BaseDatabase();
        var manager = db.GetCollection<Manager>().FindOne(x => x.IsMaster);

        ManagerName = new ManagerDataItem<string> { Label = "管理人", OldValue = manager.Name, NewValue = manager.Name, Updater = (a, b) => b.Name = a! };

        ArtificialPerson = new ManagerDataItem<string> { Label = "实控人", OldValue = manager.ArtificialPerson, NewValue = manager.ArtificialPerson, Updater = (a, b) => b.ArtificialPerson = a! };

        RegisterNo = manager.RegisterNo;//new ManagerDataItem<string> { Label = "实控人", OldValue = manager.RegisterNo, NewValue = manager.RegisterNo, Updater = (a, b) => b.RegisterNo = a! };



        RegisterCapital = new ManagerDataItem<decimal> { Label = "注册资金", OldValue = manager.RegisterCapital, NewValue = manager.RegisterCapital, Format="{0}万元", Updater = (a, b) => b.RegisterCapital = a! };

    }


    [RelayCommand]
    public void UpdateManagerInfo(IDatabaseUpdater dataItem)
    {
        dataItem.Update();
    }
}
