using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using Microsoft.Win32;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
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
public partial class CustomerViewModel : EditableControlViewModelBase<Investor>
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))] public enum NaturalType { [Description("非员工")] NonEmployee, [Description("员工")] Employee };

    public static RiskEvaluation[] RiskEvaluations { get; } = [RiskEvaluation.C1, RiskEvaluation.C2, RiskEvaluation.C3, RiskEvaluation.C4, RiskEvaluation.C5];

    public static EntityType[] EntityTypes { get; } = [Models.EntityType.Natural, Models.EntityType.Institution, Models.EntityType.Product,];

    public static string QualificationTips { get; } =
        """
        专业投资者

        （一）经有关金融监管部门批准设立的金融机构，包括证券公司、期货公司、基金管理公司及其子公司、商业银行、保险公司、信托公司、财务公司等；经行业协会备案或者登记的证券公司子公司、期货公司子公司、私募基金管理人。

        （二）上述机构面向投资者发行的理财产品，包括但不限于证券公司资产管理产品、基金管理公司及其子公司产品、期货公司资产管理产品、银行理财产品、保险产品、信托产品、经行业协会备案的私募基金。

        （三）社会保障基金、企业年金等养老基金，慈善基金等社会公益基金，合格境外机构投资者（QFII）、人民币合格境外机构投资者（RQFII）。

        （四）同时符合下列条件的法人或者其他组织：

            1.最近1年末净资产不低于2000万元；

            2.最近1年末金融资产不低于1000万元；

            3.具有2年以上证券、基金、期货、黄金、外汇等投资经历。

        （五）同时符合下列条件的自然人：

            1.金融资产不低于500万元，或者最近3年个人年均收入不低于50万元；

            2.具有2年以上证券、基金、期货、黄金、外汇等投资经历，或者具有2年以上金融产品设计、投资、风险管理及相关工作经历，或者属于本条第（一）项规定的专业投资者的高级管理人员、获得职业资格认证的从事金融相关业务的注册会计师和律师。
            前款所称金融资产，是指银行存款、股票、债券、基金份额、资产管理计划、银行理财产品、信托计划、保险产品、期货及其他衍生产品等。

        ------------------------------------------------------------------------------------

        普通投资者

        （一）净资产不低于1000万元的单位；
        （二）金融资产不低于300万元或者最近三年个人年均收入不低于50万元的个人
        
        """;

    public ChangeableViewModel<Investor, string> Name { get; }

    public ChangeableViewModel<Investor, EntityType> EntityType { get; } = new() { InitFunc = x => x.EntityType, UpdateFunc = (x, y) => x.EntityType = y, ClearFunc = x => x.EntityType = Models.EntityType.Unk, Label = "客户类型" };


    [ObservableProperty]
    public partial AmacInvestorType[]? InvestorTypes { get; set; }


    public ChangeableViewModel<Investor, AmacInvestorType> Type { get; } = new() { InitFunc = x => x.Type, UpdateFunc = (x, y) => x.Type = y, ClearFunc = x => x.Type = AmacInvestorType.None, Label = "" };



    public ChangeableViewModel<Investor, IDType> IDType { get; } = new() { InitFunc = x => x.Identity.Type, UpdateFunc = (x, y) => x.Identity = x.Identity with { Type = y }, ClearFunc = x => x.Identity = x.Identity with { Type = default }, Label = "证件类型" };

    public ChangeableViewModel<Investor, string> Identity { get; } = new() { InitFunc = x => x.Identity.Id, UpdateFunc = (x, y) => x.Identity = x.Identity with { Id = y! }, ClearFunc = x => x.Identity = x.Identity with { Id = string.Empty } };

    public ChangeableViewModel<Investor, string> Email { get; } = new() { InitFunc = x => x.Email, UpdateFunc = (x, y) => x.Email = y, ClearFunc = x => x.Email = null, Label = "Email" };

    public ChangeableViewModel<Investor, string> Phone { get; } = new() { InitFunc = x => x.Phone, UpdateFunc = (x, y) => x.Phone = y, ClearFunc = x => x.Phone = null, Label = "联系方式" };

    public MultipleFileViewModel IDCards { get; }


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


    public ObservableCollection<RiskAssessmentViewModel> RiskAssessments { get; }


    public IEnumerable<TransferRecordByFund>? TransferRecords { get; set; }


    [ObservableProperty]
    public partial QualificationViewModel? SelectedQualification { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAssessmentFileChoosed))]
    public partial string? AssessmentFile { get; set; }


    public bool IsAssessmentFileChoosed => File.Exists(AssessmentFile);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddRiskAssessmentCommand))]
    public partial RiskEvaluation? NewEvaluation { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RiskConflict))]
    [NotifyCanExecuteChangedFor(nameof(AddRiskAssessmentCommand))]
    public partial DateTime? NewDate { get; set; }


    public bool RiskConflict => RiskAssessments.Any(x => x.Date.Year == NewDate?.Year && x.Date.Month == NewDate?.Month && x.Date.Day == NewDate?.Day);

    public bool CanAddRiskAssessment => IsAssessmentFileChoosed && NewDate != default && NewEvaluation != default;



    //public CustomerViewModel()
    //{
    //    Qualifications = new();
    //}


    public CustomerViewModel(int id)
    {
        using var db = DbHelper.Base();
        var investor = db.GetCollection<Investor>().FindById(id);
        Id = id;

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

        IDCards = new()
        {
            Label = "身份证明",
            Files = [.. (investor.IDCards ?? new())],
            OnAddFile = (x, y) => SetFile<Investor>(Id, x => { if (x.IDCards is null) x.IDCards = new(); return x.IDCards; }, x),
            OnDeleteFile = x => DeleleFile<Investor>(Id, x => x.IDCards, x),
        };


        Efficient.Init(investor);

        var iq = db.GetCollection<InvestorQualification>().Find(x => x.InvestorId == Id || (x.InvestorId == 0 && x.InvestorName == investor.Name && x.IdentityCode == investor.Identity.Id)).ToArray();
        Qualifications = [.. iq.Select(x => QualificationViewModel.From(x, investor.Type, investor.EntityType))];

        RiskAssessments = [.. db.GetCollection<RiskAssessment>().Find(x => x.InvestorId == Id).Select(x => new RiskAssessmentViewModel(x))];

        // TA 记录汇总
        var ta = db.GetCollection<TransferRecord>().Find(x => x.CustomerId == Id).GroupBy(x => x.FundName!);
        List<TransferRecordByFund> trbf = new();
        foreach (var tf in ta)
        {
            var fund = tf.Key;
            var Share = tf.Sum(x => x.ShareChange());
            var Deposit = tf.Where(x => x.Type switch { TransferRecordType.Subscription or TransferRecordType.Purchase or TransferRecordType.MoveIn => true, _ => false }).Sum(x => x.ConfirmedNetAmount);
            var Withdraw = tf.Where(x => x.Type switch { TransferRecordType.Redemption or TransferRecordType.Redemption or TransferRecordType.MoveOut or TransferRecordType.Distribution => true, _ => false }).Sum(x => x.ConfirmedNetAmount);

            var daily = db.GetDailyCollection(tf.First().FundId).Find(x => x.NetValue > 0).OrderByDescending(x => x.Date).FirstOrDefault();
            var nv = daily?.NetValue ?? 0;
            var Asset = Share * nv;
            var profit = Asset + Withdraw - Deposit;

            trbf.Add(new TransferRecordByFund
            {
                FundId = tf.First().FundId,
                FundName = tf.Key,
                Asset = Asset,
                Profit = profit,
                Records = tf as IEnumerable<TransferRecord>
            });
        }

        TransferRecords = trbf;
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
                IDTypes = [Models.IDType.UnifiedSocialCreditCode, Models.IDType.OrganizationCode, Models.IDType.BusinessLicenseNumber, Models.IDType.RegistrationNumber, Models.IDType.Other];
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
                IDTypes = [Models.IDType.ProductFilingCode, Models.IDType.UnifiedSocialCreditCode, Models.IDType.BusinessLicenseNumber, Models.IDType.OrganizationCode, Models.IDType.Other];
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
    public void ChooseAssessmentFile()
    {
        var fd = new OpenFileDialog();
        fd.Filter = "文档|*.pdf;*.jpg;*.png;*.jpeg";
        if (!fd.ShowDialog() ?? true) return;

        AssessmentFile = fd.FileName;

        // 判断文件名中是否有日期和评估等级
        var m = Regex.Match(fd.FileName, @"(\d{8}).*?(C\d)");
        if (m.Success)
        {
            try
            {
                NewDate = DateTime.TryParseExact(m.Groups[1].Value, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var date) ? date : null;
                NewEvaluation = Enum.Parse<RiskEvaluation>(m.Groups[2].Value, true);
            }
            catch { }
        }

    }


    [RelayCommand(CanExecute = nameof(CanAddRiskAssessment))]
    public void AddRiskAssessment()
    {
        var fd = new FileInfo(AssessmentFile!);

        var r = new RiskAssessment
        {
            Date = DateOnly.FromDateTime(NewDate!.Value),
            Level = NewEvaluation!.Value,
            InvestorId = Id,
        };

        using var db = DbHelper.Base();
        db.GetCollection<RiskAssessment>().DeleteMany(x => x.InvestorId == Id && x.Date == DateOnly.FromDateTime(NewDate.Value));

        db.GetCollection<RiskAssessment>().Insert(r);

        string destFileName = $"{r.Id}{Path.GetExtension(fd.Name)}";
        File.Copy(fd.FullName, @$"files\evaluation\" + destFileName);
        r.Path = destFileName;
        db.GetCollection<RiskAssessment>().Update(r);

        foreach (var item in RiskAssessments.ToArray())
        {
            if (item.InvestorId == Id && item.Date == DateOnly.FromDateTime(NewDate.Value))
                RiskAssessments.Remove(item);
        }
        RiskAssessments.Add(new RiskAssessmentViewModel(r));
    }

    [RelayCommand]
    public void CancelRiskAssessment()
    {
        AssessmentFile = null;
        NewDate = null;
        NewEvaluation = null;
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



    //[RelayCommand]
    //public void AddFile(IFileSelector obj)
    //{
    //    if (obj is not MultiFileViewModel<Investor> v) return;

    //    var fd = new OpenFileDialog();
    //    fd.Filter = v.Filter;
    //    if (fd.ShowDialog() != true)
    //        return;

    //    var fi = new FileInfo(fd.FileName);
    //    if (fi is not null)
    //        SetFile(v, fi);

    //}

    private FileStorageInfo? SetFile<T>(int id, Func<T, List<FileStorageInfo>> func, FileInfo fi)
    {
        string hash = fi.ComputeHash()!;

        // 保存副本
        var dir = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "files", "investor", Id.ToString(), "cards"));

        var tar = FileHelper.CopyFile2(fi, dir.FullName);
        if (tar is null)
        {
            Log.Error($"保存文件出错，{fi.Name}");
            HandyControl.Controls.Growl.Error($"无法保存{fi.Name}，文件名异常或者存在过多重名文件");
            return null;
        }

        var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar);

        using var db = DbHelper.Base();
        var q = db.GetCollection<T>().FindById(Id);
        var l = func(q);
        FileStorageInfo fsi = new()
        {
            Title = "",
            Path = path,
            Hash = hash,
            Time = DateTime.Now
        };
        l!.Add(fsi);
        db.GetCollection<T>().Update(q);
        return fsi;
    }


    private void DeleleFile<T>(int id, Func<T, List<FileStorageInfo>?> func, FileStorageInfo file)
    {
        using var db = DbHelper.Base();
        var q = db.GetCollection<T>().FindById(Id);

        var l = func(q);
        if (l is null) return;
        var old = l.Find(x => x.Path is not null && file.Path is not null && Path.GetFullPath(x.Path) == Path.GetFullPath(file.Path));
        if (old is not null)
        {
            l.Remove(old);

            try { File.Delete(old.Path!); } catch(Exception e) { Log.Error($"delete file failed {e}"); }
        }
        db.GetCollection<T>().Update(q);
    }
    //private void SetFile(MultiFileViewModel<Investor> v, FileInfo fi)
    //{
    //    string hash = fi.ComputeHash()!;

    //    // 保存副本
    //    var dir = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "files", "qualification", Id.ToString()));

    //    var tar = FileHelper.CopyFile2(fi, dir.FullName);
    //    if (tar is null)
    //    {
    //        Log.Error($"保存合投文件出错，{fi.Name}");
    //        HandyControl.Controls.Growl.Error($"无法保存{fi.Name}，文件名异常或者存在过多重名文件");
    //        return;
    //    }

    //    var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar);

    //    if (v is MultiFileViewModel<Investor> vm)
    //    {
    //        v.Files!.Add(new PartFileViewModel { MultiFile = vm, File = new FileInfo(tar), });
    //        v.Exists = true;
    //        using var db = DbHelper.Base();
    //        var q = db.GetCollection<Investor>().FindById(Id);

    //        var l = vm.GetProperty(q);
    //        if (l is null)
    //        {
    //            vm.SetProperty(q, new());
    //            l = vm.GetProperty(q);
    //        }

    //        l!.Add(new FileStorageInfo
    //        {
    //            Name = vm.Label,
    //            Path = path,
    //            Hash = hash,
    //            Time = fi.LastWriteTime
    //        });
    //        db.GetCollection<Investor>().Update(q);
    //    }
    //}

    //[RelayCommand]
    //public void DeleteFile(IFileViewModel v)
    //{
    //    if (v is PartFileViewModel m && m.MultiFile is MultiFileViewModel<Investor> multi)
    //    {
    //        if (m.File is null) return;

    //        try
    //        {
    //            using var db = DbHelper.Base();
    //            var q = db.GetCollection<Investor>().FindById(Id);

    //            var l = multi.GetProperty(q);
    //            if (l is null) return;
    //            var old = l.Find(x => new FileInfo(x.Path!).FullName == m.File.FullName);
    //            if (old is not null) l.Remove(old);
    //            db.GetCollection<Investor>().Update(q);

    //            multi.Files!.Remove(m);
    //        }
    //        catch (Exception e)
    //        {
    //            Log.Error($"删除【{Id}】合投文件失败{e.Message}");
    //            HandyControl.Controls.Growl.Error("无法删除文件");
    //        }
    //    }
    //}









    protected override Investor InitNewEntity() => new Investor { Name = string.Empty };



    public partial class TransferRecordByFund:ObservableObject
    {
        public int FundId { get; set; }

        public string? FundName { get; set; }

        public decimal Asset { get; set; }



        public IEnumerable<TransferRecord>? Records { get; set; }

        public decimal Profit { get; internal set; }

        [RelayCommand]
        public void OpenFund()
        {
            WeakReferenceMessenger.Default.Send(new OpenFundMessage(FundId));
        }
    }
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


public partial class RiskAssessmentViewModel : ObservableObject
{
    public int Id { get; set; }

    public int InvestorId { get; set; }

    public DateOnly Date { get; set; }

    public RiskEvaluation Level { get; set; }

    public string? Path { get; }

    public bool FileIsExists => string.IsNullOrWhiteSpace(Path) ? false : File.Exists(Path);

    public RiskAssessmentViewModel(RiskAssessment risk)
    {
        Id = risk.Id;
        InvestorId = risk.InvestorId;
        Date = risk.Date;
        Level = risk.Level;
        Path = risk.Path;
    }


    [RelayCommand(CanExecute = nameof(FileIsExists))]
    public void ViewFile()
    {
        if (Path is null) return;

        var file = new FileInfo(Path);
        if (Path is not null && file.Exists)
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(file.FullName) { UseShellExecute = true });
        else WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Warning, "风险调查问卷不存在"));
    }
}