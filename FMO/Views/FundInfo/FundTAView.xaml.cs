using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Utilities;

namespace FMO;

/// <summary>
/// FundTAView.xaml 的交互逻辑
/// </summary>
public partial class FundTAView : UserControl
{
    public FundTAView()
    {
        InitializeComponent();
    }
}



public partial class FundTAViewModel : ObservableObject
{
    public FundTAViewModel(int fundId)
    {
        FundId = fundId;

        using var db = DbHelper.Base();
        IsCleared = db.GetCollection<Fund>().FindById(fundId).Status > FundStatus.StartLiquidation;
        var ta = db.GetCollection<TransferRecord>().Find(x => x.FundId == fundId).ToList();
        var ts = db.GetCollection<FundShareRecord>().Find(x => x.FundId == fundId).OrderBy(x => x.Date).ToList();
        Records = ta; 

        var daily = db.GetDailyCollection(fundId).FindAll().OrderByDescending(x => x.Date).FirstOrDefault(x => x.NetValue > 0);
        var nv = daily?.NetValue ?? 0;
        NetValueDate = daily?.Date;

        if (ta.Count > 0)
        {
            // 按投资人分
            var cur = ta.GroupBy(x => x.CustomerId).Select(x => new { Id = x.Key, Name = x.First().CustomerName, Record = x, Share = x.Sum(y => y.ShareChange()) }).OrderByDescending(x => x.Share);
            InvestorCount = cur.Count(x=>x.Share > 0);
            CurrentTotalShare = cur.Sum(x => x.Share);
            CurrentShareDate = ta.Max(x => x.ConfirmedDate);
            CurrentShares = cur.Select(x => new InvestorShareViewModel { Id = x.Id, Name = x.Name, Share = x.Share, Asset = x.Share * nv, Profit = x.Record.Sum(y => y.AmountChange()) + x.Share * nv  }).ToList();
            CurrentTotalProfit = CurrentShares.Sum(x => x.Profit ?? 0);
        }

        if (DataTracker.FundTips.Any(x => x.FundId == fundId && x.Type == TipType.FundShareNotPair))
            CurrentShareNotPair = true;

        // 认购时份额
        var subscription = ta.Where(x => x.Type == TARecordType.Subscription);
        InitialShares = new(subscription.Select(x => new InvestorShareViewModel { Id = x.Id, Name = x.CustomerName, Share = x.ConfirmedShare }));
        InitialTotalShare = subscription.Sum(x => x.ConfirmedShare);
    }

    public int FundId { get; }

    public IList<TransferRecord> Records { get; set; }

    /// <summary>
    /// 清盘
    /// </summary>
    [ObservableProperty]
    public partial bool IsCleared { get; set; }

    [ObservableProperty]
    public partial int InvestorCount { get; set; }

    [ObservableProperty]
    public partial decimal CurrentTotalShare { get; set; }


    [ObservableProperty]
    public partial decimal CurrentTotalProfit { get; set; }

    [ObservableProperty]
    public partial DateOnly CurrentShareDate { get; set; }

    [ObservableProperty]
    public partial List<InvestorShareViewModel>? CurrentShares { get; set; }

    [ObservableProperty]
    public partial bool CurrentShareNotPair { get; set; }

    [ObservableProperty]
    public partial DateOnly? NetValueDate { get; set; }


    [ObservableProperty]
    public partial List<InvestorShareViewModel>? InitialShares { get; set; }


    [ObservableProperty]
    public partial decimal InitialTotalShare { get; set; }


    [ObservableProperty]
    public partial bool InvestorDetailIsOpen { get; set; }

    [ObservableProperty]
    public partial IEnumerable<TransferRecord>? InvestorDetail { get; set; }

    [RelayCommand]
    public void ShowInvestorDetail(InvestorShareViewModel v)
    {
        InvestorDetailIsOpen = true;
        InvestorDetail = Records.Where(x => x.CustomerId == v.Id);
    }
}

public class InvestorShareViewModel
{
    public int Id { get; set; }

    public string? Name { get; set; }
     
    public decimal Share { get; set; }

    /// <summary>
    /// 市值
    /// </summary>
    public decimal Asset { get; set; }


    public decimal? Profit { get; set; } 
}
