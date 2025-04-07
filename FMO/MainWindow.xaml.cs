using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using Serilog;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        using var db = DbHelper.Base();// DbHelper.Base();
        Title += " - " + db.GetCollection<Manager>().FindOne(x => x.IsMaster)?.Name;
        if(db.FileStorage.Exists("icon.main"))
        {
            using var ms = new MemoryStream();
            db.FileStorage.Download("icon.main", ms);
            BitmapImage bitmapSource = new BitmapImage();
            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.OnLoad;
            bitmapSource.StreamSource = ms;
            bitmapSource.EndInit();
            Icon = bitmapSource;
        }

    }
}



public partial class MainWindowViewModel : ObservableRecipient, IRecipient<string>, IRecipient<OpenFundMessage>
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
        IsActive = true;
        Pages = new ObservableCollection<TabItem>([GenerateHomePageTab()]);
    }

    public void Receive(string message)
    {
        HandyControl.Controls.Growl.Info(message);
    }

    protected override void OnActivated()
    {
        WeakReferenceMessenger.Default.Register<OpenFundMessage>(this);
        WeakReferenceMessenger.Default.Register<string,string>(this, "toast");
    }

    public void Receive(OpenFundMessage message)
    {
        var db = DbHelper.Base(); 
        var fund = db.GetCollection<Fund>().FindById(message.Id);
        var ele = db.GetCollection<FundElements>().FindById(message.Id);
        if (ele is null)
        {
            ele = FundElements.Create(message.Id);
            db.GetCollection<FundElements>().Insert(ele);
        }

        // 检查要求
        //var flows = db.GetCollection<FundFlow>().Find(x => x.FundId == fund.Id).Select(x => x.Id).ToArray();
        

        //if (ele.Init()) db.GetCollection<FundElements>().Update(ele);
        db.Dispose();
        if (fund is null) return;

        var page = Pages.FirstOrDefault(x => x.Content is FundInfoPage p && p.Tag.ToString() == fund.Name);
        if (page is null)
        {
            var obj = new FundInfoPage() { Tag = fund.Name, DataContext = new FundInfoPageViewModel(fund, ele) };
            page = new TabItem { Header = GenerateHeader(fund.ShortName ?? fund.Name ?? "Fund"), Content = obj };
            Pages.Add(page);
        }
        page.IsSelected = true;
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
                    var page = Pages.FirstOrDefault(x => x.Content is PlatformPage);
                    if (page is null)
                    {
                        page = new TabItem { Header = GenerateHeader("外部平台"), Content = new PlatformPage() };
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

            case "Task":
                {
                    var page = Pages.FirstOrDefault(x => x.Content is TaskPage);
                    if (page is null)
                    {
                        page = new TabItem { Header = GenerateHeader("任务"), Content = new TaskPage() };
                        Pages.Add(page);
                    }

                    page.IsSelected = true;
                    break;
                }

            case "Statement":
                {
                    var page = Pages.FirstOrDefault(x => x.Content is StatementPage);
                    if (page is null)
                    {
                        page = new TabItem { Header = GenerateHeader("报表"), Content = new StatementPage() };
                        Pages.Add(page);
                    }

                    page.IsSelected = true;
                    break;
                }

            case "Customer":
                {
                    var page = Pages.FirstOrDefault(x => x.Content is CustomerPage);
                    if (page is null)
                    {
                        page = new TabItem { Header = GenerateHeader("投资人"), Content = new CustomerPage() };
                        Pages.Add(page);
                    }

                    page.IsSelected = true;
                    break;
                }

            case "TA":
                {
                    var page = Pages.FirstOrDefault(x => x.Content is TransferRecordPage);
                    if (page is null)
                    {
                        page = new TabItem { Header = GenerateHeader(" TA "), Content = new TransferRecordPage() };
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
        {
            if (tabItem.Content is FrameworkElement e && e.DataContext is not null)
                e.DataContext = null;

            tabItem.Content = null;
            tabItem.DataContext = null;
            Pages.Remove(tabItem);
            
        }
    }



    [RelayCommand]
    public void test()
    {
        Log.Warning(DateTime.Now.ToString());
    }

}

