using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using System.ComponentModel;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// CustomerView.xaml 的交互逻辑
/// </summary>
public partial class CustomerView : UserControl
{
    public CustomerView()
    {
        InitializeComponent();
    }
}



/// <summary>
/// customer vm
/// </summary>
public partial class CustomerViewModel : ObservableObject
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))] public enum NaturalType { [Description("非员工")] NonEmployee, [Description("员工")] Employee };



    public static EntityType[] EntityTypes { get; } = [Models.EntityType.Natural, Models.EntityType.Institution, Models.EntityType.Product,];


    public static NaturalType[] NaturalTypes { get; } = [NaturalType.NonEmployee, NaturalType.Employee];

    public static IEnumerable<InstitutionWithGroup> InstitutionTypes { get; } = new List<InstitutionType>([InstitutionType.LegalEntity, InstitutionType.LimitedPartnership, InstitutionType.IndividualProprietorship, InstitutionType.Other, InstitutionType.QFII, InstitutionType.Foreign, InstitutionType.Other]).Index().Select(x => new InstitutionWithGroup { Category = x.Index < 4 ? "境内" : "境外", Value = x.Item });





    //[ObservableProperty]
    //public partial string? Name { get; set; }

    public CustomerUnitRefrenceViewModel<string> Name { get; } = new() { InitFunc = x => x.Name };

    public CustomerUnitValueViewModel<EntityType> EntityType { get; } = new() { InitFunc = x => x.EntityType, Label = "客户类型" };


    public CustomerUnitValueViewModel<NaturalType> InvestorNaturalType { get; } = new() { InitFunc = x => x.Type switch { Models.InvestorType.Employee => NaturalType.Employee, _ => NaturalType.NonEmployee } };

    [ObservableProperty]
    public partial InvestorType[] InvestorTypes { get; set; }


    [ObservableProperty]
    public partial InvestorType Type { get; set; }


    [ObservableProperty]
    public partial string? Identity { get; set; }


    [ObservableProperty]
    public partial IDType? IDType { get; set; }

    public int Id { get; }

    [ObservableProperty]
    public partial QualifiedInvestorType QualifiedInvestorType { get; set; }

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

    //  public CustomerUnitRefrenceViewModel<Enum> DetailType { get; } = new() { InitFunc = x => x switch { NaturalInvestor c => c.DetailType, InstitutionInvestor c => c.DetailType, ProductInvestor c => c.ProductType, _ => throw new NotImplementedException() } };

    [ObservableProperty]
    public partial Enum[]? DetailTypes { get; set; }

    public CustomerViewModel()
    {

    }


    public CustomerViewModel(Investor x)
    {
        Name.Init(x);
        EntityType.Init(x);
        // DetailType.Init(x);

        //switch (x)
        //{
        //    case NaturalInvestor c:
        //        DetailTypes = [DetailCustomerType.NonEmployee, DetailCustomerType.Employee];
        //        break;
        //    default:
        //        DetailTypes = [];
        //        break;
        //}


        Identity = x.Identity.Id;
        IDType = x.Identity.Type;

        Id = x.Id;



        //switch (x)
        //{
        //    case NaturalInvestor c:
        //        Nation = c.Nation;
        //        break;

        //    case InstitutionInvestor c:

        //        break;

        //    case ProductInvestor c:

        //        break;
        //    default:
        //        break;
        //}
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




    public class InstitutionWithGroup
    {
        public string? Category { get; set; }

        public InstitutionType Value { get; set; }
    }
}

