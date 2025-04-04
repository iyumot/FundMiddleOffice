using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using FMO.Shared;

namespace FMO;

public partial class ManageFeeInfoViewModel : ObservableObject, IEquatable<ManageFeeInfoViewModel>
{
    [ObservableProperty]
    public partial FundFeeType? Type { get; set; }

    [ObservableProperty]
    public partial decimal? Value { get; set; }

    [ObservableProperty]
    public partial string? Other { get; set; }


    public ManageFeeInfoViewModel() { }

    public ManageFeeInfoViewModel(FundFeeInfo? info)
    {
        Type = info?.Type;
        Value = info?.Fee;
        Other = info?.Other;
    }

    public bool Equals(ManageFeeInfoViewModel? other)
    {
        return Type == other?.Type && Value == other?.Value;
    }

    internal FundFeeInfo Build()
    {
        return new FundFeeInfo { Fee = Value ?? default, Type = Type ?? default };
    }
}

public partial class DataExtraViewModel<T> : ObservableObject, IEquatable<DataExtraViewModel<T>> where T : struct
{
    [ObservableProperty]
    public partial T? Data { get; set; }


    [ObservableProperty]
    public partial string? Other { get; set; }


    public DataExtraViewModel() { }

    public DataExtraViewModel(DataExtra<T>? info)
    {
        Data = info?.Data;
        Other = info?.Extra;
    }

    public bool Equals(DataExtraViewModel<T>? other)
    {
        return EqualityComparer<T?>.Default.Equals(Data, other?.Data) && Other == other?.Other;
    }

    internal DataExtra<T> Build()
    {
        return new DataExtra<T> { Data = Data, Extra = Other };
    }
}



public partial class SealingInfoViewModel : ObservableObject, IEquatable<SealingInfoViewModel>
{
    [ObservableProperty]
    public partial SealingType? Type { get; set; }

    [ObservableProperty]
    public partial int? Month { get; set; }

    [ObservableProperty]
    public partial string? Other { get; set; }


    public SealingInfoViewModel() { }

    public SealingInfoViewModel(SealingRule? info)
    {
        Type = info?.Type;
        Month = info?.Month;
        Other = info?.Extra;
    }

    public bool Equals(SealingInfoViewModel? other)
    {
        return Type == other?.Type && Month == other?.Month && Other == other?.Other;
    }

    internal SealingRule Build()
    {
        return new SealingRule { Type = Type ?? default, Month = Month ?? 0, Extra = Other };
    }
}

[AutoChangeableViewModel(typeof(FundInvestmentManager))]
public partial class InvestmentManagerInfoViewModel;



[AutoChangeableViewModel(typeof(BankAccount))]
public partial class BankAccountInfoViewModel : IDataValidation
{

    private string? _Deposit;

    public string? Deposit
    {
        get { if (string.IsNullOrWhiteSpace(_Deposit)) _Deposit = BankOfDeposit; return _Deposit; }
        set { _Deposit = value; BankOfDeposit = value; }
    }


    public bool IsValid() => Bank?.Length > 3 && Name?.Length > 1 && Number?.Length > 5;
}

public partial class BankChangeableViewModel<T> : ChangeableViewModel<T, BankAccountInfoViewModel>
{
    public override bool CanConfirm => base.CanConfirm && (NewValue?.IsValid() ?? false);
}




[AutoChangeableViewModel(typeof(FundFeeInfo))]
public partial class FundFeeInfoViewModel
{
    public override string ToString()
    {
        return !HasFee ? "无" : Type switch { FundFeeType.Fix => $"固定费用：{Fee}元 / 年", FundFeeType.Ratio => $"{Fee}% / 年", FundFeeType.Other => Other, _ => $"未设置" } + (GuaranteedFee > 0 ? $" 有保底：{GuaranteedFee} / 年" : "");
    }
}



[AutoChangeableViewModel(typeof(AgencyInfo))]
public partial class AgencyInfoViewModel : IDataValidation
{
    public bool IsValid() => !HasAgency || !string.IsNullOrWhiteSpace(Name);

    public override string ToString() => HasAgency switch { true => Name!, _ => "-" };
}

[AutoChangeableViewModel(typeof(TemporarilyOpenInfo))]
public partial class TemporarilyOpenInfoViewModel : IDataValidation
{
    public bool IsValid() => !IsAllowed || (AllowPurchase || AllowRedemption);

    public override string ToString() => !IsAllowed ? "不允许临开" : $"允许{(AllowPurchase ? "申购" : "")}{(AllowRedemption ? "赎回" : "")}";
}


[AutoChangeableViewModel(typeof(FundPurchaseRule))]
public partial class FundPurchaseRuleViewModel
{
    public override string ToString()
    {
        return MinDeposit is null ? "未设置" : $"{MinDeposit / 10000}万起投" + (AdditionalDeposit > 0 ? $"，追加{AdditionalDeposit / 10000}万起" : "") + (HasRequirement ? Statement : "");
    }
}

[AutoChangeableViewModel(typeof(FeePayInfo))]
public partial class FeePayInfoViewModel
{
    public override string? ToString()
    {
        return Type switch { FeePayFrequency.Month => "按月支付", FeePayFrequency.Quarter => "按季支付", FeePayFrequency.Other => Other,  _ => "未设置" };
    }
}




[AutoChangeableViewModel(typeof(CoolingPeriodInfo))]
public partial class CoolingPeriodInfoViewModel
{
    public override string? ToString()
    {
        return Type switch { CoolingPeriodType.OneDay => "24小时",  CoolingPeriodType.Other => Other, _ => "未设置" };
    }
}



[AutoChangeableViewModel(typeof(CallbackInfo))]
public partial class CallbackInfoViewModel
{
    public override string? ToString()
    {
        return IsRequired ? "需要" : "不适用";
    }
}