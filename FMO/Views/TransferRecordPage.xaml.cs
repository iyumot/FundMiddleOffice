using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// TransferRecordPage.xaml 的交互逻辑
/// </summary>
public partial class TransferRecordPage : UserControl
{
    public TransferRecordPage()
    {
        InitializeComponent();
    }
}


public partial class TransferRecordPageViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<TransferRecord>? Records { get; set; }



    [ObservableProperty]
    public partial ObservableCollection<TransferRequest>? Requests { get; set; }

    public TransferRecordPageViewModel()
    {
        using var db = DbHelper.Base();

        Records = new ObservableCollection<TransferRecord>(db.GetCollection<TransferRecord>().FindAll().OrderByDescending(x => x.ConfirmedDate));
        Requests = new ObservableCollection<TransferRequest>(db.GetCollection<TransferRequest>().FindAll().OrderByDescending(x => x.RequestDate));
    }









    [RelayCommand]
    public void CalcFee()
    {
        try
        {
            var di = new DirectoryInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName).Parent!;

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = Path.Combine(di.FullName, "FMO.FeeCalc.exe"), WorkingDirectory = Directory.GetCurrentDirectory() });
        }
        catch (Exception e)
        {

            HandyControl.Controls.Growl.Error($"无法启动计算器，{e.Message}");
        }
    }

}