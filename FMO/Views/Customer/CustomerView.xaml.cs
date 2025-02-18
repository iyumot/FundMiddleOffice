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

    //public static IEnumerable<InstitutionWithGroup> InstitutionTypes { get; } = new List<InstitutionType>([InstitutionType.LegalEntity, InstitutionType.LimitedPartnership, InstitutionType.IndividualProprietorship, InstitutionType.Other, InstitutionType.QFII, InstitutionType.Foreign, InstitutionType.Other]).Index().Select(x => new InstitutionWithGroup { Category = x.Index < 4 ? "境内" : "境外", Value = x.Item });

   // [ObservableProperty]
    //public partial InstitutionType? SelectedInstitutionType { get; set; }



    //[ObservableProperty]
    //public partial string? Name { get; set; }

    public CustomerUnitRefrenceViewModel<string> Name { get; } = new() { InitFunc = x => x.Name };

    public CustomerUnitValueViewModel<EntityType> EntityType { get; } = new() { InitFunc = x => x.EntityType, Label = "客户类型" };


    public CustomerUnitValueViewModel<NaturalType> InvestorNaturalType { get; } = new() { InitFunc = x => x.Type switch { Models.AmacInvestorType.Employee => NaturalType.Employee, _ => NaturalType.NonEmployee } };

    [ObservableProperty]
    public partial AmacInvestorType[]? InvestorTypes { get; set; }


    [ObservableProperty]
    public partial AmacInvestorType? Type { get; set; }


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
        EntityType.PropertyChanged += EntityType_PropertyChanged;
        EntityType_PropertyChanged(null, null);
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

    private void EntityType_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (EntityType.Data.New)
        {
            case Models.EntityType.Natural:
                InvestorTypes = [AmacInvestorType.NonEmployee, AmacInvestorType.Employee];
                break;
            case Models.EntityType.Institution:
                InvestorTypes = [AmacInvestorType.Manager, AmacInvestorType.LegalEntity, AmacInvestorType.IndividualProprietorship, AmacInvestorType.NonLegalEntity, AmacInvestorType.QFII, AmacInvestorType.Foreign, AmacInvestorType.DirectFinancialInvestment];
                break;
            case Models.EntityType.Product:
                InvestorTypes = Enum.GetValues<AmacInvestorType>().Where(x => x > AmacInvestorType.Product).ToArray();
                break;
            case null:
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






    //partial void OnSelectedInstitutionTypeChanged(InstitutionType? oldValue, InstitutionType? newValue)
    //{
    //    switch (newValue)
    //    {
    //        case Models.InstitutionType.LegalEntity:
    //            InvestorTypes = Enum.GetValues<AmacInvestorType>().Where(x=> (int)x > 100 && (int)x < 1000).ToArray();
    //            break;
    //        case Models.InstitutionType.LimitedPartnership:
    //            InvestorTypes = Enum.GetValues<AmacInvestorType>().Where(x => (int)x > 1000 && (int)x < 2000).ToArray();
    //            break;
    //        case Models.InstitutionType.IndividualProprietorship:
    //            InvestorTypes = Enum.GetValues<AmacInvestorType>().Where(x => (int)x > 2000 && (int)x < 3000).ToArray();
    //            break;
    //        case Models.InstitutionType.QFII:
    //            break;
    //        case Models.InstitutionType.Foreign:
    //            break;
    //        default:
    //            InvestorTypes = Enum.GetValues<AmacInvestorType>().ToArray();
    //            break;
    //    }


    //    if (InvestorTypes.Length == 1) Type = InvestorTypes[0];
    //    else if (Type is not null && !InvestorTypes.Contains(Type.Value)) Type = null;
    //}

    public class InstitutionWithGroup
    {
        public string? Category { get; set; }

        public InstitutionType Value { get; set; }
    }
}

