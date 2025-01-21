using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Utilities;
using System.ComponentModel;
using System.Diagnostics;
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




public partial class ReadOnlyDataItem<T> : ObservableObject
{
    public required string Label { get; set; }


    [ObservableProperty]
    //[NotifyPropertyChangedFor("IsChanged")]
    public partial T? NewValue { get; set; }


    public string? Format { get; set; }

}


public abstract partial class DataItem<T, TEntity> : ReadOnlyDataItem<T>
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsChanged))]
    public partial T? OldValue { get; set; }

    public bool IsChanged => NewValue is not null && (NewValue is string s ? !string.IsNullOrWhiteSpace(s) : true) && !NewValue.Equals(OldValue);

    public required Action<T?, TEntity> Updater { get; set; }

    public abstract void Update();


    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(NewValue))
            OnPropertyChanged(nameof(IsChanged));
    }
}

public partial class ManagerDataItem<T> : DataItem<T, Manager>, IDatabaseUpdater
{

    public override void Update()
    {
        using var db = new BaseDatabase();
        var manager = db.GetCollection<Manager>().FindOne(x => x.IsMaster);
        Updater(NewValue, manager);
        db.GetCollection<Manager>().Update(manager);
        OldValue = NewValue;
    }
}

public partial class ManagerDateExItem : DataItem<DateOnly?, Manager>, IDatabaseUpdater
{

    /// <summary>
    /// 无固定期限
    /// </summary>
    [ObservableProperty]
    public partial bool IsLongTerm { get; set; }

    public override void Update()
    {
        using var db = new BaseDatabase();
        var manager = db.GetCollection<Manager>().FindOne(x => x.IsMaster);
        if (IsLongTerm)
            Updater(DateOnly.MaxValue, manager);
        else
            Updater(NewValue, manager);
        db.GetCollection<Manager>().Update(manager);
        OldValue = NewValue;
    }
}



public partial class ManagerPageViewModel : ObservableObject
{
    public string AmacPageUrl { get; set; }

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

    /// <summary>
    /// 注册资本
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<decimal> RegisterCapital { get; set; }

    /// <summary>
    /// 实缴
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<decimal?> RealCapital { get; set; }


    [ObservableProperty]
    public partial ManagerDataItem<DateOnly> SetupDate { get; set; }

    [ObservableProperty]
    public partial ManagerDateExItem ExpireDate { get; set; }




    [ObservableProperty]
    public partial ManagerDataItem<DateOnly> RegisterDate { get; set; }


    /// <summary>
    /// 电话
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<string> Telephone { get; set; }

    /// <summary>
    /// 传真
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<string> Fax { get; set; }

    /// <summary>
    /// 统一信用代码
    /// </summary> 
    [ObservableProperty]
    public partial ReadOnlyDataItem<string> InstitutionCode { get; set; }

    /// <summary>
    /// 注册地址
    /// </summary>
    [ObservableProperty]
    public partial ReadOnlyDataItem<string> RegisterAddress { get; set; }



    /// <summary>
    /// 办公地址
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<string?> OfficeAddress { get; set; }


    /// <summary>
    /// 经营范围
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<string> BusinessScope { get; set; }


    /// <summary>
    /// 官网
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<string> WebSite { get; set; }







    [ObservableProperty]
    public partial bool IsReadOnly { get; set; } = true;


    public ManagerPageViewModel()
    {
        using var db = new BaseDatabase();
        var manager = db.GetCollection<Manager>().FindOne(x => x.IsMaster);

        AmacPageUrl = $"https://gs.amac.org.cn/amac-infodisc/res/pof/manager/{manager.AmacId}.html";

        ManagerName = new ManagerDataItem<string> { Label = "管理人", OldValue = manager.Name, NewValue = manager.Name, Updater = (a, b) => b.Name = a! };

        ArtificialPerson = new ManagerDataItem<string> { Label = "实控人", OldValue = manager.ArtificialPerson, NewValue = manager.ArtificialPerson, Updater = (a, b) => b.ArtificialPerson = a! };

        RegisterNo = manager.RegisterNo;//new ManagerDataItem<string> { Label = "实控人", OldValue = manager.RegisterNo, NewValue = manager.RegisterNo, Updater = (a, b) => b.RegisterNo = a! };



        RegisterCapital = new ManagerDataItem<decimal> { Label = "注册资本", OldValue = manager.RegisterCapital, NewValue = manager.RegisterCapital, Format = "{0}万元", Updater = (a, b) => b.RegisterCapital = a! };
        RealCapital = new ManagerDataItem<decimal?> { Label = "实缴资本", OldValue = manager.RealCapital, NewValue = manager.RealCapital, Format = "{0}万元", Updater = (a, b) => b.RegisterCapital = a ?? default };


        SetupDate = new ManagerDataItem<DateOnly> { Label = "成立日期", OldValue = manager.SetupDate, NewValue = manager.SetupDate, Format = "yyyy-MM-dd", Updater = (a, b) => b.SetupDate = a };
        DateOnly? ed = manager.ExpireDate == default || manager.ExpireDate == DateOnly.MaxValue ? null : manager.ExpireDate;
        ExpireDate = new ManagerDateExItem { Label = "核销日期", OldValue = ed, NewValue = ed, IsLongTerm = manager.ExpireDate == DateOnly.MaxValue, Format = "yyyy-MM-dd", Updater = (a, b) => b.ExpireDate = a ?? default };
        RegisterDate = new ManagerDataItem<DateOnly> { Label = "登记日期", OldValue = manager.RegisterDate, NewValue = manager.RegisterDate, Format = "yyyy-MM-dd", Updater = (a, b) => b.RegisterDate = a };

        Telephone = new ManagerDataItem<string> { Label = "固定电话", OldValue = manager.Telephone, NewValue = manager.Telephone, Updater = (a, b) => b.Telephone = a };
        Fax = new ManagerDataItem<string> { Label = "传真", OldValue = manager.Fax, NewValue = manager.Fax, Updater = (a, b) => b.Fax = a };

        InstitutionCode = new ReadOnlyDataItem<string> { Label = "统一信用代码", NewValue = manager.Id, };
        RegisterAddress = new ReadOnlyDataItem<string> { Label = "注册地址", NewValue = manager.RegisterAddress, };
        OfficeAddress = new ManagerDataItem<string?> { Label = "办公地址", OldValue = manager.OfficeAddress, NewValue = manager.OfficeAddress, Updater = (a, b) => b.OfficeAddress = a };
        BusinessScope = new ManagerDataItem<string> { Label = "经营范围", OldValue = manager.BusinessScope, NewValue = manager.BusinessScope, Updater = (a, b) => b.BusinessScope = a };

    }


    [RelayCommand]
    public void UpdateManagerInfo(IDatabaseUpdater dataItem)
    {
        dataItem.Update();
    }

    [RelayCommand]
    public void OpenAmacPage()
    {
        try { Process.Start(new ProcessStartInfo(AmacPageUrl) { UseShellExecute = true }); } catch { }
    }
}
