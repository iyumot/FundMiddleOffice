using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FMO;


public partial class ContractModifyFlowViewModel : ContractRelatedFlowViewModel, IElementChangable
{

    private const string SingleShareName = "单一份额";


    [ObservableProperty]
    public partial bool ModifyName { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<FileInfo>? SupplementaryFile { get; set; }



    [ObservableProperty]
    public partial FileInfo? RiskDisclosureDocument { get; set; }







    [SetsRequiredMembers]
    public ContractModifyFlowViewModel(ContractModifyFlow flow, Mutable<ShareClass[]>? shareClass) : base(flow, shareClass)
    {

        if (shareClass is not null && shareClass.GetValue(FlowId).Value is ShareClass[] shares)
            Shares = new ObservableCollection<string>(shares.Select(x => x.Name));
        else
            Shares = new ObservableCollection<string>([SingleShareName]);

        ///补充协议
        SupplementaryFile = new(flow.SupplementaryFile?.Files.Select(x => new FileInfo(x.Path)) ?? Array.Empty<FileInfo>());
        SupplementaryFile.CollectionChanged += SupplementaryFile_CollectionChanged;

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