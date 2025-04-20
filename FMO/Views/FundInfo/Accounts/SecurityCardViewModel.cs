using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using Microsoft.Win32;
using Serilog;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// 股卡
/// </summary>
public partial class SecurityCardViewModel : ObservableObject
{
    public SecurityCardViewModel(SecurityCard x)
    {
        Id = x.Id;
        FundId = x.FundId;
        Name = x.Name;
        SerialNo = x.SerialNo;
        CardNo = x.CardNo;
        UniversalNo = x.UniversalNo;
        FundCode = x.FundCode;
        Date = x.Date;
        Tag = x.Type switch { SecurityCardType.ShangHai => "沪", SecurityCardType.ShenZhen => "深", _ => x.CardNo.StartsWith('B') ? "沪" : "深" };
        File = new FileInfo(@$"files\accounts\security\{SerialNo}-{CardNo}.pdf");
    }

    public int Id { get; set; }

    public int FundId { get; set; }

    /// <summary>
    /// 流水号
    /// </summary>
    [ObservableProperty] public partial string? SerialNo { get; set; }

    /// <summary>
    /// 子账户号
    /// </summary>
    [ObservableProperty] public partial string? CardNo { get; set; }

    /// <summary>
    /// 一码通
    /// </summary>
    [ObservableProperty] public partial string? UniversalNo { get; set; }

    [ObservableProperty] public partial string? Name { get; set; }

    [ObservableProperty] public partial string? FundCode { get; set; }

    /// <summary>
    /// 申请日期
    /// </summary>
    public DateOnly Date { get; set; }

    public string Tag { get; set; }

    public FileInfo File { get; set; }

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
            d.DefaultExt = ".pdf";
            d.Filter = "Pdf文件|*.pdf";
            if (d.ShowDialog() == true)
                System.IO.File.Copy(File.FullName, d.FileName);
        }
        catch (Exception ex)
        {
            Log.Error($"文件另存为失败: {ex.Message}");
        }
    }

    [RelayCommand]
    public void Copy()
    {
        Clipboard.SetDataObject(new DataObject(CardNo));
    }
}


