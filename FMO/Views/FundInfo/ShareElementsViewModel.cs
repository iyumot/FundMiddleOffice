using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using FMO.Shared;
using System.Diagnostics.CodeAnalysis;

namespace FMO;

/// <summary>
/// 与份额相关的
/// </summary>
public partial class ShareElementsViewModel : ObservableObject
{

    public required string Class { get; set; }

    /// <summary>
    /// 锁定期
    /// </summary>
   // [ObservableProperty]
   // public partial PortionElementItemWithEnumViewModel<SealingType, int>? LockingRule { get; set; }



    /// <summary>
    /// 管理费
    /// </summary>
    public ChangeableViewModel<FundElements, FundFeeInfoViewModel> ManageFee { get; }


    /// <summary>
    /// 认购费
    /// </summary>
    public ChangeableViewModel<FundElements, FundFeeInfoViewModel> SubscriptionFee { get; set; }


    /// <summary>
    /// 申购费
    /// </summary>
    public ChangeableViewModel<FundElements, FundFeeInfoViewModel> PurchaseFee { get; set; }



    /// <summary>
    /// 赎回费
    /// </summary>
    public ChangeableViewModel<FundElements, FundFeeInfoViewModel> RedemptionFee { get; set; }











    [SetsRequiredMembers]
    public ShareElementsViewModel(int shareId, string share, FundElements elements, int flowid)
    {
        Class = share;

        //(var id, var dic) = elements.LockingRule!.GetValue(flowid);

        //LockingRule = new ElementItemViewModelSealing(elements, nameof(FundElements.LockingRule), flowid, "锁定期");

        //LockingRule = new PortionElementItemWithEnumViewModel<SealingType, int>(elements, shareId, share, nameof(FundElements.LockingRule), flowid, "锁定期");
        //LockingRule.DisplayGenerator = (a, b, c) => a switch { SealingType.No => "无", SealingType.Has => $"{b}个月", SealingType.Other => c, _ => throw new NotImplementedException() };
 
        ManageFee = new ChangeableViewModel<FundElements, FundFeeInfoViewModel>
        {
            Label = "管理费",
            InitFunc = x => new(x.ManageFee.GetValue(shareId, flowid).Value),
            UpdateFunc = (x,y) => { if (y is not null) x.ManageFee.SetValue(shareId, y.Build(), flowid); },
            ClearFunc = x=>x.ManageFee.RemoveValue(shareId, flowid)
        };
        ManageFee.Init(elements);

        SubscriptionFee = new ChangeableViewModel<FundElements, FundFeeInfoViewModel>
        {
            Label = "认购费",
            InitFunc = x => new(x.SubscriptionFee.GetValue(shareId, flowid).Value),
            UpdateFunc = (x, y) => { if (y is not null) x.SubscriptionFee.SetValue(shareId, y.Build(), flowid); },
            ClearFunc = x => x.SubscriptionFee.RemoveValue(shareId, flowid)
        };
        SubscriptionFee.Init(elements);

        PurchaseFee = new ChangeableViewModel<FundElements, FundFeeInfoViewModel>
        {
            Label = "申购费",
            InitFunc = x => new(x.PurchaseFee.GetValue(shareId, flowid).Value),
            UpdateFunc = (x, y) => { if (y is not null) x.PurchaseFee.SetValue(shareId, y.Build(), flowid); },
            ClearFunc = x => x.ManageFee.RemoveValue(shareId, flowid)
        };
        SubscriptionFee.Init(elements);

        RedemptionFee = new ChangeableViewModel<FundElements, FundFeeInfoViewModel>
        {
            Label = "赎回费",
            InitFunc = x => new(x.RedemptionFee.GetValue(shareId, flowid).Value),
            UpdateFunc = (x, y) => { if (y is not null) x.RedemptionFee.SetValue(shareId, y.Build(), flowid); },
            ClearFunc = x => x.RedemptionFee.RemoveValue(shareId, flowid)
        };
        RedemptionFee.Init(elements);

   
    }

}
