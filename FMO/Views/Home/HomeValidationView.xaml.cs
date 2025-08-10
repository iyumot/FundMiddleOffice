﻿using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Utilities;
using System.Windows.Controls;
using System.Windows.Data;

namespace FMO;

/// <summary>
/// HomeValidationView.xaml 的交互逻辑
/// </summary>
public partial class HomeValidationView : UserControl
{
    public HomeValidationView()
    {
        InitializeComponent();
    }
}


public partial class HomeValidationViewModel : ObservableObject
{
    public HomeValidationViewModel()
    {
        ClearDateMissing.View.Filter += (o) => o switch { IDataTip d => d.Tags.Contains(nameof(FundClearDateMissingRule)), _ => false };
        DailyMissing.View.Filter += (o) => o switch { IDataTip d => d.Tags.Contains(nameof(FundDailyMissingRule)), _ => false };
        ShareNotPair.View.Filter = (o) => o switch { IDataTip d => d.Tags.Contains(nameof(FundSharePairRule)), _ => false };
        FundOverdue.View.Filter = (o) => o switch { IDataTip d => d.Tags.Contains(nameof(FundOverdueRule)), _ => false };
    }

    /// <summary>
    /// 基金已 清盘，但是未设置清盘日期
    /// </summary>
    public CollectionViewSource ClearDateMissing { get; } = new() { Source = DataObserver.Instance.Tips };

    /// <summary>
    /// 净值日期缺失
    /// </summary>
    public CollectionViewSource DailyMissing { get; } = new() { Source = DataObserver.Instance.Tips };

    /// <summary>
    /// 净值中的份额与TA的份额不一致
    /// </summary>
    public CollectionViewSource ShareNotPair { get; } = new() { Source = DataObserver.Instance.Tips };

    /// <summary>
    /// 净值中的份额与TA的份额不一致
    /// </summary>
    public CollectionViewSource FundOverdue { get; } = new() { Source = DataObserver.Instance.Tips };
}