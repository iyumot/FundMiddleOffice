using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Logging;
using FMO.Models;
using Microsoft.Win32;
using System.IO;
using System.Windows; 

namespace FMO.Shared;










public partial class ReadOnlyFileMetaViewModel : ObservableObject
{

    /// <summary>
    /// "Word Documents|*.doc|Office Files|*.doc;*.xls;*.ppt"
    /// </summary>

     
    public string? Id => Meta?.Id;
     
    public   string? Name => Meta?.Name;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Exists))]
    public partial FileMeta? Meta { get; set; }

    public string? DisplayName => GetShort(Name);

    public bool Exists => Meta?.Exists ?? false;



    private string? GetShort(string? name, int cnt = 20)
    {
        int a = cnt / 3, b = cnt * 2 / 3;
        return name switch { string s => s.Length > a + b ? s[..a] + " ...... " + s[^b..] : s, _ => name };
    }




    [RelayCommand]
    public void View()
    {
        if (string.IsNullOrWhiteSpace(Id)) return;

        try
        {
            Directory.CreateDirectory(@$"temp\{Id}");
            string tmp = @$"temp\{Id}\{Name}";

            if (!File.Exists(tmp))
                FileMeta.CreateHardLink(@$"hardlink\{Id}", tmp);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tmp) { UseShellExecute = true });
        }
        catch (Exception e) { LogEx.Error(e); WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Warning, "无法打开文件")); }
    }


    [RelayCommand]
    public void Copy()
    {
        if (Exists) return;

        try
        {
            Directory.CreateDirectory(@$"temp\{Id}");
            string tmp = @$"temp\{Id}\{Name}";
            if (!File.Exists(tmp))
                FileMeta.CreateHardLink(@$"hardlink\{Id}", tmp);

            tmp = Path.GetFullPath(tmp);
            var obj = new DataObject(DataFormats.FileDrop, new string[] { tmp });
            obj.SetText(tmp);
            Clipboard.SetDataObject(obj);
        }
        catch (Exception e) { LogEx.Error(e); WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Warning, "无法复制文件")); }
    }


    [RelayCommand]
    public void SaveAs()
    {
        if (Exists) return;

        try
        {
            var d = new SaveFileDialog();
            d.FileName = Name!;
            if (d.ShowDialog() == true)
                File.Copy(@$"hardlink\{Id}", d.FileName);
        }
        catch (Exception e) { LogEx.Error(e); WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Warning, "文件另存为失败")); }
    }

    internal void Clear()
    {
        Meta = null;
    }
}



public partial class FileMetaViewModel : ReadOnlyFileMetaViewModel
{
    public string? Filter { get; set; }


    /// <summary>
    /// Extension -> SpecificFileName
    /// </summary>
    public Func<string?, string>? SpecificFileName { get; set; }


    public string? SaveFolder { get; set; }


    public delegate void OnSetFileHandle(FileMeta fileMeta);
    public delegate void OnDeleteFileHandle(string Id);
    public event OnSetFileHandle? OnSetFile;
    public event OnDeleteFileHandle? OnDelete;

    [RelayCommand]
    public void Choose()
    {
        var fd = new OpenFileDialog();
        fd.Filter = Filter;
        if (fd.ShowDialog() != true) return;

        var newf = new FileInfo(fd.FileName);
        var desire = Path.Combine(SaveFolder ?? "files", SpecificFileName is null ? newf.Name : SpecificFileName(newf.Extension));

        Meta = FileMeta.Create(newf, desire);
        OnSetFile?.Invoke(Meta);
    }

    [RelayCommand]
    public void Delete()
    {
        if (Id is null) return;
        OnDelete?.Invoke(Id);
        Clear();
    }
}


public partial class SimpleFileViewModel : FileMetaViewModel
{
    public SimpleFileViewModel(SimpleFile? file = null)
    {
        Meta = file?.File;
    }

    public string? Label { get; set; }


}


public class SealedFileMetaViewModel
{
    public FileMetaViewModel Normal { get; init; } = new();

    public FileMetaViewModel Sealed { get; init; } = new();
}

public partial class SealedFileViewModel : SealedFileMetaViewModel
{
    public string? Label { get; set; }

}



public partial class MultiFileViewModel : ObservableObject
{
    public string? Label { get; set; }


    public List<FileMetaViewModel> Files { get; } = new();

}


public partial class MultiSealedFileViewModel : ObservableObject
{ 
    public MultiSealedFileViewModel(MultiSealedFile x)
    { 
        Label = x.Label;
        if (x.Files is not null)
            Files = [.. x.Files.Select(x => new SealedFileMetaViewModel {
                Normal = new FileMetaViewModel { Meta = x.Normal },
                Sealed = new FileMetaViewModel{ Meta = x.Sealed }
            })];
    }

    public string? Label { get; set; }


    public List<SealedFileMetaViewModel> Files { get; init; } = new();





    [RelayCommand]
    public void AddFile()
    {
 
    }
}


