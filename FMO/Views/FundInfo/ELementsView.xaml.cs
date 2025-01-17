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

public partial class ElementItemViewModel : ObservableObject
{
    public required string Label { get; set; }


}


public partial class ElementItemViewModel<T> : ElementItemViewModel
{

    [ObservableProperty]
    public partial T? OldValue { get; set; }

    [ObservableProperty]
    public partial T? Data { get; set; }


    public required Func<FundElements, Mutable<T>> Property { get; set; }
}

public partial class ElementsViewModel : ObservableObject
{
    public static RiskLevel[] RiskLevels { get; } = [Models.RiskLevel.R1, Models.RiskLevel.R2, Models.RiskLevel.R3, Models.RiskLevel.R4, Models.RiskLevel.R5];

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty]
    public partial int FundId { get; set; }


    [ObservableProperty]
    public partial int FlowId { get; set; }


    public DateOnly SetupDate { get; set; }


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
    public partial ObservableCollection<string>? ManagerFee { get; set; }



    partial void OnFlowIdChanged(int oldValue, int newValue)
    {
        using var db = new BaseDatabase();
        var fund = db.GetCollection<Fund>().FindById(FundId);
        var flow = db.GetCollection<FundFlow>().FindById(newValue);
        bool isori = flow is ContractFinalizeFlow;
        var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);

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

        (fid, RiskLevel? rl) = elements.RiskLevel!.GetValue(newValue);
        RiskLevel = new ElementItemViewModel<RiskLevel?>
        {
            Label = elements.RiskLevel!.Description!,
            OldValue = fid == newValue ? rl : isori ? null : rl,
            Data = fid == newValue ? rl : null,
            Property = x => x.RiskLevel!
        };

        (fid, int? iv) = elements.DurationInMonths!.GetValue(newValue);
        DurationInMonths = new ElementItemViewModel<int?>
        {
            Label = elements.DurationInMonths!.Description!,
            OldValue = fid == newValue ? iv : isori ? null : iv,
            Data = fid == newValue ? iv : null,
            Property = x => x.DurationInMonths!
        };

        (fid, DateOnly? date) = elements.ExpirationDate!.GetValue(newValue);
        ExpirationDate = new ElementItemViewModel<DateOnly?>
        {
            Label = elements.ExpirationDate!.Description!,
            OldValue = fid == newValue ? date : isori ? null : date,
            Data = fid == newValue ? date : null,
            Property = x => x.ExpirationDate!
        };
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
            default:
                break;
        }

    }




}