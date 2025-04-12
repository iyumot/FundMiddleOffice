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
        Task.Run(() =>
        {
            using var db = DbHelper.Base();

            IOrderedEnumerable<TransferRecord> tr = db.GetCollection<TransferRecord>().FindAll().OrderByDescending(x => x.ConfirmedDate);
            IOrderedEnumerable<TransferRequest> tr2 = db.GetCollection<TransferRequest>().FindAll().OrderByDescending(x => x.RequestDate);

            App.Current.Dispatcher.BeginInvoke(() =>
            {
                Records = new ObservableCollection<TransferRecord>(tr);
                Requests = new ObservableCollection<TransferRequest>(tr2);
            });
        });
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