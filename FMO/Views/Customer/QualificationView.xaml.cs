using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using Microsoft.Win32;
using Serilog;
using System.ComponentModel;
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

    public FileViewModel<InvestorQualification> InfomationSheet { get; }


    public FileViewModel<InvestorQualification> CommitmentLetter { get; }

    public FileViewModel<InvestorQualification> Notice { get; }

    public FileViewModel<InvestorQualification> TaxDeclaration { get; }



    public MultiFileViewModel<InvestorQualification> CertificationFiles { get; set; }

    public FileViewModel<InvestorQualification> ProofOfExperience { get; }

    public FileViewModel<InvestorQualification> Authorization { get; }

    public FileViewModel<InvestorQualification> Agent { get; }

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
        if (id == -1) return;

        using var db = DbHelper.Base();
        var obj = db.GetCollection<InvestorQualification>().FindById(Id);

        NeedExperience = obj.ProofType != QualificationFileType.Product && obj.ProofType != QualificationFileType.FinancialInstitution;
        Name = obj.InvestorName;


        InfomationSheet = new()
        {
            Label = "基本信息表",
            GetProperty = x => x.InfomationSheet,
            SetProperty = (x, y) => x.InfomationSheet = y,

        };
        InfomationSheet.Init(obj);

        CommitmentLetter = new()
        {
            Label = "合格投资者承诺函",
            GetProperty = x => x.CommitmentLetter,
            SetProperty = (x, y) => x.CommitmentLetter = y,
        };
        CommitmentLetter.Init(obj);

        Notice = new()
        {
            Label = "普通/专业投资者告知书",
            GetProperty = x => x.Notice,
            SetProperty = (x, y) => x.Notice = y,
        };
        Notice.Init(obj);

        TaxDeclaration = new()
        {
            Label = "普通/税收文件",
            GetProperty = x => x.TaxDeclaration,
            SetProperty = (x, y) => x.TaxDeclaration = y,
        };
        TaxDeclaration.Init(obj);

        CertificationFiles = new MultiFileViewModel<InvestorQualification>
        {
            Label = "证明文件",
            GetProperty = x => x.CertificationFiles,
            SetProperty = (x, y) => x.CertificationFiles = y,
        };
        CertificationFiles.Files = obj.CertificationFiles is null ? new() : new(obj.CertificationFiles.Select(x => new PartFileViewModel { MultiFile = CertificationFiles, File = new FileInfo(x.Path!) }));
        CertificationFiles.Init(obj);





        ProofOfExperience = new()
        {
            Label = "投资经历",
            GetProperty = x => x.ProofOfExperience,
            SetProperty = (x, y) => x.ProofOfExperience = y,
        };
        ProofOfExperience.Init(obj);

        Authorization = new()
        {
            Label = "代理人授权书",
            GetProperty = x => x.Authorization,
            SetProperty = (x, y) => x.Authorization = y,
        };
        Authorization.Init(obj);

        Agent = new()
        {
            Label = "代理人证件",
            GetProperty = x => x.Agent,
            SetProperty = (x, y) => x.Agent = y,
        };
        Agent.Init(obj);


        (HasError, Statement) = obj.Check();
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

            //using var db = DbHelper.Base();
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
        using var db = DbHelper.Base();
        var obj = db.GetCollection<InvestorQualification>().FindById(Id);
        obj.Result = IsProfessional ? QualifiedInvestorType.Professional : QualifiedInvestorType.Normal;
        obj.ProofType = ProofType ?? QualificationFileType.None;
        obj.ExperienceType = ExperienceType ?? QualificationExperienceType.None;
        obj.NetAssets = NetAssets;
        obj.Income = Income;
        obj.FinancialAssets = FinancialAssets;


        (HasError, Statement) = obj.Check();

        db.GetCollection<InvestorQualification>().Update(obj);

        WeakReferenceMessenger.Default.Send(obj);

        IsReadOnly = true;
    }

    [RelayCommand]
    public void SetFile(IFileSelector obj)
    {
        if (obj is not FileViewModel<InvestorQualification> v) return;

        var fd = new OpenFileDialog();
        fd.Filter = v.Filter;
        if (fd.ShowDialog() != true)
            return;

        var old = v.File;

        var fi = new FileInfo(fd.FileName);
        if (fi is not null)
            SetFile(v, fi);

        try { if (old is not null) File.Delete(old.FullName); } catch { }
    }

    [RelayCommand]
    public void AddFile(IFileSelector obj)
    {
        if (obj is not MultiFileViewModel<InvestorQualification> v) return;

        var fd = new OpenFileDialog();
        fd.Filter = v.Filter;
        if (fd.ShowDialog() != true)
            return;

        var fi = new FileInfo(fd.FileName);
        if (fi is not null)
            SetFile(v, fi);

    }

    private void SetFile(MultiFileViewModel<InvestorQualification> v, FileInfo fi)
    {
        string hash = fi.ComputeHash()!;

        // 保存副本
        var dir = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "files", "qualification", Id.ToString()));

        var tar = FileHelper.CopyFile2(fi, dir.FullName);
        if (tar is null)
        {
            Log.Error($"保存合投文件出错，{fi.Name}");
            HandyControl.Controls.Growl.Error($"无法保存{fi.Name}，文件名异常或者存在过多重名文件");
            return;
        }

        var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar);

        if (v is MultiFileViewModel<InvestorQualification> vm)
        {
            v.Files!.Add(new PartFileViewModel { MultiFile = vm, File = new FileInfo(tar), });
            v.Exists = true;
            using var db = DbHelper.Base();
            var q = db.GetCollection<InvestorQualification>().FindById(Id);

            var l = vm.GetProperty(q);
            if (l is null)
            {
                vm.SetProperty(q, new());
                l = vm.GetProperty(q);
            }

            l!.Add(new FileStorageInfo
            {
                Name = vm.Label,
                Path = path,
                Hash = hash,
                Time = fi.LastWriteTime
            });
            db.GetCollection<InvestorQualification>().Update(q);
        }
    }

    [RelayCommand]
    public void DeleteFile(IFileViewModel v)
    {
        if (v is FileViewModel<InvestorQualification> vm)
        {
            try
            {
                using var db = DbHelper.Base();
                var q = db.GetCollection<InvestorQualification>().FindById(Id);
                vm.SetProperty(q, new FileStorageInfo
                {
                    Name = vm.Label,
                    Path = null,
                    Hash = null,
                    Time = default
                });
                db.GetCollection<InvestorQualification>().Update(q);

                v.File = null;
            }
            catch (Exception e)
            {
                Log.Error($"删除【{Id}】合投文件失败{e.Message}");
                HandyControl.Controls.Growl.Error("无法删除文件");
            }
        }
        else if (v is PartFileViewModel m && m.MultiFile is MultiFileViewModel<InvestorQualification> multi)
        {
            if (m.File is null) return;

            try
            {
                using var db = DbHelper.Base();
                var q = db.GetCollection<InvestorQualification>().FindById(Id);

                var l = multi.GetProperty(q);
                if (l is null) return;
                var old = l.Find(x => new FileInfo(x.Path!).FullName == m.File.FullName);
                if (old is not null) l.Remove(old);
                db.GetCollection<InvestorQualification>().Update(q);

                multi.Files!.Remove(m);
            }
            catch (Exception e)
            {
                Log.Error($"删除【{Id}】合投文件失败{e.Message}");
                HandyControl.Controls.Growl.Error("无法删除文件");
            }
        }
    }

    private void SetFile(IFileViewModel v, FileInfo fi)
    {
        string hash = fi.ComputeHash()!;

        // 保存副本
        var dir = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "files", "qualification", Id.ToString()));

        var tar = FileHelper.CopyFile2(fi, dir.FullName);
        if (tar is null)
        {
            Log.Error($"保存合投文件出错，{fi.Name}");
            HandyControl.Controls.Growl.Error($"无法保存{fi.Name}，文件名异常或者存在过多重名文件");
            return;
        }

        var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar);

        if (v is FileViewModel<InvestorQualification> vm)
        {
            v.File = new FileInfo(tar);
            using var db = DbHelper.Base();
            var q = db.GetCollection<InvestorQualification>().FindById(Id);
            vm.SetProperty(q, new FileStorageInfo
            {
                Name = vm.Label,
                Path = path,
                Hash = hash,
                Time = fi.LastWriteTime
            });
            db.GetCollection<InvestorQualification>().Update(q);
        }
    }


    [RelayCommand]
    public void ChooseFile(IFileSelector obj)
    {
        if (obj is not FileViewModel<InvestorQualification> v) return;

        var fd = new OpenFileDialog();
        fd.Filter = v.Filter;
        if (fd.ShowDialog() != true)
            return;

        var old = v.File;

        var fi = new FileInfo(fd.FileName);
        if (fi is not null)
            SetFile(v, fi);

        try { if (old is not null) File.Delete(old.FullName); } catch { }
    }


    [RelayCommand]
    public void Clear(IFileSelector obj)
    {
        if (obj is FileViewModel<InvestorQualification> fvm)
        {
            using var db = DbHelper.Base();
            var q = db.GetCollection<InvestorQualification>().FindById(Id);
            fvm.SetProperty(q, null);
            db.GetCollection<InvestorQualification>().Update(q);
            fvm.File = null;
        }
    }

    protected override InvestorQualification InitNewEntity() => new InvestorQualification();
}


