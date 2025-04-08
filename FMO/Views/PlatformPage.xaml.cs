using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.IO;
using FMO.IO.DS;
using FMO.IO.Trustee;
using FMO.Utilities;
using Serilog;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FMO;

/// <summary>
/// PlatformPage.xaml 的交互逻辑
/// </summary>
public partial class PlatformPage : UserControl
{
    public PlatformPage()
    {
        InitializeComponent();

    }
}


/// <summary>
/// Page的vm
/// </summary>
public partial class PlatformPageViewModel : ObservableObject
{

    private static bool _firstLoad = true;

    public ObservableCollection<PlatformPageViewModelTrustee> Trustees { get; } = new();

    public ObservableCollection<PlatformPageViewModelDigital> Digitals { get; } = new();



    public PlatformPageViewModel()
    {
        /// 读取所有托管插件
        /// 

        //if (_firstLoad)
        {

            var files = new DirectoryInfo("plugins").GetFiles("*.dll");


            foreach (var file in files)
            {
                try
                {
                    var assembly = Assembly.LoadFile(file.FullName);
                    TryAddTrustee(assembly);
                    TryAddSignature(assembly);
                }
                catch (Exception e)
                {
                    Log.Error($"[{file.Name}]加载插件失败{e.Message}");
                }
            }

            _firstLoad = false;
        }




        // using var db = DbHelper.Trustee();


    }

