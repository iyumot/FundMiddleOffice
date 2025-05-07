using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;

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
public partial class CustomerViewModel : EditableControlViewModelBase<Investor>
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))] public enum NaturalType { [Description("非员工")] NonEmployee, [Description("员工")] Employee };



    public static EntityType[] EntityTypes { get; } = [Models.EntityType.Natural, Models.EntityType.Institution, Models.EntityType.Product,];


    public ChangeableViewModel<Investor, string> Name { get; }

    public ChangeableViewModel<Investor, EntityType> EntityType { get; } = new() { InitFunc = x => x.EntityType, UpdateFunc = (x, y) => x.EntityType = y, ClearFunc = x => x.EntityType = Models.EntityType.Unk, Label = "客户类型" };


    [ObservableProperty]
    public partial AmacInvestorType[]? InvestorTypes { get; set; }


    public ChangeableViewModel<Investor, AmacInvestorType> Type { get; } = new() { InitFunc = x => x.Type, UpdateFunc = (x, y) => x.Type = y, ClearFunc = x => x.Type = AmacInvestorType.None, Label = "" };



    public ChangeableViewModel<Investor, IDType> IDType { get; } = new() { InitFunc = x => x.Identity.Type, UpdateFunc = (x, y) => x.Identity = x.Identity with { Type = y }, ClearFunc = x => x.Identity = x.Identity with { Type = default }, Label = "证件类型" };

    public ChangeableViewModel<Investor, string> Identity { get; } = new() { InitFunc = x => x.Identity.Id, UpdateFunc = (x, y) => x.Identity = x.Identity with { Id = y! }, ClearFunc = x => x.Identity = x.Identity with { Id = string.Empty } };

    public ChangeableViewModel<Investor, string> Email { get; } = new() { InitFunc = x => x.Email, UpdateFunc = (x, y) => x.Email = y, ClearFunc = x => x.Email = null, Label = "Email" };

    public ChangeableViewModel<Investor, string> Phone { get; } = new() { InitFunc = x => x.Phone, UpdateFunc = (x, y) => x.Phone = y, ClearFunc = x => x.Phone = null, Label = "联系方式" };





    [ObservableProperty]
    public partial IDType[]? IDTypes { get; set; }


    [ObservableProperty]
    public partial QualifiedInvestorType QualifiedInvestorType { get; set; }

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


    //public EntityDateEfficientViewModel<Investor> Efficient { get; } = new() { InitFunc = x => x.Efficient, UpdateFunc = (x, y) => x.Efficient = y, ClearFunc = x => x.Efficient = default, Label = "证件有效期" };
    public ChangeableViewModel<Investor, DateEfficientViewModel> Efficient { get; } = new() { InitFunc = x => new DateEfficientViewModel(x.Efficient), UpdateFunc = (x, y) => x.Efficient = y?.Build() ?? default, Label = "证件有效期" };

    [ObservableProperty]
    public partial RiskLevel? RiskLevel { get; set; }

    [ObservableProperty]
    public partial Enum[]? DetailTypes { get; set; }


    public ObservableCollection<QualificationViewModel> Qualifications { get; }

    [ObservableProperty]
    public partial QualificationViewModel? SelectedQualification { get; set; }

    //public CustomerViewModel()
    //{
    //    Qualifications = new();
    //}


    public CustomerViewModel(Investor investor)
    {
        Id = investor.Id;

        Name = new ChangeableViewModel<Investor, string>(investor, init: x => x.Name, update: (x, y) => x.Name = y ?? string.Empty, clear: x => x.Name = string.Empty);


        //Name.Init(investor);
        EntityType.Init(investor);
        EntityType.PropertyChanged += EntityType_PropertyChanged;
        EntityType_PropertyChanged(null, null);

        Type.Init(investor);
        Type.PropertyChanged += Type_PropertyChanged;
        Type_PropertyChanged(null, null);

        Identity.Init(investor);
        IDType.Init(investor);

        Email.Init(investor);
        Phone.Init(investor);


        Efficient.Init(investor);

        using var db = DbHelper.Base();
        var iq = db.GetCollection<InvestorQualification>().Find(x => x.InvestorId == Id || (x.InvestorId == 0 && x.InvestorName == investor.Name && x.IdentityCode == investor.Identity.Id)).ToArray();
        Qualifications = new(iq.Select(x => QualificationViewModel.From(x, investor.Type, investor.EntityType)));
    }

    private void Type_PropertyChanged(object? sender, PropertyChangedEventArgs? e)
    {
        switch (Type.NewValue)
        {
            case AmacInvestorType.NonEmployee:
            case AmacInvestorType.Employee:
                IDTypes = Enum.GetValues<IDType>().Where(x => x >= Models.IDType.IdentityCard && x < Models.IDType.Institusion).ToArray();
                break;
            case AmacInvestorType.Manager:
            case AmacInvestorType.LegalEntity:
            case AmacInvestorType.IndividualProprietorship:
            case AmacInvestorType.NonLegalEntity:
            case AmacInvestorType.Foreign:
            case AmacInvestorType.DirectFinancialInvestment:
                IDTypes = [Models.IDType.UnifiedSocialCreditCode, Models.IDType.OrganizationCodeCertificate, Models.IDType.BusinessLicenseNumber, Models.IDType.RegistrationNumber, Models.IDType.Other];
                break;
            case AmacInvestorType.QFII:
                IDTypes = [Models.IDType.SecuritiesBusinessLicense, Models.IDType.Other];
                break;
            case AmacInvestorType.Product:
            case AmacInvestorType.PrivateFundProduct:
            case AmacInvestorType.SecuritiesCompanyAssetManagementPlan:
            case AmacInvestorType.FundCompanyAssetManagementPlan:
            case AmacInvestorType.FuturesCompanyAssetManagementPlan:
                IDTypes = [Models.IDType.ProductFilingCode, Models.IDType.ProductRegistrationCode, Models.IDType.Other];
                break;
            case AmacInvestorType.TrustPlan:
            case AmacInvestorType.CommercialBankFinancialProduct:
            case AmacInvestorType.InsuranceAssetManagementPlan:
            case AmacInvestorType.SocialWelfareFund:
            case AmacInvestorType.PensionFund:
            case AmacInvestorType.SocialSecurityFund:
            case AmacInvestorType.EnterpriseAnnuity:
                IDTypes = [Models.IDType.UnifiedSocialCreditCode, Models.IDType.BusinessLicenseNumber, Models.IDType.Other];
                break;
            case AmacInvestorType.GovernmentGuidanceFund:
                IDTypes = [Models.IDType.ProductFilingCode, Models.IDType.UnifiedSocialCreditCode, Models.IDType.BusinessLicenseNumber, Models.IDType.OrganizationCodeCertificate, Models.IDType.Other];
                break;

            default:
                break;
        }
    }

    private void EntityType_PropertyChanged(object? sender, PropertyChangedEventArgs? e)
    {
        switch (EntityType.NewValue)
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
            default:
                break;
        }
    }

    //[RelayCommand]
    //public void Delete(UnitViewModel unit)
    //{
    //    if (unit is IEntityViewModel<Investor> entity)
    //    {
    //        using var db = DbHelper.Base();
    //        var v = db.GetCollection<Investor>().FindById(Id);

    //        if (v is not null)
    //        {
    //            entity.RemoveValue(v);
    //            unit.Reset();
    //            db.GetCollection<Investor>().Upsert(v);

    //            WeakReferenceMessenger.Default.Send(v);
    //        }
    //    }
    //}

    //[RelayCommand]
    //public void Reset(UnitViewModel unit)
    //{
    //    var ps = unit.GetType().GetProperties().Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(PropertyViewModel<>));
    //    foreach (var item in ps)
    //    {
    //        var ty = item.PropertyType!;
    //        object? obj = item.GetValue(unit);
    //        ty.GetProperty("New")!.SetValue(obj, ty.GetProperty("Old")!.GetValue(obj));
    //    }
    //}

    //[RelayCommand]
    //public void Modify(UnitViewModel unit)
    //{
    //    if (unit is IEntityViewModel<Investor> entity)
    //    {
    //        using var db = DbHelper.Base();
    //        var v = db.GetCollection<Investor>().FindById(Id);

    //        if (v is not null)
    //            entity.UpdateEntity(v);
    //        else if (Name.OldValue is not null)
    //            v = new Investor { Name = Name.OldValue };

    //        if (v is not null)
    //        {
    //            db.GetCollection<Investor>().Upsert(v);
    //            if (Id == 0) Id = v.Id;

    //            WeakReferenceMessenger.Default.Send(v);
    //        }
    //    }
    //    unit.Apply();
    //}


    protected override void ModifyOverride(IPropertyModifier unit)
    {
        base.ModifyOverride(unit);

        var db = DbHelper.Base();
        var customer = db.GetCollection<Investor>().FindById(Id);
        db.Dispose();
        if (customer is not null)
            WeakReferenceMessenger.Default.Send(customer);
    }


    protected override void SaveOverride()
    {
        base.SaveOverride();


        var db = DbHelper.Base();
        var customer = db.GetCollection<Investor>().FindById(Id);
        db.Dispose();
        if (customer is not null)
            WeakReferenceMessenger.Default.Send(customer);
    }


    [RelayCommand]
    public void AddQualification()
    {
        using var db = DbHelper.Base();
        InvestorQualification entity = new InvestorQualification { InvestorId = Id };
        db.GetCollection<InvestorQualification>().Insert(entity);
        Qualifications.Add(QualificationViewModel.From(entity, Type.OldValue, EntityType.OldValue));
    }

    [RelayCommand]
    public void DeleteQualification(QualificationViewModel v)
    {
        if (v.Date.OldValue is not null && HandyControl.Controls.MessageBox.Show("确认删除吗？", button: System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.No)
            return;


        Qualifications.Remove(v);

        if (!v.IsFinished)
        {
            using var db = DbHelper.Base();
            db.GetCollection<InvestorQualification>().Delete(v.Id);
        }
    }

    protected override Investor InitNewEntity() => new Investor { Name = string.Empty };
}



