using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Shared;
using System.IO;
using System.Windows;

namespace FMO;

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


    public bool LackRequest { get; set; }


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

    [RelayCommand]
    public void OpenInvestorView()
    {
        var wnd = new HandyControl.Controls.Window
        {
            MaxHeight = App.Current.MainWindow.ActualHeight,
            Content = new CustomerView() { Margin = new Thickness(10)},
            DataContext = new CustomerViewModel(CustomerId!.Value),
            SizeToContent =  SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = App.Current.MainWindow,          
        };
        wnd.KeyDown += (s, e) => { if (e.Key == System.Windows.Input.Key.Escape) wnd.Close(); };
        wnd.ShowDialog();
    }


    [RelayCommand]
    public void OpenFund() => WeakReferenceMessenger.Default.Send(new OpenFundMessage(FundId!.Value));


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
