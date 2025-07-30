using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExcelDataReader;
using FMO.TPL;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.Loader;
using System.Windows;

namespace FMO.TemplateManager;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}


public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        Directory.CreateDirectory("data");
        using var db = DbHelper.Base();
        Templates = [.. db.GetCollection<TemplateInfo>().FindAll()];
    }

    [ObservableProperty]
    public partial ObservableCollection<TemplateInfo> Templates { get; set; }

    [ObservableProperty]
    public partial TemplateInfo? SelectedTemplate { get; set; }


    [ObservableProperty]
    public partial IEnumerable<string>? TplFiles { get; set; }

    [ObservableProperty]
    public partial string? SelectedFileName { get; set; }


    [ObservableProperty]
    public partial DataTable? Sample { get; set; }



    [ObservableProperty]
    public partial bool ShowCustomFileName { get; set; }



    [RelayCommand]
    public void AddTemplate(DragEventArgs args)
    {
        if (args.Data.GetData(DataFormats.FileDrop) is not string[] paths) return;


        foreach (var f in paths)
        {
            if (!f.ToLower().EndsWith(".zip"))
                continue;

            ParseTpl(f);
        }
    }


    [RelayCommand]
    public void Modify()
    {
        if (SelectedTemplate is null) return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(SelectedTemplate.Path, $"{SelectedFileName}.xlsx"),
                UseShellExecute = true
            });
        }
        catch { }
    }


    partial void OnSelectedTemplateChanged(TemplateInfo? value)
    {
        if (value is null)
        {
            TplFiles = null;
            return;
        }

        TplFiles = new DirectoryInfo(value.Path).GetFiles("*.xlsx").Select(x => x.Name[0..^5]);
    }

    partial void OnSelectedFileNameChanged(string? value)
    {
        if (value is null || SelectedTemplate is null)
        {
            Sample = null;
            return;
        }

        using var fs = new FileStream(Path.Combine(SelectedTemplate.Path, $"{value}.xlsx"), FileMode.Open);
        using var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(fs);
        Sample = reader.AsDataSet().Tables[0];
    }









    private void ParseTpl(string f)
    {
        using var fs = new FileStream(f, FileMode.Open);
        using ZipArchive archive = new ZipArchive(fs);

        var entry = archive.GetEntry("tpl.dll");
        if (entry is null)
            return;

        // 解析
        AssemblyLoadContext context = new AssemblyLoadContext(Guid.NewGuid().ToString(), true);

        using var ms = new MemoryStream();
        using var es = entry.Open();
        es.CopyTo(ms);
        ms.Seek(0, SeekOrigin.Begin);

        var assembly = context.LoadFromStream(ms);
        var gen = assembly.DefinedTypes.FirstOrDefault(x => x.IsAssignableTo(typeof(IExporter)));
        if (gen is null) return;

        var obj = Activator.CreateInstance(gen) as IExporter;

        var gid = obj!.Id;
        var di = Directory.CreateDirectory(@$"files\tpl\{gid}");
        archive.ExtractToDirectory(di.FullName, true);

        using var db = DbHelper.Base();
        TemplateInfo entity = new(gid, obj.Name, obj.Description, gen.FullName!, obj.Suit, di.FullName);
        db.GetCollection<TemplateInfo>().Upsert(entity);
        Templates.Add(entity);
        context.Unload();
    }
}
