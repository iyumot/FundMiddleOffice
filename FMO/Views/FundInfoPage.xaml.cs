using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// FundInfoPage.xaml 的交互逻辑
/// </summary>
public partial class FundInfoPage : UserControl
{
    public FundInfoPage()
    {
        InitializeComponent();
    }

}


public partial class FundInfoPageViewModel : ObservableObject
{
    public Fund Fund { get; init; }

    private bool _initialized;

    public FundInfoPageViewModel(Fund fund)
    {
        this.Fund = fund;

        FundName = fund.Name;
        FundShortName = fund.ShortName;
        SetupDate = fund.SetupDate;
        RegistDate = fund.AuditDate;

        CollectionAccount = 
            """
            fjsdlfjalj
            fsajfl32
            """;


        _initialized = true;
    }


    [ObservableProperty]
    public partial bool IsEditable { get; set; }

    [ObservableProperty]
    public partial string? FundName { get; set; }
     

    [ObservableProperty]
    public partial string? FundShortName { get; set; }



    [ObservableProperty]
    public partial RiskLevel RiskLevel { get; set; }



    [ObservableProperty]
    public partial DateOnly SetupDate { get; set; }



    [ObservableProperty]
    public partial DateOnly RegistDate { get; set; }

    /// <summary>
    /// 投资范围
    /// </summary>
    [ObservableProperty]
    public partial string? InvestmentScope { get; set; }


    /// <summary>
    /// 募集账户
    /// </summary>
    [ObservableProperty]
    public partial string? CollectionAccount { get; set; }








}
