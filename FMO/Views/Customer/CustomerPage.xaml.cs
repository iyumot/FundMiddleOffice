using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;
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

public partial class CustomerPageViewModel : ObservableRecipient
{

    public ObservableCollection<CustomerViewModel> Customers { get; }

    [ObservableProperty]
    public partial CustomerViewModel? Selected { get; set; }


    public CustomerPageViewModel()
    {
        using var db = new BaseDatabase();

        var cusomers = db.GetCollection<IInvestor>().FindAll().ToArray();




        Customers = new(cusomers.Select(x => new CustomerViewModel(x)));




    }


}

/// <summary>
/// customer vm
/// </summary>
public partial class CustomerViewModel : ObservableObject
{
    //[ObservableProperty]
    //public partial string? Name { get; set; }

    public CustomerUnitRefrenceViewModel<string> Name { get; } = new() { InitFunc = x => x.Name };


    [ObservableProperty]
    public partial InvestorType Type { get; set; }


    [ObservableProperty]
    public partial string? Identity { get; set; }


    [ObservableProperty]
    public partial IDType? IDType { get; set; }

    public int Id { get; }

    [ObservableProperty]
    public partial QualifiedInvestorType InvestorType { get; set; }

    /// <summary>
    /// 特殊合格投资者
    /// </summary>
    [ObservableProperty]
    public partial bool IsSpecial { get; set; }


    [ObservableProperty]
    public partial bool IsReadOnly { get; set; } = true;

    /// <summary>
    /// 民族
    /// </summary>

    [ObservableProperty]
    public partial string? Nation { get; set; }


    [ObservableProperty]
    public partial DateEfficient Efficient { get; set; }


    [ObservableProperty]
    public partial RiskLevel? RiskLevel { get; set; }

    public CustomerUnitRefrenceViewModel<Enum> DetailType { get; } = new() { InitFunc = x => x switch { NaturalInvestor c => c.DetailType, InstitutionInvestor c=>c.DetailType, ProductInvestor c=>c.ProductType,_=> throw new NotImplementedException() } };

    [ObservableProperty]
    public partial Enum[]? DetailTypes { get; set; }

    public CustomerViewModel()
    {

    }


    public CustomerViewModel(IInvestor x)
    {
        Name.Init(x);
        DetailType.Init(x);

        switch (x)
        {
            case NaturalInvestor c:
                DetailTypes = [DetailCustomerType.NonEmployee, DetailCustomerType.Employee];
                break;
            default:
                DetailTypes = [];
                break;
        }


        Identity = x.Identity.Id;
        IDType = x.Identity.Type;
        Type = x.CustomerType;
        Id = x.Id;



        switch (x)
        {
            case NaturalInvestor c:
                Nation = c.Nation;
                break;

            case InstitutionInvestor c:

                break;

            case ProductInvestor c:

                break;
            default:
                break;
        }
    }


    [RelayCommand]
    public void Delete(UnitViewModel unit)
    {

    }



    [RelayCommand]
    public void Reset(UnitViewModel unit)
    {

    }

    [RelayCommand]
    public void Modify(UnitViewModel unit)
    {

    }


}

