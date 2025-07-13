using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using Serilog;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;

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


public partial class TransferRecordPageViewModel : ObservableObject, IRecipient<TransferRecord>, IRecipient<PageTAMessage>
{
    [ObservableProperty]
    public partial ObservableCollection<TransferRecordViewModel>? Records { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<TransferOrderViewModel>? Orders { get; set; }

    public CollectionViewSource RecordsSource { get; } = new();
    public CollectionViewSource OrderSource { get; } = new();



    [ObservableProperty]
    public partial string? SearchKeyword { get; set; }

    [ObservableProperty]
    public partial int TabIndex { get; set; } = 2;

    [ObservableProperty]
    public partial ObservableCollection<TransferRequest>? Requests { get; set; }

    public CollectionViewSource RequestsSource { get; set; } = new();

    FileSystemWatcher? watcher;

    public TransferRecordPageViewModel()
    {
        WeakReferenceMessenger.Default.RegisterAll(this);

        RequestsSource.Filter += (s, e) => e.Accepted = string.IsNullOrWhiteSpace(SearchKeyword) ? true :
        e.Item switch { TransferRequest r => r.CustomerName.Contains(SearchKeyword) || r.FundName.Contains(SearchKeyword) || r.CustomerIdentity.Contains(SearchKeyword), _ => true };
        RecordsSource.Filter += (s, e) => e.Accepted = string.IsNullOrWhiteSpace(SearchKeyword) ? true :
        e.Item switch { TransferRecordViewModel r => (r.CustomerName?.Contains(SearchKeyword) ?? false) || (r.FundName?.Contains(SearchKeyword) ?? false) || (r.CustomerIdentity?.Contains(SearchKeyword) ?? false), _ => true };

        Task.Run(() =>
        {
            using var db = DbHelper.Base();

            var tr = db.GetCollection<TransferRecord>().FindAll();
            IOrderedEnumerable<TransferRequest> tr2 = db.GetCollection<TransferRequest>().FindAll().OrderByDescending(x => x.RequestDate);
            var t3 = db.GetCollection<TransferOrder>().FindAll();

            App.Current.Dispatcher.BeginInvoke(() =>
            {
                Records = [..tr.Select(x => new TransferRecordViewModel(x))];
                Requests = new ObservableCollection<TransferRequest>(tr2);
                Orders = [..t3.Select(x => new TransferOrderViewModel(x))];

                RecordsSource.Source = Records;
                RequestsSource.Source = Requests;
                OrderSource.Source = Orders;
            });
        });

        try
        {

            // 增加文件监控
            watcher = new FileSystemWatcher("files\\tac");
            watcher.EnableRaisingEvents = true;
            watcher.Created += (s, e) =>
            {
                if (e?.Name is null || Records is null) return;

                var m = Regex.Match(e.Name, @"\d+");
                if (m.Success && int.Parse(m.Value) is int id)
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
        catch (Exception e)
        {
            Log.Error($"{e}");
        }
    }





    partial void OnSearchKeywordChanged(string? value)
    {
        if (RequestsSource.View is null)
            Task.Run(() => App.Current.Dispatcher.BeginInvoke(() => Refresh()));
        else Refresh();
    }


    private void Refresh()
    {
        RequestsSource.View?.Refresh();
        RecordsSource.View?.Refresh();
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

    [RelayCommand]
    public void DeleteRecord(TransferRecordViewModel r)
    {
        using var db = DbHelper.Base();
        db.GetCollection<TransferRecord>().Delete(r.Id);

        Records?.Remove(r);

        if (r?.FundId is not null)
            DataTracker.CheckShareIsPair(r.FundId.Value);
    }

    [RelayCommand]
    public void DeleteRequest(TransferRequest r)
    {
        using var db = DbHelper.Base();
        db.GetCollection<TransferRequest>().Delete(r.Id);

        Requests?.Remove(r);
    }

    [RelayCommand]
    public void AddTARecord()
    {
        switch (TabIndex)
        {
            case 0:
                AddOrder(); break;

            case 1:
                AddRequest(); break;

            case 2:
                AddRecord(); break;
            default:
                break;
        }
    }

    private void AddRecord()
    {
        var wnd = new AddTAWindow();
        wnd.Owner = App.Current.MainWindow;
        if (wnd.ShowDialog() switch { true => false, _ => true })
            return;


        RecordsSource.View.Refresh();

        if (wnd.DataContext is AddTAWindowViewModel vm && vm.SelectedFund is not null)
            DataTracker.CheckShareIsPair(vm.SelectedFund.Id);
    }

    private void AddRequest()
    {
        throw new NotImplementedException();
    }

    private void AddOrder()
    {
        var wnd = new AddOrderWindow();
        wnd.Owner = App.Current.MainWindow;
        var context = new AddOrderWindowViewModel();
        wnd.DataContext = context;
        if (wnd.ShowDialog() switch { true => false, _ => true })
            return;

        var order = context.Order;
        Orders?.Add(order);

        using var db = DbHelper.Base();
        db.GetCollection<TransferOrder>().Insert(order.Build());
    }

    public void Receive(TransferRecord message)
    {
        var old = Records!.FirstOrDefault(x => x.Id == message.Id);
        if (old is not null)
            old.UpdateFrom(message);
        else Records!.Add(new TransferRecordViewModel(message));
    }

    public void Receive(TransferRequest message)
    {
        var old = Requests!.FirstOrDefault(x => x.Id == message.Id);
        if (old is not null)
            Requests!.Remove(old);
        Requests!.Add(message);
    }

    public void Receive(PageTAMessage message)
    {
        TabIndex = message.TabIndex;
        SearchKeyword = message.Search;
    }
}



[AutoChangeableViewModel(typeof(TransferRecord))]
partial class TransferRecordViewModel
{
    public FileInfo File => new FileInfo(@$"files\tac\{Id}.pdf");


    public bool FileExists => System.IO.File.Exists(File.FullName);
}


[AutoChangeableViewModel(typeof(TransferOrder))]
partial class TransferOrderViewModel
{
}