using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.TPL;
using FMO.Utilities;
using Microsoft.Win32;
using Serilog;

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


public partial class FundInfoPageViewModel : ObservableRecipient, IRecipient<FundShareChangedMessage>, IRecipient<FundDailyUpdateMessage>, IRecipient<FundStrategyChangedMessage>,IRecipient<FundAccountChangedMessage>
{
    public Fund Fund { get; init; }

    public int FundId { get; private set; }

    private bool _initialized;


    //FileSystemWatcher sheetFolderWatcher;


#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable CS9264 // 退出构造函数时，不可为 null 的属性必须包含非 null 值。请考虑添加 ‘required’ 修饰符，或将属性声明为可为 null，或添加 ‘[field: MaybeNull, AllowNull]’ 特性。
    [SetsRequiredMembers]
    public FundInfoPageViewModel(Fund fund, FundElements ele)
    {
        this.Fund = fund;

        FundId = fund.Id;
        FundName = fund.Name;
        FundShortName = fund.ShortName;
        SetupDate = fund.SetupDate;
        RegistDate = fund.AuditDate;
        InitiateDate = fund.InitiateDate == default ? null : fund.InitiateDate;
        FundCode = fund.Code;
        FundStatus = fund.Status;

        CollectionAccount = ele.CollectionAccount.Value?.ToString();
        CustodyAccount = ele.CustodyAccount.Value?.ToString();

        InitFlows(fund, ele);



        RegistrationLetter = new LatestFileViewModel { Name = "备案函", File = Flows?.Select(x => x switch { RegistrationFlowViewModel a => a.RegistrationLetter, ContractModifyFlowViewModel b => b.RegistrationLetter, _ => null }).Where(x => x is not null && x.File is not null).LastOrDefault()?.File };


        /// 监控估值表文件夹
        //WatchSheetFolder(fund, ele);

        RiskLevel = ele.RiskLevel?.Value;

        // 净值
        var db = DbHelper.Base();
        DailyValues = new ObservableCollection<DailyValue>(db.GetDailyCollection(Fund.Id).FindAll().OrderByDescending(x => x.Date).IntersectBy(TradingDay.Days, x => x.Date));
        var strategies = db.GetCollection<FundStrategy>().Find(x => x.FundId == fund.Id).ToList();
        var names = db.GetCollection<FundElements>().FindById(FundId).FullName.Changes.Values.ToArray() ?? [];
        db.Dispose();
        App.Current.Dispatcher.BeginInvoke(() =>
        {
            DailySource.Source = DailyValues;
            DailySource.View.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(DailyValue.Date), System.ComponentModel.ListSortDirection.Descending));
            DailySource.View.Refresh();
        });

        var ll = DailyValues;
        CurveViewDataContext = new DailyValueCurveViewModel
        {
            FundId = Fund.Id,
            FundName = Fund.ShortName,
            Data = ll.OrderBy(x => x.Date).ToList(),
            SetupDate = Fund.SetupDate,
            StartDate = ll.LastOrDefault()?.Date,
            EndDate = ll.FirstOrDefault()?.Date,
            Strategies = strategies
        };


        StrategyDataContext = new(FundId, fund.SetupDate);
        AccountsDataContext = new(FundId, FundCode!, names);
        TADataContext = new(FundId);

         
        IsActive = true;
        _initialized = true;
    }

