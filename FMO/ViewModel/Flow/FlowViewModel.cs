using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Utilities;
using Microsoft.Win32;
using Serilog;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Windows.Controls;

namespace FMO;



public partial class FlowFileViewModel : ObservableObject
{
    public required string Name { get; set; }

    [ObservableProperty]
    public partial FileInfo? File { get; set; }

    /// <summary>
    /// 保存的路径
    /// </summary>
    public required string Folder { get; init; }


    [ObservableProperty]
    public required partial string Property { get; set; }

    /// <summary>
    /// 文件类型筛选
    /// </summary>
    public string? Filter { get; set; }

    public int FundId { get; init; }

    public int FlowId { get; init; }


    [SetsRequiredMembers]
    public FlowFileViewModel(int fundId, int flowId, string name, string? path, string folder, string property)
    {
        if (!string.IsNullOrWhiteSpace(path))
            File = new FileInfo(path);

        FundId = fundId;
        FlowId = flowId;
        Name = name;
        Folder = folder;
        Property = property;
    }

    [RelayCommand]
    public void View()
    {
        if(File?.Exists??false) 
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(File.FullName) { UseShellExecute = true }); } catch { }
    }

    [RelayCommand]
    public void Change()
    {
        var fd = new OpenFileDialog();
        fd.Filter = Filter;
        if (fd.ShowDialog() != true)
            return;


        var fi = new FileInfo(fd.FileName);
        
        SetFile(fi);
    }


    public void SetFile(FileInfo? fi)
    {
        if (fi is null || !fi.Exists)
        {
            using var db = DbHelper.Base();
            var flow = db.GetCollection<FundFlow>().FindById(FlowId);
            if (flow!.GetType().GetProperty(Property) is PropertyInfo property && property.PropertyType == typeof(FileStorageInfo))
                property.SetValue(flow, null);

            db.GetCollection<FundFlow>().Update(flow);
            File = null;
        }
        else
        {
            string hash = fi.ComputeHash()!;

            // 保存副本
            var dir = FundHelper.GetFolder(FundId);
            dir = dir.CreateSubdirectory(Folder);
            var tar = FileHelper.CopyFile2(fi, dir.FullName);
            if (tar is null)
            {
                Log.Error($"保存Flow文件出错，{fi.FullName}");
                HandyControl.Controls.Growl.Error($"无法保存{fi.FullName}，文件名异常或者存在过多重名文件");
                return;
            }

            var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar);

            using var db = DbHelper.Base();
            var flow = db.GetCollection<FundFlow>().FindById(FlowId);
            if (flow!.GetType().GetProperty(Property) is PropertyInfo property && property.PropertyType == typeof(FileStorageInfo))
                property.SetValue(flow, new FileStorageInfo(tar, hash, fi.LastWriteTime));

            db.GetCollection<FundFlow>().Update(flow);

            File = new FileInfo(tar);
        }
    }

    [RelayCommand]
    public void Clear()
    {
        using var db = DbHelper.Base();
        var flow = db.GetCollection<FundFlow>().FindById(FlowId);
        if (flow!.GetType().GetProperty(Property) is PropertyInfo property && property.PropertyType == typeof(FileStorageInfo))
            property.SetValue(flow, null);

        db.GetCollection<FundFlow>().Update(flow);
        File = null;
    }

    [RelayCommand]
    public void Print()
    {
        if (File is null || !File.Exists) return;


        PrintDialog printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            // 获取默认打印机名称
            string printerName = printDialog.PrintQueue.Name;

            // 使用系统默认的PDF阅读器打印PDF文档
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = File.FullName;
            process.StartInfo.Verb = "print"; 
            process.Start();

            // 等待打印任务完成
            process.WaitForExit();
        }
    }


    [RelayCommand]
    public void SaveAs()
    {
        if (File is null || !File.Exists) return;

        try
        {
            var d = new SaveFileDialog();
            d.FileName = File.Name;
            if (d.ShowDialog() == true)
                System.IO.File.Copy(File.FullName, d.FileName);
        }
        catch (Exception ex)
        {
            Log.Error($"文件另存为失败: {ex.Message}");
        }
    }


    public void OnDrop(string s)
    {
        var fi = new FileInfo(s);
        if (!fi.Exists) return;

        string hash = fi.ComputeHash()!;

        // 保存副本
        var dir = FundHelper.GetFolder(FundId);
        dir = dir.CreateSubdirectory(Folder);
        var tar = FileHelper.CopyFile2(fi, dir.FullName);
        if (tar is null)
        {
            Log.Error($"保存Flow文件出错，{s}");
            HandyControl.Controls.Growl.Error($"无法保存{fi.Name}，文件名异常或者存在过多重名文件");
            return;
        }

        var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar);

        using var db = DbHelper.Base();
        var flow = db.GetCollection<FundFlow>().FindById(FlowId);
        if (flow!.GetType().GetProperty(Property) is PropertyInfo property && property.PropertyType == typeof(FileStorageInfo))
            property.SetValue(flow, new FileStorageInfo(tar, hash, fi.LastWriteTime));

        db.GetCollection<FundFlow>().Update(flow);

        File = new FileInfo(tar);
    }

    [RelayCommand]
    public void Save()
    {
        if (File is null)
        {
            using var db = DbHelper.Base();
            var flow = db.GetCollection<FundFlow>().FindById(FlowId);
            if (flow!.GetType().GetProperty(Property) is PropertyInfo property && property.PropertyType == typeof(FileStorageInfo))
                property.SetValue(flow, null);
        }
        else
        {
            string hash = File.ComputeHash()!;

            // 保存副本
            var dir = FundHelper.GetFolder(FundId);
            dir = dir.CreateSubdirectory(Folder);
            var tar = FileHelper.CopyFile(File, dir.FullName);

            var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path);

            using var db = DbHelper.Base();
            var flow = db.GetCollection<FundFlow>().FindById(FlowId);
            //if (flow!.GetType().GetProperty(Property) is PropertyInfo property && property.PropertyType == typeof(FundFileInfo))
            //    property.SetValue(flow, new FundFileInfo(tar.Path, hash,  ));

          
        }
    }
}


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
    public partial ObservableCollection<CustomFileInfoViewModel>? CustomFiles { get; set; }

    public string CustomFileFolder { get; set; } = "Files";

    /// <summary>
    /// 只读模式
    /// </summary>
    [ObservableProperty]
    public partial bool IsReadOnly { get; set; } = true;

    [ObservableProperty]
    public partial bool IsSettingDate { get; set; }

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

        Date = flow.Date?.ToDateTime(default) ?? null;

        CustomFiles = flow.CustomFiles is not null ? new(flow.CustomFiles.Select(x => new CustomFileInfoViewModel { Id = x.Id, Name = x.Name, FileInfo = x.Path is null ? null : new FileInfo(x.Path) })) : new();
        foreach (var item in CustomFiles)
            item.PropertyChanged += CustomFile_PropertyChanged;

        CustomFiles.CollectionChanged += CustomFiles_CollectionChanged;

        IsReadOnly = flow.Finished;

    }


    partial void OnIsReadOnlyChanged(bool oldValue, bool newValue)
    {
        using var db = DbHelper.Base();
        var flow = db.GetCollection<FundFlow>().FindById(FlowId);
        flow.Finished = newValue;
        db.GetCollection<FundFlow>().Update(flow);
    }

    partial void OnDateChanged(DateTime? oldValue, DateTime? newValue)
    {
        if (newValue is null || !Initialized) return;

        using var db = DbHelper.Base();
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

        CustomFileInfoViewModel item = new();
        item.PropertyChanged += CustomFile_PropertyChanged;
        CustomFiles.Add(item);
    }

    [RelayCommand]
    public void DeleteFile(CustomFileInfoViewModel file)
    {
        CustomFiles?.Remove(file);
    }



    protected void CustomFile_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not CustomFileInfoViewModel cfi) return;

        using var db = DbHelper.Base();
        var flow = db.GetCollection<FundFlow>().FindById(FlowId);

        var file = flow!.CustomFiles?.FirstOrDefault(x => x.Id == cfi.Id);
        if (file is null) return;
        if (e.PropertyName == nameof(CustomFileInfoViewModel.Name) && cfi.Name is not null)
            file.Name = cfi.Name;

        if (e.PropertyName == nameof(CustomFileInfoViewModel.FileInfo) && cfi.FileInfo is not null)
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
            using var db = DbHelper.Base();
            var dir = FundHelper.GetFolder(FundId);
            var flow = db.GetCollection<FundFlow>().FindById(FlowId);
            if (flow!.CustomFiles is null)
                flow.CustomFiles = new();

            foreach (CustomFileInfoViewModel item in e.NewItems)
            {
                if (item.FileInfo is null || !item.FileInfo.Exists)
                {
                    //flow!.CustomFiles.Add(new FundFileInfo { Name = item.Name ?? "未命名" });
                    continue;
                }

                string hash = item.FileInfo.ComputeHash()!;

                // 保存副本
                dir = dir.CreateSubdirectory(CustomFileFolder);
                var tar = FileHelper.CopyFile(item.FileInfo, dir.FullName);

                var fileVersion = new FileStorageInfo(tar.Path, hash, item.FileInfo.LastWriteTime);// { Name = item.Name ?? "未命名", Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path), Hash = hash, Time = item.FileInfo.LastWriteTime };

                flow!.CustomFiles.Add(fileVersion);
            }
            db.GetCollection<FundFlow>().Update(flow!);
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems is not null)
        {
            using var db = DbHelper.Base();
            var flow = db.GetCollection<FundFlow>().FindById(FlowId);
            if (flow?.CustomFiles is null) return;

            foreach (CustomFileInfoViewModel item in e.OldItems)
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


    protected void SaveFile<T>(FileInfo newValue, string folder, Func<T, FileStorageInfo?> file, Action<T> initFile) where T : FundFlow
    {
        using var db = DbHelper.Base();

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

        if (fi is null)
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



