﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.IO.AMAC;
using FMO.Models;
using FMO.Utilities;

namespace FMO;

/// <summary>
/// FundsPage.xaml 的交互逻辑
/// </summary>
public partial class FundsPage : UserControl
{
    public FundsPage()
    {
        InitializeComponent();
    }


    private void OnOpenFund(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is FundsPageViewModel.FundViewModel vm)
            WeakReferenceMessenger.Default.Send(new OpenFundMessage(vm.Id));
    }

}

/// <summary>
/// 
/// </summary>
public partial class FundsPageViewModel : ObservableRecipient, IRecipient<Fund>
{
    [ObservableProperty]
    public partial ObservableCollection<FundViewModel> Funds { get; set; }

    public CollectionViewSource DataViewSource { get; init; }

    /// <summary>
    /// 基金总数
    /// </summary>
    [ObservableProperty]
    public partial int TotalCount { get; set; }

    /// <summary>
    /// 清盘基金总数
    /// </summary>
    [ObservableProperty]
    public partial int ClearCount { get; set; }



    public FundPageUiConfig UiConfig { get; set; }

    [ObservableProperty]
    public partial string? FundKeyword { get; set; }

    /// <summary>
    [SetsRequiredMembers]
    public FundsPageViewModel()
    {
        IsActive = true;

        UiConfig = FundPageUiConfig.Load();
        UiConfig.PropertyChanged += UiConfig_PropertyChanged;

        var db = DbHelper.Base();
        var funds = db.GetCollection<Fund>().FindAll().ToArray();
        db.Dispose();

        Funds = new(funds.Select(x => FundViewModel.FromFund(x)));

        DataViewSource = new CollectionViewSource { Source = Funds };
        DataViewSource.Filter += DataViewSource_Filter;

        TotalCount = funds.Count();
        ClearCount = Funds.Count(x => x.IsCleared);
    }


    private void DataViewSource_Filter(object sender, FilterEventArgs e)
    {
        if (e.Item is FundViewModel v && v.IsCleared && !UiConfig.ShowCleared)
            e.Accepted = false;
        else if (!string.IsNullOrWhiteSpace(FundKeyword) && e.Item is FundViewModel v2 && !(v2.Name?.Contains(FundKeyword) ?? false))
            e.Accepted = false;
        else
            e.Accepted = true;
    }

    /// 基金信息更新
    /// </summary>
    /// <param name="message"></param>
    public void Receive(Fund fund)
    {
        var fvm = Funds.FirstOrDefault(x => x.Id == fund.Id || x.Code == fund.Code);
        if (fvm is not null)
        {
            if (!fvm.IsEnable && fund.PublicDisclosureSynchronizeTime != default)
                fvm.IsEnable = true;

            if (fvm.Name != fund.Name)
                fvm.Name = fund.Name;

            if (fvm.Code != fund.Code)
                fvm.Code = fund.Code;

            if (fvm.SetupDate != fund.SetupDate)
                fvm.SetupDate = fund.SetupDate;

            if (fvm.AuditDate != fund.AuditDate)
                fvm.AuditDate = fund.AuditDate;


            fvm.IsCleared = fund.Status switch { FundStatus.Liquidation or FundStatus.EarlyLiquidation or FundStatus.LateLiquidation => true, _ => false };
        }
        else
        {
            Funds.Add(FundViewModel.FromFund(fund));

            TotalCount = Funds.Count();
        }

        /// 更新
        ClearCount = Funds.Count(x => x.IsCleared);

    }



    /// <summary>
    /// 从协会更新基金数据
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task SyncFromAmac()
    {
        try
        {
            using var db = DbHelper.Base();
            var manager = db.GetCollection<Manager>().FindOne(x => x.IsMaster);

            var funds = await AmacAssist.CrawleManagerInfo(manager);

            /// 新增或者改名的
            var newf = funds./*ExceptBy(Funds.Select(x => x.Name), x => x.Name).*/Select(x => new Fund
            {
                Name = x.Name!,
                ShortName = Fund.GetDefaultShortName(x.Name!),
                Url = "https://gs.amac.org.cn/amac-infodisc/res/pof" + x.Url,
                AsAdvisor = x.IsAdvisor
            });

            using HttpClient client = new HttpClient();

            foreach (var f in newf)
            {
                await AmacAssist.SyncFundInfoAsync(f, client);

                var old = Funds.FirstOrDefault(x => x.Code == f.Code);

                if (old is null)
                {
                    db.GetCollection<Fund>().Insert(f);
                    Funds.Add(FundViewModel.FromFund(f));
                    TotalCount += 1;
                    if (f.Status > FundStatus.StartLiquidation)
                        ClearCount += 1;
                }
                else
                {
                    if (!old.IsCleared && f.Status > FundStatus.StartLiquidation)
                    {
                        old.IsCleared = true;
                        var oldf = db.GetCollection<Fund>().FindById(old.Id);
                        oldf.Status = f.Status;
                        db.GetCollection<Fund>().Update(oldf);
                        ClearCount += 1;
                    }
                }
                await Task.Delay(200);
            }

            HandyControl.Controls.Growl.Error($"更新基金信息完成");
        }
        catch (Exception e)
        {
            HandyControl.Controls.Growl.Error($"更新基金信息失败，{e.Message}");
        }


    }


