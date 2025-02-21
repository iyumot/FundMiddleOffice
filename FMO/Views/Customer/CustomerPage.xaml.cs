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

public partial class CustomerPageViewModel : ObservableRecipient,IRecipient<Investor>
{

    public ObservableCollection<Investor> Customers { get; }

    [ObservableProperty]
    public partial Investor? Selected { get; set; }


    [ObservableProperty]
    public partial CustomerViewModel? Detail { get; set; }




    public CustomerPageViewModel()
    {
        IsActive = true;

        using var db = new BaseDatabase();

        var cusomers = db.GetCollection<Investor>().FindAll().ToArray();

        if (cusomers.Length == 0)
            cusomers = [
                new Investor { Name = "张三", EntityType = EntityType.Natural},
                new Investor { Name = "某公司", EntityType = EntityType.Institution},
                new Investor { Name = "某产品", EntityType = EntityType.Product},
            ];

        Customers = new(cusomers);// new(cusomers.Select(x => new CustomerViewModel(x)));




    }


    [RelayCommand]
    public void AddInvestor()
    {
        Customers.Add(new Investor { Name = ""});
    }

    [RelayCommand]
    public void RemoveInvestor()
    {
        if (Selected is null) return;

        if(Selected.Id != 0)
        {
            using var db = new BaseDatabase();
            db.GetCollection<Investor>().Delete(Selected.Id); 
        }
         
        Application.Current.Dispatcher.BeginInvoke((() => { Customers.Remove(Selected); Selected = null; }));
        
    }

    partial void OnSelectedChanged(Investor? oldValue, Investor? newValue)
    {
        Detail = newValue is null ? null : new CustomerViewModel(newValue);
    }

    public void Receive(Investor message)
    {
        Application.Current.Dispatcher.BeginInvoke((() => {

            try
            {
                for (int i = 0; i < Customers.Count; i++)
                {
                    if (Customers[i].Id == message.Id)
                    {
                        Customers.RemoveAt(i);
                        Customers.Insert(i, message);
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
 