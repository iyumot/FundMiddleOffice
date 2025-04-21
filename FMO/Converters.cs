using OxyPlot.Axes;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace FMO;


public class FileExistsToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool show = false;
        switch (value)
        {
            case string s:
                show = File.Exists(s);
                break;
            case FileInfo fi:
                show = fi.Exists;
                break;
            default:
                show = false;
                break;
        }

        if (parameter switch { bool r => r, string s => s == "true", _ => false })
            return !show ? Visibility.Visible : Visibility.Collapsed;

        return show ? Visibility.Visible : Visibility.Collapsed;


    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}



public class FilePathToNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string s ? Path.GetFileName(s) : value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


/// <summary>
/// 值有效->可见
/// </summary>
public class ValueIsEffectToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool show = false;
        switch (value)
        {
            case string s:
                show = !string.IsNullOrWhiteSpace(s);
                break;

            case DateOnly d:
                show = d.DayNumber > 719162; // 1970/1/1
                break;

            case DateTime d:
                show = d.Year > 1970; // 1970/1/1
                break;

            default:
                show = value != default;
                break;
        }

        if (parameter switch { bool r => r, string s => s == "true", _ => false })
            return !show ? Visibility.Visible : Visibility.Collapsed;

        return show ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ValueIsSameConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values.Distinct().Count() == 1;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


public class DataTimeDateOnlySwitchConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch { DateTime d => DateOnly.FromDateTime(d), DateOnly d => new DateTime(d, new TimeOnly()), _ => value };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch { DateTime d => DateOnly.FromDateTime(d), DateOnly d => new DateTime(d, new TimeOnly()), _ => value };
    }
}


public class ToDataTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch { DateTime d => d, DateOnly d => new DateTime(d, new TimeOnly()), _ => value };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch { DateTime d => DateOnly.FromDateTime(d), DateOnly d => new DateTime(d, new TimeOnly()), _ => value };
    }
}


public class EnumDescriptionConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum e)
        {
            var fieldInfo = value.GetType().GetField(e.ToString());
            var descriptionAttribute = Attribute.GetCustomAttribute(fieldInfo!, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return descriptionAttribute?.Description ?? value.ToString();
        }
        return value;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        //if (value is Enum e)
        //{
        //    var c = TypeDescriptor.GetConverter(value.GetType());
        //    return c.ConvertTo(value, typeof(string));
        //}
        //else if (value is string s && targetType.IsEnum)
        //{
        //    var c = TypeDescriptor.GetConverter(targetType);
        //    return c.ConvertFrom(value);
        //}
        return value;
    }
}


public class EnumIsOtherToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Enum e && e.ToString() == "Other" ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


public class ZeroToBlankConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch { 0 or 0d or 0L or 0m => "", _ => value };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class LongTextToShortConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int cnt = parameter switch { int n => n, string s => int.TryParse(s, out var n) ? n : 16, _ => 16 };
        int a = cnt / 3, b = cnt * 2 / 3;
        return value switch { string s => s.Length > a + b ? s[..a] + " ...... " + s[^b..] : s, _ => value };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// ta确认函是否存在
/// </summary>
public class TACExistConveter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch { int id => File.Exists(@$"files\tac\{id}.pdf"), _ => value };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


public class OxyDateTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return DateTimeAxis.ToDateTime(doubleValue, new TimeSpan(24,0,0));
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            return DateTimeAxis.ToDouble(dateTime);
        }
        return value;
    }
}