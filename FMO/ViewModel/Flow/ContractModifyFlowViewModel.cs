using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FMO;

public partial class ContractModifyFlowViewModel : FlowViewModel, IElementChangable
{

    private const string SingleShareName = "单一份额";


    [ObservableProperty]
    public partial bool ModifyName { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<FileInfo>? SupplementaryFile { get; set; }

    /// <summary>
    /// 定稿合同
    /// </summary>
    [ObservableProperty]
    public partial FileInfo? Contract { get; set; }


    [ObservableProperty]
    public partial FileInfo? RiskDisclosureDocument { get; set; }


    /// <summary>
    /// 募集账户函
    /// </summary>
    [ObservableProperty]
    public partial FileInfo? CollectionAccount { get; set; }


    /// <summary>
    /// 托管账户函
    /// </summary>
    [ObservableProperty]
    public partial FileInfo? CustodyAccount { get; set; }



    public ObservableCollection<string> Shares { get; set; }


    [ObservableProperty]
    public partial bool IsDividingShare { get; set; }









    [SetsRequiredMembers]
    public ContractModifyFlowViewModel(ContractModifyFlow flow, Mutable<ShareClass[]>? shareClass) : base(flow)
    {
        if (!string.IsNullOrWhiteSpace(flow.ContractFile?.Path))
            Contract = new FileInfo(flow.ContractFile.Path);


        if (!string.IsNullOrWhiteSpace(flow.CollectionAccountFile?.Path))
            CollectionAccount = new FileInfo(flow.CollectionAccountFile.Path);


        if (!string.IsNullOrWhiteSpace(flow.CustodyAccountFile?.Path))
            CustodyAccount = new FileInfo(flow.CustodyAccountFile.Path);


        if (shareClass is not null && shareClass.GetValue(FlowId).Value is ShareClass[] shares)
            Shares = new ObservableCollection<string>(shares.Select(x => x.Name));
        else
            Shares = new ObservableCollection<string>([SingleShareName]);

        ///补充协议
        SupplementaryFile = new(flow.SupplementaryFile?.Files.Select(x => new FileInfo(x.Path))??Array.Empty<FileInfo>());
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


    [RelayCommand]
    public void DivideShares()
    {
        IsDividingShare = true;

        ///最大5类
        if (Shares.Count > 5) return;

        if (Shares.Count == 1)
            Shares[0] = "A";
        Shares.Add(((char)('A' + Shares.Count)).ToString());
    }

    [RelayCommand]
    public void DeleteShare(string s)
    {
        if (s == SingleShareName)
            return;

        Shares.Remove(s);
        if (Shares.Count == 1)
            Shares[0] = SingleShareName;
    }

    [RelayCommand]
    public void ConfirmShares()
    {
        IsDividingShare = false;

        using var db = new BaseDatabase();
        var elements = db.GetCollection<FundElements>().FindById(FundId);

        if (elements.ShareClasses is null)
            elements.ShareClasses = new(nameof(FundElements.ShareClasses), Shares.Select(x => new ShareClass { Name = x }).ToArray());

        else
            elements.ShareClasses.SetValue(Shares.Select(x => new ShareClass { Name = x }).ToArray(), FlowId);

        db.GetCollection<FundElements>().Update(elements);
    }
}