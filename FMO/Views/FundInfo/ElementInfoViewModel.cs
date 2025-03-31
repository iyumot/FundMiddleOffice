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


    public DataExtraViewModel(){}

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
        return new SealingRule {  Type = Type??default , Month = Month??0, Extra = Other };
    }
}

[AutoChangeableViewModel(typeof(FundInvestmentManager))]
public partial class InvestmentManagerInfoViewModel;



[AutoChangeableViewModel(typeof(BankAccount))]
public partial class BankAccountInfoViewModel: IDataValidation
{

    private string? _Deposit;

    public string? Deposit
    {
        get { if (string.IsNullOrWhiteSpace(_Deposit)) _Deposit = BankOfDeposit;  return _Deposit; }
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
        return Type switch { FundFeeType.Fix => $"固定费用：{Fee}元 / 年", FundFeeType.Ratio => $"{Fee}% / 年", FundFeeType.Other => Other, _ => $"未设置" } + (GuaranteedFee > 0 ? $" 有保底：{GuaranteedFee} / 年" : "");
    }
}



[AutoChangeableViewModel(typeof(AgencyInfo))]
public partial class AgencyInfoViewModel : IDataValidation
{
    public bool IsValid() => !HasAgency || !string.IsNullOrWhiteSpace(Name);

    public override string ToString() => HasAgency switch { true => Name!, _ => "-" };
}