    /// <summary>
    /// 加载托管插件，一个dll只加载一个
    /// </summary>
    /// <param name="assembly"></param>
    void TryAddTrustee(Assembly assembly)
    {
        var type = assembly.GetTypes().FirstOrDefault(x => x.GetInterface(typeof(ITrusteeAssist).FullName!) is not null);
        if (type is null) return;

        ITrusteeAssist trusteeAssist = (ITrusteeAssist)Activator.CreateInstance(type)!;

        Stream? iconStream = null;
        var res = assembly.GetManifestResourceNames();
        var name = res.FirstOrDefault(x => x.Contains(".logo."));
        if (name is not null)
            iconStream = assembly.GetManifestResourceStream(name);

        var icon = new BitmapImage();
        icon.BeginInit();
        icon.StreamSource = iconStream;
        icon.EndInit();

        Trustees.Add(new PlatformPageViewModelTrustee(trusteeAssist, trusteeAssist.Name, icon));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assembly"></param>
    void TryAddSignature(Assembly assembly)
    {
        var type = assembly.GetTypes().FirstOrDefault(x => x.GetInterface(typeof(IDigitalSignature).FullName!) is not null);
        if (type is null) return;

        IDigitalSignature assist = (IDigitalSignature)Activator.CreateInstance(type)!;

        Stream? iconStream = null;
        var res = assembly.GetManifestResourceNames();
        var name = res.FirstOrDefault(x => x.Contains(".logo."));
        if (name is not null)
            iconStream = assembly.GetManifestResourceStream(name);

        using var db = DbHelper.Platform();
        var acc = db.GetCollection<PlatformAccount>().FindById(assist.Identifier);

        assist.UserID = acc?.UserId;
        assist.Password = acc?.Password;

        var icon = new BitmapImage();
        icon.BeginInit();
        icon.StreamSource = iconStream;
        icon.EndInit();

        Digitals.Add(new PlatformPageViewModelDigital(assist, assist.Name, icon));
    }

}

public partial class PlatformPageViewModelDigital : ObservableRecipient//, IRecipient<string>
{
    /// <summary>
    /// 图标
    /// </summary>
    public ImageSource? Icon { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 托管助手 
    /// </summary>
    public required IDigitalSignature Assist { get; set; }


    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    /// <summary>
    /// 初始化
    /// </summary>
    [ObservableProperty]
    public partial bool IsInitialized { get; set; }



    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LoginStatus))]
    public partial bool IsLogin { get; set; }


    [ObservableProperty]
    public partial bool NeedLogin { get; set; }

    public string LoginStatus => IsLogin ? "已登陆" : "未登陆";



    [ObservableProperty]
    public partial bool ShowAccount { get; set; }

    [ObservableProperty]
    public partial string? UserId { get; set; }


    [ObservableProperty]
    public partial string? Password { get; set; }



    /// <summary>
    /// 同步项
    /// </summary>
    public SyncButtonData[] Buttons { get; set; }


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SynchronizeDataCommand))]
    public partial bool SyncCommandCanExecute { get; set; } = true;


    [SetsRequiredMembers]
    public PlatformPageViewModelDigital(IDigitalSignature assist, string name, ImageSource? icon)
    {
        Icon = icon;
        Name = name;
        Assist = assist;


        UserId = assist.UserID;
        Password = assist.Password;
        IsLogin = false;

        Buttons = [
            new SyncButtonData((Geometry)App.Current.Resources["f.address-card"]  , SynchronizeDataCommand, SyncCustomers,"客户资料"),
            new SyncButtonData((Geometry)App.Current.Resources["f.certificate"]  , SynchronizeDataCommand, SyncQualifications,"合投材料"),  ];

        //using var db = DbHelper.Trustee();
        ////Config = db.GetCollection<TrusteeConfig>().FindOne(x => x.Id == Assist.Identifier) ?? new TrusteeConfig { Id = assist.Identifier };

        //IsActive = true;

        //WeakReferenceMessenger.Default.Register(this, "Trustee.LogOut");

        NeedLogin = !IsLogin;
        //if (IsEnabled) Task.Run(async () => { await Task.Delay(2000); StartWork(); });


        Task.Run(async () =>
        {
            try
            {

                using var page = await Automation.AcquirePage(assist.Identifier);
                //var page = pw.Page;

                await assist.PrepareLoginAsync(page);

                IsLogin = await assist.LoginValidationAsync(page);

                if (IsLogin)
                    await assist.EndLoginAsync(page);

            }
            catch (Exception e)
            {
                HandyControl.Controls.Growl.Error($"初始化登录{assist.Name}失败");
            }
            IsInitialized = true;
        });

    }


    [RelayCommand(CanExecute = nameof(NeedLogin))]
    public async Task Login()
    {
        NeedLogin = false;

        try
        {
            IsLogin = await Assist.LoginAsync();
        }
        catch { IsLogin = false; }

        NeedLogin = true;
    }





    [RelayCommand(CanExecute = nameof(SyncCommandCanExecute))]
    public async Task SynchronizeData(SyncButtonData btn)
    {
        SyncCommandCanExecute = false;

        btn.IsRunning = true;
        try { await btn.SyncProcess(); } catch (Exception ex) { }
        btn.IsRunning = false;

        SyncCommandCanExecute = true;
    }

    public async Task SyncCustomers()
    {
        await Assist.SynchronizeCustomerAsync();

    }


    public async Task SyncQualifications()
    {
        await Assist.SynchronizeQualificatoinAsync();
    }


    [RelayCommand]
    public void SaveAccount()
    {
        Assist.UserID = UserId;
        Assist.Password = Password;
        using var db = DbHelper.Platform();
        db.GetCollection<PlatformAccount>().Upsert(new PlatformAccount { Id=Assist.Identifier, UserId = UserId, Password = Password });
    }

}



/// <summary>
/// 单个项的vm
/// </summary>
public partial class PlatformPageViewModelTrustee : ObservableRecipient, IRecipient<string>
{
    [SetsRequiredMembers]
    public PlatformPageViewModelTrustee(ITrusteeAssist assist, string name, ImageSource? icon)
    {
        Icon = icon;
        Name = name;
        Assist = assist;


        Buttons = [ new SyncButtonData((Geometry)App.Current.Resources["f.receipt"]  , SynchronizeDataCommand, SyncFundRaisingRecord, "募集户流水"),
            new SyncButtonData((Geometry)App.Current.Resources["f.file-invoice"]  , SynchronizeDataCommand, SyncBankRecord,"托管户流水"),
            new SyncButtonData((Geometry)App.Current.Resources["f.address-card"]  , SynchronizeDataCommand, SyncCustomers,"客户资料"),
            new SyncButtonData((Geometry)App.Current.Resources["f.money-bill-transfer"]  , SynchronizeDataCommand, SyncTA,"TA"),     ];

        using var db = DbHelper.Trustee();
        Config = db.GetCollection<TrusteeConfig>().FindOne(x => x.Id == Assist.Identifier) ?? new TrusteeConfig { Id = assist.Identifier };

        IsActive = true;

        WeakReferenceMessenger.Default.Register(this, "Trustee.LogOut");

        NeedLogin = !IsLogin;
        if (IsEnabled) Task.Run(async () => { await Task.Delay(2000); StartWork(); });


    }

