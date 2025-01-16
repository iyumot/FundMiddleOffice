using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Utilities;
using Serilog;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// FundInfoPage.xaml 的交互逻辑
/// </summary>
public partial class FundInfoPage : UserControl
{
    public FundInfoPage()
    {
        InitializeComponent();
    }

}


public partial class FundInfoPageViewModel : ObservableObject
{
    public Fund Fund { get; init; }

    private bool _initialized;

    public FundInfoPageViewModel(Fund fund)
    {
        this.Fund = fund;

        FundName = fund.Name;
        FundShortName = fund.ShortName;
        SetupDate = fund.SetupDate;
        RegistDate = fund.AuditDate;
        InitiateDate = fund.InitiateDate == default ? null : fund.InitiateDate;
        FundCode = fund.Code;

        CollectionAccount = fund.CollectionAccount?.ToString();


        var db = new BaseDatabase();
        var flows = db.GetCollection<FundFlow>().Find(x => x.FundId == fund.Id).ToList();
        if (!flows.Any(x => x is InitiateFlow))
        {
            var f = new InitiateFlow { FundId = fund.Id, ElementFiles = new VersionedFileInfo { Name = "基金要素" }, ContractFiles = new VersionedFileInfo { Name = "基金合同" }, CustomFiles = new() };
            flows.Insert(0, f);
            db.GetCollection<FundFlow>().Insert(f);
        }

        if(fund.Status >= FundStatus.ContractFinalized && !flows.Any(x=> x is ContractFinalizeFlow))
        {
            var f = new ContractFinalizeFlow { FundId = fund.Id,  ContractFile = new FundFileInfo { Name = "基金合同" }, CustomFiles = new() };
            flows.Insert(1, f);
            db.GetCollection<FundFlow>().Insert(f);
        }

        if(fund.Status >= FundStatus.Setup && !flows.Any(x => x is SetupFlow))
        {
            var f = new SetupFlow { FundId = fund.Id, PaidInCapitalProof = new FundFileInfo { Name = "实缴出资证明" }, CustomFiles = new() };
            flows.Insert(2, f);
            db.GetCollection<FundFlow>().Insert(f);
        }


        db.Dispose();


        Flows = new ObservableCollection<ObservableObject>();
        foreach (var f in flows)
        {
            switch (f)
            {
                case InitiateFlow d:
                    Flows.Add(new InitiateFlowViewModel(d));
                    break;

                case ContractFinalizeFlow d:
                    Flows.Add(new ContractFinalizeFlowViewModel(d, fund.ShareClasses)); 
                    break;

                case SetupFlow d:
                    Flows.Add(new SetupFlowViewModel(d));
                    break;
                default:
                    break;
            }
        }




        _initialized = true;
    }


    [ObservableProperty]
    public partial bool IsEditable { get; set; }

    [ObservableProperty]
    public partial string? FundName { get; set; }


    [ObservableProperty]
    public partial string? FundShortName { get; set; }


    [ObservableProperty]
    public partial string? FundCode { get; set; }


    [ObservableProperty]
    public partial RiskLevel RiskLevel { get; set; }



    [ObservableProperty]
    public partial DateOnly? SetupDate { get; set; }



    [ObservableProperty]
    public partial DateOnly? RegistDate { get; set; }


    [ObservableProperty]
    public partial DateOnly? InitiateDate { get; set; }

    /// <summary>
    /// 投资范围
    /// </summary>
    [ObservableProperty]
    public partial string? InvestmentScope { get; set; }


    /// <summary>
    /// 募集账户
    /// </summary>
    [ObservableProperty]
    public partial string? CollectionAccount { get; set; }


    [ObservableProperty]
    public partial string? CustodyAccount { get; set; }


    /// <summary>
    /// 初始要素文件
    /// </summary>
    [ObservableProperty]
    public partial FileInfo? InitiateElementFile { get; set; }


    /// <summary>
    /// 初始基金合同
    /// </summary>
    [ObservableProperty]
    public partial FileInfo? InitiateFundContractFile { get; set; }


    [ObservableProperty]
    public partial ObservableCollection<ObservableObject> Flows { get; set; }






    [RelayCommand]
    public void NavigateToAmac()
    {
        try { if (Fund.Url is not null) System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(Fund.Url) { UseShellExecute = true }); } catch { }
    }


    partial void OnInitiateFundContractFileChanged(FileInfo? oldValue, FileInfo? newValue)
    {
        var dir = Fund.Folder();

        if (!dir.Exists)
            dir.Create();

        if (!dir.Exists)
        {
            Log.Error($"[{FundName}]存储文件夹无法创建,{dir}");
            HandyControl.Controls.Growl.Error($"[{FundName}]存储文件夹无法创建");
            return;
        }









    }
}