//public partial class EntityDateEfficientViewModel<T> : UnitViewModel, IEntityViewModel<T> where T : notnull
//{
//    public PropertyViewModel<bool> LongTerm { get; } = new();

//    public PropertyViewModel<DateTime?> Begin { get; } = new();

//    public PropertyViewModel<DateTime?> End { get; } = new();

//    public override bool CanConfirm => HasUnsavedValue;

//    public override bool CanDelete => !HasUnsavedValue && (Begin.Old is not null || End.Old is not null || LongTerm.Old);

//    public override bool HasUnsavedValue => Begin.IsChanged || End.IsChanged || LongTerm.IsChanged;

//    public Func<T, DateEfficient>? InitFunc { get; set; }

//    public Action<T, DateEfficient>? UpdateFunc { get; set; }

//    public Action<T>? ClearFunc { get; set; }

//    public void Init(T param)
//    {
//        if (param is not null && InitFunc is not null)
//        {
//            var v = InitFunc(param);
//            Begin.Old = v.Begin.HasValue ? new DateTime(v.Begin.Value, default) : null;
//            Begin.New = v.Begin.HasValue ? new DateTime(v.Begin.Value, default) : null;
//            End.Old = v.End.HasValue ? new DateTime(v.End.Value, default) : null;
//            End.New = v.End.HasValue ? new DateTime(v.End.Value, default) : null;
//            LongTerm.Old = v.LongTerm;
//            LongTerm.New = v.LongTerm;
//        }
//        SubscribeChanges();
//    }

