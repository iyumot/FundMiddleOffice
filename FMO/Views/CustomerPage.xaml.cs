using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using FMO.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

public partial class CustomerPageViewModel : ObservableRecipient
{

    public ObservableCollection<CustomerViewModel> Customers { get; }  


    public CustomerPageViewModel()
    {
        using var db = new BaseDatabase();

        var cusomers = db.GetCollection<ICustomer>().FindAll().ToArray();




        Customers = new(cusomers.Select(x=>new CustomerViewModel(x)));




    }

   
}

/// <summary>
/// customer vm
/// </summary>
public partial class CustomerViewModel:ObservableObject
{
    [ObservableProperty]
    public partial string? Name { get; set; }


    [ObservableProperty]
    public partial CustomerType Type { get; set; }


    [ObservableProperty]
    public partial string? Identity { get; set; }


    [ObservableProperty]
    public partial IDType? IDType { get; set; }

    public int Id { get; }

    [ObservableProperty]
    public partial InvestorType InvestorType { get; set; }

    /// <summary>
    /// 特殊合格投资者
    /// </summary>
    [ObservableProperty]
    public partial bool IsSpecial { get; set; }


    /// <summary>
    /// 民族
    /// </summary>

    [ObservableProperty] 
    public partial string? Nation { get; set; }


    [ObservableProperty]
    public partial DateEfficient Efficient { get; set; }


    [ObservableProperty]
    public partial RiskLevel? RiskLevel { get; set; }


    public CustomerViewModel()
    {

    }


    public CustomerViewModel(ICustomer x)
    {
        Name = x.Name;
        Identity = x.Identity.Id;
        IDType = x.Identity.Type;
        Type = x.CustomerType;
        Id = x.Id;
        


        switch (x)
        {
            case NaturalCustomer c:
                Nation = c.Nation;
                break;

            case InstitutionCustomer c:

                break;

            case ProductCustomer c:

                break;
            default:
                break;
        }
    }




}