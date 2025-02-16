﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// CustomerPage.xaml 的交互逻辑
/// </summary>
public partial class CustomerPage : UserControl
{




    public CustomerPage()
    {
        InitializeComponent();
    }
}

public partial class CustomerPageViewModel : ObservableRecipient
{

    public ObservableCollection<CustomerViewModel> Customers { get; }

    [ObservableProperty]
    public partial CustomerViewModel? Selected { get; set; }


    public CustomerPageViewModel()
    {
        using var db = new BaseDatabase();

        var cusomers = db.GetCollection<Investor>().FindAll().ToArray();

        if (cusomers.Length == 0)
            cusomers = [
                new Investor { Name = "张三", EntityType = EntityType.Natural},
                new Investor { Name = "某公司", EntityType = EntityType.Institution},
                new Investor { Name = "某产品", EntityType = EntityType.Product},
            ];

        Customers = new(cusomers.Select(x => new CustomerViewModel(x)));




    }


}

