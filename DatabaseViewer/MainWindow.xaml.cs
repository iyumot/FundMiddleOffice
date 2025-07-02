using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using FMO.Utilities;
using LiteDB;
using System.IO;
using System.Runtime.Loader;
using System.Windows;
using FMO.Trustee;


namespace DatabaseViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        Directory.SetCurrentDirectory("e:\\fmo");
        InitializeComponent();
    }
}





public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        Databases = [DbHelper.Base(), DbHelper.Platform()];

        AssemblyLoadContext.Default.LoadFromAssemblyName(new System.Reflection.AssemblyName("FMO.Trustee"));
    }

    public ILiteDatabase[] Databases { get; set; }


    [ObservableProperty]
    public partial ILiteDatabase? SelectedDatabase { get; set; }


    [ObservableProperty]
    public partial IEnumerable<string>? Tables { get; set; }


    [ObservableProperty]
    public partial string? SelectedTable { get; set; }


    [ObservableProperty]
    public partial object? Data { get; set; }


    partial void OnSelectedDatabaseChanged(ILiteDatabase? value)
    {
        Tables = value?.GetCollectionNames().Where(x => !x.StartsWith("_"));
    }


    partial void OnSelectedTableChanged(string? value)
    {
        var doc = value is null || SelectedDatabase is null ? null : SelectedDatabase.GetCollection(value).FindAll();

        if (doc is null)
        {
            Data = null;
            return;
        }


        if (value?.StartsWith("fv_") ?? false)
        {
            Data = doc!.Select(x => BsonMapper.Global.ToObject<DailyValue>(x));
            return;
        }

        var types = AssemblyLoadContext.Default.Assemblies.Where(x => x.FullName!.Contains("FMO")).SelectMany(x => x.GetTypes());

        if (types.FirstOrDefault(x => x.Name == value) is Type type)
            Data = doc.Select(x => BsonMapper.Global.ToObject(type, x));
        else Data = doc;



    }
}