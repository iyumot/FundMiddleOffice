﻿using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Shared;
using FMO.Trustee;
using FMO.Utilities;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FMO;

[AutoChangeableViewModel(typeof(TransferRecord))]
partial class TransferRecordViewModel : ITransferViewModel, IHasOrderViewModel
{
    public FileInfo File => new FileInfo(@$"files\tac\{Id}.pdf");


    public bool FileExists => System.IO.File.Exists(File.FullName);

    int IHasOrderViewModel.OrderId => OrderId!.Value;

    public bool HasOrder => OrderId != 0;

    public bool LackOrder => IsOrderRequired && OrderId == 0 && !Background;

    public bool IsSameManager { get; set; }

    /// <summary>
    /// 是否有同日同向订单
    /// </summary>
    public bool HasBrotherRecord { get; set; }

    public bool FirstTrade { get; set; }

    public bool LackRequest => Type != TransferRecordType.Distribution && !IsLiquidating && RequestId == 0 && !Background;

    /// <summary>
    /// 互投的产品，托管后台赎回付费，没有order request
    /// </summary>
    //public bool BackRedemption => Type == TransferRecordType.ForceRedemption && IsSameManager && RequestId == 0;


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
            Content = new CustomerView() { Margin = new Thickness(10) },
            DataContext = new CustomerViewModel(InvestorId!.Value),
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = App.Current.MainWindow,
        };
        wnd.KeyDown += (s, e) => { if (e.Key == System.Windows.Input.Key.Escape) wnd.Close(); };
        wnd.ShowDialog();
    }


    [RelayCommand]
    public void OpenFund() => WeakReferenceMessenger.Default.Send(new OpenFundMessage(FundId!.Value));

    [RelayCommand]
    public void ViewOrder()
    {
        if (OrderId is null) return;

        var wnd = new ModifyOrderWindow();
        wnd.DataContext = new ModifyOrderWindowViewModel(OrderId.Value);
        wnd.Owner = App.Current.MainWindow;
        wnd.ShowDialog();
    }

    [RelayCommand]
    public void ViewJson()
    {
        if (ExternalId?.Split('.') is string[] arr && arr.Length > 0)
        {
            using var db = DbHelper.Platform();
            var json = db.GetCollection($"{arr[0]}_{nameof(ITrustee.QueryTransferRecords)}").Find($"JsonId='{ExternalId}'").LastOrDefault();

            if (json is null) return;

            Window wnd = new Window
            {
                Width = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = App.Current.MainWindow,
                Content = new DataGrid
                {
                    ItemsSource = json?.ToDictionary(x => x.Key, x => x.Value),
                    Style = App.Current.FindResource("DataGrid.Small") as Style
                }
            };

            wnd.ShowDialog();
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


    public bool IsBuy() => Type!.Value.IsBuy();
    public bool IsSell() => Type!.Value.IsSell();
}
