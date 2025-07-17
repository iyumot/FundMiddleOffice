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
    public FileStorageInfo? Build(string? file = null)
    {
        var fi = file is null ? File : new FileInfo(file);

        if (fi is null || !fi.Exists || SaveFolder is null) return null;
        var di = new DirectoryInfo(SaveFolder);
        if (!di.Exists) try { di.Create(); } catch { return null; }


        // 保存副本 
        var tar = FileHelper.CopyFile2(fi, SaveFolder, SpecificFileName is null ? null : SpecificFileName());
        if (tar is null)
        {
            Log.Error($"保存文件出错，{fi.FullName}");
            HandyControl.Controls.Growl.Error($"无法保存{fi.FullName}，文件名异常或者存在过多重名文件");
            return null;
        }

        string hash = new FileInfo(tar).ComputeHash()!;
        return new FileStorageInfo(tar, hash, fi.LastWriteTime);
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


        var obj = new DataObject(DataFormats.FileDrop, new string[] { File.FullName });
        obj.SetText(File.FullName);
        Clipboard.SetDataObject(obj);
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
public partial class FileViewModel : FileViewModelBase, IFileViewModel, IFileSelector
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



public partial class SingleFileViewModel : ObservableObject, IFileSelector//,IFileViewModel
{
    [ObservableProperty]
    public partial string Label { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Exists))]
    [NotifyPropertyChangedFor(nameof(Deleted))]
    [NotifyCanExecuteChangedFor(nameof(DeleteFileCommand))]
    public partial FileStorageInfo? File { get; set; }

    public bool Exists => File?.Exists ?? false;

    public bool Deleted => File is not null && !File.Exists;

    public bool CanDelete => File is null ? false : System.IO.File.Exists(File?.Path);

    public string? Filter { get; set; } = "文件|*.*";

    public required Func<FileInfo, string, FileStorageInfo?> OnSetFile { get; set; }

    public required Action<FileStorageInfo> OnDeleteFile { get; set; }


    public Action? FileChanged { get; set; }

    [RelayCommand]
    public void SetFile()
    {
        var fd = new OpenFileDialog();
        fd.Filter = Filter ?? "";
        if (fd.ShowDialog() != true)
            return;

        var fi = new FileInfo(fd.FileName);
        if (fi is not null)
        {
            File = OnSetFile(fi, Label);
            FileChanged?.Invoke();
        }
    }


    [RelayCommand(CanExecute = nameof(CanDelete))]
    public void DeleteFile()
    {
        try
        {
            if (File is null) return;
            OnDeleteFile(File); 
            File = null;
            FileChanged?.Invoke();
        }
        catch { }
    }


    [RelayCommand]
    public void View()
    {
        if (File?.Exists ?? false)
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(File.Path!) { UseShellExecute = true }); } catch { }
    }




    [RelayCommand]
    public void Copy()
    {
        if (File?.Path is null || !File.Exists) return;


        var obj = new DataObject(DataFormats.FileDrop, new string[] { File.Path });
        obj.SetText(File.Path);
        Clipboard.SetDataObject(obj);
    }


    [RelayCommand]
    public void SaveAs()
    {
        if (File?.Path is null || !File.Exists) return;

        try
        {
            var d = new SaveFileDialog();
            d.FileName = File.Name;
            d.Filter = Filter;
            d.DefaultExt = Path.GetExtension(File.Path);
            if (d.ShowDialog() == true)
                System.IO.File.Copy(File.Path, d.FileName!);
        }
        catch (Exception ex)
        {
            Log.Error($"文件另存为失败: {ex.Message}");
        }
    }
}




public partial class MultipleFileViewModel : ObservableObject, IFileSelector
{
    public required string Label { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteFileCommand))]
    public partial ObservableCollection<FileStorageInfo>? Files { get; set; }

    public string? Filter { get; set; }

    public required Func<FileInfo, string, FileStorageInfo?> OnAddFile { get; set; }

    public required Action<FileStorageInfo> OnDeleteFile { get; set; }

    [RelayCommand]
    public void AddFile()
    {
        var fd = new OpenFileDialog();
        fd.Filter = Filter ?? "";
        if (fd.ShowDialog() != true)
            return;

        var fi = new FileInfo(fd.FileName);
        var nf = OnAddFile(fi, Label);
        if (nf is null) return;

        if (Files is null) Files = [nf];
        else Files.Add(nf);
    }


    [RelayCommand]
    public void DeleteFile(FileStorageInfo? file)
    {
        if (file is not null)
        {
            OnDeleteFile(file);

            Files?.Remove(file);
        }
    }


    [RelayCommand]
    public void View(FileStorageInfo file)
    {
        if (file?.Path is not null)
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(file.Path) { UseShellExecute = true }); } catch { }
    }



    [RelayCommand]
    public void Copy(FileStorageInfo file)
    {
        if (file?.Path is null) return;


        var obj = new DataObject(DataFormats.FileDrop, new string[] { file.Path });
        obj.SetText(file.Path);
        Clipboard.SetDataObject(obj);
    }



    [RelayCommand]
    public void SaveAs(FileStorageInfo file)
    {
        if (file?.Path is null) return;

        try
        {
            var d = new SaveFileDialog();
            d.FileName = file.Title;
            if (d.ShowDialog() == true)
                System.IO.File.Copy(file.Path, d.FileName);
        }
        catch (Exception ex)
        {
            Log.Error($"文件另存为失败: {ex.Message}");
        }
    }
}





