using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Utilities;
using Microsoft.Win32;
using Serilog;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FMO.Shared;

public interface IFileViewModel
{
    bool Exists { get; }

    bool Deleted { get; }

    FileInfo? File { get; set; }

}


public interface IFileSelector
{
    string Label { get; set; }

    string? Filter { get; set; }
}


public interface IFileSetter
{
    void SetFile(IFileViewModel? file, string path);
}


public partial class FileViewModelBase : ObservableObject, IFileViewModel
{
    [ObservableProperty]
    public partial string Label { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Exists))]
    [NotifyPropertyChangedFor(nameof(Deleted))]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    public partial FileInfo? File { get; set; }

    /// <summary>
    /// "Word Documents|*.doc|Office Files|*.doc;*.xls;*.ppt"
    /// </summary>
    public string? Filter { get; set; }


    public string? DisplayName => GetShort(File?.Name);

    public bool Exists => File?.Exists ?? false;

    public bool Deleted => File is not null && !File.Exists;


    /// <summary>
    /// 特定名称
    /// </summary>
    public Func<string>? SpecificFileName { get; set; }


    public string? SaveFolder { get; set; }

    private string? GetShort(string? name, int cnt = 20)
    {
        int a = cnt / 3, b = cnt * 2 / 3;
        return name switch { string s => s.Length > a + b ? s[..a] + " ...... " + s[^b..] : s, _ => name };
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public FileStorageInfo? Build()
    {
        if (File is null || SaveFolder is null) return null;
        var di = new DirectoryInfo(SaveFolder);
        if (!di.Exists) try { di.Create(); } catch { return null; }


        // 保存副本 
        var tar = FileHelper.CopyFile2(File, SaveFolder, SpecificFileName is null ? null : SpecificFileName());
        if (tar is null)
        {
            Log.Error($"保存文件出错，{File.FullName}");
            HandyControl.Controls.Growl.Error($"无法保存{File.FullName}，文件名异常或者存在过多重名文件");
            return null;
        }

        string hash = new FileInfo(tar).ComputeHash()!;
        return new FileStorageInfo(tar, hash, File.LastWriteTime);
    }

    [RelayCommand]
    public void View()
    {
        if (File?.Exists ?? false)
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(File.FullName) { UseShellExecute = true }); } catch { }
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
    public void Copy()
    {
        if (File is null || !File.Exists) return;

        Clipboard.SetDataObject(new DataObject(DataFormats.FileDrop, new string[] { File.FullName }));
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
}

/// <summary>
/// 非泛型
/// </summary>
public partial class FileViewModel : FileViewModelBase,IFileViewModel, IFileSelector
{



    public required Func<object, FileStorageInfo?> GetProperty { get; set; }

    public required Action<object, FileStorageInfo?> SetProperty { get; set; }
     

    public void Init(object entity)
    {
        var p = GetProperty(entity);

        if (p?.Path is not null)
            File = new FileInfo(p.Path);

    }
}

/// <summary>
/// 泛型
/// </summary>
/// <typeparam name="T"></typeparam>
public partial class FileViewModel<T> : FileViewModelBase, IFileSelector
{
     

    public required Func<T, FileStorageInfo?> GetProperty { get; set; }

    public required Action<T, FileStorageInfo?> SetProperty { get; set; }





    public void Init(T entity)
    {
        var p = GetProperty(entity);

        if (p?.Path is not null)
            File = new FileInfo(p.Path);

    }

}


public partial class PartFileViewModel : FileViewModelBase, IFileViewModel
{
    public required IFileSelector MultiFile { get; set; }
}

public partial class MultiFileViewModel<T> : ObservableObject, IFileSelector
{

    [ObservableProperty]
    public partial ObservableCollection<PartFileViewModel>? Files { get; set; }

    [ObservableProperty]
    public partial bool Exists { get; set; }

    [ObservableProperty]
    public partial string Label { get; set; }

    public string? Filter { get; set; }

    public required Func<T, List<FileStorageInfo>?> GetProperty { get; set; }

    public required Action<T, List<FileStorageInfo>> SetProperty { get; set; }





    public void Init(T entity)
    {
        var p = GetProperty(entity);

        if (p is not null)
        {
            Files = new(p.Where(x => x.Path is not null).Select(x => new PartFileViewModel { MultiFile = this, File = new FileInfo(x.Path!) }));
        }
    }

}