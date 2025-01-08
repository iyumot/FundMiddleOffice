using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using Serilog;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace FMO;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : HandyControl.Controls.Window
{
    public MainWindow()
    {
        InitializeComponent();

        Width = SystemParameters.FullPrimaryScreenWidth * 0.9;
        Height = SystemParameters.FullPrimaryScreenHeight * 0.85;


        // 管理人名称
        using var db = new BaseDatabase();
        Title += " - " + db.GetCollection<Manager>().FindOne(x => x.IsMaster)?.Name;
    }
}



public partial class MainWindowViewModel : ObservableObject, IRecipient<string>
{

    [ObservableProperty]
    public partial string? Title { get; set; }

    /// <summary>
    /// 通知
    /// </summary>
    [ObservableProperty]
    public partial string? Toast { get; set; }


    [ObservableProperty]
    public partial ObservableCollection<TabItem> Pages { get; private set; }

    public MainWindowViewModel()
    {
        Pages = new ObservableCollection<TabItem>([GenerateHomePageTab()]);
    }

    public void Receive(string message)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private TabItem GenerateHomePageTab()
    {
        var ti = new TabItem();
        var page = new HomePage(); 
        ti.Header = new HomePageHeader();
        ti.Content = page;
        ti.DataContext = new HomePageViewModel();
        return ti;
    }

    private DockPanel GenerateHeader(string head)
    {
        var dock = new DockPanel();
        var btn = new Button();
        btn.SetBinding(Button.CommandProperty, "ClosePageCommand");
        btn.SetBinding(Button.CommandParameterProperty, new Binding { RelativeSource = new RelativeSource { AncestorType = typeof(TabItem) } });
        btn.Style = App.Current.Resources["ButtonDefault.Small"] as Style;
        btn.BorderThickness = new Thickness(0);
        HandyControl.Controls.IconElement.SetGeometry(btn, App.Current.Resources["CloseGeometry"] as Geometry);
        DockPanel.SetDock(btn, Dock.Right);
        dock.Children.Add(btn);
        dock.Children.Add(new TextBlock { Text = head, VerticalAlignment = VerticalAlignment.Center });
        return dock;
    }

    [RelayCommand]
    public void OpenPage(string id)
    {
        switch (id)
        {
            case "Trustee":
                {
                    var page = Pages.FirstOrDefault(x => x.Content is TrusteePage);
                    if (page is null)
                    {
                        page = new TabItem { Header = GenerateHeader("托管平台"), Content = new TrusteePage() };
                        Pages.Add(page);
                    }

                    page.IsSelected = true;
                    break;
                }

            case "Funds":
                {
                    var page = Pages.FirstOrDefault(x => x.Content is FundsPage);
                    if (page is null)
                    {
                        page = new TabItem { Header = GenerateHeader("基金总览"), Content = new FundsPage() };
                        Pages.Add(page);
                    }

                    page.IsSelected = true;
                    break;
                }
            default:
                {
                    Type? type = Type.GetType($"FMO.{id}");
                    if (type is null) break;

                    var page = Pages.FirstOrDefault(x => x.Content?.GetType() == type);
                    if (page is null)
                    {
                        var obj = Activator.CreateInstance(type) as UserControl;
                        page = new TabItem { Header = GenerateHeader(obj?.Tag as string ?? "新标签"), Content = obj };
                        Pages.Add(page);
                    }

                    page.IsSelected = true;
                }
                break;
        }

    }

    [RelayCommand]
    public void ClosePage(TabItem tabItem)
    {
        if (tabItem is not null)
            Pages.Remove(tabItem);
    }



    [RelayCommand]
    public void test()
    {
        Log.Warning(DateTime.Now.ToString());
    }
}
