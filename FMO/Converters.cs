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