//    public void UpdateEntity(T obj)
//    {
//        if (obj is not null && UpdateFunc is not null && CanConfirm && BuildDateEfficient() is DateEfficient efficient)
//        {
//            UpdateFunc(obj, efficient);
//            Apply();
//        }
//    }

//    public void RemoveValue(T obj)
//    {
//        LongTerm.New = false;
//        LongTerm.Old = false;
//        Begin.New = null;
//        Begin.Old = null;
//        End.New = null;
//        End.Old = null;

//        if (obj is not null && ClearFunc is not null)
//            ClearFunc(obj);
//    }

//    public override string ToString()
//    {
//        return $"{Begin.New?.ToString("yyyy-MM-dd")}-{(LongTerm.New ? "长期" : End.New?.ToString("yyyy-MM-dd"))}";
//    }

//    private DateEfficient? BuildDateEfficient()
//    {
//        if (LongTerm.New)
//            return Begin.New is not null && Begin.New != default(DateTime) ? new DateEfficient { LongTerm = true, Begin = DateOnly.FromDateTime(Begin.New!.Value) } : null;
//        else
//            return Begin.New is not null && Begin.New != default(DateTime) && End.New is not null && End.New != default(DateTime) ? new DateEfficient { LongTerm = false, Begin = DateOnly.FromDateTime(Begin.New!.Value), End = DateOnly.FromDateTime(End.New!.Value) } : null;
//    }
//}

public partial class DateEfficientViewModel : ObservableObject, IEquatable<DateEfficientViewModel>
{

    [ObservableProperty]
    public partial DateOnly? Begin { get; set; }

    [ObservableProperty]
    public partial DateOnly? End { get; set; }

    [ObservableProperty]
    public partial bool IsLongTerm { get; set; }


    public DateEfficientViewModel() { }

    public DateEfficientViewModel(DateEfficient efficient)
    {
        Begin = efficient.Begin == default ? null : efficient.Begin;
        End = efficient.End == default || efficient.End == DateOnly.MaxValue ? null : efficient.End;
        IsLongTerm = efficient.LongTerm;
    }

    public DateEfficient Build() => new DateEfficient { LongTerm = IsLongTerm, Begin = Begin, End = End };

    public bool Equals(DateEfficientViewModel? other)
    {
        return Begin == other?.Begin && End == other?.End && IsLongTerm == other?.IsLongTerm;
    }

    public override string ToString()
    {
        if (Begin >= End) return string.Empty;

        return $"{Begin?.ToString("yyyy-MM-dd")}-{(IsLongTerm ? "长期" : End?.ToString("yyyy-MM-dd"))}";
    }
}