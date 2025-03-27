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
    [ObservableProperty]
    public partial PortionElementItemWithEnumViewModel<SealingType, int>? LockingRule { get; set; }



    /// <summary>
    /// 管理费
    /// </summary>
    [ObservableProperty]
    public partial PortionElementItemWithEnumViewModel<FundFeeType, decimal>? ManageFee { get; set; }

    public ChangeableViewModel<FundElements, Tuple<FundFeeType, decimal>> ManageFee2 { get;  }


    /// <summary>
    /// 认购费
    /// </summary>
    [ObservableProperty]
    public partial PortionElementItemWithEnumViewModel<FundFeeType, decimal>? SubscriptionFee { get; set; }


    /// <summary>
    /// 申购费
    /// </summary>
    [ObservableProperty]
    public partial PortionElementItemWithEnumViewModel<FundFeeType, decimal>? PurchaseFee { get; set; }



    /// <summary>
    /// 赎回费
    /// </summary>
    [ObservableProperty]
    public partial PortionElementItemWithEnumViewModel<FundFeeType, decimal>? RedemptionFee { get; set; }











    [SetsRequiredMembers]
    public ShareElementsViewModel(int shareId, string share, FundElements elements, int flowid)
    {
        Class = share;

        //(var id, var dic) = elements.LockingRule!.GetValue(flowid);

        //LockingRule = new ElementItemViewModelSealing(elements, nameof(FundElements.LockingRule), flowid, "锁定期");

        LockingRule = new PortionElementItemWithEnumViewModel<SealingType, int>(elements, shareId, share, nameof(FundElements.LockingRule), flowid, "锁定期");
        LockingRule.DisplayGenerator = (a, b, c) => a switch { SealingType.No => "无", SealingType.Has => $"{b}个月", SealingType.Other => c, _ => throw new NotImplementedException() };

        ManageFee = new PortionElementItemWithEnumViewModel<FundFeeType, decimal>(elements, shareId, share, nameof(FundElements.ManageFee), flowid, "管理费");
        ManageFee.DisplayGenerator = (a, b, c) => a switch { FundFeeType.Fix => $"每年固定{b}元", FundFeeType.Ratio => $"{b}%", FundFeeType.Other => c, _ => throw new NotImplementedException() };

       // ManageFee2 = new ChangeableViewModel<FundElements, Tuple<FundFeeType, decimal>>(elements,)

        SubscriptionFee = new PortionElementItemWithEnumViewModel<FundFeeType, decimal>(elements, shareId, share, nameof(FundElements.SubscriptionFee), flowid, "认购费");
        SubscriptionFee.DisplayGenerator = (a, b, c) => a switch { FundFeeType.Fix => $"单笔{b}元", FundFeeType.Ratio => $"{b}%", FundFeeType.Other => c, _ => throw new NotImplementedException() };

        PurchaseFee = new PortionElementItemWithEnumViewModel<FundFeeType, decimal>(elements, shareId, share, nameof(FundElements.PurchaseFee), flowid, "申购费");
        PurchaseFee.DisplayGenerator = (a, b, c) => a switch { FundFeeType.Fix => $"单笔{b}元", FundFeeType.Ratio => $"{b}%", FundFeeType.Other => c, _ => throw new NotImplementedException() };


        RedemptionFee = new PortionElementItemWithEnumViewModel<FundFeeType, decimal>(elements, shareId, share, nameof(FundElements.RedemptionFee), flowid, "赎回费");
        RedemptionFee.DisplayGenerator = (a, b, c) => a switch { FundFeeType.Fix => $"单笔{b}元", FundFeeType.Ratio => $"{b}%", FundFeeType.Other => c, _ => throw new NotImplementedException() };



    }

}
