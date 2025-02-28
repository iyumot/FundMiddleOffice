using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Serilog;
using System.IO;
using System.Windows.Controls;

namespace FMO;




public partial class FileViewModel : ObservableObject
{
    public required string Id { get; set; }

    /// <summary>
    /// 标识
    /// </summary>
    [ObservableProperty]
    public partial string? Label { get; set; }

    [ObservableProperty]
    public partial FileInfo? File { get; set; }


    /// <summary>
    /// 文件类型筛选
    /// </summary>
    public string? Filter { get; set; }


    public Action<FileViewModel>? SaveFunc { get; set; }

    [RelayCommand]
    public void View()
    {
        if (File?.Exists ?? false)
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
        if (fi is not null)
            SetFile(fi);
    }

    protected virtual void OnChanged(FileInfo fi)
    {

    }
    protected virtual void OnClear()
    {

    }

    [RelayCommand]
    public void Clear()
    {
        OnClear();
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

        SetFile(fi);
    }

    private void SetFile(FileInfo fi)
    {
        File = fi;

        if (fi is not null)
        {
            if (fi.Exists && SaveFunc is not null)
                SaveFunc(this);

            OnChanged(fi);
        }
    }
}

