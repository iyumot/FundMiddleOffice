using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Shared;
using System.Windows;

namespace FMO;

[AutoChangeableViewModel(typeof(TransferRequest))]
partial class TransferRequestViewModel
{
    [RelayCommand]
    public void OpenInvestorView()
    {
        var wnd = new HandyControl.Controls.Window
        {
            MaxHeight = App.Current.MainWindow.ActualHeight,
            Content = new CustomerView() { Margin = new Thickness(10) },
            DataContext = new CustomerViewModel(CustomerId!.Value),
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = App.Current.MainWindow,
        };
        wnd.KeyDown += (s, e) => { if (e.Key == System.Windows.Input.Key.Escape) wnd.Close(); };
        wnd.ShowDialog();
    }


    [RelayCommand]
    public void OpenFund() => WeakReferenceMessenger.Default.Send(new OpenFundMessage(FundId!.Value));
}
