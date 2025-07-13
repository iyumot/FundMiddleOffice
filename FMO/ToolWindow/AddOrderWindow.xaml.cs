using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FMO;

/// <summary>
/// AddOrderWindow.xaml 的交互逻辑
/// </summary>
public partial class AddOrderWindow : Window
{
    public AddOrderWindow()
    {
        InitializeComponent();
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox lb && DataContext is AddOrderWindowViewModel vm)
            vm.SelectedRecords = lb.SelectedItems.OfType<TransferRecord>();
    }
}


public partial class AddOrderWindowViewModel : ObservableObject
{
    public TransferOrderViewModel? Order { get; internal set; }


    public Fund[] Funds { get; set; }

    public CollectionViewSource FundSource { get; } = new();


    [ObservableProperty]
    public partial bool IsReadOnly { get; set; }

    [ObservableProperty]
    public partial string? SearchFundKey { get; set; }


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    public partial Fund? SelectedFund { get; set; }



    public Investor[] Investors { get; set; }

    public CollectionViewSource InvestorSource { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    public partial string? SearchInvestorKey { get; set; }


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    public partial Investor? SelectedInvestor { get; set; }


    [ObservableProperty]
    public partial IEnumerable<TransferRecord>? SelectedRecords { get; set; }


    [ObservableProperty]
    public partial TransferRecordType? SelectedType { get; set; }

    [ObservableProperty]
    public partial DateTime? Date { get; set; }

    [ObservableProperty]
    public partial decimal Number { get; set; }


    public TransferRecordType[] Types { get; } = [TransferRecordType.Subscription, TransferRecordType.Purchase, TransferRecordType.Redemption];


    public CollectionViewSource RecordSource { get; }

    private TransferRecord[] _records;

    private int[]? _investorIdInSelectedFund;


    public SingleFileViewModel Contract { get; }

    public SingleFileViewModel RiskDisclosure { get; }


    public SingleFileViewModel OrderFile { get; }


    public SingleFileViewModel Video { get; }

    /// <summary>
    /// 补录
    /// </summary>
    [ObservableProperty]
    public partial bool SupplementaryMode { get; set; }


    public ObservableCollection<string> Tips { get; } = new();

    public AddOrderWindowViewModel()
    {
        using var db = DbHelper.Base();
        Funds = db.GetCollection<Fund>().FindAll().ToArray();

        FundSource.Source = Funds;
        FundSource.Filter += (s, e) => e.Accepted = string.IsNullOrWhiteSpace(SearchFundKey) || SearchFundKey == SelectedFund?.Name ? true : e.Item switch { Fund f => f.Name.Contains(SearchFundKey), _ => true };


        Investors = db.GetCollection<Investor>().FindAll().ToArray();
        InvestorSource.Source = Investors;
        InvestorSource.Filter += (s, e) =>
        {
            // 如果是补录模式，投资人只能是当前基金的投资人
            var sel = !SupplementaryMode ? true : _investorIdInSelectedFund?.Contains((e.Item as Investor)?.Id ?? 0) ?? false;

            e.Accepted = sel && (string.IsNullOrWhiteSpace(SearchInvestorKey) || SearchInvestorKey == SelectedInvestor?.Name ? true : e.Item switch { Investor f => f.Name.Contains(SearchInvestorKey), _ => true });
        };

        _records = db.GetCollection<TransferRecord>().FindAll().ToArray();
        RecordSource = new() { Source = _records };
        RecordSource.Filter += (s, e) => e.Accepted = e.Item switch
        {
            TransferRecord r => r.FundId == SelectedFund?.Id && r.CustomerId == SelectedInvestor?.Id && r.Type switch { TransferRecordType.Subscription or TransferRecordType.Purchase or TransferRecordType.ForceRedemption or TransferRecordType.Redemption => true, _ => false },
            _ => true
        };



        Contract = new()
        {
            Label = "基金合同",
            OnSetFile = (x, y) => SetFile(),
            OnDeleteFile = x => DeleteFile(x)
        };
        RiskDisclosure = new()
        {
            Label = "风险揭示书",
            OnSetFile = (x, y) => SetFile(),
            OnDeleteFile = x => DeleteFile(x)
        };
        OrderFile = new()
        {
            Label = "认申赎单",
            OnSetFile = (x, y) => SetFile(),
            OnDeleteFile = x => DeleteFile(x)
        };
        Video = new()
        {
            Label = "双录",
            OnSetFile = (x, y) => SetFile(),
            OnDeleteFile = x => DeleteFile(x)
        };
    }

    private FileStorageInfo? SetFile()
    {
        throw new NotImplementedException();
    }

    private void DeleteFile(FileStorageInfo x)
    {
        throw new NotImplementedException();
    }



    partial void OnSearchFundKeyChanged(string? value) => FundSource.View.Refresh();

    partial void OnSearchInvestorKeyChanged(string? value) => InvestorSource.View.Refresh();


    partial void OnSelectedFundChanged(Fund? value)
    {
        _investorIdInSelectedFund = _records?.Where(x => x.FundId == value?.Id).Select(x => x.CustomerId).ToArray();
        InvestorSource.View.Refresh();
    }

    partial void OnSupplementaryModeChanged(bool value)
    {
        InvestorSource.View.Refresh();
    }

    partial void OnSelectedInvestorChanged(Investor? value)
    {
        RecordSource.View.Refresh();
    }

    partial void OnDateChanged(DateTime? value)
    {
        if (!SupplementaryMode) return;

        var tip = "签约日期晚于申请日期";
        var max = SelectedRecords?.Any() ?? false ? SelectedRecords.Max(x => x.RequestDate) : RecordSource.View.Cast<TransferRecord>().Max(x => x.RequestDate);

        bool need = value is not null && DateOnly.FromDateTime(value.Value) > max;
        if (!need)
            Tips.Remove(tip);
        else if (!Tips.Contains(tip))
            Tips.Add(tip);
    }



    partial void OnSelectedRecordsChanged(IEnumerable<TransferRecord>? value)
    {
        if (!SupplementaryMode || Date is null) return;

        var tip = "签约日期晚于申请日期";
        var max = SelectedRecords?.Any() ?? false ? SelectedRecords.Max(x => x.RequestDate) : RecordSource.View.Cast<TransferRecord>().Max(x => x.RequestDate);

        bool need = value is not null && DateOnly.FromDateTime(Date.Value) > max;
        if (!need)
            Tips.Remove(tip);
        else if (!Tips.Contains(tip))
            Tips.Add(tip);
    }







    public bool CanConfirm => SelectedFund is not null && SelectedInvestor is not null;






    [RelayCommand(CanExecute = nameof(CanConfirm))]
    public void Confirm()
    {

        App.Current.Windows[^1].Close();
    }



}

