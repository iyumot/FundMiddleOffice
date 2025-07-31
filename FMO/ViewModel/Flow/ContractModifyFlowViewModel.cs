using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Shared;
using FMO.TPL;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;

namespace FMO;

public partial class ContractModifyFlowViewModel : ContractRelatedFlowViewModel, IElementChangable
{



    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ModifyCollectionAccount))]
    [NotifyPropertyChangedFor(nameof(ModifyCustodyAccount))]
    public partial bool ModifyName { get; set; }


    [ObservableProperty]
    public partial bool ModifyBySupplementary { get; set; }


    public bool ModifyCollectionAccount { get => ModifyName || field; set => SetProperty(ref field, value); }

    public bool ModifyCustodyAccount { get => ModifyName || field; set => SetProperty(ref field, value); }

    /// <summary>
    /// 变更投资经理
    /// </summary>
    [ObservableProperty]
    public partial bool ModifyInvestmentManager { get; set; }

    [ObservableProperty]
    public partial FileViewModel? RegistrationLetter { get; set; }

    /// <summary>
    /// 补充协议
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<FileInfo>? SupplementaryFile { get; set; }

    /// <summary>
    /// 变更公告
    /// </summary>
    [ObservableProperty]
    public partial FileViewModel? Announcement { get; set; }


    [ObservableProperty]
    public partial FileViewModel? SealedAnnouncement { get; set; }

    public FileViewModel CommitmentLetter { get; }

    public FileViewModel SealedCommitmentLetter { get; }

    /// <summary>
    /// 签署的补充协议
    /// </summary>
    public FileViewModel SignedSupplementary { get; set; }


    [SetsRequiredMembers]
    public ContractModifyFlowViewModel(ContractModifyFlow flow) : base(flow)
    {
        if (flow.Section.HasFlag(ContractModifySection.Name))
            ModifyName = true;
        if (flow.Section.HasFlag(ContractModifySection.InvestManager))
            ModifyInvestmentManager = true;
        if (flow.Section.HasFlag(ContractModifySection.ShareClass))
            ModifyShareClass = true;
        if (flow.Section.HasFlag(ContractModifySection.CollectionAccount))
            ModifyCollectionAccount = true;
        if (flow.Section.HasFlag(ContractModifySection.CustodyAccount))
            ModifyCustodyAccount = true;

        ModifyBySupplementary = flow.ModifyBySupplementary;
        if (flow.SupplementaryFile?.Files.Any() ?? false)
            ModifyBySupplementary = true;

        Contract.SpecificFileName = x =>
        {
            using var db = DbHelper.Base();
            var fund = db.GetCollection<Fund>().FindById(FundId);

            return $"{fund.Name}_基金合同_{Date:yyyy年MM月dd日}{x}";
        };


        ///补充协议
        SupplementaryFile = new(flow.SupplementaryFile?.Files.Select(x => new FileInfo(x.Path)) ?? Array.Empty<FileInfo>());
        SupplementaryFile.CollectionChanged += SupplementaryFile_CollectionChanged;

        SignedSupplementary = new()
        {
            Label = "签署的协议",
            Filter = "压缩文件|*.zip;*.rar;*.gzip;*.7z",
            SaveFolder = FundHelper.GetFolder(FundId, "SignedSupplementary"),
            GetProperty = x => x switch { ContractModifyFlow f => f.SignedSupplementary, _ => null },
            SetProperty = (x, y) => { if (x is ContractModifyFlow f) f.SignedSupplementary = y; },
        }; SignedSupplementary.Init(flow);

        RegistrationLetter = new()
        {
            Label = "备案函",
            SaveFolder = FundHelper.GetFolder(FundId, "Registration"),
            GetProperty = x => x switch { ContractModifyFlow f => f.RegistrationLetter, _ => null },
            SetProperty = (x, y) => { if (x is ContractModifyFlow f) f.RegistrationLetter = y; },
        }; RegistrationLetter.Init(flow);
        Announcement = new()
        {
            Label = "变更公告",
            SaveFolder = FundHelper.GetFolder(FundId, "Announcement"),
            GetProperty = x => x switch { ContractModifyFlow f => f.Announcement, _ => null },
            SetProperty = (x, y) => { if (x is ContractModifyFlow f) f.Announcement = y; },
        }; Announcement.Init(flow);
        SealedAnnouncement = new()
        {
            Label = "变更公告",
            SaveFolder = FundHelper.GetFolder(FundId, "Announcement"),
            GetProperty = x => x switch { ContractModifyFlow f => f.SealedAnnouncement, _ => null },
            SetProperty = (x, y) => { if (x is ContractModifyFlow f) f.SealedAnnouncement = y; },
            Filter = "PDF (*.pdf)|*.pdf;",
            SpecificFileName = x =>
            {
                using var db = DbHelper.Base();
                var fund = db.GetCollection<Fund>().FindById(FundId);

                return $"{fund.Name}_变更公告_{Date:yyyy年MM月dd日}{x}";
            }
        }; SealedAnnouncement.Init(flow);


        CommitmentLetter = new()
        {
            Label = "信息变更承诺函",
            SaveFolder = FundHelper.GetFolder(FundId, "Registration"),
            GetProperty = x => x switch { ContractModifyFlow f => f.CommitmentLetter, _ => null },
            SetProperty = (x, y) => { if (x is ContractModifyFlow f) f.CommitmentLetter = y; },
        }; CommitmentLetter.Init(flow);


        SealedCommitmentLetter = new()
        {
            Label = "信息变更承诺函",
            SaveFolder = FundHelper.GetFolder(FundId, "Registration"),
            GetProperty = x => x switch { ContractModifyFlow f => f.SealedCommitmentLetter, _ => null },
            SetProperty = (x, y) => { if (x is ContractModifyFlow f) f.SealedCommitmentLetter = y; },
            SpecificFileName = x =>
            {
                using var db = DbHelper.Base();
                var fund = db.GetCollection<Fund>().FindById(FundId);

                return $"{fund.Name}_信息变更承诺函_{Date:yyyy年MM月dd日}{x}";
            }
        }; SealedCommitmentLetter.Init(flow);


        Initialized = true;
    }

    private void SupplementaryFile_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems is not null)
        {
            using var db = DbHelper.Base();
            var fund = db.GetCollection<Fund>().FindById(FundId);

            foreach (FileInfo item in e.NewItems)
            {
                string hash = item.ComputeHash()!;

                // 保存副本
                var dir = fund.Folder();
                dir = dir.CreateSubdirectory("Supplementary");
                var tar = FileHelper.CopyFile(item, dir.FullName);

                FileVersion fileVersion = new FileVersion { Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path), Hash = hash, Time = item.LastWriteTime };

                var flow = db.GetCollection<FundFlow>().FindById(FlowId) as ContractModifyFlow;

                if (flow!.SupplementaryFile is null)
                    flow.SupplementaryFile = new VersionedFileInfo { Name = nameof(flow.SupplementaryFile) };

                flow!.SupplementaryFile.Files.Add(fileVersion);
                db.GetCollection<FundFlow>().Update(flow);
            }
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems is not null)
        {
            using var db = DbHelper.Base();
            var flow = db.GetCollection<FundFlow>().FindById(FlowId) as ContractModifyFlow;


            foreach (FileInfo item in e.OldItems)
            {
                var rp = Path.GetRelativePath(Directory.GetCurrentDirectory(), item.FullName);
                var file = flow!.SupplementaryFile?.Files.FirstOrDefault(x => rp == x.Path || x.Path == item.FullName);
                if (file is not null)
                    flow.SupplementaryFile!.Files.Remove(file);
            }

            db.GetCollection<FundFlow>().Update(flow!);
        }

    }



    [RelayCommand]
    public void GenerateFile(FileViewModel v)
    {
        if (v == CommitmentLetter)
        {
            try
            {
                // 需要先设置日期
                if (Date is null)
                {
                    HandyControl.Controls.Growl.Warning("请先设置变更日期");
                    return;
                }

                using var db = DbHelper.Base();
                var fund = db.GetCollection<Fund>().FindById(FundId);
                string path = @$"{FundHelper.GetFolder(FundId)}\Registration\信息变更承诺函_{Date:yyyy年MM月dd日}.docx";
                var fi = new FileInfo(path);
                if (!fi.Directory!.Exists) fi.Directory.Create();

                if (Tpl.GenerateByPredefined(path, "信息变更承诺函.docx", new { Name = fund.Name, Code = fund.Code, Date = Date }))
                {
                    if (CommitmentLetter.File?.Exists ?? false)
                        CommitmentLetter.File.Delete();
                    SetFile(v, path);
                }
                else HandyControl.Controls.Growl.Error("生成文件失败，请查看Log，检查模板是否存在");
            }
            catch { }
        }
    }


    [RelayCommand]
    public void SaveModifySection()
    {
        ContractModifySection section = default;
        if (ModifyName) section |= ContractModifySection.Name;
        if (ModifyInvestmentManager) section |= ContractModifySection.InvestManager;
        if (ModifyShareClass) section |= ContractModifySection.ShareClass;
        if (ModifyCollectionAccount) section |= ContractModifySection.CollectionAccount;
        if (ModifyCustodyAccount) section |= ContractModifySection.CustodyAccount;

        using var db = DbHelper.Base();
        var flow = db.GetCollection<FundFlow>().FindById(FlowId) as ContractModifyFlow;
        if (flow is not null)
        {
            flow.ModifyBySupplementary = ModifyBySupplementary;
            flow.Section = section;
            db.GetCollection<FundFlow>().Update(flow);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);


    }

    protected override void UpdateFileDateOverride()
    {
        if (Date is null) return;

        var d = DateOnly.FromDateTime(Date.Value);
        using var db = DbHelper.Base();
        if (db.GetCollection<FundFlow>().FindById(FlowId) is ContractModifyFlow flow)
        {
            if (Contract.File is not null && SwitchDate(Contract, d) && flow.ContractFile is not null)
                flow.ContractFile.Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), Contract.File.FullName);

            if (Announcement?.File is not null && SwitchDate(Announcement, d) && flow.Announcement is not null)
                flow.Announcement.Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), Announcement.File.FullName);
            if (SealedAnnouncement?.File is not null && SwitchDate(SealedAnnouncement, d) && flow.SealedAnnouncement is not null)
                flow.SealedAnnouncement.Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), SealedAnnouncement.File.FullName);

            if (CommitmentLetter?.File is not null && SwitchDate(CommitmentLetter, d) && flow.CommitmentLetter is not null)
                flow.CommitmentLetter.Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), CommitmentLetter.File.FullName);

            if (SealedCommitmentLetter?.File is not null && SwitchDate(SealedCommitmentLetter, d) && flow.SealedCommitmentLetter is not null)
                flow.SealedCommitmentLetter.Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), SealedCommitmentLetter.File.FullName);


            db.GetCollection<FundFlow>().Update(flow);
        }

    }


    private bool SwitchDate(FileViewModel file, DateOnly d)
    {
        if (file.File is null) return false;


        var m = Regex.Match(file.File.Name, @$"_(?:\d{{4}}年\d+月\d+日)?\{file.File.Extension}");
        if (!m.Success)
            return false;

        string destFileName = Path.Combine(file.File.DirectoryName!, file.File.Name[..m.Index] + $"_{d:yyyy年MM月dd日}{file.File.Extension}");
        file.File.MoveTo(destFileName);
        file.File = new FileInfo(destFileName);

        return true;
    }

}