    /// <summary>
    /// 图标
    /// </summary>
    public ImageSource? Icon { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 托管助手 
    /// </summary>
    public required ITrusteeAssist Assist { get; set; }

    /// <summary>
    /// 配置文件
    /// </summary>
    private TrusteeConfig Config { get; set; }

    /// <summary>
    /// 同步项
    /// </summary>
    public SyncButtonData[] Buttons { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    //[ObservableProperty]
    public bool IsEnabled { get { return Config.IsEnabled; } set { if (Config.IsEnabled == value) return; Config.IsEnabled = value; OnPropertyChanged(); SaveConfig(); if (value) StartWork(); } }


    public bool IsLogin { get => Config.IsLogedIn; set { if (Config.IsLogedIn == value) return; Config.IsLogedIn = value; OnPropertyChanged(); OnPropertyChanged(nameof(LoginStatus)); SaveConfig(); } }


    public string LoginStatus => IsLogin ? "已登陆" : "未登陆";



    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SynchronizeDataCommand))]
    public partial bool SyncCommandCanExecute { get; set; } = true;


    /// <summary>
    /// 未登陆，需要登陆
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial bool NeedLogin { get; set; }

    protected void SaveConfig()
    {
        using var db = DbHelper.Trustee();
        LiteDB.ILiteCollection<TrusteeConfig> c = db.GetCollection<TrusteeConfig>();

        c.Upsert(Config);
    }

    /// <summary>
    /// 启动程序
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private async void StartWork()
    {
        //if (IsEnabled && !await Assist.LoginAsync())
        //{
        //    IsLogin = false;
        //    return;
        //}

        //IsLogin = true;
        //HandyControl.Controls.Growl.Success($"托管平台【{Assist.Name}】登陆成功");


        /// 同步募集户流水
        //Task.Run(() =>
        //{

        //});




    }

    #region 同步
    /// <summary>
    /// 同步函数
    /// </summary>
    /// <returns></returns>
    public delegate Task SyncProcess();


    [RelayCommand(CanExecute = nameof(SyncCommandCanExecute))]
    public async Task SynchronizeData(SyncButtonData btn)
    {
        SyncCommandCanExecute = false;

        btn.IsRunning = true;
        try { await btn.SyncProcess(); } catch (Exception ex) { }
        btn.IsRunning = false;

        SyncCommandCanExecute = true;
    }

    public async Task SyncCustomers()
    {
        await Assist.SynchronizeCustomerAsync();

    }

    public async Task SyncTA()
    {
       // await Assist.SynchronizeTransferRequestAsync();

        await Assist.SynchronizeTransferRecordAsync(); 
    }

    public async Task SyncFundRaisingRecord()
    {
        await Task.Delay(10000);

    }



    public async Task SyncBankRecord()
    {
        await Task.Delay(10000);

    }

    #endregion



    [RelayCommand(CanExecute = nameof(NeedLogin))]
    public async Task Login()
    {
        NeedLogin = false;

        IsLogin = await Assist.LoginAsync();

        NeedLogin = true;
    }





    public void Receive(string message)
    {
        if (message == Assist.Identifier)
        {
            IsLogin = false;

            using var db = DbHelper.Trustee();
            var config = db.GetCollection<TrusteeConfig>().FindOne(x => x.Id == Assist.Identifier) ?? new TrusteeConfig { Id = Assist.Identifier };

            config.IsLogedIn = false;

            db.GetCollection<TrusteeConfig>().Upsert(config);
        }
    }
}







public partial class SyncButtonData(Geometry Icon, ICommand Command, PlatformPageViewModelTrustee.SyncProcess SyncProcess, string Description) : ObservableObject
{
    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    public Geometry Icon { get; } = Icon;

    public ICommand Command { get; } = Command;

    public PlatformPageViewModelTrustee.SyncProcess SyncProcess { get; } = SyncProcess;

    public string Description { get; } = Description;
}
