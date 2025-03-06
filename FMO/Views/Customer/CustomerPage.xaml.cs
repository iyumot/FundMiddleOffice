using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using Serilog;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// CustomerPage.xaml 的交互逻辑
/// </summary>
public partial class CustomerPage : UserControl
{




    public CustomerPage()
    {
        InitializeComponent();
    }
}

public partial class CustomerPageViewModel : ObservableRecipient, IRecipient<Investor>
{

    public ObservableCollection<InvestorReadOnlyViewModel> Customers { get; }

    [ObservableProperty]
    public partial InvestorReadOnlyViewModel? Selected { get; set; }


    [ObservableProperty]
    public partial CustomerViewModel? Detail { get; set; }




    public CustomerPageViewModel()
    {
        IsActive = true;

        using var db = new BaseDatabase();

        var cusomers = db.GetCollection<Investor>().FindAll().ToArray();

        //if (cusomers.Length == 0)
        //    cusomers = [
        //        new Investor { Id = 1, Name = "张三", EntityType = EntityType.Natural},
        //        new Investor { Id = 2, Name = "某公司", EntityType = EntityType.Institution},
        //        new Investor { Id = 3, Name = "某产品", EntityType = EntityType.Product},
        //    ];

        Customers = new(cusomers.Select(x=> new InvestorReadOnlyViewModel(x)));// new(cusomers.Select(x => new CustomerViewModel(x)));




    }


    [RelayCommand]
    public void AddInvestor()
    {
        Customers.Add(new InvestorReadOnlyViewModel(new Investor { Name = "" }));
    }

    [RelayCommand]
    public void RemoveInvestor()
    {
        if (Selected is null) return;

        if (Selected.Id != 0)
        {
            if (Selected.Name?.Length > 1 && Selected.Identity != default && HandyControl.Controls.MessageBox.Show($"是否删除投资人 【{Selected.Name}】资料", "提示", button: System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.No)
                return;

            using var db = new BaseDatabase();
            db.GetCollection<Investor>().Delete(Selected.Id);
        }

        Application.Current.Dispatcher.BeginInvoke((() => { Customers.Remove(Selected); Selected = null; }));

    }

    partial void OnSelectedChanged(InvestorReadOnlyViewModel? oldValue, InvestorReadOnlyViewModel? newValue)
    {
        Detail = newValue is null ? null : new CustomerViewModel(newValue.Investor);
    }

    public void Receive(Investor message)
    {
        Application.Current.Dispatcher.BeginInvoke((() =>
        {

            try
            {
                for (int i = 0; i < Customers.Count; i++)
                {
                    if (Customers[i].Id == message.Id)
                    {
                        Customers[i].Update(message);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"更新investor出错 {e.Message}");
            }

        }));
    }
}


/// <summary>
/// 用于列表展示
/// </summary>
public partial class InvestorReadOnlyViewModel : ObservableObject
{
    public int Id { get; }


    [ObservableProperty]
    public partial string? Name { get; set; }

    [ObservableProperty]
    public partial AmacInvestorType Type { get; set; }

    [ObservableProperty]
    public partial Identity Identity { get; set; }


    [ObservableProperty]
    public partial DateEfficient Efficient { get; set; }

    [ObservableProperty]
    public partial RiskLevel RiskLevel { get; set; }

    public Investor Investor { get; set; }

    public InvestorReadOnlyViewModel(Investor investor)
    {
        Id = investor.Id;
        Name = investor.Name;
        Type = investor.Type;

        Identity = investor.Identity;
        Efficient = investor.Efficient;

        RiskLevel = investor.RiskLevel;
        Investor = investor;
    }
    
    public void Update(Investor investor)
    {
        Name = investor.Name;
        Type = investor.Type;

        Identity = investor.Identity;
        Efficient = investor.Efficient;

        RiskLevel = investor.RiskLevel;
        Investor = investor;
    }

}
