using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

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

        //SubscriptionFee = new ChangeableViewModel<FundElements, FundFeeInfoViewModel>
        //{
        //    Label = "认购费",
        //    InitFunc = x => new(x.SubscriptionFee.GetValue(shareId, flowid).Value),
        //    InheritedFunc = x => x.SubscriptionFee.GetValue(shareId, flowid).FlowId switch { -1 => false, int i => i < flowid },
        //    UpdateFunc = (x, y) => { if (y is not null) x.SubscriptionFee.SetValue(shareId, y.Build(), flowid); },
        //    ClearFunc = x => x.SubscriptionFee.RemoveValue(shareId, flowid),
        //    DisplayFunc = x => x?.HasFee ?? false ? x?.Type switch { FundFeeType.Fix => $"固定{x.Fee}元", FundFeeType.Ratio => $"{x.Fee}%", FundFeeType.Other => x.Other, _ => $"未设置" } + (x?.GuaranteedFee > 0 ? $" 有保底：{x.GuaranteedFee}元" : "") : "无"
        //};
        //SubscriptionFee.Init(elements);

        SubscriptionRule = new ChangeableViewModel<FundElements, FundPurchaseRuleViewModel>
        {
            Label = "认购规则",
            InitFunc = x => new(x.SubscriptionRule.GetValue(shareId, flowid).Value),
            InheritedFunc = x => x.SubscriptionRule.GetValue(shareId, flowid).FlowId switch { -1 => false, int i => i < flowid },
            UpdateFunc = (x, y) => { if (y is not null) x.SubscriptionRule.SetValue(shareId, y.Build(), flowid); },
            ClearFunc = x => x.SubscriptionRule.RemoveValue(shareId, flowid)
        };
        SubscriptionRule.Init(elements);



        //PurchaseFee = new ChangeableViewModel<FundElements, FundFeeInfoViewModel>
        //{
        //    Label = "申购费",
        //    InitFunc = x => new(x.PurchaseFee.GetValue(shareId, flowid).Value),
        //    InheritedFunc = x => x.PurchaseFee.GetValue(shareId, flowid).FlowId switch { -1 => false, int i => i < flowid },
        //    UpdateFunc = (x, y) => { if (y is not null) x.PurchaseFee.SetValue(shareId, y.Build(), flowid); },
        //    ClearFunc = x => x.PurchaseFee.RemoveValue(shareId, flowid),
        //    DisplayFunc = x => x?.HasFee ?? false ? x?.Type switch { FundFeeType.Fix => $"固定{x.Fee}元", FundFeeType.Ratio => $"{x.Fee}%", FundFeeType.Other => x.Other, _ => $"未设置" } + (x?.GuaranteedFee > 0 ? $" 有保底：{x.GuaranteedFee}元" : "") : "无"
        //};
        //PurchaseFee.Init(elements);

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



public partial class ShareElementsViewModel2<TProperty, TViewModel> : ObservableObject where TProperty : notnull
{


    [SetsRequiredMembers]
    public ShareElementsViewModel2(int fundid, int flowId, FundElements elements, ShareClass[] sc, Func<FundElements, PortionMutable<TProperty>> property, Func<TProperty?, TViewModel> o2v, Func<TViewModel?, TProperty> v2o)
    {
        FundId = fundid;
        FlowId = flowId;
        Classes = sc;
        ObjectToViewModel = o2v;
        ViewModelToObject = v2o;
        Property = property;

        var prop = Property(elements);
        var (id, dic) = prop.GetValue(flowId);

        //单一份额
        if (dic is null || dic.Count == 0 || (dic!.Count == 1 && dic.First().Key == -1))
        {
            UnifiedClass = true;
            var u = new ChangeableViewModel<FundElements, TViewModel>
            {
                InitFunc = x => ObjectToViewModel(dic?.Count switch { > 0 => dic!.First().Value, _ => default }),
                UpdateFunc = (x, y) => Property(x).SetValue(-1, ViewModelToObject(y), flowId),
                ClearFunc = x => Property(x).RemoveValue(flowId),
                DisplayFunc = DisplayFunc
            };
            u.Init(elements);
            Data.Add(u);
        }
        else
        {
            UnifiedClass = false;

            foreach (var c in sc)
            {
                var v = prop.GetValue(c.Id, flowId);

                var u = new ChangeableViewModel<FundElements, TViewModel>
                {
                    Label = c.Name,
                    InitFunc = x => ObjectToViewModel(v.Value),
                    UpdateFunc = (x, y) => Property(x).SetValue(-1, ViewModelToObject(y), flowId),
                    ClearFunc = x => Property(x).RemoveValue(flowId),
                    DisplayFunc = DisplayFunc
                };
                u.Init(elements);
                Data.Add(u);
            }

        }
    }


