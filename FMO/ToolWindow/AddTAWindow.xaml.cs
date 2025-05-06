using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using FMO.Utilities;

namespace FMO;

/// <summary>
/// AddTAWindow.xaml 的交互逻辑
/// </summary>
public partial class AddTAWindow : Window
{
    public AddTAWindow()
    {
        InitializeComponent();
    }

    private void ComboBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (sender is ComboBox obj) obj.IsDropDownOpen = true;
    }

    private void ComboBox_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is ComboBox obj) obj.IsDropDownOpen = true;
    }
}


public partial class AddTAWindowViewModel : ObservableObject
{
    public Fund[] Funds { get; set; }

    public CollectionViewSource FundSource { get; } = new();

    [ObservableProperty]
    public partial string? SearchFundKey { get; set; }


    [ObservableProperty]
    public partial Fund? SelectedFund { get; set; }



    public Investor[] Investors { get; set; }

    public CollectionViewSource InvestorSource { get; } = new();

    [ObservableProperty]
    public partial string? SearchInvestorKey { get; set; }


    [ObservableProperty]
    public partial Investor? SelectedInvestor { get; set; }


    public TARecordType[] Types { get; } = [TARecordType.Subscription, TARecordType.Purchase, TARecordType.Redemption, TARecordType.ForceRedemption, TARecordType.Distribution];








    public AddTAWindowViewModel()
    {
        using var db = DbHelper.Base();
        Funds = db.GetCollection<Fund>().FindAll().ToArray();

        FundSource.Source = Funds;
        FundSource.Filter += (s, e) => e.Accepted = string.IsNullOrWhiteSpace(SearchFundKey) ? true : e.Item switch { Fund f => f.Name.Contains(SearchFundKey), _ => true };


        Investors = db.GetCollection<Investor>().FindAll().ToArray();
        InvestorSource.Source = Investors;
        InvestorSource.Filter += (s, e) => e.Accepted = string.IsNullOrWhiteSpace(SearchInvestorKey) ? true : e.Item switch { Fund f => f.Name.Contains(SearchInvestorKey), _ => true };


    }






    partial void OnSearchFundKeyChanged(string? value) => FundSource.View.Refresh();


    partial void OnSearchInvestorKeyChanged(string? value) => InvestorSource.View.Refresh();











}