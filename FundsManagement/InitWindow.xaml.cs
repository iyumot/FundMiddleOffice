using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.IO.AMAC;
using FMO.Models;
using FMO.Utilities;
using Microsoft.Win32;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Windows;

namespace FMO;

/// <summary>
/// InitWindow.xaml 的交互逻辑
/// </summary>
public partial class InitWindow : Window
{
    public InitWindow()
    {
        InitializeComponent();
    }













}


public partial class InitWindowViewModel : ObservableRecipient, IRecipient<InitStep2Info>
{
    [ObservableProperty]
    public partial ManagerInfo[]? ManagerOptions { get; set; }

    [ObservableProperty]
    public partial string? ManagerName { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SetUpCommand))]
    public partial Manager? Manager { get; set; }

    [ObservableProperty]
    public partial bool ShowProgress { get; set; }


    [ObservableProperty]
    public partial double InitProgress { get; set; }


    [ObservableProperty]
    public partial bool ShowStep1 { get; set; }


    [ObservableProperty]
    public partial bool ShowStep2 { get; set; }


    [ObservableProperty]
    public partial string? CurrentScale { get; set; }


    [ObservableProperty]
    public partial bool IsNetworkDisconnected { get; set; }



    [ObservableProperty]
    public partial int PreNewRuleFundCount { get; set; }


    [ObservableProperty]
    public partial int NormalFundCount { get; set; }


    [ObservableProperty]
    public partial int AdviseFundCount { get; set; }



    [RelayCommand]

    public void Close()
    {
        App.Current.Windows[^1].Close();
        App.Current.Shutdown();
    }


    public bool CanInit => Manager is not null;

    /// <summary>
    /// 初始化进程
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanInit))]
    public async Task SetUpAsync()
    {
        if (Manager is null) return;

        ShowStep1 = false;
        ShowStep2 = true;
        ShowProgress = true;
        var funds = await AmacAssist.CrawleManagerInfo(Manager);

        ///保存数据库
        Manager.IsMaster = true;

        var db = new BaseDatabase();
        db.GetCollection<Manager>().Insert(Manager);
        db.GetCollection<FundBasicInfo>().InsertBulk(funds);


        Restart();
       
    }

    [RelayCommand]
    public void ChooseFolder()
    {
        OpenFolderDialog dialog = new OpenFolderDialog();
        var r = dialog.ShowDialog();
        if(r ?? false)
        {
            Config.Default.WorkFolder = dialog.FolderName;
            Config.Default.Save();

            Restart();
        }
    }


    public void Restart()
    {
        var field = App.Current.GetType().GetField("mutex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var mutex = field!.GetValue(App.Current) as Mutex;
        mutex!.Close();

        System.Diagnostics.Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName);

        Close();
    }


    /// <summary>
    /// 根据关键字检索相关管理人
    /// </summary>
    /// <param name="value"></param>
    async partial void OnManagerNameChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        ///选中一个名称
        var sel = ManagerOptions?.FirstOrDefault(x => x.ManagerName == value);

        if (sel is not null)
        {
            OnSelectManager(sel);
            return;
        }

        Manager = null;
        ManagerOptions = await AmacAssist.GetInstitutionInfoFromAmac(value);

        ///如果是复制的全称
        sel = ManagerOptions?.FirstOrDefault(x => x.ManagerName == value);

        if (sel is not null)
            OnSelectManager(sel);
    }

    private void OnSelectManager(ManagerInfo sel)
    {
        Manager = new Manager
        {
            Id = "unset",
            Name = sel.ManagerName!,
            AmacId = sel.Id!,
            RegisterAddress = sel.RegisterAddress,
            RegisterDate = new DateTime(1970, 1, 1).AddMilliseconds(sel.RegisterDate).ToLocalTime(),
            OfficeAddress = sel.OfficeAddress,
            SetupDate = DateOnly.FromDateTime(new DateTime(1970, 1, 1).AddMilliseconds(sel.EstablishDate).ToLocalTime()),
            RegisterNo = sel.RegisterNo!,
            ArtificialPerson = sel.ArtificialPersonName,
            FundCount = sel.FundCount,
            HasCreditTips = sel.HasCreditTips,
            HasSpecialTips = sel.HasSpecialTips,
            MemberType = sel.MemberType
        };

        ShowStep1 = true;
        ShowStep2 = false;
        ShowProgress = false;
        CurrentScale = null;
        PreNewRuleFundCount = 0;
        NormalFundCount = 0;
        AdviseFundCount = 0;
    }

    public void Receive(object message)
    {
        if (message is int i)
            InitProgress = i;
        else if (message is double d)
            InitProgress = d;

    }
    public void Receive(InitStep2Info message)
    {
        InitProgress = message.Progress;

        if (message.CurrentScale is not null)
            CurrentScale = message.CurrentScale;

        if (message.PreRuleCount is not null)
            PreNewRuleFundCount = message.PreRuleCount.Value;

        if (message.NormalCount is not null)
            NormalFundCount = message.NormalCount.Value;

        if (message.AdviseCount is not null)
            AdviseFundCount = message.AdviseCount.Value;

    }

    public InitWindowViewModel()
    {
        IsActive = true;

        IsNetworkDisconnected = !NetworkInterface.GetIsNetworkAvailable();
        NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
    }

    private void NetworkChange_NetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e)
    {

        IsNetworkDisconnected = !NetworkInterface.GetIsNetworkAvailable();
    }

    protected override void OnActivated()
    {
        WeakReferenceMessenger.Default.Register<InitStep2Info, string>(this, AmacAssist.MessageToken);
    }


}














