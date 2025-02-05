using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;

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
    public partial PredefinedFileViewModel? RegistrationLetter { get; set; }


    [ObservableProperty]
    public partial ObservableCollection<FileInfo>? SupplementaryFile { get; set; }

    /// <summary>
    /// 变更公告
    /// </summary>
    [ObservableProperty]
    public partial PredefinedFileViewModel? Announcement { get; set; }


    [ObservableProperty]
    public partial PredefinedFileViewModel? SealedAnnouncement { get; set; }

    [SetsRequiredMembers]
    public ContractModifyFlowViewModel(ContractModifyFlow flow, Mutable<ShareClass[]>? shareClass) : base(flow, shareClass)
    {
        ///补充协议
        SupplementaryFile = new(flow.SupplementaryFile?.Files.Select(x => new FileInfo(x.Path)) ?? Array.Empty<FileInfo>());
        SupplementaryFile.CollectionChanged += SupplementaryFile_CollectionChanged;

        RegistrationLetter = new PredefinedFileViewModel(FundId, FlowId, "备案函", flow.RegistrationLetter?.Path, "Registration", nameof(ContractModifyFlowViewModel.RegistrationLetter)) { Filter = "PDF (*.pdf)|*.pdf;" };
        Announcement = new(FundId, FlowId, "变更公告", flow.Announcement?.Path, "Announcement", nameof(ContractModifyFlowViewModel.Announcement)) { Filter = "PDF (*.pdf)|*.pdf;" };
        SealedAnnouncement = new(FundId, FlowId, "变更公告", flow.SealedAnnouncement?.Path, "Announcement", nameof(ContractModifyFlowViewModel.SealedAnnouncement)) { Filter = "PDF (*.pdf)|*.pdf;" };

        Initialized = true;
    }

    private void SupplementaryFile_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems is not null)
        {
            using var db = new BaseDatabase();
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
            using var db = new BaseDatabase();
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



}