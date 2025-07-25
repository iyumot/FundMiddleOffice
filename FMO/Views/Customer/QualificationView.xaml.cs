using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using Serilog;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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


public partial class QualificationViewModel : EditableControlViewModelBase<InvestorQualification>
{
    public string? Name { get; set; }


    [ObservableProperty]
    public partial bool IsFinished { get; set; }


    //[ObservableProperty]
    //public partial DateTime? Date { get; set; }

    public ChangeableViewModel<InvestorQualification, DateOnly?> Date { get; } = new() { InitFunc = x => x.Date == default ? null : x.Date, UpdateFunc = (x, y) => x.Date = y ?? default, ClearFunc = x => x.Date = default };

    [ObservableProperty]
    public partial QualificationFileType[]? ProofTypes { get; set; }

    [ObservableProperty]
    public partial QualificationExperienceType[]? ExperienceTypes { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowTax))]
    [NotifyPropertyChangedFor(nameof(MethodNotice))]
    public partial QualificationFileType? ProofType { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MethodNotice))]
    public partial QualificationExperienceType? ExperienceType { get; set; }

    [ObservableProperty]
    public partial bool IsProfessional { get; set; }

    // public EntityValueViewModel<InvestorQualification, bool> IsProfessional { get; } = new() { InitFunc = x => x.Result == QualifiedInvestorType.Professional, UpdateFunc = (x, y) => x.Result = y ?? false ? QualifiedInvestorType.Professional : QualifiedInvestorType.Normal, ClearFunc = x => x.Result = default };

    [ObservableProperty]
    public partial bool IgnoreError { get; set; }


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
    [NotifyPropertyChangedFor(nameof(MethodNotice))]
    public partial decimal? NetAssets { get; set; }

    /// <summary>
    /// 金融资产（万） （个人）
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MethodNotice))]
    public partial decimal? FinancialAssets { get; set; }

    /// <summary>
    /// 近三年年均收入
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MethodNotice))]
    public partial decimal? Income { get; set; }

    // public FileViewModel<InvestorQualification> InfomationSheet { get; }
    public SingleFileViewModel InfomationSheet { get; }

    public SingleFileViewModel CommitmentLetter { get; }

    public SingleFileViewModel Notice { get; }

    public SingleFileViewModel TaxDeclaration { get; }



    //public MultiFileViewModel<InvestorQualification> CertificationFiles { get; set; }

    public MultipleFileViewModel CertificationFiles { get; }



    public SingleFileViewModel ProofOfExperience { get; }

    public SingleFileViewModel Authorization { get; }

    public SingleFileViewModel Agent { get; }

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

    [SetsRequiredMembers]
    public QualificationViewModel(int id)
    {
        Id = id;
        if (id == -1) return;

        using var db = DbHelper.Base();
        var obj = db.GetCollection<InvestorQualification>().FindById(Id);

        NeedExperience = obj.ProofType != QualificationFileType.Product && obj.ProofType != QualificationFileType.FinancialInstitution;
        Name = obj.InvestorName;
        IgnoreError = obj.IsSettled;

        InfomationSheet = new()
        {
            Label = "基本信息表",
            File = obj.InfomationSheet,
            OnSetFile = (x, y) => SetFile(x, y, (a, b) => a.InfomationSheet = b),
            OnDeleteFile = (x) => DeleteFile(a => a.InfomationSheet = null),
            FileChanged = () => Check()
        };
        //InfomationSheet.Init(obj);

        CommitmentLetter = new()
        {
            Label = "合格投资者承诺函",
            File = obj.CommitmentLetter,
            OnSetFile = (x, y) => SetFile(x, y, (a, b) => a.CommitmentLetter = b),
            OnDeleteFile = (x) => DeleteFile(a => a.CommitmentLetter = null),
            FileChanged = () => Check()
        };

        Notice = new()
        {
            Label = "普通/专业投资者告知书",
            File = obj.Notice,
            OnSetFile = (x, y) => SetFile(x, y, (a, b) => a.Notice = b),
            OnDeleteFile = (x) => DeleteFile(a => a.Notice = null),
            FileChanged = () => Check()
        };

        TaxDeclaration = new()
        {
            Label = "普通/税收文件",
            File = obj.TaxDeclaration,
            OnSetFile = (x, y) => SetFile(x, y, (a, b) => a.TaxDeclaration = b),
            OnDeleteFile = (x) => DeleteFile(a => a.TaxDeclaration = null),
            FileChanged = () => Check()
        };


        CertificationFiles = new()
        {
            Label = "证明文件",
            Files = [.. (obj.CertificationFiles ?? new())],
            OnAddFile = (x, y) => SetCertFile(Id, x),
            OnDeleteFile = x => DeleteCertFile(x),
            FileChanged = () => Check()
        };



        ProofOfExperience = new()
        {
            Label = "投资经历",
            File = obj.ProofOfExperience,
            OnSetFile = (x, y) => SetFile(x, y, (a, b) => a.ProofOfExperience = b),
            OnDeleteFile = (x) => DeleteFile(a => a.ProofOfExperience = null),
            FileChanged = () => Check()
        };

        Authorization = new()
        {
            Label = "代理人授权书",
            File = obj.Authorization,
            OnSetFile = (x, y) => SetFile(x, y, (a, b) => a.Authorization = b),
            OnDeleteFile = (x) => DeleteFile(a => a.Authorization = null)
        };

        Agent = new()
        {
            Label = "代理人证件",
            File = obj.Agent,
            OnSetFile = (x, y) => SetFile(x, y, (a, b) => a.Agent = b),
            OnDeleteFile = (x) => DeleteFile(a => a.Agent = null)
        };
         
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

        obj.Check();

        return obj;
    }


    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(FinancialAssets):
            case nameof(NetAssets):
            case nameof(Income):
            case nameof(ProofType):
            case nameof(ExperienceType):
                Check();
                break;
            default:
                break;
        }
    }

    partial void OnProofTypeChanged(QualificationFileType? value)
    {
        NeedExperience = (value ?? QualificationFileType.None) != QualificationFileType.Product && (value ?? QualificationFileType.None) != QualificationFileType.FinancialInstitution;
    }

    [RelayCommand]
    public void SaveQualification()
    {
        using var db = DbHelper.Base();
        var obj = db.GetCollection<InvestorQualification>().FindById(Id);
        if (Date.NewValue is not null)
            obj.Date = Date.NewValue.Value;
        obj.Result = IsProfessional ? QualifiedInvestorType.Professional : QualifiedInvestorType.Normal;
        obj.ProofType = ProofType ?? QualificationFileType.None;
        obj.ExperienceType = ExperienceType ?? QualificationExperienceType.None;
        obj.NetAssets = NetAssets;
        obj.Income = Income;
        obj.FinancialAssets = FinancialAssets;
        obj.IsSettled = IgnoreError;

        db.GetCollection<InvestorQualification>().Update(obj);

        WeakReferenceMessenger.Default.Send(obj);

        if (Date.IsValueChanged)
            Date.Apply();
        IsReadOnly = true;
    }





    private FileStorageInfo? SetFile(FileInfo fi, string label, Action<InvestorQualification, FileStorageInfo> func)
    {
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

        var fsi = new FileStorageInfo
        {
            Title = label,
            Path = path,
            Hash = hash,
            Time = fi.LastWriteTime
        };

        using var db = DbHelper.Base();
        var q = db.GetCollection<InvestorQualification>().FindById(Id);
        func(q, fsi);
        db.GetCollection<InvestorQualification>().Update(q);

        return fsi;
    }


    private void DeleteFile(Action<InvestorQualification> func)
    {
        using var db = DbHelper.Base();
        var q = db.GetCollection<InvestorQualification>().FindById(Id);
        func(q);
        db.GetCollection<InvestorQualification>().Update(q);

        (HasError, Statement) = q.Check();
    }

    private FileStorageInfo? SetCertFile(int id, FileInfo fi)
    {
        string hash = fi.ComputeHash()!;

        // 保存副本
        var dir = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "files", "qualification", id.ToString()));

        var tar = FileHelper.CopyFile2(fi, dir.FullName);
        if (tar is null)
        {
            Log.Error($"保存文件出错，{fi.Name}");
            HandyControl.Controls.Growl.Error($"无法保存{fi.Name}，文件名异常或者存在过多重名文件");
            return null;
        }

        var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar);

        using var db = DbHelper.Base();
        var q = db.GetCollection<InvestorQualification>().FindById(Id);
        if (q.CertificationFiles is null) q.CertificationFiles = new();

        var l = q.CertificationFiles;
        FileStorageInfo fsi = new()
        {
            Title = "",
            Path = path,
            Hash = hash,
            Time = DateTime.Now
        };
        l!.Add(fsi);
        db.GetCollection<InvestorQualification>().Update(q);


        (HasError, Statement) = q.Check();
        return fsi;
    }


    private void DeleteCertFile(FileStorageInfo file)
    {
        using var db = DbHelper.Base();
        var q = db.GetCollection<InvestorQualification>().FindById(Id);

        var l = q.CertificationFiles;
        if (l is null) return;
        var old = l.Find(x => x.Path is not null && file.Path is not null && Path.GetFullPath(x.Path) == Path.GetFullPath(file.Path));
        if (old is not null)
        {
            l.Remove(old);

            try { File.Delete(old.Path!); } catch (Exception e) { Log.Error($"delete file failed {e}"); }
        }
        db.GetCollection<InvestorQualification>().Update(q);


        (HasError, Statement) = q.Check();
    }

    public void Check()
    {
        HasError = false;
        List<string> info = new();
        if (Date.OldValue?.Year < 1970)
        {
            HasError = true;
            info.Add("无日期");
        }

        if (ProofType is null || ProofType == QualificationFileType.None)
        {
            HasError = true;
            info.Add("无认证类型");
        }

        if (IsProfessional && ExperienceType is null)
        {
            HasError = true;
            info.Add("无投资经历");
        }

        if (!InfomationSheet.Exists)
        {
            HasError = true;
            info.Add("基本信息表");
        }

        if (!CommitmentLetter.Exists)
        {
            HasError = true;
            info.Add("承诺函");
        }

        //if (!Notice.Exists)
        //{
        //    HasError = true;
        //    info.Add("普通/专业投资者告知书");
        //}
        if (CertificationFiles?.Files is null || CertificationFiles.Files?.Count == 0)
        {
            HasError = true;
            info.Add("证明文件");
        }
        if (IsProfessional && ProofType != QualificationFileType.Product && ProofType != QualificationFileType.FinancialInstitution && !ProofOfExperience.Exists)
        {
            HasError = true;
            info.Add("投资经历");
        }



        Statement = string.Join('，', info);
    }
    

    protected override InvestorQualification InitNewEntity() => new InvestorQualification();
}