    [RelayCommand]
    public void Issue()
    {
        var wnd = new IssueNewFundWindow();
        var dc = new IssueNewFundWindowViewModel();
        wnd.DataContext = dc;
        wnd.Owner = App.Current.MainWindow;
        var r = wnd.ShowDialog();
        if (r is null || !r.Value) return;

        if (dc.Name is not null)
        {
            var f = new Fund { Name = dc.Name, ShortName = dc.ShortName, Code = dc.Code };
            FundHelper.InitNew(f);


            WeakReferenceMessenger.Default.Send(f);
        }
    }

    private void UiConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {

        switch (e.PropertyName)
        {
            case nameof(UiConfig.ShowCleared):

                DataViewSource.View.Refresh();
                break;
            default:
                break;
        }
    }


    partial void OnFundKeywordChanged(string? value)
    {
        DataViewSource.View.Refresh();
    }




    public partial class FundPageUiConfig : ObservableObject
    {

        [ObservableProperty]
        public partial bool ShowCleared { get; set; }



        [ObservableProperty]
        public partial bool ShowSetupDate { get; set; }


        [ObservableProperty]
        public partial bool ShowAuditDate { get; set; }


        [ObservableProperty]
        public partial bool ShowCode { get; set; } = true;

        private bool DeferSave { get; set; } = true;




        public FundPageUiConfig()
        {
        }


        public static FundPageUiConfig Load()
        {
            if (!File.Exists("config\\fundpage.json")) return new();

            using var fs = new FileStream("config\\fundpage.json", FileMode.Open);

            var obj = JsonSerializer.Deserialize<FundPageUiConfig>(fs) ?? new();
            obj.DeferSave = false;
            return obj;
        }


        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (DeferSave) return;

            using var fs = new FileStream("config\\fundpage.json", FileMode.Create);
            JsonSerializer.Serialize(fs, this);
            fs.Flush();
        }

    }



    public partial class FundViewModel : ObservableObject
    {
        public int Id { get; init; }


        [ObservableProperty]
        public partial string? Name { get; set; }

        /// <summary>
        /// 主体名称
        /// </summary>
        [ObservableProperty]
        public partial string? MajorName { get; set; }

        /// <summary>
        /// 次要名称
        /// </summary>
        [ObservableProperty]
        public partial string? SeniorName { get; set; }

        [ObservableProperty]
        public partial bool IsEnable { get; set; }


        /// <summary>
        /// 已清算
        /// </summary>
        [ObservableProperty]
        public partial bool IsCleared { get; set; }

        /// <summary>
        /// 成立日期
        /// </summary>
        [ObservableProperty]
        public partial DateOnly SetupDate { get; set; }

        /// <summary>
        /// 备案日期
        /// </summary>
        [ObservableProperty]
        public partial DateOnly AuditDate { get; set; }

        /// <summary>
        /// 备案号
        /// </summary>
        [ObservableProperty]
        public partial string? Code { get; set; }


        [ObservableProperty]
        public partial ObservableCollection<FundTip> Tips { get; set; }

        [ObservableProperty]
        public partial bool ShowTips { get; set; } = false; 

        public bool HasTip => Tips?.Count > 0;


        partial void OnTipsChanged(ObservableCollection<FundTip> value)
        {
            if (value is not null)
                value.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasTip));

            OnPropertyChanged(nameof(HasTip));
        }

        public static FundViewModel FromFund(Fund fund)
        {
            var m = Regex.Match(fund.Name, @"(\w+?)((?:私募|证券|期货|集合)\w+(?:基金|计划))");
             
            return new FundViewModel
            {
                Id = fund.Id,
                Name = fund.Name,
                MajorName = m.Success ? m.Groups[1].Value : fund.Name,
                SeniorName = m.Success ? m.Groups[2].Value : "",
                IsEnable = fund.Status < FundStatus.Normal || fund.PublicDisclosureSynchronizeTime != default,
                IsCleared = fund.Status switch { FundStatus.Liquidation or FundStatus.EarlyLiquidation or FundStatus.LateLiquidation or FundStatus.AdvisoryTerminated => true, _ => false },
                SetupDate = fund.SetupDate,
                AuditDate = fund.AuditDate,
                Code = fund.Code,
                Tips = new(DataTracker.FundTips.Where(x => x.FundId == fund.Id))
            };
        }
    }
}

