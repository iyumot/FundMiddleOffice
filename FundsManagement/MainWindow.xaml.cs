using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FMO;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow :  HandyControl.Controls.Window
{
    public MainWindow()
    {
        InitializeComponent();

    }
}



public partial class MainWindowViewModel : ObservableObject, IRecipient<string>
{
    [ObservableProperty]
    public partial string? Title {get; set;}

    /// <summary>
    /// 通知
    /// </summary>
    [ObservableProperty]
    public partial string? Toast { get; set; }

    public void Receive(string message)
    {
       
    }


    [RelayCommand]
    public void test()
    {
        Log.Warning(DateTime.Now.ToString());
    }
}
