using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using Microsoft.Win32;
using Serilog;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;

namespace FMO.Shared;

public interface IFileViewModel
{
    bool Exists { get; }

    FileInfo? File { get; set; }

}


public interface IFileSelector
{
    string Label { get; set; }

    string? Filter { get; set; }
}


public partial class FileViewModelBase : ObservableObject, IFileViewModel
{

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Exists))]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    public partial FileInfo? File { get; set; }

    public string? DisplayName => GetShort(File?.Name);

    public bool Exists => File?.Exists ?? false;


    private string? GetShort(string? name, int cnt = 20)
    {
        int a = cnt / 3, b = cnt * 2 / 3;
        return name switch { string s => s.Length > a + b ? s[..a] + " ...... " + s[^b..] : s, _ => name };
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

public partial class FileViewModel<T> : FileViewModelBase, IFileSelector
{


    [ObservableProperty]
    public partial string Label { get; set; }

    public string? Filter { get; set; }

    public required Func<T, FileStorageInfo?> GetProperty { get; set; }

    public required Action<T, FileStorageInfo?> SetProperty { get; set; }





    public void Init(T entity)
    {
        var p = GetProperty(entity);

        if (p?.Path is not null)
        {
            File = new FileInfo(p.Path);

        }
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