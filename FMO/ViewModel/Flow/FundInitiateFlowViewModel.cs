using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FMO;

public partial class InitiateFlowViewModel : FlowViewModel
{


    /// <summary>
    /// 要素文件
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<FileInfo>? ElementFiles { get; set; }

    /// <summary>
    /// 要素文件
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<FileInfo>? ContractFiles { get; set; }



    [SetsRequiredMembers]
    public InitiateFlowViewModel(InitiateFlow flow):base(flow)
    {
        CustomFileFolder = "Initiate";

        ElementFiles = new(flow.ElementFiles.Files.Select(x => new FileInfo(x.Path)));
        ElementFiles.CollectionChanged += ElementFiles_CollectionChanged;

        ContractFiles = new(flow.ContractFiles.Files.Select(x => new FileInfo(x.Path)));
        ContractFiles.CollectionChanged += ContractFiles_CollectionChanged;

        Initialized = true;
    }





    /// <summary>
    /// 要素文件更新
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ElementFiles_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
                dir = dir.CreateSubdirectory("Element");
                var tar = FileHelper.CopyFile(item, dir.FullName);

                FileVersion fileVersion = new FileVersion { Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path), Hash = hash, Time = item.LastWriteTime };

                var flow = db.GetCollection<FundFlow>().FindById(FlowId) as InitiateFlow;

                flow!.ElementFiles.Files.Add(fileVersion);
                db.GetCollection<FundFlow>().Update(flow);
            }
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems is not null)
        {
            using var db = new BaseDatabase();
            var flow = db.GetCollection<FundFlow>().FindById(FlowId) as InitiateFlow;


            foreach (FileInfo item in e.OldItems)
            {
                var rp = Path.GetRelativePath(Directory.GetCurrentDirectory(), item.FullName);
                var file = flow!.ElementFiles.Files.FirstOrDefault(x => rp == x.Path || x.Path == item.FullName);
                if (file is not null)
                    flow.ElementFiles.Files.Remove(file);
            }

            db.GetCollection<FundFlow>().Update(flow!);
        }

    }

    private void ContractFiles_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
                dir = dir.CreateSubdirectory("Contract");
                var tar = FileHelper.CopyFile(item, dir.FullName);

                FileVersion fileVersion = new FileVersion { Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path), Hash = hash, Time = item.LastWriteTime };

                var flow = db.GetCollection<FundFlow>().FindById(FlowId) as InitiateFlow;

                flow!.ContractFiles.Files.Add(fileVersion);
                db.GetCollection<FundFlow>().Update(flow);
            }
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems is not null)
        {
            using var db = new BaseDatabase();
            var flow = db.GetCollection<FundFlow>().FindById(FlowId) as InitiateFlow;


            foreach (FileInfo item in e.OldItems)
            {
                var rp = Path.GetRelativePath(Directory.GetCurrentDirectory(), item.FullName);
                var file = flow!.ContractFiles.Files.FirstOrDefault(x => rp == x.Path || x.Path == item.FullName);
                if (file is not null)
                    flow.ContractFiles.Files.Remove(file);
            }

            db.GetCollection<FundFlow>().Update(flow!);
        }
    }

}



