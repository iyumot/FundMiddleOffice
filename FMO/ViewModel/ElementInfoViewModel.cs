using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using FMO.Shared;
using System.Text;
using System.Text.RegularExpressions;

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
        set { _Deposit = value; BankOfDeposit = value; SetDeposit(value); }
    }

    private void SetDeposit(string? str)
    {
        if (string.IsNullOrWhiteSpace(str)) return;
        var m = Regex.Match(str, @"(\w+银行)(?:.*公司)?(\w+)?");
        if (!m.Success) return;

        Bank = m.Groups[1].Value;
        if (m.Groups.Count > 2)
            Branch = m.Groups[2].Value;
    }

    public bool IsValid() => Bank?.Length > 3 && Name?.Length > 1 && Number?.Length > 5;

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(Name))
            builder.Append($"户名：{Name}\n");

        if (!string.IsNullOrWhiteSpace(Number))
            builder.Append($"账号：{Number}\n");

        if (!string.IsNullOrWhiteSpace(BankOfDeposit))
            builder.Append($"开户行：{BankOfDeposit}\n");

        if (!string.IsNullOrWhiteSpace(LargePayNo))
            builder.Append($"大额支付号：{LargePayNo}\n");

        if (!string.IsNullOrWhiteSpace(SwiftCode))
            builder.Append($"SWIFT：{SwiftCode}\n");

        return builder.ToString();
    }
}

public partial class BankChangeableViewModel<T> : ChangeableViewModel<T, BankAccountInfoViewModel>
{
    //public override bool CanConfirm => base.CanConfirm && (NewValue?.IsValid() ?? false);
}




[AutoChangeableViewModel(typeof(FundFeeInfo))]
public partial class FundFeeInfoViewModel:IDataValidation
{
    public bool IsValid() => Type switch { FundFeeType.Ratio or FundFeeType.Fix => Fee > 0, FundFeeType.Other => Other?.Length > 0,_=> false };

    public override string ToString()
    {
        return !HasFee ? "无" : Type switch { FundFeeType.Fix => $"固定费用：{Fee}元 / 年", FundFeeType.Ratio => $"{Fee}% / 年", FundFeeType.Other => Other, _ => $"未设置" } + (GuaranteedFee > 0 ? $" 有保底：{GuaranteedFee} / 年" : "");
    }
}

[AutoChangeableViewModel(typeof(RedemptionFeeInfo))]
public partial class RedemptionFeeInfoViewMdoel :  IDataValidation
{
     
    public bool IsValid() => Type switch { FundFeeType.ByTime => Parts?.Count > 1, _ => true };

    //    public RedemptionFeeInfoViewMdoel(FMO.Models.RedemptionFeeInfo? instance) : base(instance)
    //    {
    //        instance.Parts;
    //    }




    //    public partial class PartFeeViewModel:ObservableObject
    //    {
    //        [ObservableProperty]
    //        public partial int Month { get; set; }

    //        [ObservableProperty]
    //        public partial bool Include { get; set; }

    //        [ObservableProperty]
    //        public partial decimal Fee { get; set; }
    //    }
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

    public override string ToString() => !IsAllowed ? "不允许临开" : (IsLimited ? "仅合同变更、法规变更时，" : "") + $"允许{(AllowPurchase ? "申购" : "")}{(AllowRedemption ? "赎回" : "")}";
}


[AutoChangeableViewModel(typeof(FundPurchaseRule))]
public partial class FundPurchaseRuleViewModel:IDataValidation
{
    public string? FeeName { get; set; }

    public bool IsValid() => !HasFee || Type switch { FundFeeType.Ratio or FundFeeType.Fix => Fee > 0, FundFeeType.Other => Other?.Length > 0, _ => false };

    public override string ToString()
    {
        var a = MinDeposit is null ? "未设置" : $"{MinDeposit / 10000}万起投" + (AdditionalDeposit > 0 ? $"，追加{AdditionalDeposit / 10000}万起" : "") + (HasRequirement ? Statement : "");
        var b = HasFee ? $"   {FeeName}：" + PayMethod switch { FundFeePayType.Out => "价外收取", FundFeePayType.Extra => "额外收取", FundFeePayType.Other => PayOther, _ => "" } + Type switch { FundFeeType.Ratio => $"{Fee}%", FundFeeType.Fix => $"{Fee}元", FundFeeType.Other => Other, _ => "未知费用" } : "";
        var c = HasGuaranteedFee ? $"  保底 {GuaranteedFee}元" : "";
        return a + b + c;
    }
}

[AutoChangeableViewModel(typeof(FeePayInfo))]
public partial class FeePayInfoViewModel
{
    public override string? ToString()
    {
        return Type switch { FeePayFrequency.Month => "按月支付", FeePayFrequency.Quarter => "按季支付", FeePayFrequency.Other => Other, _ => "未设置" };
    }
}




[AutoChangeableViewModel(typeof(CoolingPeriodInfo))]
public partial class CoolingPeriodInfoViewModel
{
    public override string? ToString()
    {
        return Type switch { CoolingPeriodType.OneDay => "24小时", CoolingPeriodType.Other => Other, _ => "未设置" };
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