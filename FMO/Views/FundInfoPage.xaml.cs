using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using Microsoft.Win32;
using Serilog;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;
using static System.Net.WebRequestMethods;

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


public partial class FundInfoPageViewModel : ObservableRecipient, IRecipient<FundShareChangedMessage>
{
    public Fund Fund { get; init; }

    private bool _initialized;

    [SetsRequiredMembers]
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable CS9264 // 退出构造函数时，不可为 null 的属性必须包含非 null 值。请考虑添加 ‘required’ 修饰符，或将属性声明为可为 null，或添加 ‘[field: MaybeNull, AllowNull]’ 特性。
    public FundInfoPageViewModel(Fund fund, FundElements ele)
#pragma warning restore CS9264 // 退出构造函数时，不可为 null 的属性必须包含非 null 值。请考虑添加 ‘required’ 修饰符，或将属性声明为可为 null，或添加 ‘[field: MaybeNull, AllowNull]’ 特性。
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    {
        this.Fund = fund;

        FundName = fund.Name;
        FundShortName = fund.ShortName;
        SetupDate = fund.SetupDate;
        RegistDate = fund.AuditDate;
        InitiateDate = fund.InitiateDate == default ? null : fund.InitiateDate;
        FundCode = fund.Code;

        //CollectionAccount = fund.CollectionAccount?.ToString();


        InitFlows(fund, ele);

        RegistrationLetter = new LatestFileViewModel { Name = "备案函", File = Flows?.Select(x=>x switch { RegistrationFlowViewModel a => a.RegistrationLetter,  ContractModifyFlowViewModel b => b.RegistrationLetter, _=> null}).Where(x=>x is not null && x.File is not null).LastOrDefault()?.File };

        RiskLevel = ele.RiskLevel?.Value;

        IsActive = true;
        _initialized = true;
    }