#pragma warning restore CS9264 // 退出构造函数时，不可为 null 的属性必须包含非 null 值。请考虑添加 ‘required’ 修饰符，或将属性声明为可为 null，或添加 ‘[field: MaybeNull, AllowNull]’ 特性。
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    //private void WatchSheetFolder(Fund fund, FundElements ele)
    //{
    //    /// 监控估值表文件夹
    //    sheetFolderWatcher = new FileSystemWatcher(FundHelper.GetFolder(fund.Id, "Sheet"));
    //    sheetFolderWatcher.EnableRaisingEvents = true;
    //    sheetFolderWatcher.Created += (s, e) =>
    //    {
    //        try
    //        {
    //            using var fs = new FileStream(e.FullPath, FileMode.Open);
    //            var v = ValuationSheetHelper.ParseExcel(fs);
    //            if (v.dy is null)
    //            {
    //                Log.Warning($"解析估值表 {e.Name} 出错");
    //                return;
    //            }

    //            if (v.code == FundCode || ele.FullName!.HasValue(v.fn))
    //            {
    //                var db = DbHelper.Base();
    //                db.GetDailyCollection(fund.Id).Upsert(v.dy);
    //            }
    //        }
    //        catch (Exception er)
    //        {
    //            Log.Warning($"解析估值表 {e.Name} 出错 {er.Message}");
    //        }
    //    };
    //}

    private void InitFlows(Fund fund, FundElements ele)
    {
        var db = DbHelper.Base();
        var flows = db.GetCollection<FundFlow>().Find(x => x.FundId == fund.Id).ToList();
        if (!flows.Any(x => x is InitiateFlow))
        {
            var f = new InitiateFlow { FundId = fund.Id, ElementFiles = new VersionedFileInfo { Name = "基金要素" }, ContractFiles = new VersionedFileInfo { Name = "基金合同" }, CustomFiles = new() };
            flows.Insert(0, f);
            db.GetCollection<FundFlow>().Insert(f);
        }

        if (!flows.Any(x => x is ContractFinalizeFlow))
        {
            var f = new ContractFinalizeFlow { FundId = fund.Id, ContractFile = new FileStorageInfo("基金合同"), CustomFiles = new() };
            flows.Insert(1, f);
            db.GetCollection<FundFlow>().Insert(f);
        }

        if (fund.Status >= FundStatus.Setup && !flows.Any(x => x is SetupFlow))
        {
            var f = new SetupFlow { FundId = fund.Id, Date = fund.SetupDate, PaidInCapitalProof = new FileStorageInfo("实缴出资证明"), CustomFiles = new() };
            flows.Insert(2, f);
            db.GetCollection<FundFlow>().Insert(f);
        }


        if (fund.Status >= FundStatus.Registration && !flows.Any(x => x is RegistrationFlow))
        {
            var f = new RegistrationFlow { FundId = fund.Id, Date = fund.AuditDate, CustomFiles = new() };
            flows.Add(f);
            db.GetCollection<FundFlow>().Insert(f);
        }

        if (fund.Status >= FundStatus.StartLiquidation && !flows.Any(x => x is LiquidationFlow))
        {
            var f = new LiquidationFlow { FundId = fund.Id, CustomFiles = new() };
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

                case LiquidationFlow d:
                    Flows.Add(new LiquidationFlowViewModel(d));
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
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            foreach (FlowViewModel flow in e.NewItems!)
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

    #region Property
    [ObservableProperty]
    public partial bool IsEditable { get; set; }

    [ObservableProperty]
    public partial string? FundName { get; set; }


    [ObservableProperty]
    public partial string? FundShortName { get; set; }


    [ObservableProperty]
    public partial string? FundCode { get; set; }


    [ObservableProperty]
    public partial FundStatus FundStatus { get; set; }


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


    [ObservableProperty]
    public partial LatestFileViewModel? RegistrationLetter { get; set; }

    public ObservableCollection<DailyValue> DailyValues { get; }

    public CollectionViewSource DailySource { get; } = new();


    /// <summary>
    /// 要素 上下文
    /// </summary>
    [ObservableProperty]
    public partial ElementsViewModel ElementsViewDataContext { get; set; }


    [ObservableProperty]
    public partial DailyValueCurveViewModel CurveViewDataContext { get; set; }

    [ObservableProperty]
    public partial FundStrategyViewModel StrategyDataContext { get; set; }

    [ObservableProperty]
    public partial FundAccountsViewModel AccountsDataContext { get; set; }

    
    [ObservableProperty]
    public partial FundTAViewModel TADataContext { get; set; }
    #endregion

    /// <summary>
    /// 打开基金公示
    /// </summary>
    [RelayCommand]
    public void NavigateToAmac()
    {
        try { if (Fund.Url is not null) System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(Fund.Url) { UseShellExecute = true }); } catch { }
    }


    [RelayCommand]
    public void SetupFund()
    {
        using var db = DbHelper.Base();
        if (!Flows.Any(x => x is SetupFlowViewModel))
        {
            var flow = new SetupFlow { FundId = Fund.Id };
            db.GetCollection<FundFlow>().Insert(flow);
            var ele = db.GetCollection<FundElements>().FindById(Fund.Id);
            db.Dispose();
            Flows.Add(new SetupFlowViewModel(flow));
        }
        if (!Flows.Any(x => x is RegistrationFlowViewModel))
        {
            var flow = new RegistrationFlow { FundId = Fund.Id };
            db.GetCollection<FundFlow>().Insert(flow);
            var ele = db.GetCollection<FundElements>().FindById(Fund.Id);
            db.Dispose();
            Flows.Add(new RegistrationFlowViewModel(flow));
        }

    }

    /// <summary>
    /// 发起合同变更 
    /// </summary>
    [RelayCommand]
    public void CreateContractModify()
    {
        var flow = new ContractModifyFlow { FundId = Fund.Id };
        var db = DbHelper.Base();
        db.GetCollection<FundFlow>().Insert(flow);
        var ele = db.GetCollection<FundElements>().FindById(Fund.Id);
        db.Dispose();
        Flows.Add(new ContractModifyFlowViewModel(flow, ele.ShareClasses));
    }


    [RelayCommand]
    public void CreateClearFlow()
    {
        if (FundStatus >= FundStatus.StartLiquidation) return;

        if (Flows.Any(x => x is LiquidationFlowViewModel)) return;

        var flow = new LiquidationFlow { FundId = Fund.Id };
        var db = DbHelper.Base();
        db.GetCollection<FundFlow>().Insert(flow);
        var fund = db.GetCollection<Fund>().FindById(Fund.Id);
        fund.Status = FundStatus.StartLiquidation;
        db.GetCollection<Fund>().Update(fund);
        db.Dispose();
        Flows.Add(new LiquidationFlowViewModel(flow));
        WeakReferenceMessenger.Default.Send(new FundStatusChangedMessage(default, default) { FundId = fund.Id, Status = fund.Status });
    }


    /// <summary>
    /// 废除流程
    /// </summary>
    /// <param name="flow"></param>
    [RelayCommand]
    public void DeleteFlow(FlowViewModel flow)
    {
        using var db = DbHelper.Base();
        db.GetCollection<FundFlow>().Delete(flow.FlowId);
        if (flow is LiquidationFlowViewModel && FundStatus == FundStatus.StartLiquidation)
        {
            FundStatus = FundStatus.Normal;

            var fund = db.GetCollection<Fund>().FindById(Fund.Id);
            fund.Status = FundStatus.Normal;
            db.GetCollection<Fund>().Update(fund);

            WeakReferenceMessenger.Default.Send(new FundStatusChangedMessage(default, default) { FundId = fund.Id, Status = fund.Status });
        }
        Flows.Remove(flow);
    }




    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshNetValuesCommand))]
    public partial bool CanRefreshNetValues { get; set; } = true;

    #region Nv List

    [RelayCommand(CanExecute = nameof(CanRefreshNetValues))]
    public void RefreshNetValues()
    {
        CanRefreshNetValues = false;
        Task.Run(() =>
        {
            var fd = FundHelper.GetFolder(Fund.Id, "Sheet");
            var di = new DirectoryInfo(fd);
            if (!di.Exists)
            {
                HandyControl.Controls.Growl.Info("未发现本基金的估值表");
                App.Current.Dispatcher.BeginInvoke(() => CanRefreshNetValues = true);
                return;
            }

            ConcurrentBag<(string? name, string? code, DailyValue? daily)> bag = new();

            try
            {
                Parallel.ForEach(di.GetFiles(), f =>
                {
                    using var fs = f.OpenRead();
                    var item = ValuationSheetHelper.ParseExcel(fs);
                    if (item.dy is not null)
                        item.dy.SheetPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), f.FullName);
                    else Log.Error($"解析{f.Name}出错");

                    bag.Add(item);
                });


                var data = bag.OrderBy(x => x.daily?.Date).ToArray();

                // 从属验证
                var err = data.Where(x => x.code != Fund.Code && x.name != Fund.Name).ToArray();
                if (err.Length != 0)
                {
                    Log.Error($"{FundName} 解析全部估值表出错 发现{err.Length}个文件不属于本基金\n{string.Join('\n', err.Select(x => x.name))}))");
                    HandyControl.Controls.Growl.Info($"发现{err.Length}个文件不属于本基金\n{string.Join('\n', err.Select(x => x.name))}))");
                }

                var avaliable = data.Where(x => x.code == Fund.Code || x.name == Fund.Name);
                if (!avaliable.Any()) return;


                using var db = DbHelper.Base();
                var c = db.GetDailyCollection(Fund.Id);
                c.Upsert(avaliable.Select(x => x.daily!));


                App.Current.Dispatcher.BeginInvoke(() =>
                {
                    var ll = new List<DailyValue>(db.GetDailyCollection(Fund.Id).FindAll().OrderByDescending(x => x.Date).IntersectBy(TradingDay.Days, x => x.Date).ToList());


                    DailySource.Source = ll;
                    DailySource.View.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(DailyValue.Date), System.ComponentModel.ListSortDirection.Descending));
                    DailySource.View.Refresh();


                    WeakReferenceMessenger.Default.Send(new FundDailyUpdateMessage(default, default) { FundId = Fund.Id, Daily = avaliable.Select(x => x.daily).OrderBy(x => x.Date).FirstOrDefault()! });
                });
            }
            catch (Exception e)
            {
                Log.Error($"{FundName} 解析全部估值表出错 {e.Message}");
            }

            App.Current.Dispatcher.BeginInvoke(() => CanRefreshNetValues = true);
        });
    }


    [RelayCommand]
    public void ExportNetValues()
    {
        var last = DailyValues.FirstOrDefault(x => x.NetValue > 0);
        if (last is null) return;

        var fd = new SaveFileDialog();
        fd.FileName = $"{FundName} 每日净值 {last.Date:yyyy-MM-dd}.xlsx";
        fd.Filter = "Excel|*.xlsx";
        var r = fd.ShowDialog();
        if (r is null || !r.Value) return;

        ExcelTpl.GenerateFromTemplate(fd.FileName, "sigle_fund_nvlist.xlsx", new { nvs = DailyValues.Where(x => x.NetValue > 0) });
    }
    #endregion


    [RelayCommand]
    public void ViewSheet(DailyValue daily)
    {
        string? path = daily?.SheetPath;

        if (path is null)
        {
            var di = new DirectoryInfo(Path.Combine(FundHelper.GetFolder(FundId, "Sheet")));
            var fis = di.GetFiles().Where(x => x.Name.Contains(daily!.Date.ToString("yyyyMMdd")));
            if (fis.Count() == 1)
                path = fis.First().FullName;
        }

        if (path is not null)
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true }); } catch { }
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

    public void Receive(FundDailyUpdateMessage message)
    {
        if (message.FundId == Fund.Id)
        {
            var old = DailyValues.FirstOrDefault(x => x.Id == message.Daily.Id);
            if (old is not null)
                DailyValues.Remove(old);

            DailyValues.Add(message.Daily);
             
            CurveViewDataContext.Data = DailyValues.OrderBy(x => x.Date).ToList();
        }
    }

    public void Receive(FundStrategyChangedMessage message)
    {
        if (message.FundId == FundId)
        {
            using var db = DbHelper.Base();
            var strategies = db.GetCollection<FundStrategy>().Find(x => x.FundId == FundId).ToList();
            CurveViewDataContext.Strategies = strategies.ToList();
        }
    }

    public void Receive(FundAccountChangedMessage message)
    {
        switch (message.Type)
        {
            case FundAccountType.None:
                break;
            case FundAccountType.Collection:
                CollectionAccount = ElementsViewDataContext.CollectionAccount?.OldValue?.ToString();
                break;
            case FundAccountType.Custody:
                CustodyAccount = ElementsViewDataContext.CustodyAccount?.OldValue?.ToString();
                break;
            default:
                break;
        }
    }
}



/// <summary>
/// 最新的文件版本视图
/// </summary>
public partial class LatestFileViewModel : ObservableObject
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