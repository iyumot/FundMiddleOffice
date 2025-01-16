using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Utilities;
using Serilog;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FMO;

/// <summary>
/// 基类
/// </summary>
public partial class FlowViewModel : ObservableObject
{

    public int FundId { get; set; }

    public int FlowId { get; set; }

    public required string Name { get; set; }

    /// <summary>
    /// 流程日期
    /// </summary>
    [ObservableProperty]
    public partial DateTime? Date { get; set; }


    /// <summary>
    /// 要素文件
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<CustomFileInfo>? CustomFiles { get; set; }

    public string CustomFileFolder { get; set; } = "Files";

    /// <summary>
    /// 只读模式
    /// </summary>
    [ObservableProperty]
    public partial bool IsReadOnly { get; set; } = true;

    /// <summary>
    /// 初始化
    /// </summary>
    protected bool Initialized { get; set; }


    [SetsRequiredMembers]
    public FlowViewModel(FundFlow flow)
    {
        FlowId = flow.Id;

        FundId = flow.FundId;

        Name = flow.Name;

        // 未初始化是null，初始化是default，初始化不会触发保存
        Date = flow.Date?.ToDateTime(default) ?? default(DateTime);

        CustomFiles = flow.CustomFiles is not null ? new(flow.CustomFiles.Select(x => new CustomFileInfo { Id = x.Id, Name = x.Name, FileInfo = x.Path is null ? null : new FileInfo(x.Path) })) : new();
        foreach (var item in CustomFiles)
            item.PropertyChanged += CustomFile_PropertyChanged;

        CustomFiles.CollectionChanged += CustomFiles_CollectionChanged;

        IsReadOnly = flow.Finished;

    }


    partial void OnIsReadOnlyChanged(bool oldValue, bool newValue)
    { 
        using var db = new BaseDatabase();
        var flow = db.GetCollection<FundFlow>().FindById(FlowId);
        flow.Finished = newValue;
        db.GetCollection<FundFlow>().Update(flow);
    }

    partial void OnDateChanged(DateTime? oldValue, DateTime? newValue)
    {
        if (oldValue is null || newValue is null) return;

        using var db = new BaseDatabase();
        var flow = db.GetCollection<FundFlow>().FindById(FlowId);
        flow!.Date = DateOnly.FromDateTime(newValue.Value);
        db.GetCollection<FundFlow>().Update(flow);
    }


    [RelayCommand]
    public void AddCustomFile()
    {
        if (CustomFiles is null)
            CustomFiles = new();

        // 如果存在未设置的，则不增加
        var last = CustomFiles.LastOrDefault();
        if (last is not null && (string.IsNullOrWhiteSpace(last.Name) || last.Name == "未命名") && last.FileInfo is null)
            return;

        CustomFileInfo item = new();
        item.PropertyChanged += CustomFile_PropertyChanged;
        CustomFiles.Add(item);
    }

    [RelayCommand]
    public void DeleteFile(CustomFileInfo file)
    {
        CustomFiles?.Remove(file);
    }



    protected void CustomFile_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not CustomFileInfo cfi) return;

        using var db = new BaseDatabase();
        var flow = db.GetCollection<FundFlow>().FindById(FlowId);

        var file = flow!.CustomFiles?.FirstOrDefault(x => x.Id == cfi.Id);
        if (file is null) return;
        if (e.PropertyName == nameof(CustomFileInfo.Name) && cfi.Name is not null)
            file.Name = cfi.Name;

        if (e.PropertyName == nameof(CustomFileInfo.FileInfo) && cfi.FileInfo is not null)
        {
            var dir = FundHelper.GetFolder(FundId);
            // 保存副本
            dir = dir.CreateSubdirectory(CustomFileFolder);
            var tar = FileHelper.CopyFile(cfi.FileInfo, dir.FullName);
            file.Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path);
            file.Hash = cfi.FileInfo.ComputeHash()!;
            file.Time = cfi.FileInfo.LastWriteTime;
        }
        db.GetCollection<FundFlow>().Update(flow);
    }


    protected void CustomFiles_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems is not null)
        {
            using var db = new BaseDatabase();
            var dir = FundHelper.GetFolder(FundId);
            var flow = db.GetCollection<FundFlow>().FindById(FlowId);
            if (flow!.CustomFiles is null)
                flow.CustomFiles = new();

            foreach (CustomFileInfo item in e.NewItems)
            {
                if (item.FileInfo is null || !item.FileInfo.Exists)
                {
                    flow!.CustomFiles.Add(new FundFileInfo { Name = item.Name ?? "未命名" });
                    continue;
                }

                string hash = item.FileInfo.ComputeHash()!;

                // 保存副本
                dir = dir.CreateSubdirectory(CustomFileFolder);
                var tar = FileHelper.CopyFile(item.FileInfo, dir.FullName);

                var fileVersion = new FundFileInfo { Name = item.Name ?? "未命名", Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path), Hash = hash, Time = item.FileInfo.LastWriteTime };

                flow!.CustomFiles.Add(fileVersion);
            }
            db.GetCollection<FundFlow>().Update(flow!);
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems is not null)
        {
            using var db = new BaseDatabase();
            var flow = db.GetCollection<FundFlow>().FindById(FlowId);
            if (flow?.CustomFiles is null) return;

            foreach (CustomFileInfo item in e.OldItems)
            {
                if (item.FileInfo is null) continue;

                var rp = Path.GetRelativePath(Directory.GetCurrentDirectory(), item.FileInfo.FullName);
                var file = flow!.CustomFiles.FirstOrDefault(x => rp == x.Path || x.Path == item.FileInfo.FullName);
                if (file is not null)
                    flow.CustomFiles.Remove(file);

                item.PropertyChanged -= CustomFile_PropertyChanged;
            }

            db.GetCollection<FundFlow>().Update(flow!);
        }
    }


    protected void SaveFile<T>(FileInfo newValue, string folder, Func<T, FundFileInfo?> file, Action<T> initFile) where T : FundFlow
    {
        using var db = new BaseDatabase();

        string hash = newValue.ComputeHash()!;

        // 保存副本
        var dir = FundHelper.GetFolder(FundId);
        dir = dir.CreateSubdirectory(folder);
        var tar = FileHelper.CopyFile(newValue, dir.FullName);

        var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path);
        var flow = db.GetCollection<FundFlow>().FindById(FlowId) as T;
        if (flow is null)
            throw new Exception($"Flow【{FlowId}】 对应类型不是 {typeof(T)}");


        var fi = file(flow);
        if (fi is null)
        {
            initFile(flow);
            fi = file(flow);
        }

        if(fi is null)
        {
            Log.Error($"保存文件[{newValue.Name}]出错 {FundId} {Name} {file} 属性为null");
            HandyControl.Controls.Growl.Error($"保存文件出错 {newValue.Name}");
            return;
        }

        fi.Path = path;
        fi.Hash = hash;
        fi.Time = newValue.LastWriteTime;
        db.GetCollection<FundFlow>().Update(flow);
    }
}