    private void InitFlows(Fund fund, FundElements ele)
    {
        var db = new BaseDatabase();
        var flows = db.GetCollection<FundFlow>().Find(x => x.FundId == fund.Id).ToList();
        if (!flows.Any(x => x is InitiateFlow))
        {
            var f = new InitiateFlow { FundId = fund.Id, ElementFiles = new VersionedFileInfo { Name = "基金要素" }, ContractFiles = new VersionedFileInfo { Name = "基金合同" }, CustomFiles = new() };
            flows.Insert(0, f);
            db.GetCollection<FundFlow>().Insert(f);
        }

        if (fund.Status >= FundStatus.ContractFinalized && !flows.Any(x => x is ContractFinalizeFlow))
        {
            var f = new ContractFinalizeFlow { FundId = fund.Id, ContractFile = new FundFileInfo("基金合同"), CustomFiles = new() };
            flows.Insert(1, f);
            db.GetCollection<FundFlow>().Insert(f);
        }

        if (fund.Status >= FundStatus.Setup && !flows.Any(x => x is SetupFlow))
        {
            var f = new SetupFlow { FundId = fund.Id, Date = fund.SetupDate, PaidInCapitalProof = new FundFileInfo("实缴出资证明"), CustomFiles = new() };
            flows.Insert(2, f);
            db.GetCollection<FundFlow>().Insert(f);
        }


        if (fund.Status >= FundStatus.Registration && !flows.Any(x => x is RegistrationFlow))
        {
            var f = new RegistrationFlow { FundId = fund.Id, Date = fund.AuditDate, CustomFiles = new() };
            flows.Add(f);
            db.GetCollection<FundFlow>().Insert(f);
        }


        db.Dispose();

        Flows = new ObservableCollection<FlowViewModel>();
        Flows.CollectionChanged += Flows_CollectionChanged;

        foreach (var f in flows)
        {
            switch (f)
            {
                case InitiateFlow d:
                    Flows.Add(new InitiateFlowViewModel(d));
                    break;

                case ContractModifyFlow d:
                    Flows.Add(new ContractModifyFlowViewModel(d, ele.ShareClasses));
                    break;

                case ContractFinalizeFlow d:
                    Flows.Add(new ContractFinalizeFlowViewModel(d, ele.ShareClasses));
                    break;

                case SetupFlow d:
                    Flows.Add(new SetupFlowViewModel(d));
                    break;

                case RegistrationFlow d:
                    Flows.Add(new RegistrationFlowViewModel(d));
                    break;
                default:
                    break;
            }
        }


        FlowsSource = new CollectionViewSource { Source = Flows };
        FlowsSource.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(FlowViewModel.FlowId), System.ComponentModel.ListSortDirection.Descending));
        FlowsSource.Filter += FlowsSource_Filter;


        ElementsViewDataContext = new ElementsViewModel { FundId = Fund.Id };

        SelectedFlowInElements = Flows.LastOrDefault(x => x is IElementChangable);

    }

    private void Flows_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            foreach(FlowViewModel flow in e.NewItems!)
            {
                switch (flow)
                {
                    case RegistrationFlowViewModel a:
                        a.RegistrationLetter!.PropertyChanged += RegistrationLetter_PropertyChanged;
                        break;

                    case ContractModifyFlowViewModel a:
                        a.RegistrationLetter!.PropertyChanged += RegistrationLetter_PropertyChanged;
                        break;
                    default:
                        break;
                } 
            }
        }    
    }

    /// <summary>
    /// 更新最新备案函
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RegistrationLetter_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        RegistrationLetter = new LatestFileViewModel { Name = "备案函", File = Flows?.Select(x => x switch { RegistrationFlowViewModel a => a.RegistrationLetter, ContractModifyFlowViewModel b => b.RegistrationLetter, _ => null }).Where(x => x is not null && x.File is not null).LastOrDefault()?.File };
    }



    private void FlowsSource_Filter(object sender, FilterEventArgs e)
    {
        e.Accepted = e.Item switch { ContractFinalizeFlowViewModel or ContractModifyFlowViewModel => true, _ => false };
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
    public partial RiskLevel? RiskLevel { get; set; }



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

    /// <summary>
    /// 流程
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<FlowViewModel> Flows { get; set; }

    /// <summary>
    /// 流程，在要素页中
    /// </summary>
    public CollectionViewSource FlowsSource { get; set; }

    /// <summary>
    /// 选中的要素对应流程
    /// </summary>
    [ObservableProperty]
    public partial FlowViewModel? SelectedFlowInElements { get; set; }

    /// <summary>
    /// 要素 上下文
    /// </summary>
    [ObservableProperty]
    public partial ElementsViewModel ElementsViewDataContext { get; set; }


    [ObservableProperty]
    public partial LatestFileViewModel? RegistrationLetter { get; set; }


    /// <summary>
    /// 打开基金公示
    /// </summary>
    [RelayCommand]
    public void NavigateToAmac()
    {
        try { if (Fund.Url is not null) System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(Fund.Url) { UseShellExecute = true }); } catch { }
    }

    /// <summary>
    /// 发起合同变更 
    /// </summary>
    [RelayCommand]
    public void CreateContractModify()
    {
        var flow = new ContractModifyFlow { FundId = Fund.Id };
        var db = new BaseDatabase();
        db.GetCollection<FundFlow>().Insert(flow);
        var ele = db.GetCollection<FundElements>().FindOne(ele => ele.FundId == Fund.Id);
        db.Dispose();
        Flows.Add(new ContractModifyFlowViewModel(flow, ele.ShareClasses));
    }

    /// <summary>
    /// 废除流程
    /// </summary>
    /// <param name="flow"></param>
    [RelayCommand]
    public void DeleteFlow(FlowViewModel flow)
    {
        using var db = new BaseDatabase();
        db.GetCollection<FundFlow>().Delete(flow.FlowId);

        Flows.Remove(flow);
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

    /// <summary>
    /// 当flow变动时
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    partial void OnSelectedFlowInElementsChanged(FlowViewModel? oldValue, FlowViewModel? newValue)
    {
        if (newValue is not null)
            ElementsViewDataContext.FlowId = newValue.FlowId;
        else if (Flows.FirstOrDefault(x => x is IElementChangable) is FlowViewModel f)
            ElementsViewDataContext.FlowId = f.FlowId;
    }



    public void Receive(FundShareChangedMessage message)
    {
        foreach (var f in Flows)
        {
            if (f is ContractRelatedFlowViewModel vm && f.FlowId > message.FlowId)
                vm.InitShare();
        }
    }
}


public partial class LatestFileViewModel:ObservableObject
{
    public required string Name { get; set; }

    [ObservableProperty]
    public partial FileInfo? File { get; set; }


    [RelayCommand]
    public void View()
    {
        if (File?.Exists ?? false)
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(File.FullName) { UseShellExecute = true }); } catch { }
    }

     

    [RelayCommand]
    public void Print()
    {
        if (File is null || !File.Exists) return;


        PrintDialog printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            // 获取默认打印机名称
            string printerName = printDialog.PrintQueue.Name;

            // 使用系统默认的PDF阅读器打印PDF文档
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = File.FullName;
            process.StartInfo.Verb = "print";
            process.Start();

            // 等待打印任务完成
            process.WaitForExit();
        }
    }


    [RelayCommand]
    public void SaveAs()
    {
        if (File is null || !File.Exists) return;

        try
        {
            var d = new SaveFileDialog();
            d.FileName = File.Name;
            if (d.ShowDialog() == true)
                System.IO.File.Copy(File.FullName, d.FileName);
        }
        catch (Exception ex)
        {
            Log.Error($"文件另存为失败: {ex.Message}");
        }
    }
}