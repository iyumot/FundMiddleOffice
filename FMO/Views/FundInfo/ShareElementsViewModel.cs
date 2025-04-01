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

    public ChangeableViewModel<FundElements, FundPurchaseRuleViewModel> SubscriptionRule { get; set; }


    /// <summary>
    /// 申购费
    /// </summary>
    public ChangeableViewModel<FundElements, FundFeeInfoViewModel> PurchaseFee { get; set; }

    public ChangeableViewModel<FundElements, FundPurchaseRuleViewModel> PurchasRule { get; set; }



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
            InheritedFunc = x => x.ManageFee.GetValue(shareId, flowid).FlowId switch { -1 => false, int i => i < flowid },
            UpdateFunc = (x, y) => { if (y is not null) x.ManageFee.SetValue(shareId, y.Build(), flowid); },
            ClearFunc = x => x.ManageFee.RemoveValue(shareId, flowid)
        };
        ManageFee.Init(elements);

        SubscriptionFee = new ChangeableViewModel<FundElements, FundFeeInfoViewModel>
        {
            Label = "认购费",
            InitFunc = x => new(x.SubscriptionFee.GetValue(shareId, flowid).Value),
            InheritedFunc = x => x.SubscriptionFee.GetValue(shareId, flowid).FlowId switch { -1 => false, int i => i < flowid },
            UpdateFunc = (x, y) => { if (y is not null) x.SubscriptionFee.SetValue(shareId, y.Build(), flowid); },
            ClearFunc = x => x.SubscriptionFee.RemoveValue(shareId, flowid),
            DisplayFunc = x => x?.HasFee ?? false ? x?.Type switch { FundFeeType.Fix => $"固定{x.Fee}元", FundFeeType.Ratio => $"{x.Fee}%", FundFeeType.Other => x.Other, _ => $"未设置" } + (x?.GuaranteedFee > 0 ? $" 有保底：{x.GuaranteedFee}元" : "") : "无"
        };
        SubscriptionFee.Init(elements);

        SubscriptionRule = new ChangeableViewModel<FundElements, FundPurchaseRuleViewModel>
        {
            Label = "认购规则",
            InitFunc = x => new(x.SubscriptionRule.GetValue(shareId, flowid).Value),
            InheritedFunc = x => x.SubscriptionRule.GetValue(shareId, flowid).FlowId switch { -1 => false, int i => i < flowid },
            UpdateFunc = (x, y) => { if (y is not null) x.SubscriptionRule.SetValue(shareId, y.Build(), flowid); },
            ClearFunc = x => x.SubscriptionRule.RemoveValue(shareId, flowid)
        };
        SubscriptionRule.Init(elements);



        PurchaseFee = new ChangeableViewModel<FundElements, FundFeeInfoViewModel>
        {
            Label = "申购费",
            InitFunc = x => new(x.PurchaseFee.GetValue(shareId, flowid).Value),
            InheritedFunc = x => x.PurchaseFee.GetValue(shareId, flowid).FlowId switch { -1 => false, int i => i < flowid },
            UpdateFunc = (x, y) => { if (y is not null) x.PurchaseFee.SetValue(shareId, y.Build(), flowid); },
            ClearFunc = x => x.PurchaseFee.RemoveValue(shareId, flowid),
            DisplayFunc = x => x?.HasFee ?? false ? x?.Type switch { FundFeeType.Fix => $"固定{x.Fee}元", FundFeeType.Ratio => $"{x.Fee}%", FundFeeType.Other => x.Other, _ => $"未设置" } + (x?.GuaranteedFee > 0 ? $" 有保底：{x.GuaranteedFee}元" : "") : "无"
        };
        PurchaseFee.Init(elements);

        PurchasRule = new ChangeableViewModel<FundElements, FundPurchaseRuleViewModel>
        {
            Label = "申购规则",
            InitFunc = x => new(x.PurchasRule.GetValue(shareId, flowid).Value),
            InheritedFunc = x => x.PurchasRule.GetValue(shareId, flowid).FlowId switch { -1 => false, int i => i < flowid },
            UpdateFunc = (x, y) => { if (y is not null) x.PurchasRule.SetValue(shareId, y.Build(), flowid); },
            ClearFunc = x => x.PurchasRule.RemoveValue(shareId, flowid)
        };
        PurchasRule.Init(elements);


        RedemptionFee = new ChangeableViewModel<FundElements, FundFeeInfoViewModel>
        {
            Label = "赎回费",
            InitFunc = x => new(x.RedemptionFee.GetValue(shareId, flowid).Value),
            InheritedFunc = x => x.RedemptionFee.GetValue(shareId, flowid).FlowId switch { -1 => false, int i => i < flowid },
            UpdateFunc = (x, y) => { if (y is not null) x.RedemptionFee.SetValue(shareId, y.Build(), flowid); },
            ClearFunc = x => x.RedemptionFee.RemoveValue(shareId, flowid)
        };
        RedemptionFee.Init(elements);


    }

}
