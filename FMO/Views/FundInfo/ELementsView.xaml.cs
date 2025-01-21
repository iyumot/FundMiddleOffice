using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;
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











public partial class ElementsViewModel : ObservableObject
{
    public static RiskLevel[] RiskLevels { get; } = [Models.RiskLevel.R1, Models.RiskLevel.R2, Models.RiskLevel.R3, Models.RiskLevel.R4, Models.RiskLevel.R5];

    public static FundMode[] FundModes { get; } = [Models.FundMode.Open, Models.FundMode.Close, Models.FundMode.Other];


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
    public partial ElementItemViewModel<string>? FullName { get; set; }



    [ObservableProperty]
    public partial ElementItemViewModel<string>? ShortName { get; set; }


    [ObservableProperty]
    public partial ElementItemViewModel<RiskLevel?> RiskLevel { get; set; }





    [ObservableProperty]
    public partial ElementItemViewModel<int?>? DurationInMonths { get; set; }

    [ObservableProperty]
    public partial ElementItemViewModel<DateOnly?>? ExpirationDate { get; set; }



    [ObservableProperty]
    public partial ElementItemViewModel<BankAccount?>? CollectionAccount { get; set; }



    [ObservableProperty]
    public partial ElementItemViewModel<BankAccount?>? CustodyAccount { get; set; }



    [ObservableProperty]
    public partial ElementItemViewModel<decimal?>? StopLine { get; set; }



    [ObservableProperty]
    public partial ElementItemViewModel<decimal?>? WarningLine { get; set; }




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
    public partial ObservableCollection<string>? ManagerFee { get; set; }


    #endregion

    partial void OnFlowIdChanged(int oldValue, int newValue)
    {
        using var db = new BaseDatabase();
        var fund = db.GetCollection<Fund>().FindById(FundId);
        var flow = db.GetCollection<FundFlow>().FindById(newValue);
        bool isori = flow is ContractFinalizeFlow;
        var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);

        var type = GetType();
        SetupDate = fund.SetupDate;

        FullName = new ElementItemViewModel<string>(elements, nameof(FullName), FlowId);
        ShortName = new ElementItemViewModel<string>(elements, nameof(ShortName), FlowId);

        if (isori)
        {
            if (FullName.Data.New == default)
                FullName.Data.New = fund.Name;

            if (ShortName.Data.New == default)
                ShortName.Data.New = fund.ShortName;


        }


        RiskLevel = new ElementItemViewModel<RiskLevel?>(elements, nameof(FundElements.RiskLevel), FlowId);

        DurationInMonths = new ElementItemViewModel<int?>(elements, nameof(FundElements.DurationInMonths), FlowId);
        ExpirationDate = new ElementItemViewModel<DateOnly?>(elements, nameof(FundElements.ExpirationDate), FlowId);



        CollectionAccount = new ElementItemViewModel<BankAccount?>(elements, nameof(FundElements.CollectionAccount), FlowId);
        CustodyAccount = new ElementItemViewModel<BankAccount?>(elements, nameof(FundElements.CustodyAccount), FlowId);


        WarningLine = new ElementItemViewModel<decimal?>(elements, nameof(FundElements.WarningLine), FlowId);
        StopLine = new ElementItemViewModel<decimal?>(elements, nameof(FundElements.StopLine), FlowId);

        FundModeInfo = new ElementItemFundModeViewModel(elements, nameof(FundElements.FundModeInfo), FlowId);

        SealingRule = new ElementItemViewModelSealing(elements, nameof(FundElements.SealingRule), FlowId);
        LockingRule = new ElementItemViewModelSealing(elements, nameof(FundElements.LockingRule), FlowId);


        SealingTypes = FundModeInfo.Data.New switch { Models.FundMode.Close => [], _ => [SealingType.No, SealingType.Has, SealingType.Other], };
        IsSealingFund = FundModeInfo.Data.New == Models.FundMode.Close;
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




}