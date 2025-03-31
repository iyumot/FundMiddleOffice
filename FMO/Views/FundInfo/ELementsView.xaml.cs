﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// ELementsView.xaml 的交互逻辑
/// </summary>
public partial class ElementsView : UserControl
{
    public ElementsView()
    {
        InitializeComponent();
    }
}



public partial class ElementsViewModel : EditableControlViewModelBase<FundElements>, IRecipient<FundShareChangedMessage>
{
    public static RiskLevel[] RiskLevels { get; } = [Models.RiskLevel.R1, Models.RiskLevel.R2, Models.RiskLevel.R3, Models.RiskLevel.R4, Models.RiskLevel.R5];

    public static FundMode[] FundModes { get; } = [Models.FundMode.Open, Models.FundMode.Close, Models.FundMode.Other];

    public static FundFeeType[] FundFeeTypes { get; } = [FundFeeType.Ratio, FundFeeType.Fix, FundFeeType.Other];

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    public partial int FundId { get; set; }


    [ObservableProperty]
    public partial int FlowId { get; set; }


    public DateOnly SetupDate { get; set; }

    #region 要素

    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, string>? FullName { get; set; }



    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, string> ShortName { get; set; }


    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, RiskLevel> RiskLevel { get; set; }





    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, int?> DurationInMonths { get; set; }

    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, DateOnly?> ExpirationDate { get; set; }



