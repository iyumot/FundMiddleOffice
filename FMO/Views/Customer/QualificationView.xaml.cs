using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using Serilog;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// QualificationView.xaml 的交互逻辑
/// </summary>
public partial class QualificationView : UserControl
{
    public QualificationView()
    {
        InitializeComponent();
    }
}


public partial class QualificationViewModel : ObservableObject
{
    public int Id { get; private set; }

    public string? Name { get; set; }

    [ObservableProperty]
    public partial bool IsReadOnly { get; set; } = true;

    [ObservableProperty]
    public partial bool IsFinished { get; set; }


    //[ObservableProperty]
    //public partial DateTime? Date { get; set; }

    public EntityValueViewModel<InvestorQualification, DateOnly> Date { get; } = new() { InitFunc = x => x.Date == default ? null : x.Date, UpdateFunc = (x, y) => x.Date = y ?? default, ClearFunc = x => x.Date = default };

    [ObservableProperty]
    public partial QualificationFileType[]? ProofTypes { get; set; }

    [ObservableProperty]
    public partial QualificationExperienceType[]? ExperienceTypes { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowTax))]
    public partial QualificationFileType? ProofType { get; set; }

    [ObservableProperty]
    public partial QualificationExperienceType? ExperienceType { get; set; }

    [ObservableProperty]
    public partial bool IsProfessional { get; set; }

    // public EntityValueViewModel<InvestorQualification, bool> IsProfessional { get; } = new() { InitFunc = x => x.Result == QualifiedInvestorType.Professional, UpdateFunc = (x, y) => x.Result = y ?? false ? QualifiedInvestorType.Professional : QualifiedInvestorType.Normal, ClearFunc = x => x.Result = default };


    [ObservableProperty]
    public partial bool NeedExperience { get; set; }

    [ObservableProperty]
    public partial AmacInvestorType InvestorType { get; set; }

    /// <summary>
    /// 需要代理人
    /// </summary>
    public bool ShowAgent { get; init; }

    public bool ShowNatural { get; init; }

    public bool ShowTax => ProofType != QualificationFileType.Product && ProofType != QualificationFileType.FinancialInstitution;


    [ObservableProperty]
    public partial decimal? NetAssets { get; set; }

    /// <summary>
    /// 金融资产（万） （个人）
    /// </summary>
    [ObservableProperty]
    public partial decimal? FinancialAssets { get; set; }

    /// <summary>
    /// 近三年年均收入
    /// </summary>
    [ObservableProperty]
    public partial decimal? Income { get; set; }


    public FileViewModel InfomationSheet { get; }

    public FileViewModel CommitmentLetter { get; }

    public FileViewModel Notice { get; }

    public FileViewModel TaxDeclaration { get; }


    public FileViewModel CertificationMaterials { get; }

    public FileViewModel ProofOfExperience { get; }

    public FileViewModel Authorization { get; }

    public FileViewModel Agent { get; }

    /// <summary>
    /// 代理人是法代
    /// </summary>
    public bool AgentIsLeagal { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string? Statement { get; set; }

    /// <summary>
    /// 认定方式说明
    /// </summary>
    public string MethodNotice => MakeMethod();

    private string MakeMethod()
    {
        string s = "";
        switch (ProofType)
        {
            case QualificationFileType.Financial:
                s = FinancialAssets switch { >= 500 => "500万金融资产证明", >= 300 => "300万金融资产证明", > 0 => "金融资产证明(无效金额)", _ => "金融资产证明(请填写金额)" };
                break;
            case QualificationFileType.Income:
                s = Income switch { >= 50 => "50万年均收入证明", > 0 => "年均收入证明(无效金额)", _ => "年均收入证明(请填写金额)" };
                break;
            case QualificationFileType.Employee:
                s = "管理人员工";
                break;
            case QualificationFileType.NetAssets:
                s = IsProfessional ? (NetAssets switch { >= 2000 => "年末净资产>2000万", > 0 => "年末净资产(无效金额)", _ => "年末净资产(请填写金额)" } + FinancialAssets switch { >= 1000 => "1000万金融资产证明", > 0 => "金融资产证明(无效金额)", _ => "金融资产证明(请填写金额)" }) : NetAssets switch { >= 1000 => "年末净资产>1000万", _ => "年末净资产(请填写金额)" };
                break;
            case QualificationFileType.FinancialInstitution:
                s = "金融机构";
                break;
            case QualificationFileType.Product:
                s = "基金产品";
                break;
            default:
                break;
        }

        switch (ExperienceType)
        {
            case QualificationExperienceType.Invest:
                s += " + 2年投资经历";
                break;
            case QualificationExperienceType.Work:
                s += " + 2年以上金融产品设计、投资、风险管理及相关工作经历";
                break;
            case QualificationExperienceType.Senior:
                s += " + 特殊专业机构投资者的高级管理人员";
                break;
            case QualificationExperienceType.Lawyer:
                s += " + 获得职业资格认证的从事金融相关业务的注册会计师和律师";
                break;
        }


        return s;
    }


    //public QualificationViewModel()
    //{
    //    AssetsFile = new FileViewModel { Id = nameof(AssetsFile), SaveFunc = x => Save(x) };
    //    InfomationSheet = new FileViewModel { Id = nameof(InfomationSheet), Label = "基本信息表", SaveFunc = x => Save(x) };
    //}

    public QualificationViewModel(int id)
    {
        Id = id;
        using var db = new BaseDatabase();
        var obj = db.GetCollection<InvestorQualification>().FindById(Id);

        NeedExperience = obj.ProofType != QualificationFileType.Product && obj.ProofType != QualificationFileType.FinancialInstitution;
        Name = obj.InvestorName;
        InfomationSheet = new FileViewModel
        {
            Id = nameof(InfomationSheet),
            Label = "基本信息表",
            File = obj.InfomationSheet?.Path is null ? null : new FileInfo(obj.InfomationSheet.Path),
            SaveFunc = x => Save(x),
            StoreFunc = x => StoreFile(x),
            ClearFunc = x => ClearFile(x)
        };

        CommitmentLetter = new FileViewModel
        {
            Id = nameof(CommitmentLetter),
            Label = "合格投资者承诺函",
            File = obj.CommitmentLetter?.Path is null ? null : new FileInfo(obj.CommitmentLetter.Path),
            SaveFunc = x => Save(x),
            StoreFunc = x => StoreFile(x),
            ClearFunc = x => ClearFile(x)
        };

        Notice = new FileViewModel
        {
            Id = nameof(Notice),
            Label = "普通/专业投资者告知书",
            File = obj.Notice?.Path is null ? null : new FileInfo(obj.Notice.Path),
            SaveFunc = x => Save(x),
            StoreFunc = x => StoreFile(x),
            ClearFunc = x => ClearFile(x)
        };
        TaxDeclaration = new FileViewModel
        {
            Id = nameof(TaxDeclaration),
            Label = "税收文件",
            File = obj.TaxDeclaration?.Path is null ? null : new FileInfo(obj.TaxDeclaration.Path),
            SaveFunc = x => Save(x),
            StoreFunc = x => StoreFile(x),
            ClearFunc = x => ClearFile(x)
        };

        CertificationMaterials = new FileViewModel
        {
            Id = nameof(CertificationMaterials),
            Label = "证明文件",
            File = obj.CertificationMaterials?.Path is null ? null : new FileInfo(obj.CertificationMaterials.Path),
            SaveFunc = x => Save(x),
            StoreFunc = x => StoreFile(x),
            ClearFunc = x => ClearFile(x)
        };

        ProofOfExperience = new FileViewModel
        {
            Id = nameof(ProofOfExperience),
            Label = "投资经历",
            File = obj.ProofOfExperience?.Path is null ? null : new FileInfo(obj.ProofOfExperience.Path),
            SaveFunc = x => Save(x),
            StoreFunc = x => StoreFile(x),
            ClearFunc = x => ClearFile(x)
        };
        Authorization = new FileViewModel
        {
            Id = nameof(Authorization),
            Label = "代理人授权书",
            File = obj.Authorization?.Path is null ? null : new FileInfo(obj.Authorization.Path),
            SaveFunc = x => Save(x),
            StoreFunc = x => StoreFile(x),
            ClearFunc = x => ClearFile(x)
        };
        Agent = new FileViewModel
        {
            Id = nameof(Agent),
            Label = "代理人证件",
            File = obj.Agent?.Path is null ? null : new FileInfo(obj.Agent.Path),
            SaveFunc = x => Save(x),
            StoreFunc = x => StoreFile(x),
            ClearFunc = x => ClearFile(x)
        };
    }

    private void ClearFile(FileViewModel v)
    {
        using var db = new BaseDatabase();
        var obj = db.GetCollection<InvestorQualification>().FindById(Id);

        if (obj!.GetType().GetProperty(v.Id) is PropertyInfo property && property.PropertyType == typeof(FileStorageInfo))
            property.SetValue(obj, null);
        db.GetCollection<InvestorQualification>().Update(obj);

        v.File?.Delete();
    }

    private string? StoreFile(FileViewModel v)
    {
        var fi = v.File;
        if (fi is null) return null;

        string hash = fi.ComputeHash()!;

        // 保存副本
        var dir = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "files", "qualification", Id.ToString()));

        var tar = FileHelper.CopyFile2(fi, dir.FullName);
        if (tar is null)
        {
            Log.Error($"保存合投文件出错，{fi.Name}");
            HandyControl.Controls.Growl.Error($"无法保存{fi.Name}，文件名异常或者存在过多重名文件");
            return null;
        }

        var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar);
        return path;
    }

    private void Save(FileViewModel v)
    {
        var fi = v.File;
        if (fi is null) return;

        string hash = fi.ComputeHash()!;

        using var db = new BaseDatabase();
        var obj = db.GetCollection<InvestorQualification>().FindById(Id);

        if (obj!.GetType().GetProperty(v.Id) is PropertyInfo property && property.PropertyType == typeof(FileStorageInfo))
            property.SetValue(obj, new FileStorageInfo(fi.FullName, hash, fi.LastWriteTime));
        db.GetCollection<InvestorQualification>().Update(obj);

    }




    public static QualificationViewModel From(InvestorQualification x, AmacInvestorType investor, EntityType entityType)
    {
        var obj = new QualificationViewModel(x.Id)
        {
            Name = x.InvestorName,
            IsProfessional = x.Result == QualifiedInvestorType.Professional,
            //  EntityType = entityType,
            InvestorType = investor,
            ShowAgent = entityType != EntityType.Natural,
            ShowNatural = entityType == EntityType.Natural,
            FinancialAssets = x.FinancialAssets,
            Income = x.Income,
            NetAssets = x.NetAssets
        };

        obj.Date.Init(x);
        //obj.IsProfessional.Init(x);

        switch (entityType)
        {
            case EntityType.Natural:
                obj.ProofTypes = [QualificationFileType.Financial, QualificationFileType.Income, QualificationFileType.Employee];
                break;
            case EntityType.Institution:
                obj.ProofTypes = [QualificationFileType.NetAssets, QualificationFileType.FinancialInstitution];
                break;
            case EntityType.Product:
                obj.ProofTypes = [QualificationFileType.Product];
                break;
            default:
                break;
        }

        if (obj.ProofTypes?.Length == 1 && x.ProofType == QualificationFileType.None)
            obj.ProofType = obj.ProofTypes[0];
        else
            obj.ProofType = x.ProofType;

        obj.ExperienceTypes = entityType == EntityType.Natural ? [QualificationExperienceType.Invest, QualificationExperienceType.Work, QualificationExperienceType.Senior, QualificationExperienceType.Lawyer] : [QualificationExperienceType.Invest];
        obj.ExperienceType = x.ExperienceType;

        (obj.HasError, obj.Statement) = x.Check();

        return obj;
    }


    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            //case nameof(Date):

            //using var db = new BaseDatabase();
            //var obj = db.GetCollection<InvestorQualification>().FindById(Id);
            //obj.Date = Date;
            //   break;
        }
    }

    partial void OnProofTypeChanged(QualificationFileType? value)
    {
        NeedExperience = (value ?? QualificationFileType.None) != QualificationFileType.Product && (value ?? QualificationFileType.None) != QualificationFileType.FinancialInstitution;
    }

    [RelayCommand]
    public void SaveQualification()
    {
        using var db = new BaseDatabase();
        var obj = db.GetCollection<InvestorQualification>().FindById(Id);
        obj.Result = IsProfessional ? QualifiedInvestorType.Professional : QualifiedInvestorType.Normal;
        obj.ProofType = ProofType ?? QualificationFileType.None;
        obj.ExperienceType = ExperienceType ?? QualificationExperienceType.None;
        obj.NetAssets = NetAssets;
        obj.Income = Income;
        obj.FinancialAssets = FinancialAssets;


        var (a, b) = obj.Check();

        HasError = a;
        Statement = b;

        db.GetCollection<InvestorQualification>().Update(obj);

        IsReadOnly = true;
    }



    [RelayCommand]
    public void Delete(UnitViewModel unit)
    {
        if (unit is IEntityViewModel<InvestorQualification> entity)
        {
            using var db = new BaseDatabase();
            var v = db.GetCollection<InvestorQualification>().FindById(Id);

            if (v is not null)
            {
                entity.RemoveValue(v);
                entity.Init(v);
                db.GetCollection<InvestorQualification>().Upsert(v);

                WeakReferenceMessenger.Default.Send(v);
            }
        }
    }

    [RelayCommand]
    public void Reset(UnitViewModel unit)
    {
        var ps = unit.GetType().GetProperties().Where(x => x.PropertyType.IsGenericType && (x.PropertyType.GetGenericTypeDefinition() == typeof(ValueViewModel<>) || x.PropertyType.GetGenericTypeDefinition() == typeof(RefrenceViewModel<>)));
        foreach (var item in ps)
        {
            var ty = item.PropertyType!;
            object? obj = item.GetValue(unit);
            ty.GetProperty("New")!.SetValue(obj, ty.GetProperty("Old")!.GetValue(obj));
        }
    }

    [RelayCommand]
    public void Modify(UnitViewModel unit)
    {
        if (unit is IEntityViewModel<InvestorQualification> property)
        {
            using var db = new BaseDatabase();
            var v = db.GetCollection<InvestorQualification>().FindById(Id);

            if (v is not null)
                property.UpdateEntity(v);

            if (v is not null)
            {
                db.GetCollection<InvestorQualification>().Upsert(v);
                if (Id == 0) Id = v.Id;

                //WeakReferenceMessenger.Default.Send(v);
            }
        }
        unit.Apply();
    }
}


