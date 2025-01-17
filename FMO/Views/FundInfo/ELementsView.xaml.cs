using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.Reflection;
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

public partial class ElementItemViewModel : ObservableObject
{
    public required string Label { get; set; }


}

public class ExItem<T> where T : Enum
{
    public T? Type { get; set; }

    public string? Extra { get; set; }
}


public partial class ElementItemViewModel<T> : ElementItemViewModel
{

    [ObservableProperty]
    public partial T? OldValue { get; set; }

    [ObservableProperty]
    public partial T? Data { get; set; }


    public required Func<FundElements, Mutable<T>> Property { get; set; }
}


public partial class ElementItemViewModelExtra<T> : ElementItemViewModel where T : struct
{

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    public partial T? OldValue { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    public partial T? Data { get; set; }


    [ObservableProperty]
    public partial string? OldExtra { get; set; }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    public partial string? Extra { get; set; }


    public bool CanConfirm => Data is not null && !Data.Equals(OldValue) && (Data.ToString() != "Other" || !string.IsNullOrWhiteSpace(Extra));

    public required Func<FundElements, Mutable<DataExtra<T>>> Property { get; set; }

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
    public partial ElementItemViewModel<RiskLevel?>? RiskLevel { get; set; }


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
    public partial ElementItemViewModel<FundMode?>? FundMode { get; set; }

    [ObservableProperty]
    public partial ElementItemViewModel<string>? OtherFundMode { get; set; }


    [ObservableProperty]
    public partial ElementItemViewModelExtra<FundMode>? FundModeInfo { get; set; }








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

        (int fid, string? str) = elements.FullName!.GetValue(newValue);
        FullName = new ElementItemViewModel<string>
        {
            Label = elements.FullName!.Description!,
            OldValue = fid == newValue ? str : isori ? null : str,
            Data = string.IsNullOrWhiteSpace(str) ? fund.Name : str,
            Property = x => x.FullName!
        };

        (fid, str) = elements.ShortName!.GetValue(newValue);
        ShortName = new ElementItemViewModel<string>
        {
            Label = elements.ShortName!.Description!,
            OldValue = fid == newValue ? str : isori ? null : str,
            Data = string.IsNullOrWhiteSpace(str) ? fund.ShortName : str,
            Property = x => x.ShortName!
        };


        Fill(type.GetProperty(nameof(RiskLevel))!, elements.RiskLevel!, newValue, isori, x => x.RiskLevel!);


        Fill(type.GetProperty(nameof(DurationInMonths))!, elements.DurationInMonths!, newValue, isori, x => x.DurationInMonths!);
        Fill(type.GetProperty(nameof(ExpirationDate))!, elements.ExpirationDate!, newValue, isori, x => x.ExpirationDate!);


        Fill(type.GetProperty(nameof(CollectionAccount))!, elements.CollectionAccount!, newValue, isori, x => x.CollectionAccount!);
        Fill(type.GetProperty(nameof(CustodyAccount))!, elements.CustodyAccount!, newValue, isori, x => x.CustodyAccount!);

        Fill(type.GetProperty(nameof(WarningLine))!, elements.WarningLine!, newValue, isori, x => x.WarningLine!);
        Fill(type.GetProperty(nameof(StopLine))!, elements.StopLine!, newValue, isori, x => x.StopLine!);


        //Fill(type.GetProperty(nameof(FundMode))!, elements.FundMode!, newValue, isori, x => x.FundMode!);
        //Fill(type.GetProperty(nameof(OtherFundMode))!, elements.OtherFundModel!, newValue, isori, x => x.OtherFundModel!);

        Fill2(type.GetProperty(nameof(FundModeInfo))!, elements.FundModeInfo!, newValue, isori, x => x.FundModeInfo!);

        // Fill2(type.GetProperty(nameof(FundModeInfo))!, elements.FundMode!, elements.OtherFundModel!, newValue, isori, x => x.FundMode!, x=>x.OtherFundModel!);
    }