    [ObservableProperty]
    public partial BankChangeableViewModel<FundElements>? CollectionAccount { get; set; }


    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, BankAccountInfoViewModel>? CustodyAccount { get; set; }




    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, decimal?>? StopLine { get; set; }



    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, decimal?>? WarningLine { get; set; }




    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSealingFund))]
    public partial ChangeableViewModel<FundElements, DataExtraViewModel<FundMode>>? FundModeInfo { get; set; }


    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, SealingInfoViewModel>? SealingRule { get; set; }


    //[ObservableProperty]
    //public partial ElementItemViewModelSealing? LockingRule { get; set; }

    [ObservableProperty]
    public partial SealingType[]? SealingTypes { get; set; } = [SealingType.No, SealingType.Has, SealingType.Other];

    [ObservableProperty]
    public partial bool IsSealingFund { get; set; }



    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, string> OpenDayInfo { get; set; }



    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, AgencyInfoViewModel> TrusteeInfo { get; set; }

    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, AgencyInfoViewModel> OutsourcingInfo { get; set; }


    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, FundFeeInfoViewModel> TrusteeFee { get; set; }


    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, FundFeeInfoViewModel> OutsourcingFee { get; set; }


    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, string> InvestmentManagers { get; set; }




    //[ObservableProperty]
    //public partial ElementRefrenceWithBooleanViewModel<string>? PerformanceBenchmarks { get; set; }


    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, string>? InvestmentObjective { get; set; }


    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, string>? InvestmentScope { get; set; }


    [ObservableProperty]
    public partial ChangeableViewModel<FundElements, string>? InvestmentStrategy { get; set; }



    /// <summary>
    /// 与份额相关的
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<ShareElementsViewModel> PortionElements { get; set; } = new();


    [ObservableProperty]
    public partial bool OnlyOneShare { get; set; }





    #endregion

    partial void OnFlowIdChanged(int oldValue, int newValue)
    {
        Id = FundId;
        using var db = DbHelper.Base();
        var fund = db.GetCollection<Fund>().FindById(FundId);
        var flow = db.GetCollection<FundFlow>().FindById(newValue);
        bool isori = flow is ContractFinalizeFlow;
        var elements = db.GetCollection<FundElements>().FindById(FundId);

        if (elements is null)
            elements = new FundElements { FundId = FundId };

        var type = GetType();
        SetupDate = fund.SetupDate;

        FullName = new ChangeableViewModel<FundElements, string>
        {
            Label = "基金全称",
            InitFunc = x => x.FullName.GetValue(newValue).Value,
            UpdateFunc = (x, y) => x.FullName.SetValue(y!, newValue),
            ClearFunc = x => x.FullName.RemoveValue(newValue)
        };
        FullName.Init(elements);

        ShortName = new ChangeableViewModel<FundElements, string>
        {
            Label = "基金简称",
            InitFunc = x => x.ShortName.GetValue(newValue).Value,
            UpdateFunc = (x, y) => x.ShortName.SetValue(y!, newValue),
            ClearFunc = x => x.ShortName.RemoveValue(newValue)
        };
        ShortName.Init(elements);


        if (isori)
        {
            if (FullName.NewValue == default)
                FullName.NewValue = fund.Name;

            if (ShortName.NewValue == default)
                ShortName.NewValue = fund.ShortName;
        }

        FundModeInfo = new ChangeableViewModel<FundElements, DataExtraViewModel<FundMode>>
        {
            Label = "运作方式",
            InitFunc = x => new(x.FundModeInfo!.GetValue(newValue).Value),
            UpdateFunc = (x, y) => x.FundModeInfo!.SetValue(y!.Build(), newValue),
            ClearFunc = x => x.FundModeInfo!.RemoveValue(newValue),
            DisplayFunc = x => x?.Data switch { FundMode.Open => "开放式", FundMode.Close => "封闭式", FundMode.Other => x.Other, _ => "-" }
        };
        FundModeInfo.Init(elements);


        SealingRule = new ChangeableViewModel<FundElements, SealingInfoViewModel>
        {
            Label = "封闭期",
            InitFunc = x => new(x.SealingRule!.GetValue(newValue).Value),
            UpdateFunc = (x, y) => x.SealingRule!.SetValue(y!.Build(), newValue),
            ClearFunc = x => x.SealingRule!.RemoveValue(newValue),
            DisplayFunc = x => x?.Type switch { SealingType.No => "无", SealingType.Has => $"{x.Month}个月", SealingType.Other => x.Other, _ => "-" }
        };
        SealingRule.Init(elements);

        RiskLevel = new ChangeableViewModel<FundElements, RiskLevel>
        {
            Label = "风险等级",
            InitFunc = x => x.RiskLevel!.GetValue(newValue).Value,
            UpdateFunc = (x, y) => x.RiskLevel!.SetValue(y, newValue),
            ClearFunc = x => x.RiskLevel!.RemoveValue(newValue),

        };
        RiskLevel.Init(elements);

        DurationInMonths = new ChangeableViewModel<FundElements, int?>
        {
            Label = "存续期",
            InitFunc = x => x.DurationInMonths!.GetValue(newValue).Value switch { 0 => null, var n => n },
            UpdateFunc = (x, y) => { if (y is not null) x.DurationInMonths!.SetValue(y.Value, newValue); },
            ClearFunc = x => x.DurationInMonths!.RemoveValue(newValue),
        };
        DurationInMonths.Init(elements);

        ExpirationDate = new ChangeableViewModel<FundElements, DateOnly?>
        {
            Label = "到期日",
            InitFunc = x => ValueFormat(x.ExpirationDate!.GetValue(newValue).Value),
            UpdateFunc = (x, y) => { if (y is not null) x.ExpirationDate!.SetValue(y.Value, newValue); },
            ClearFunc = x => x.ExpirationDate!.RemoveValue(newValue),
        };
        ExpirationDate.Init(elements);



        CollectionAccount = new BankChangeableViewModel<FundElements>
        {
            Label = "募集账户",
            InitFunc = x => new(x.CollectionAccount.GetValue(newValue).Value),
            UpdateFunc = (x, y) => x.CollectionAccount.SetValue(y!.Build(), newValue),
            ClearFunc = x => x.CollectionAccount.RemoveValue(newValue),
            DisplayFunc = x => BankString(x)
        };
        CollectionAccount.Init(elements);


        CustodyAccount = new ChangeableViewModel<FundElements, BankAccountInfoViewModel>
        {
            Label = "托管账户",
            InitFunc = x => new(x.CustodyAccount.GetValue(newValue).Value),
            UpdateFunc = (x, y) => x.CustodyAccount.SetValue(y!.Build(), newValue),
            ClearFunc = x => x.CustodyAccount.RemoveValue(newValue),
            DisplayFunc = x => BankString(x)
        };
        CustodyAccount.Init(elements);


        StopLine = new ChangeableViewModel<FundElements, decimal?>
        {
            Label = "止损线",
            InitFunc = x => ValueFormat(x.StopLine.GetValue(newValue).Value),
            UpdateFunc = (x, y) => { if (y is not null) x.StopLine!.SetValue(y.Value, newValue); },
            ClearFunc = x => x.StopLine.RemoveValue(newValue),
        };
        StopLine.Init(elements);

        WarningLine = new ChangeableViewModel<FundElements, decimal?>
        {
            Label = "预警线",
            InitFunc = x => ValueFormat(x.WarningLine.GetValue(newValue).Value),
            UpdateFunc = (x, y) => { if (y is not null) x.WarningLine!.SetValue(y.Value, newValue); },
            ClearFunc = x => x.WarningLine.RemoveValue(newValue),
        };
        WarningLine.Init(elements);

        //FundModeInfo = new ElementItemFundModeViewModel(elements, nameof(FundElements.FundModeInfo), FlowId, "运作方式");

        //SealingRule = new ElementItemViewModelSealing(elements, nameof(FundElements.SealingRule), FlowId, "封闭期");
        //// LockingRule = new ElementItemViewModelSealing(elements, nameof(FundElements.LockingRule), FlowId, "锁定期");


        //IsSealingFund = FundModeInfo.Data.New == Models.FundMode.Close;

        OpenDayInfo = new ChangeableViewModel<FundElements, string>
        {
            Label = "开放日规则",
            InitFunc = x => x.OpenDayInfo.GetValue(newValue).Value,
            UpdateFunc = (x, y) => x.OpenDayInfo.SetValue(y!, newValue),
            ClearFunc = x => x.OpenDayInfo.RemoveValue(newValue)
        };
        OpenDayInfo.Init(elements);

        TrusteeFee = new ChangeableViewModel<FundElements, FundFeeInfoViewModel>
        {
            Label = "托管费",
            InitFunc = x => new(x.TrusteeFee.GetValue(newValue).Value),
            UpdateFunc = (x, y) => x.TrusteeFee.SetValue(y!.Build(), newValue),
            ClearFunc = x => x.TrusteeFee.RemoveValue(newValue),
            DisplayFunc = x => x?.ToString() ?? "-"
        };
        TrusteeFee.Init(elements);


        OutsourcingFee = new ChangeableViewModel<FundElements, FundFeeInfoViewModel>
        {
            Label = "外包费",
            InitFunc = x => new(x.OutsourcingFee.GetValue(newValue).Value),
            UpdateFunc = (x, y) => x.OutsourcingFee.SetValue(y!.Build(), newValue),
            ClearFunc = x => x.OutsourcingFee.RemoveValue(newValue),
            DisplayFunc = x => x is null ? "-" : x.ToString()
        };
        OutsourcingFee.Init(elements);

        TrusteeInfo = new ChangeableViewModel<FundElements, AgencyInfoViewModel>
        {
            Label = "托管机构",
            InitFunc = x => new(x.TrusteeInfo.GetValue(newValue).Value),
            UpdateFunc = (x, y) => { if (y is not null) x.TrusteeInfo!.SetValue(y.Build(), newValue); },
            ClearFunc = x => x.TrusteeInfo.RemoveValue(newValue),
        };
        TrusteeInfo.Init(elements);

        OutsourcingInfo = new ChangeableViewModel<FundElements, AgencyInfoViewModel>
        {
            Label = "外包机构",
            InitFunc = x => new(x.OutsourcingInfo.GetValue(newValue).Value),
            UpdateFunc = (x, y) => { if (y is not null) x.OutsourcingInfo!.SetValue(y.Build(), newValue); },
            ClearFunc = x => x.OutsourcingInfo.RemoveValue(newValue),
        };
        OutsourcingInfo.Init(elements);

        //PerformanceBenchmarks = new(elements, nameof(FundElements.PerformanceBenchmarks), FlowId, "业绩比较基准");
        //PerformanceBenchmarks.DisplayGenerator = (a, b) => a switch { true => b, false => "无", _ => ElementItemViewModel.UnsetValue };


        InvestmentObjective = new ChangeableViewModel<FundElements, string>
        {
            Label = "投资目标",
            InitFunc = x => x.InvestmentObjective.GetValue(newValue).Value,
            UpdateFunc = (x, y) => { if (y is not null) x.InvestmentObjective!.SetValue(y, newValue); },
            ClearFunc = x => x.InvestmentObjective.RemoveValue(newValue),
        };
        InvestmentObjective.Init(elements);


        InvestmentScope = new ChangeableViewModel<FundElements, string>
        {
            Label = "投资范围",
            InitFunc = x => x.InvestmentScope.GetValue(newValue).Value,
            UpdateFunc = (x, y) => { if (y is not null) x.InvestmentScope!.SetValue(y, newValue); },
            ClearFunc = x => x.InvestmentScope.RemoveValue(newValue),
        };
        InvestmentScope.Init(elements);

        InvestmentStrategy = new ChangeableViewModel<FundElements, string>
        {
            Label = "投资策略",
            InitFunc = x => x.InvestmentStrategy.GetValue(newValue).Value,
            UpdateFunc = (x, y) => { if (y is not null) x.InvestmentStrategy!.SetValue(y, newValue); },
            ClearFunc = x => x.InvestmentStrategy.RemoveValue(newValue),
        };
        InvestmentStrategy.Init(elements);

        InvestmentManagers = new ChangeableViewModel<FundElements, string>
        {
            Label = "投资经理",
            InitFunc = x => x.InvestmentManager.GetValue(newValue).Value,
            UpdateFunc = (x, y) => { if (y is not null) x.InvestmentStrategy!.SetValue(y, newValue); },
            ClearFunc = x => x.InvestmentStrategy.RemoveValue(newValue),
        };
        InvestmentManagers.Init(elements);

        InitElementsOfShare(elements);

    }

    private string BankString(BankAccountInfoViewModel? x)
    {
        if (x is null) return "-";

        StringBuilder builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(x.Name))
            builder.Append($"户名：{x.Name}\n");

        if (!string.IsNullOrWhiteSpace(x.Number))
            builder.Append($"账号：{x.Number}\n");

        if (!string.IsNullOrWhiteSpace(x.BankOfDeposit))
            builder.Append($"开户行：{x.BankOfDeposit}\n");

        if (!string.IsNullOrWhiteSpace(x.LargePayNo))
            builder.Append($"大额支付号：{x.LargePayNo}\n");

        if (!string.IsNullOrWhiteSpace(x.SwiftCode))
            builder.Append($"SWIFT：{x.SwiftCode}\n");

        return builder.ToString();
    }

    private void InitElementsOfShare(FundElements elements)
    {
        var shares = elements.ShareClasses!.GetValue(FlowId);
        if (shares.Value is not null)
            PortionElements = new ObservableCollection<ShareElementsViewModel>(shares.Value.Select(x => new ShareElementsViewModel(x.Id, x.Name, elements, FlowId)));
        //else
        //    throw new Exception();//  PortionElements = new ObservableCollection<ShareElementsViewModel>([new ShareElementsViewModel(FundElements.SingleShareKey, elements, FlowId)]);


        //OnlyOneShare = PortionElements.Count == 1;
    }


    //[RelayCommand]
    //public void Modify(ElementItemViewModel s)
    //{
    //    switch (s)
    //    {

    //        case ElementItemViewModelSealing v:
    //            using (var db = DbHelper.Base())
    //            {
    //                var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
    //                v.UpdateEntity(elements, FlowId);

    //                db.GetCollection<FundElements>().Update(elements);

    //            }
    //            v.Apply();
    //            break;


    //        case ElementItemViewModel v:
    //            using (var db = DbHelper.Base())
    //            {
    //                var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
    //                v.UpdateEntity(elements, FlowId);

    //                if (v.Property == nameof(DurationInMonths) && SetupDate != default && DurationInMonths!.Data.New.HasValue)
    //                    elements.ExpirationDate!.SetValue(SetupDate.AddMonths(DurationInMonths!.Data.New.Value).AddDays(-1), FlowId);

    //                db.GetCollection<FundElements>().Update(elements);
    //            }
    //            v.Apply();

    //            if (v.Property == nameof(FundElements.FullName))
    //            {
    //                ShortName!.Data.New = Fund.GetDefaultShortName(FullName!.Data.New);
    //                ShortName.Data.Old = ShortName.Data.New;
    //            }

    //            if (v.Property == nameof(DurationInMonths) && SetupDate != default && DurationInMonths!.Data.New.HasValue)
    //            {
    //                ExpirationDate!.Data.New = SetupDate.AddMonths(DurationInMonths!.Data.New.Value).AddDays(-1);
    //                ExpirationDate.Data.Old = ExpirationDate.Data.New;
    //            }

    //            break;
    //        default:


    //            break;
    //    }

    //}


    //[RelayCommand]
    //public void Delete(ElementItemViewModel s)
    //{
    //    switch (s)
    //    {

    //        case ElementItemViewModelSealing v:
    //            using (var db = DbHelper.Base())
    //            {
    //                var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
    //                v.RemoveValue(elements, FlowId);
    //                db.GetCollection<FundElements>().Update(elements);
    //            }
    //            v.Apply();
    //            break;


    //        case ElementItemViewModel v:
    //            using (var db = DbHelper.Base())
    //            {
    //                var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
    //                v.RemoveValue(elements, FlowId);

    //                db.GetCollection<FundElements>().Update(elements);
    //            }
    //            v.Apply();

    //            break;
    //        default:


    //            break;
    //    }
    //}


    //[RelayCommand]
    //public void Reset(ElementItemViewModel s)
    //{
    //    var ps = s.GetType().GetProperties();
    //    foreach (var p in ps)
    //    {
    //        if (p.PropertyType.IsGenericType && (p.PropertyType.GetGenericTypeDefinition() == typeof(ValueViewModel<>) || p.PropertyType.GetGenericTypeDefinition() == typeof(RefrenceViewModel<>)))
    //        {
    //            var obj = p.GetValue(s);
    //            if (obj is null) continue;

    //            var old = obj.GetType().GetProperty("Old")!.GetValue(obj);
    //            obj.GetType().GetProperty("New")!.SetValue(obj, old);
    //        }
    //    }
    //}


    [RelayCommand]
    public void SetBankFromClipboard(ChangeableViewModel<FundElements, BankAccountInfoViewModel> v)
    {
        try
        {
            var text = Clipboard.GetText();

            if (BankAccount.FromString(text) is BankAccount account)
                v.NewValue = new(account);

        }
        catch { }
    }

    public void Receive(FundShareChangedMessage message)
    {
        if (message.FundId == FundId && message.FlowId <= FlowId)
        {
            using var db = DbHelper.Base();
            var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
            InitElementsOfShare(elements);
        }

        //  OnFlowIdChanged(0, FlowId);
    }

    protected override FundElements InitNewEntity() => throw new NotImplementedException();

    private T? ValueFormat<T>(T d) where T : struct
    {
        return default(T).Equals(d) ? null : d;
    }
    //private DateOnly? ValueFormat(DateOnly d)
    //{
    //    return d == default ? null : d;
    //}
}



