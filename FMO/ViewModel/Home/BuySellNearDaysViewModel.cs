using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;

namespace FMO;



/// <summary>
/// 近日内的申赎
/// </summary>
public partial class BuySellNearDaysViewModel : ObservableObject
{
    [ObservableProperty]
    public partial int BuyCount { get; set; }
    [ObservableProperty]
    public partial int SellCount { get; set; }
    [ObservableProperty]
    public partial int CancelCount { get; set; }
    [ObservableProperty]
    public partial int TotalCount { get; set; }


    [ObservableProperty]
    public partial ObservableCollection<TransferRequest> BuyRequests { get; set; } = [];

    public bool ShowBuyList { get => field && BuyRequests.Count > 0; set => SetProperty(ref field, value); }

    [ObservableProperty]
    public partial ObservableCollection<TransferRequest> SellRequests { get; set; } = [];


    public bool ShowSellList { get => field && SellRequests.Count > 0; set => SetProperty(ref field, value); }












    [ObservableProperty]
    public partial DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
}



public partial class HomePageViewModel
{
    [ObservableProperty]
    public partial BuySellNearDaysViewModel? BuySellNearDaysViewModel { get; set; }

    /// <summary>
    /// 检查交易申请与流水是否匹配
    /// </summary>
    private void DailyCheckRequestIsWell()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var limit = today.AddDays(-7);
        var limitt = new DateTime(limit, default);
        using var db = DbHelper.Base();
        // 7日内的request
        var tq = db.GetCollection<TransferRequest>().Find(x => x.RequestDate > limit).ToArray();

        BuySellNearDaysViewModel = new()
        {
            BuyRequests = [.. tq.Where(x => x.RequestType.IsBuy())],
            SellRequests = [.. tq.Where(x => x.RequestType.IsSell())],
        };

        BuySellNearDaysViewModel.BuyCount = BuySellNearDaysViewModel.BuyRequests.Count;
        BuySellNearDaysViewModel.SellCount = BuySellNearDaysViewModel.SellRequests.Count;

        // 更新数量
        BuySellNearDaysViewModel.BuyRequests.CollectionChanged += (s, e) => { BuySellNearDaysViewModel.BuyCount = BuySellNearDaysViewModel.BuyRequests.Count; };
        BuySellNearDaysViewModel.SellRequests.CollectionChanged += (s, e) => { BuySellNearDaysViewModel.SellCount = BuySellNearDaysViewModel.SellRequests.Count; };


        var btrans = db.GetCollection<BankTransaction>().Find(x => x.Time > limitt).ToArray();





        foreach (var item in tq)
        {
        }
    }

}