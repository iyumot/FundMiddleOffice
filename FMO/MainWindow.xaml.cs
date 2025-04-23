using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using Serilog;

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
        if (db.FileStorage.Exists("icon.main"))
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


public partial class TabItemInfo:ObservableObject
{
    public required string Header { get; set; }

    public Brush? Background { get; set; }

    public Brush? HeaderBrush { get; set; } = Brushes.Black;

    public FrameworkElement? Content { get; set; }

    public bool IsCloseable { get; set; } = true;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

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
    public partial ObservableCollection<TabItemInfo> Pages { get; private set; }

    public MainWindowViewModel()
    {
        IsActive = true;
        Pages = new ObservableCollection<TabItemInfo>([new TabItemInfo { Header = "首页", IsCloseable = false, Content = new HomePage() }]);
    }

    public void Receive(string message)
    {
        HandyControl.Controls.Growl.Info(message);
    }

    protected override void OnActivated()
    {
        WeakReferenceMessenger.Default.Register<OpenFundMessage>(this);
        WeakReferenceMessenger.Default.Register<string, string>(this, "toast");
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
            page = new TabItemInfo { Header = fund.ShortName ?? fund.Name ?? "Fund", Content = obj, };//new TabItem { Header = GenerateHeader(fund.ShortName ?? fund.Name ?? "Fund"), Content = obj };
            Pages.Add(page);
        }

        page.IsSelected = true;
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
                        page = new TabItemInfo { Header = "外部平台", Background = Brushes.MediumSpringGreen, Content = new PlatformPage() };
                        Pages.Add(page);
                    }

                    page.IsSelected = true;
                    break;
                }

            case "FundsPage":
                {
                    var page = Pages.FirstOrDefault(x => x.Content is FundsPage);
                    if (page is null)
                    {
                        page = new TabItemInfo { Header = "基金总览", Background = Brushes.Violet, Content = new FundsPage() };
                        Pages.Add(page);
                    }

                    page.IsSelected = true;
                    break;
                }

            case "ManagerPage":
                {
                    var page = Pages.FirstOrDefault(x => x.Content is ManagerPage);
                    if (page is null)
                    {
                        page = new TabItemInfo { Header = "管理人", HeaderBrush = Brushes.White, Background = Brushes.BlueViolet, Content = new ManagerPage() };// new TabItem { Header = GenerateHeader("管理人"), Background = Brushes.BlueViolet, Foreground = Brushes.White, Content = new ManagerPage() { Foreground = Brushes.Black } };
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
                        page = new TabItemInfo { Header =  "任务", Background = Brushes.Khaki, Content = new TaskPage() };
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
                        page = new TabItemInfo { Header =  "报表", Background =  Brushes.RoyalBlue, HeaderBrush=Brushes.White, Content = new StatementPage() };
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
                        page = new TabItemInfo
                        {
                            Header =  ("投资人"),
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#49bc69")),
                            Content = new CustomerPage()
                        };
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
                        page = new TabItemInfo { Header = "  TA  ", Background = Brushes.Orange, Content = new TransferRecordPage() };
                        Pages.Add(page);
                    }

                    page.IsSelected = true;
                    break;
                }


            default:
                //{
                //    Type? type = Type.GetType($"FMO.{id}");
                //    if (type is null) break;

                //    var page = Pages.FirstOrDefault(x => x.Content?.GetType() == type);
                //    if (page is null)
                //    {
                //        var obj = Activator.CreateInstance(type) as UserControl;
                //        page = new TabItem { Header = GenerateHeader(obj?.Tag as string ?? "新标签"), Content = obj };
                //        Pages.Add(page);
                //    }

                //    page.IsSelected = true;
                //}
                break;
        }

    }

    [RelayCommand]
    public void ClosePage(TabItemInfo tabItem)
    {
        if (tabItem is not null)
        {
            if (tabItem.Content is FrameworkElement e && e.DataContext is not null)
                e.DataContext = null;

            if (tabItem.Content is not null)
                tabItem.Content.DataContext = null;
            tabItem.Content = null;
            Pages.Remove(tabItem);

        }
    }



    [RelayCommand]
    public void test()
    {
        Log.Warning(DateTime.Now.ToString());
    }

}

