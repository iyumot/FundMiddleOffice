using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel;
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


public partial class TransferRecordPageViewModel : ObservableObject, IRecipient<TransferRecord>, IRecipient<PageTAMessage>, IRecipient<TransferOrder>
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
    [NotifyPropertyChangedFor(nameof(IsRecordTabSelected))]
    public partial int TabIndex { get; set; } = 3;

    public bool IsRecordTabSelected => TabIndex == 3;

    [ObservableProperty]
    public partial ObservableCollection<TransferRequest>? Requests { get; set; }


    [ObservableProperty]
    public partial bool ShowOnlySignable { get; set; }


    public CollectionViewSource RequestsSource { get; set; } = new();

    public CollectionViewSource TranscationSource { get; set; } = new();

    public ObservableCollection<RaisingBankTranscationViewModel>? BankTransactions { get; set; }

    FileSystemWatcher? watcher;

    public TransferRecordPageViewModel()
    {
        ShowOnlySignable = true;
        WeakReferenceMessenger.Default.RegisterAll(this);

        RequestsSource.Filter += (s, e) => e.Accepted = string.IsNullOrWhiteSpace(SearchKeyword) ? true : SearchPair(e.Item, SearchKeyword);
        RecordsSource.Filter += (s, e) => e.Accepted = FilterRecord(e.Item);

        Task.Run(() =>
        {
            using var db = DbHelper.Base();

            var tr = db.GetCollection<TransferRecord>().FindAll();
            IOrderedEnumerable<TransferRequest> tr2 = db.GetCollection<TransferRequest>().FindAll().OrderByDescending(x => x.RequestDate);
            var t3 = db.GetCollection<TransferOrder>().FindAll();

            App.Current.Dispatcher.BeginInvoke(() =>
            {
                Records = [.. tr.Select(x => new TransferRecordViewModel(x))];
                Requests = new ObservableCollection<TransferRequest>(tr2);
                Orders = [.. t3.Select(x => new TransferOrderViewModel(x))];

                var funds = db.GetCollection<Fund>().FindAll().Select(x => (x.Id, x.Name, x.Code)).ToArray();
                BankTransactions = [.. db.GetCollection<RaisingBankTransaction>().FindAll().Select(x => new RaisingBankTranscationViewModel(x, funds))];


                foreach (var o in Orders)
                {
                    if (tr.Any(x => x.OrderId == o.Id))
                        o.IsComfirmed = true;
                }

                foreach (var ft in Records.OrderBy(x => x.ConfirmedDate).GroupBy(x => x.FundId))
                {
                    var last = ft.Last().ConfirmedDate;
                    var may = ft.Where(x => x.ConfirmedDate == last && (x.Type == TransferRecordType.Redemption || x.Type == TransferRecordType.ForceRedemption)).ToArray();
                    if (may.Length == 0) continue;

                    if (db.GetCollection<Fund>().FindById(ft.Key) is Fund ff && ff.ClearDate != default && last > ff.ClearDate)
                        foreach (var item in may)
                            item.RedemptionOnClear = true;
                }

                RecordsSource.SortDescriptions.Add(new SortDescription(nameof(TransferRecordViewModel.ConfirmedDate), ListSortDirection.Descending));
                RequestsSource.SortDescriptions.Add(new SortDescription(nameof(TransferRequest.RequestDate), ListSortDirection.Descending));
                OrderSource.SortDescriptions.Add(new SortDescription(nameof(TransferOrderViewModel.Date), ListSortDirection.Descending));

                RecordsSource.Source = Records;
                RequestsSource.Source = Requests;
                OrderSource.Source = Orders;

                TranscationSource.Source = BankTransactions;
                TranscationSource.SortDescriptions.Add(new SortDescription(nameof(BankTransaction.Time), ListSortDirection.Descending));
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


    private bool FilterRecord(object obj)
    {
        if (obj is not TransferRecordViewModel r || r.Type is null) return false;

        bool show = !ShowOnlySignable || TransferRecord.RequireOrder(r.Type.Value);
        return show && (string.IsNullOrWhiteSpace(SearchKeyword) ? true : SearchPair(obj, SearchKeyword));
    }


    private bool SearchPair(object obj, string key)
    {
        if (obj is TransferRecordViewModel r)
            return (r.CustomerName?.Contains(key) ?? false) || (r.FundName?.Contains(key) ?? false) || (key?.Length > 3 && (r.CustomerIdentity?.Contains(key) ?? false));

        if (obj is TransferRequest rr)
            return (rr.CustomerName?.Contains(key) ?? false) || (rr.FundName?.Contains(key) ?? false) || (key?.Length > 3 && (rr.CustomerIdentity?.Contains(key) ?? false));

        if (obj is TransferOrderViewModel o)
            return (o.InvestorName?.Contains(key) ?? false) || (o.FundName?.Contains(key) ?? false) || (key?.Length > 3 && (o.InvestorIdentity?.Contains(key) ?? false));

        return false;
    }


    partial void OnSearchKeywordChanged(string? value)
    {
        if (RequestsSource.View is null)
            Task.Run(() => App.Current.Dispatcher.BeginInvoke(() => Refresh()));
        else Refresh();
    }


    partial void OnShowOnlySignableChanged(bool value)
    {
        RecordsSource?.View?.Refresh();
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

            HandyControl.Controls.Growl.Warning($"无法启动计算器，{e.Message}");
        }
    }


    [RelayCommand]
    public void OpenFile(FileStorageInfo file)
    {
        if (file?.Path is null) return;
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(file.Path) { UseShellExecute = true }); } catch { }
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
            case 1:
                AddOrder(); break;

            //case 1:
            //    AddRequest(); break;

            case 3:
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
    }



    [RelayCommand]
    public void DeleteOrder(TransferOrderViewModel order)
    {
        if (HandyControl.Controls.MessageBox.Show("是否确认删除订单？", button: System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
        {
            using var db = DbHelper.Base();
            db.GetCollection<TransferOrder>().Delete(order.Id);

            var rr = db.GetCollection<TransferRecord>().Find(x => x.OrderId == order.Id).ToArray();
            foreach (var item in rr)
                item.OrderId = 0;
            db.GetCollection<TransferRecord>().Update(rr);

            DataTracker.LinkOrder(rr);
            Orders!.Remove(order);
        }
    }

    [RelayCommand]
    public void AbortOrder(TransferOrderViewModel r)
    {
        using var db = DbHelper.Base();
        var obj = db.GetCollection<TransferOrder>().FindById(r.Id);
        if (obj is not null)
        {
            obj.IsAborted = !obj.IsAborted;
            db.GetCollection<TransferOrder>().Update(obj);
        }

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

    public void Receive(TransferOrder message)
    {
        var old = Orders!.FirstOrDefault(x => x.Id == message.Id);
        if (old is not null)
            old.UpdateFrom(message);
        else Orders!.Add(new TransferOrderViewModel(message));
    }
}



[AutoChangeableViewModel(typeof(TransferRecord))]
partial class TransferRecordViewModel
{
    public FileInfo File => new FileInfo(@$"files\tac\{Id}.pdf");


    public bool FileExists => System.IO.File.Exists(File.FullName);

    public bool HasOrder => OrderId != 0;

    /// <summary>
    /// 是否有同日同向订单
    /// </summary>
    public bool HasBrotherRecord { get; set; }

    public bool FirstTrade { get; set; }

    /// <summary>
    /// 清盘时的赎回，不需要申请
    /// </summary>
    public bool RedemptionOnClear { get; set; }

    [RelayCommand]
    public void ModifyOrder()
    {
        var wnd = new SupplementaryOrderWindow();
        var context = new SupplementaryOrderWindowViewModel(this, FirstTrade);
        wnd.DataContext = context;
        wnd.Owner = App.Current.MainWindow;
        if (wnd.ShowDialog() ?? false)
        {
            OrderId = context.Id;
            OnPropertyChanged(nameof(HasOrder));
        }
    }

    public decimal ShareChange()
    {
        switch (Type)
        {
            case TransferRecordType.Subscription:
            case TransferRecordType.Purchase:
            case TransferRecordType.Increase:
            case TransferRecordType.Distribution:
                return ConfirmedShare ?? 0;
            case TransferRecordType.Redemption:
            case TransferRecordType.ForceRedemption:
            case TransferRecordType.Decrease:
                return -ConfirmedShare ?? 0;
            default:
                return 0;
        }
    }
}


[AutoChangeableViewModel(typeof(TransferOrder))]
partial class TransferOrderViewModel
{
    public bool IsComfirmed { get => field; set { field = value; OnPropertyChanged(nameof(IsComfirmed)); } }

}

[AutoChangeableViewModel(typeof(RaisingBankTransaction))]
partial class RaisingBankTranscationViewModel
{
    public RaisingBankTranscationViewModel(RaisingBankTransaction? instance, (int Id, string Name, string Code)[] funds) : this(instance)
    {
        if (instance is null) return;
        var fund = funds.FirstOrDefault(x => x.Id == instance.FundId);
        FundName = fund.Name;

        This = string.IsNullOrWhiteSpace(FundName) ? instance.AccountName : FundName;
    }

    public string FundName { get; }


    public string This { get; }
}