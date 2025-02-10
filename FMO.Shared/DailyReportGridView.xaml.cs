using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FMO.Shared;

/// <summary>
/// DailyReportGridView.xaml 的交互逻辑
/// </summary>
public partial class DailyReportGridView : UserControl
{
    public DailyReportGridView()
    {
        InitializeComponent();

        DataContext = new DailyReportGridViewModel();
    }
}


public partial class DailyReportGridViewModel : ObservableRecipient,IRecipient<FundDailyUpdateMessage>
{

    [ObservableProperty]
    public partial DailyReportItem[]? Data { get; set; }



    public DailyReportGridViewModel()
    {
        IsActive = true;
        Init();

    }

    public void Receive(FundDailyUpdateMessage message)
    {
        if (Data is null) return;

        var item = Data.FirstOrDefault(x => x.Fund?.Id == message.FundId);
        if (item is null) return;

        if (message.Daily.Date < item.Daily?.Date) return;

        using var db = new BaseDatabase();
        var dy = db.GetDailyCollection(item.Fund!.Id).Query().OrderByDescending(x => x.Date).Where(x => x.NetValue > 0 && x.CumNetValue > 0).Limit(5).ToArray();
        if (!dy.Any()) return;

        item.Daily = dy.First();
        item.Shares = item.Daily.Share / 10000;
        item.NetAsset = item.Daily.NetAsset / 10000;

        var td = TradingDay.Current;
        var gapdays = TradingDay.CountBetween(item.Daily.Date, td);
        item.DateBrush = gapdays switch
        {
            > 7 => Brushes.Red,
            > 3 => Brushes.PaleVioletRed,
            _ => Brushes.Black
        };

        item.DateFontWeight = gapdays switch { <= 2 => FontWeights.Normal, _ => FontWeights.Bold };


        if (dy.Length > 1)
            item.ChangeByPrev = (item.Daily!.CumNetValue - dy[1].CumNetValue) * 100;

        var yearfirst = db.GetDailyCollection(item.Fund.Id).Query().OrderBy(x => x.Date).Where(x => x.Date.Year == td.Year).FirstOrDefault();

        if (yearfirst is not null)
            item.ChangeByYear = (item.Daily!.CumNetValue - yearfirst.CumNetValue);
    }

    internal void Init()
    {
        using var db = new BaseDatabase();
        var funds = db.GetCollection<Fund>().Find(x => x.Status == FundStatus.Normal).ToArray();
       

        //最近的交易日
        var td = TradingDay.Current;
        var tmp = funds.Select(x => new DailyReportItem { Fund = x }).ToArray();

        foreach (var item in tmp)
        {
            var dy = db.GetDailyCollection(item.Fund!.Id).Query().OrderByDescending(x => x.Date).Where(x => x.NetValue > 0 && x.CumNetValue > 0).Limit(5).ToArray();
            if (!dy.Any()) continue;

            item.Daily = dy.First();
            item.Shares = item.Daily.Share / 10000;
            item.NetAsset = item.Daily.NetAsset / 10000;

            var gapdays = TradingDay.CountBetween(item.Daily.Date, td);
            item.DateBrush = gapdays switch
            {
                > 7 => Brushes.Red,
                > 3 => Brushes.PaleVioletRed,
                _ => Brushes.Black
            };

            item.DateFontWeight = gapdays switch { <= 2 => FontWeights.Normal, _ => FontWeights.Bold };


            if (dy.Length > 1)
                item.ChangeByPrev = (item.Daily!.CumNetValue - dy[1].CumNetValue) * 100;

            var yearfirst = db.GetDailyCollection(item.Fund.Id).Query().OrderBy(x => x.Date).Where(x => x.Date.Year == td.Year).FirstOrDefault();

            if (yearfirst is not null)
                item.ChangeByYear = (item.Daily!.CumNetValue - yearfirst.CumNetValue);
        }

        Data = tmp;
    }



    public partial class DailyReportItem : ObservableObject
    {
        [ObservableProperty]
        Fund? fund;

        [ObservableProperty]
        DailyValue? daily;

        [ObservableProperty]
        Brush? dateBrush;

        [ObservableProperty]
        FontWeight dateFontWeight;

        [ObservableProperty]
        decimal? shares;

        [ObservableProperty]
        decimal? netAsset;

        [ObservableProperty]
        decimal? changeByPrev;

        [ObservableProperty]
        decimal? changeByYear;

    }

}





public class ValueToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch { decimal v => ValueToBrush(v, parameter), _ => value };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }


    public Brush ValueToBrush(decimal value, object parameter)
    {
        double mod = parameter switch { double v => v, string s => double.TryParse(s, out double d) ? d : 1, _ => 1 };

        return value switch
        {
            > 0 => Mix((double)value * mod, Colors.Blue),
            < 0 => Mix(-(double)value * mod, Colors.Red),
            _ => Brushes.Transparent
        };
    }

    public Brush Mix(double weight, Color color)
    {
        // 确保权重在0到1之间
        weight = Math.Max(0, Math.Min(weight, 1));
        Color color1 = Colors.White;

        // 计算混合后的颜色分量
        int red = (int)((color1.R * (1 - weight)) + (color.R * weight));
        int green = (int)((color1.G * (1 - weight)) + (color.G * weight));
        int blue = (int)((color1.B * (1 - weight)) + (color.B * weight));

        // 确保颜色分量在0到255之间
        red = Math.Max(0, Math.Min(red, 255));
        green = Math.Max(0, Math.Min(green, 255));
        blue = Math.Max(0, Math.Min(blue, 255));

        return new SolidColorBrush(Color.FromRgb((byte)red, (byte)green, (byte)blue));
    }

}

public class ValueToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double mod = parameter switch { double v => v, string s => double.TryParse(s, out double d) ? d : 1, _ => 1 };


        return value switch { decimal v => Math.Abs((double)v * mod) >= 0.5 ? Brushes.White : Brushes.Black, _ => value };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
