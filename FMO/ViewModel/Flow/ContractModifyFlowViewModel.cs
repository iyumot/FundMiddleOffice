using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.TPL;
using FMO.Utilities;

namespace FMO;

public partial class ContractModifyFlowViewModel : ContractRelatedFlowViewModel, IElementChangable
{



    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ModifyCollectionAccount))]
    [NotifyPropertyChangedFor(nameof(ModifyCustodyAccount))]
    public partial bool ModifyName { get; set; }

    public bool ModifyCollectionAccount { get => ModifyName || field; set => SetProperty(ref field, value); }

    public bool ModifyCustodyAccount { get => ModifyName || field; set => SetProperty(ref field, value); }

    /// <summary>
    /// 变更投资经理
    /// </summary>
    [ObservableProperty]
    public partial bool ModifyInvestmentManager { get; set; }

    [ObservableProperty]
    public partial FlowFileViewModel? RegistrationLetter { get; set; }


    [ObservableProperty]
    public partial ObservableCollection<FileInfo>? SupplementaryFile { get; set; }

    /// <summary>
    /// 变更公告
    /// </summary>
    [ObservableProperty]
    public partial FlowFileViewModel? Announcement { get; set; }


    [ObservableProperty]
    public partial FlowFileViewModel? SealedAnnouncement { get; set; }

    public FlowFileViewModel CommitmentLetter { get; }




    [SetsRequiredMembers]
    public ContractModifyFlowViewModel(ContractModifyFlow flow, Mutable<ShareClass[]>? shareClass) : base(flow, shareClass)
    {
        ///补充协议
        SupplementaryFile = new(flow.SupplementaryFile?.Files.Select(x => new FileInfo(x.Path)) ?? Array.Empty<FileInfo>());
        SupplementaryFile.CollectionChanged += SupplementaryFile_CollectionChanged;

        RegistrationLetter = new FlowFileViewModel(FundId, FlowId, "备案函", flow.RegistrationLetter?.Path, "Registration", nameof(ContractModifyFlowViewModel.RegistrationLetter)) { Filter = "PDF (*.pdf)|*.pdf;" };
        Announcement = new(FundId, FlowId, "变更公告", flow.Announcement?.Path, "Announcement", nameof(ContractModifyFlowViewModel.Announcement)) { Filter = "文档 (*.pdf,*.doc,*.docx)|*.pdf;*.doc;*.docx;" };
        SealedAnnouncement = new(FundId, FlowId, "变更公告", flow.SealedAnnouncement?.Path, "Announcement", nameof(ContractModifyFlowViewModel.SealedAnnouncement))
        {
            Filter = "PDF (*.pdf)|*.pdf;",
            SpecificFileName = () =>
            {
                using var db = DbHelper.Base();
                var fund = db.GetCollection<Fund>().FindById(FundId);

                return $"{fund.Name}_变更公告_{Date:yyyy年MM月dd日}.pdf";
            }
        };

        CommitmentLetter = new(FundId, FlowId, "信息变更承诺函", flow.CommitmentLetter?.Path, "Registration", nameof(CommitmentLetter)) { Filter = "文档 (*.pdf,*.doc,*.docx)|*.pdf;*.doc;*.docx;" };

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
    public void GenerateFile(FlowFileViewModel v)
    {
        switch (v.Property)
        {
            case nameof(CommitmentLetter):
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
                    string path = @$"{FundHelper.GetFolder(FundId)}\Registration\{fund.Name}_信息变更承诺函_{Date:yyyy年MM月dd日}.docx";
                    var fi = new FileInfo(path);
                    if (!fi.Directory!.Exists) fi.Directory.Create();

                    if (WordTpl.GenerateFromTemplate(path, "信息变更承诺函.docx", new { Name = fund.Name, Code = fund.Code }))
                    {
                        if (CommitmentLetter.File?.Exists ?? false)
                            CommitmentLetter.File.Delete();
                        CommitmentLetter.SetFile(new System.IO.FileInfo(path));
                    }
                    else HandyControl.Controls.Growl.Error("生成文件失败，请查看Log，检查模板是否存在");
                }
                catch { }
                break;
            default:
                break;
        }
    }
}