using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace FMO;



/// <summary>
/// 近日内的申赎
/// </summary>
public partial class BuySellNearDaysViewModel : ObservableObject, IRecipient<IList<TransferRequest>>, IRecipient<NewDay>
{
    public BuySellNearDaysViewModel()
    {
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public int BuyCount => BuyRequests.Count(x => !x.IsCanceled);

    public int SellCount => SellRequests.Count(x => !x.IsCanceled);

    public int CancelCount => BuyRequests.Count(x => x.IsCanceled && x.RequestType != TransferRequestType.Abort) + SellRequests.Count(x => x.IsCanceled && x.RequestType != TransferRequestType.Abort);

    public int TotalCount => BuyRequests.Count + SellRequests.Count;


    [ObservableProperty]
    public partial ObservableCollection<TransferRequest> BuyRequests { get; set; } = [];

    public bool ShowBuyList { get => field && BuyRequests.Count > 0; set => SetProperty(ref field, value); }

    [ObservableProperty]
    public partial ObservableCollection<TransferRequest> SellRequests { get; set; } = [];


    public bool ShowSellList { get => field && SellRequests.Count > 0; set => SetProperty(ref field, value); }












    [ObservableProperty]
    public partial DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    partial void OnBuyRequestsChanged(ObservableCollection<TransferRequest> oldValue, ObservableCollection<TransferRequest> newValue)
    {
        if (oldValue is not null)
            oldValue.CollectionChanged -= OnBuyRequestsChanged;
        if (newValue is not null)
            newValue.CollectionChanged += OnBuyRequestsChanged;
    }

    partial void OnSellRequestsChanged(ObservableCollection<TransferRequest> oldValue, ObservableCollection<TransferRequest> newValue)
    {
        if (oldValue is not null)
            oldValue.CollectionChanged -= OnSellRequestsChanged;
        if (newValue is not null)
            newValue.CollectionChanged += OnSellRequestsChanged;
    }

    private void OnSellRequestsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SellCount));
        OnPropertyChanged(nameof(CancelCount));
        OnPropertyChanged(nameof(TotalCount));
    }

    private void OnBuyRequestsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(BuyCount));
        OnPropertyChanged(nameof(CancelCount));
        OnPropertyChanged(nameof(TotalCount));
    }

    public void Receive(IList<TransferRequest> message)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var limit = today.AddDays(-7);

        // 删除过期的
        App.Current.Dispatcher.BeginInvoke(() =>
        {
            for (int i = BuyRequests.Count - 1; i >= 0; i--)
            {
                if (BuyRequests[i].RequestDate <= limit)
                    BuyRequests.RemoveAt(i);
            }

            for (int i = SellRequests.Count - 1; i >= 0; i--)
            {
                if (SellRequests[i].RequestDate <= limit)
                    SellRequests.RemoveAt(i);
            }

            foreach (var item in message.Where(x => x.IsBuy() && x.RequestDate > limit).ExceptBy(BuyRequests.Select(x => x.Id), x => x.Id))
            {
                BuyRequests.Add(item);
            }
            foreach (var item in message.Where(x => x.IsSell() && x.RequestDate > limit).ExceptBy(SellRequests.Select(x => x.Id), x => x.Id))
            {
                SellRequests.Add(item);
            }
        });



    }

    public void Receive(NewDay message)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var limit = today.AddDays(-7);

        App.Current.Dispatcher.BeginInvoke(() =>
        {
            for (int i = BuyRequests.Count - 1; i >= 0; i--)
            {
                if (BuyRequests[i].RequestDate <= limit)
                    BuyRequests.RemoveAt(i);
            }

            for (int i = SellRequests.Count - 1; i >= 0; i--)
            {
                if (SellRequests[i].RequestDate <= limit)
                    SellRequests.RemoveAt(i);
            }
        });
    }
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

        //BuySellNearDaysViewModel.BuyCount = BuySellNearDaysViewModel.BuyRequests.Count;
        //BuySellNearDaysViewModel.SellCount = BuySellNearDaysViewModel.SellRequests.Count;

        // 更新数量
        //BuySellNearDaysViewModel.BuyRequests.CollectionChanged += (s, e) => { BuySellNearDaysViewModel.BuyCount = BuySellNearDaysViewModel.BuyRequests.Count; };
        //BuySellNearDaysViewModel.SellRequests.CollectionChanged += (s, e) => { BuySellNearDaysViewModel.SellCount = BuySellNearDaysViewModel.SellRequests.Count; };


        var btrans = db.GetCollection<BankTransaction>().Find(x => x.Time > limitt).ToArray();





        foreach (var item in tq)
        {
        }
    }

}