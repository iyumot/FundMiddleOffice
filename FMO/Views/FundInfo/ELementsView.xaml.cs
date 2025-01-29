using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
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




    [ObservableProperty]
    public partial PortionElementItemWithEnumViewModel<FundFeeType, decimal>? ManageFee { get; set; }

    [SetsRequiredMembers]
    public ShareElementsViewModel(string share, FundElements elements, int flowid)
    {
        Class = share;

        //(var id, var dic) = elements.LockingRule!.GetValue(flowid);

        //LockingRule = new ElementItemViewModelSealing(elements, nameof(FundElements.LockingRule), flowid, "锁定期");

        LockingRule = new PortionElementItemWithEnumViewModel<SealingType, int>(elements, share, nameof(FundElements.LockingRule), flowid, "锁定期");

        ManageFee = new PortionElementItemWithEnumViewModel<FundFeeType, decimal>(elements, share, nameof(FundElements.ManageFee), flowid, "管理费");
    }

}



public partial class ElementsViewModel : ObservableRecipient, IRecipient<FundShareChangedMessage>
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
    public partial ElementRefrenceViewModel<string>? FullName { get; set; }



    [ObservableProperty]
    public partial ElementRefrenceViewModel<string>? ShortName { get; set; }


    [ObservableProperty]
    public partial ElementValueViewModel<RiskLevel> RiskLevel { get; set; }





    [ObservableProperty]
    public partial ElementValueViewModel<int>? DurationInMonths { get; set; }

    [ObservableProperty]
    public partial ElementValueViewModel<DateOnly>? ExpirationDate { get; set; }



    [ObservableProperty]
    public partial ElementRefrenceViewModel<BankAccount>? CollectionAccount { get; set; }



    [ObservableProperty]
    public partial ElementRefrenceViewModel<BankAccount>? CustodyAccount { get; set; }



    [ObservableProperty]
    public partial ElementValueViewModel<decimal>? StopLine { get; set; }



    [ObservableProperty]
    public partial ElementValueViewModel<decimal>? WarningLine { get; set; }




    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSealingFund))]
    public partial /*ElementItemViewModelExtra<FundMode>?*/ ElementItemFundModeViewModel FundModeInfo { get; set; }


    [ObservableProperty]
    public partial ElementItemViewModelSealing? SealingRule { get; set; }


    [ObservableProperty]
    public partial ElementItemViewModelSealing? LockingRule { get; set; }

    [ObservableProperty]
    public partial SealingType[] SealingTypes { get; set; }

    [ObservableProperty]
    public partial bool IsSealingFund { get; set; }



    [ObservableProperty]
    public partial ElementRefrenceViewModel<string>? OpenDayInfo { get; set; }





    [ObservableProperty]
    public partial ElementItemWithEnumViewModel<FundFeeType, decimal>? TrusteeFee { get; set; }



    [ObservableProperty]
    public partial ElementWithBooleanViewModel<decimal>? TrusteeGuaranteedFee { get; set; }



    [ObservableProperty]
    public partial ElementItemWithEnumViewModel<FundFeeType, decimal>? OutsourcingFee { get; set; }


    [ObservableProperty]
    public partial ElementWithBooleanViewModel<decimal>? OutsourcingGuaranteedFee { get; set; }





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
        IsActive = true;

        using var db = new BaseDatabase();
        var fund = db.GetCollection<Fund>().FindById(FundId);
        var flow = db.GetCollection<FundFlow>().FindById(newValue);
        bool isori = flow is ContractFinalizeFlow;
        var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);

        var type = GetType();
        SetupDate = fund.SetupDate;

        FullName = new(elements, nameof(FullName), FlowId, "基金全称");
        ShortName = new(elements, nameof(ShortName), FlowId, "基金简称");

        if (isori)
        {
            if (FullName.Data.New == default)
                FullName.Data.New = fund.Name;

            if (ShortName.Data.New == default)
                ShortName.Data.New = fund.ShortName;


        }


        RiskLevel = new(elements, nameof(FundElements.RiskLevel), FlowId, "风险等级");

        DurationInMonths = new(elements, nameof(FundElements.DurationInMonths), FlowId, "存续期（月）");
        ExpirationDate = new(elements, nameof(FundElements.ExpirationDate), FlowId, "到期日");



        CollectionAccount = new(elements, nameof(FundElements.CollectionAccount), FlowId, "募集账户");
        CustodyAccount = new(elements, nameof(FundElements.CustodyAccount), FlowId, "托管账户");


        WarningLine = new(elements, nameof(FundElements.WarningLine), FlowId, "止损线");
        StopLine = new(elements, nameof(FundElements.StopLine), FlowId, "预警线");

        FundModeInfo = new ElementItemFundModeViewModel(elements, nameof(FundElements.FundModeInfo), FlowId, "运作方式");

        SealingRule = new ElementItemViewModelSealing(elements, nameof(FundElements.SealingRule), FlowId, "封闭期");
        // LockingRule = new ElementItemViewModelSealing(elements, nameof(FundElements.LockingRule), FlowId, "锁定期");


        SealingTypes = FundModeInfo.Data.New switch { Models.FundMode.Close => [], _ => [SealingType.No, SealingType.Has, SealingType.Other], };
        IsSealingFund = FundModeInfo.Data.New == Models.FundMode.Close;


        OpenDayInfo = new(elements, nameof(FundElements.OpenDayInfo), FlowId, "开放日规则");

        TrusteeFee = new(elements, nameof(FundElements.TrusteeFee), FlowId, "托管费");
        TrusteeGuaranteedFee = new(elements, nameof(FundElements.TrusteeGuaranteedFee), FlowId, "托管费保底");
        OutsourcingFee = new(elements, nameof(FundElements.OutsourcingFee), FlowId, "外包费");
        OutsourcingGuaranteedFee = new(elements, nameof(FundElements.OutsourcingGuaranteedFee), FlowId, "外包费保底");


        //////////////////////////////////////////////////////////////////////////////
        var shares = elements.ShareClasses!.GetValue(FlowId);
        if (shares.Value is not null)
            PortionElements = new ObservableCollection<ShareElementsViewModel>(shares.Value.Select(x => new ShareElementsViewModel(x.Name, elements, FlowId)));
        else
            PortionElements = new ObservableCollection<ShareElementsViewModel>([new ShareElementsViewModel(FundElements.SingleShareKey, elements, FlowId)]);


        OnlyOneShare = PortionElements.Count == 1;





    }




    [RelayCommand]
    public void Modify(ElementItemViewModel s)
    {
        switch (s)
        {

            case ElementItemViewModelSealing v:
                using (var db = new BaseDatabase())
                {
                    var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
                    v.UpdateEntity(elements, FlowId);

                    db.GetCollection<FundElements>().Update(elements);

                }
                v.Apply();
                break;


            case ElementItemViewModel v:
                using (var db = new BaseDatabase())
                {
                    var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
                    v.UpdateEntity(elements, FlowId);

                    if (v.Property == nameof(DurationInMonths) && SetupDate != default && DurationInMonths!.Data.New.HasValue)
                        elements.ExpirationDate!.SetValue(SetupDate.AddMonths(DurationInMonths!.Data.New.Value).AddDays(-1), FlowId);

                    db.GetCollection<FundElements>().Update(elements);
                }
                v.Apply();

                if (v.Property == nameof(FundElements.FullName))
                {
                    ShortName!.Data.New = Fund.GetDefaultShortName(FullName!.Data.New);
                    ShortName.Data.Old = ShortName.Data.New;
                }

                if (v.Property == nameof(DurationInMonths) && SetupDate != default && DurationInMonths!.Data.New.HasValue)
                {
                    ExpirationDate!.Data.New = SetupDate.AddMonths(DurationInMonths!.Data.New.Value).AddDays(-1);
                    ExpirationDate.Data.Old = ExpirationDate.Data.New;
                }

                break;
            default:


                break;
        }

    }


    [RelayCommand]
    public void Delete(ElementItemViewModel s)
    {
        switch (s)
        {

            case ElementItemViewModelSealing v:
                using (var db = new BaseDatabase())
                {
                    var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
                    v.RemoveValue(elements, FlowId);
                    db.GetCollection<FundElements>().Update(elements);
                }
                v.Apply();
                break;


            case ElementItemViewModel v:
                using (var db = new BaseDatabase())
                {
                    var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
                    v.RemoveValue(elements, FlowId);

                    db.GetCollection<FundElements>().Update(elements);
                }
                v.Apply();

                break;
            default:


                break;
        }
    }


    public void Receive(FundShareChangedMessage message)
    {
        if (message.FundId == FundId && message.FlowId == FlowId)
            OnFlowIdChanged(0, FlowId);
    }
}