    private void Fill<T>(PropertyInfo p, Mutable<T> mutable, int flowid, bool isori, Func<FundElements, Mutable<T?>> func)
    {
        (int fid, T? dec) = mutable.GetValue(flowid);

        var val = new ElementItemViewModel<T?>
        {
            Label = mutable.Description!,
            OldValue = fid == flowid ? dec : isori ? default : dec,
            Data = fid == flowid ? dec : default,
            Property = func
        };

        p.SetValue(this, val);
    }
    private void Fill2<T>(PropertyInfo p, Mutable<DataExtra<T>> mutable, int flowid, bool isori, Func<FundElements, Mutable<DataExtra<T>>> func) where T : struct
    {
        (int fid, DataExtra<T>? dec) = mutable.GetValue(flowid);
        var v1 = dec?.Data;
        var v2 = dec?.Extra;

        var val = new ElementItemViewModelExtra<T>
        {
            Label = mutable.Description!,
            OldValue = fid == flowid ? v1 : isori ? null : v1,
            Data = fid == flowid ? v1 : null,
            OldExtra = fid == flowid ? v2 : isori ? null : v2,
            Extra = fid == flowid ? v2 : null,
            Property = func,
        };

        p.SetValue(this, val);
    }



    [RelayCommand]
    public void Modify(ElementItemViewModel s)
    {
        switch (s)
        {
            case ElementItemViewModel<string> v:
                if (v.Data is null) return;

                using (var db = new BaseDatabase())
                {
                    var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
                    var p = v.Property(elements);
                    p.SetValue(v.Data, FlowId);

                    if (p.Name == nameof(FundElements.FullName))
                    {
                        ShortName!.Data = Fund.GetDefaultShortName(v.Data);
                        ShortName.OldValue = ShortName.Data;
                    }
                    //    elements.ShortName?.SetValue(Fund.GetDefaultShortName(v.Data), FlowId);

                    db.GetCollection<FundElements>().Update(elements);
                }
                v.OldValue = v.Data;

                break;

            case ElementItemViewModel<RiskLevel?> v:
                using (var db = new BaseDatabase())
                {
                    var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
                    var p = v.Property(elements);
                    p.SetValue(v.Data, FlowId);
                    db.GetCollection<FundElements>().Update(elements);
                }
                v.OldValue = v.Data;
                break;

            case ElementItemViewModel<int?> v:
                using (var db = new BaseDatabase())
                {
                    var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
                    var p = v.Property(elements);
                    p.SetValue(v.Data, FlowId);

                    if (p.Name == nameof(DurationInMonths) && SetupDate != default && v.Data.HasValue)
                    {
                        ExpirationDate!.Data = SetupDate.AddMonths(v.Data.Value).AddDays(-1);
                        ExpirationDate.OldValue = ExpirationDate.Data;
                    }

                    db.GetCollection<FundElements>().Update(elements);
                }
                v.OldValue = v.Data;
                break;

            case ElementItemViewModel<DateOnly?> v:
                using (var db = new BaseDatabase())
                {
                    var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
                    var p = v.Property(elements);
                    p.SetValue(v.Data, FlowId);

                    db.GetCollection<FundElements>().Update(elements);
                }
                v.OldValue = v.Data;
                break;


            case ElementItemViewModel<BankAccount?> v:
                using (var db = new BaseDatabase())
                {
                    var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
                    var p = v.Property(elements);
                    p.SetValue(v.Data, FlowId);

                    db.GetCollection<FundElements>().Update(elements);
                }
                v.OldValue = v.Data;
                break;

            case ElementItemViewModel<decimal?> v:
                using (var db = new BaseDatabase())
                {
                    var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
                    var p = v.Property(elements);
                    p.SetValue(v.Data, FlowId);

                    db.GetCollection<FundElements>().Update(elements);
                }
                v.OldValue = v.Data;
                break;

            //case ElementItemViewModel<FundMode?> v:
            //    using (var db = new BaseDatabase())
            //    {
            //        var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
            //        var p = v.Property(elements);
            //        p.SetValue(v.Data, FlowId);

            //        if (v.Data != Models.FundMode.Other)
            //            elements.OtherFundModel!.RemoveValue(FlowId);
            //        else
            //            elements.OtherFundModel!.SetValue(OtherFundMode!.Data, FlowId);
            //        db.GetCollection<FundElements>().Update(elements);
            //    }
            //    v.OldValue = v.Data;
            //    break;

            case ElementItemViewModelExtra<FundMode> v:
                using (var db = new BaseDatabase())
                {
                    var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
                    var p = v.Property(elements);

                    var nv = new DataExtra<FundMode> { Data = v.Data, Extra = v.Data.ToString() == "Other" ? v.Extra : null }; 
                    p.SetValue(nv, FlowId);

                    db.GetCollection<FundElements>().Update(elements);
                }
                v.OldValue = v.Data;
                break;
            default:


                break;
        }

    }




}