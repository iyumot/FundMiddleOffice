using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Office2010.Excel;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
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
    public partial ObservableCollection<TransferRecordViewModel>? Records { get; set; }



    [ObservableProperty]
    public partial ObservableCollection<TransferRequest>? Requests { get; set; }

    FileSystemWatcher watcher;

    public TransferRecordPageViewModel()
    {
        Task.Run(() =>
        {
            using var db = DbHelper.Base();

            IOrderedEnumerable<TransferRecord> tr = db.GetCollection<TransferRecord>().FindAll().OrderByDescending(x => x.ConfirmedDate);
            IOrderedEnumerable<TransferRequest> tr2 = db.GetCollection<TransferRequest>().FindAll().OrderByDescending(x => x.RequestDate);

            App.Current.Dispatcher.BeginInvoke(() =>
            {
                Records = new ObservableCollection<TransferRecordViewModel>(tr.Select(x=> new TransferRecordViewModel(x) { File = new FileInfo(@$"files\tac\{x.Id}.pdf") }));
                Requests = new ObservableCollection<TransferRequest>(tr2);
            });
        });


        // 增加文件监控
        watcher = new FileSystemWatcher("files\\tac");
        watcher.EnableRaisingEvents = true;
        watcher.Created += (s, e) =>
        {
            if (e?.Name is null || Records is null) return;

            var m = Regex.Match(e.Name, @"\d+");
            if(m.Success && int.Parse(m.Value) is int id)
            {
                Records.FirstOrDefault(x => x.Id == id)?.OnPropertyChanged(nameof(TransferRecordViewModel.FileExists));
            }
        }; 
        watcher.Renamed += (s, e) =>
        {
            if (e?.Name is null || Records is null) return;

            var m = Regex.Match(e.Name, @"\d+");
            if (m.Success && int.Parse(m.Value) is int id)
            {
                var v = Records.FirstOrDefault(x => x.Id == id);
                v?.OnPropertyChanged(nameof(TransferRecordViewModel.FileExists));
            }

            m = Regex.Match(e.OldName!, @"\d+");
            if (m.Success && int.Parse(m.Value) is int id2)
            {
                var v = Records.FirstOrDefault(x => x.Id == id2);
                v?.OnPropertyChanged(nameof(TransferRecordViewModel.FileExists));
            }
        };
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


    [RelayCommand]
    public void OpenConfirmFile(FileInfo fi)
    {  
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fi.FullName) { UseShellExecute = true }); } catch { }
    }
}

[AutoChangeableViewModel(typeof(TransferRecord))]
partial class TransferRecordViewModel
{
    public required FileInfo File { get; set; }


    public bool FileExists => System.IO.File.Exists(File.FullName);
}