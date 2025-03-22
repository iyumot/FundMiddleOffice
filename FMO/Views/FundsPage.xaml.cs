using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
            WeakReferenceMessenger.Default.Send(new OpenFundMessage { Id = vm.Id });
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


        else
            e.Accepted = true;
    }

    /// 基金信息更新
    /// </summary>
    /// <param name="message"></param>
    public void Receive(Fund fund)
    {
        var fvm = Funds.FirstOrDefault(x => x.Id == fund.Id);
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


        public static FundViewModel FromFund(Fund x)
        {
            return new FundViewModel
            {
                Id = x.Id,
                Name = x.Name,
                IsEnable = x.PublicDisclosureSynchronizeTime != default,
                IsCleared = x.Status switch { FundStatus.Liquidation or FundStatus.EarlyLiquidation or FundStatus.LateLiquidation or FundStatus.AdvisoryTerminated => true, _ => false },
                SetupDate = x.SetupDate,
                AuditDate = x.AuditDate,
                Code = x.Code
            };
        }
    }
}

public class OpenFundMessage
{
    public int Id { get; init; }

}