    public ObservableCollection<ChangeableViewModel<FundElements, TViewModel>> Data { get; } = new();

    public int FlowId { get; }

    public required ShareClass[] Classes { get; set; }

    /// <summary>
    /// 单一份额
    /// </summary>
    [ObservableProperty]
    public partial bool UnifiedClass { get; set; }


    public Func<FundElements, int, (int, TProperty)>? InitFunc { get; set; }



    public required Func<FundElements, PortionMutable<TProperty>> Property { get; set; }

    public required Func<TProperty?, TViewModel> ObjectToViewModel { get; set; }

    public required Func<TViewModel?, TProperty> ViewModelToObject { get; set; }

    public Func<TViewModel?, string?>? DisplayFunc { get; set; }
    public int FundId { get; set; }

    [RelayCommand]
    public void Divide()
    {
        UnifiedClass = false;
        var sc = Classes;

        var v = Data[0].OldValue;

        foreach (var c in sc)
        {
            var u = new ChangeableViewModel<FundElements, TViewModel>
            {
                Label = c.Name,
                InitFunc = x => JsonSerializer.Deserialize<TViewModel>(JsonSerializer.Serialize(v)),
                UpdateFunc = (x, y) => Property(x).SetValue(c.Id, ViewModelToObject(y), FlowId),
                ClearFunc = x => Property(x).RemoveValue(FlowId),
                DisplayFunc = DisplayFunc
            };
            u.NewValue = JsonSerializer.Deserialize<TViewModel>(JsonSerializer.Serialize(v));
            u.OldValue = JsonSerializer.Deserialize<TViewModel>(JsonSerializer.Serialize(v));
            Data.Add(u);
        }
        Data.RemoveAt(0);

        ///更新到数据库
        using var db = DbHelper.Base();
        var e = db.GetCollection<FundElements>().FindById(FundId);
        PortionMutable<TProperty> prop = Property(e);
        var (id, dic) = prop.GetValue(FlowId);
        var obj = ViewModelToObject(v);

        foreach (var c in sc)
            prop.SetValue(c.Id, obj, FlowId);
        prop.RemoveValue(-1, FlowId);
        db.GetCollection<FundElements>().Update(e);
    }

    [RelayCommand]
    public void Unify(ChangeableViewModel<FundElements, TViewModel> unit)
    {
        UnifiedClass = true;
        var sc = Classes;

        var v = unit.OldValue;

        var u = new ChangeableViewModel<FundElements, TViewModel>
        {
            InitFunc = x => JsonSerializer.Deserialize<TViewModel>(JsonSerializer.Serialize(v)),
            UpdateFunc = (x, y) => Property(x).SetValue(-1, ViewModelToObject(y), FlowId),
            ClearFunc = x => Property(x).RemoveValue(FlowId),
            DisplayFunc = DisplayFunc
        };
        u.NewValue = v;
        u.OldValue = JsonSerializer.Deserialize<TViewModel>(JsonSerializer.Serialize(v));
        Data.Clear();
        Data.Add(u);
          
        ///更新到数据库
        using var db = DbHelper.Base();
        var e = db.GetCollection<FundElements>().FindById(FundId);
        PortionMutable<TProperty> prop = Property(e);
        var (id, dic) = prop.GetValue(FlowId);
        var obj = ViewModelToObject(v);

        foreach (var c in sc)
            prop.RemoveValue(c.Id, FlowId);
        prop.SetValue(-1, obj, FlowId);
        db.GetCollection<FundElements>().Update(e);
    }
}