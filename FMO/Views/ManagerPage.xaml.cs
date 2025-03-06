using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FMO;

/// <summary>
/// ManagerPage.xaml 的交互逻辑
/// </summary>
public partial class ManagerPage : UserControl
{
    public ManagerPage()
    {
        InitializeComponent();
    }

    private void Grid_Drop(object sender, System.Windows.DragEventArgs e)
    {
        if (DataContext is ManagerPageViewModel vm && e.Data.GetData(DataFormats.FileDrop) is string[] s)
        {
            vm.SetLogo(s[0]);
        }
    }
}

public interface IDatabaseUpdater
{
    void Update();
}




public partial class ReadOnlyDataItem<T> : ObservableObject
{
    public required string Label { get; set; }


    [ObservableProperty]
    //[NotifyPropertyChangedFor("IsChanged")]
    public partial T? NewValue { get; set; }


    public string? Format { get; set; }

}


public abstract partial class DataItem<T, TEntity> : ReadOnlyDataItem<T>
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsChanged))]
    public partial T? OldValue { get; set; }

    public bool IsChanged => NewValue is not null && (NewValue is string s ? !string.IsNullOrWhiteSpace(s) : true) && !NewValue.Equals(OldValue);

    public required Action<T?, TEntity> Updater { get; set; }

    public abstract void Update();


    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(NewValue))
            OnPropertyChanged(nameof(IsChanged));
    }
}

public partial class ManagerDataItem<T> : DataItem<T, Manager>, IDatabaseUpdater
{

    public override void Update()
    {
        using var db = new BaseDatabase();
        var manager = db.GetCollection<Manager>().FindOne(x => x.IsMaster);
        Updater(NewValue, manager);
        db.GetCollection<Manager>().Update(manager);
        OldValue = NewValue;
    }
}

public partial class ManagerDateExItem : DataItem<DateOnly?, Manager>, IDatabaseUpdater
{

    /// <summary>
    /// 无固定期限
    /// </summary>
    [ObservableProperty]
    public partial bool? IsLongTerm { get; set; }

    public override void Update()
    {
        using var db = new BaseDatabase();
        var manager = db.GetCollection<Manager>().FindOne(x => x.IsMaster);
        if (IsLongTerm ?? false)
            Updater(DateOnly.MaxValue, manager);
        else
            Updater(NewValue, manager);
        db.GetCollection<Manager>().Update(manager);
        OldValue = NewValue;
    }


    partial void OnIsLongTermChanged(bool? oldValue, bool? newValue)
    {
        if (oldValue is not null)
            Update();
    }
}



public partial class ManagerPageViewModel : ObservableObject
{
    private int FilesId;

    public string AmacPageUrl { get; set; }

    /// <summary>
    /// 管理人名称
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<string> ManagerName { get; set; }

    /// <summary>
    /// 实控人
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<string> ArtificialPerson { get; set; }


    public ReadOnlyDataItem<string> RegisterNo { get; set; }

    /// <summary>
    /// 注册资本
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<decimal> RegisterCapital { get; set; }

    /// <summary>
    /// 实缴
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<decimal?> RealCapital { get; set; }


    [ObservableProperty]
    public partial ManagerDataItem<DateOnly> SetupDate { get; set; }

    [ObservableProperty]
    public partial ManagerDateExItem ExpireDate { get; set; }




    [ObservableProperty]
    public partial ManagerDataItem<DateOnly> RegisterDate { get; set; }


    /// <summary>
    /// 电话
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<string> Telephone { get; set; }

    /// <summary>
    /// 传真
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<string> Fax { get; set; }

    /// <summary>
    /// 统一信用代码
    /// </summary> 
    [ObservableProperty]
    public partial ReadOnlyDataItem<string> InstitutionCode { get; set; }

    /// <summary>
    /// 注册地址
    /// </summary>
    [ObservableProperty]
    public partial ReadOnlyDataItem<string> RegisterAddress { get; set; }



    /// <summary>
    /// 办公地址
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<string?> OfficeAddress { get; set; }


    /// <summary>
    /// 经营范围
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<string> BusinessScope { get; set; }


    /// <summary>
    /// 官网
    /// </summary>
    [ObservableProperty]
    public partial ManagerDataItem<string> WebSite { get; set; }

    [ObservableProperty]
    public partial ImageSource? MainLogo { get; set; }



    /// <summary>
    /// 营业执照正本
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<FileInfo>? BusinessLicense { get; set; }

    /// <summary>
    /// 营业执照副本
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<FileInfo>? BusinessLicense2 { get; set; }

    /// <summary>
    /// 开户许可证
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<FileInfo>? AccountOpeningLicense { get; set; }


    /// <summary>
    /// 章程
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<FileInfo>? CharterDocument { get; set; }

    /// <summary>
    /// 法人身份证
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<FileInfo>? LegalPersonIdCard { get; set; }



    [ObservableProperty]
    public partial bool IsReadOnly { get; set; } = true;


    public ManagerPageViewModel()
    {
        var db = new BaseDatabase();
        var manager = db.GetCollection<Manager>().FindOne(x => x.IsMaster);
        var mfile = db.GetCollection<InstitutionFiles>().FindOne(x => x.InstitutionId == manager.Id);
        if (mfile is null)
        {
            mfile = new InstitutionFiles { InstitutionId = manager.Id };
            db.GetCollection<InstitutionFiles>().Insert(mfile);
        }
        FilesId = mfile.Id;


        if (db.FileStorage.Exists("icon.main"))
        {
            using var ms = new MemoryStream();
            db.FileStorage.Download("icon.main", ms);
            ms.Seek(0, SeekOrigin.Begin);
            BitmapImage bitmapSource = new BitmapImage();
            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.OnLoad;
            bitmapSource.StreamSource = ms;
            bitmapSource.EndInit();
            MainLogo = bitmapSource;
        }
        db.Dispose();

        AmacPageUrl = $"https://gs.amac.org.cn/amac-infodisc/res/pof/manager/{manager.AmacId}.html";

        ManagerName = new ManagerDataItem<string> { Label = "管理人", OldValue = manager.Name, NewValue = manager.Name, Updater = (a, b) => b.Name = a! };

        ArtificialPerson = new ManagerDataItem<string> { Label = "实控人", OldValue = manager.ArtificialPerson, NewValue = manager.ArtificialPerson, Updater = (a, b) => b.ArtificialPerson = a! };

        RegisterNo = new ReadOnlyDataItem<string> { Label = "编码", NewValue = manager.RegisterNo };//new ManagerDataItem<string> { Label = "实控人", OldValue = manager.RegisterNo, NewValue = manager.RegisterNo, Updater = (a, b) => b.RegisterNo = a! };



        RegisterCapital = new ManagerDataItem<decimal> { Label = "注册资本", OldValue = manager.RegisterCapital, NewValue = manager.RegisterCapital, Format = "{0}万元", Updater = (a, b) => b.RegisterCapital = a! };
        RealCapital = new ManagerDataItem<decimal?> { Label = "实缴资本", OldValue = manager.RealCapital, NewValue = manager.RealCapital, Format = "{0}万元", Updater = (a, b) => b.RegisterCapital = a ?? default };


        SetupDate = new ManagerDataItem<DateOnly> { Label = "成立日期", OldValue = manager.SetupDate, NewValue = manager.SetupDate, Format = "yyyy-MM-dd", Updater = (a, b) => b.SetupDate = a };
        DateOnly? ed = manager.ExpireDate == default || manager.ExpireDate == DateOnly.MaxValue ? null : manager.ExpireDate;
        ExpireDate = new ManagerDateExItem { Label = "核销日期", OldValue = ed, NewValue = ed, IsLongTerm = manager.ExpireDate == DateOnly.MaxValue, Format = "yyyy-MM-dd", Updater = (a, b) => b.ExpireDate = a ?? default };
        RegisterDate = new ManagerDataItem<DateOnly> { Label = "登记日期", OldValue = manager.RegisterDate, NewValue = manager.RegisterDate, Format = "yyyy-MM-dd", Updater = (a, b) => b.RegisterDate = a };

        Telephone = new ManagerDataItem<string> { Label = "固定电话", OldValue = manager.Telephone, NewValue = manager.Telephone, Updater = (a, b) => b.Telephone = a };
        Fax = new ManagerDataItem<string> { Label = "传真", OldValue = manager.Fax, NewValue = manager.Fax, Updater = (a, b) => b.Fax = a };

        InstitutionCode = new ReadOnlyDataItem<string> { Label = "统一信用代码", NewValue = manager.Id, };
        RegisterAddress = new ReadOnlyDataItem<string> { Label = "注册地址", NewValue = manager.RegisterAddress, };
        OfficeAddress = new ManagerDataItem<string?> { Label = "办公地址", OldValue = manager.OfficeAddress, NewValue = manager.OfficeAddress, Updater = (a, b) => b.OfficeAddress = a };
        BusinessScope = new ManagerDataItem<string> { Label = "经营范围", OldValue = manager.BusinessScope, NewValue = manager.BusinessScope, Updater = (a, b) => b.BusinessScope = a };

        WebSite = new ManagerDataItem<string> { Label = "官网", OldValue = manager.WebSite, NewValue = manager.WebSite, Updater = (a, b) => b.WebSite = a };



        BusinessLicense = mfile?.BusinessLicense is null ? new() : new ObservableCollection<FileInfo>(mfile.BusinessLicense.Files.Select(x => new FileInfo(x.Path)));
        BusinessLicense.CollectionChanged += BusinessLicense_CollectionChanged;


        BusinessLicense2 = mfile?.BusinessLicense2 is null ? new() : new ObservableCollection<FileInfo>(mfile.BusinessLicense2.Files.Select(x => new FileInfo(x.Path)));
        BusinessLicense2.CollectionChanged += BusinessLicense2_CollectionChanged;


        AccountOpeningLicense = mfile?.AccountOpeningLicense is null ? new() : new ObservableCollection<FileInfo>(mfile.AccountOpeningLicense.Files.Select(x => new FileInfo(x.Path)));
        AccountOpeningLicense.CollectionChanged += AccountOpeningLicense_CollectionChanged;

        CharterDocument = mfile?.CharterDocument is null ? new() : new ObservableCollection<FileInfo>(mfile.CharterDocument.Files.Select(x => new FileInfo(x.Path)));
        CharterDocument.CollectionChanged += CharterDocument_CollectionChanged;


        LegalPersonIdCard = mfile?.LegalPersonIdCard is null ? new() : new ObservableCollection<FileInfo>(mfile.LegalPersonIdCard.Files.Select(x => new FileInfo(x.Path)));
        LegalPersonIdCard.CollectionChanged += LegalPersonIdCard_CollectionChanged;
    }


    [RelayCommand]
    public void UpdateManagerInfo(IDatabaseUpdater dataItem)
    {
        dataItem.Update();
    }

    [RelayCommand]
    public void OpenLink(ManagerDataItem<string> obj)
    {
        if (!string.IsNullOrWhiteSpace(obj.NewValue))
            try { Process.Start(new ProcessStartInfo(obj.NewValue) { UseShellExecute = true }); } catch { }
    }

    [RelayCommand]
    public void OpenAmacPage()
    {
        try { Process.Start(new ProcessStartInfo(AmacPageUrl) { UseShellExecute = true }); } catch { }
    }


    #region MyRegion

    private void BusinessLicense_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems is not null)
        {
            using var db = new BaseDatabase();

            foreach (FileInfo item in e.NewItems)
            {
                string hash = item.ComputeHash()!;

                // 保存副本
                var dir = Directory.CreateDirectory("Manager");
                var tar = FileHelper.CopyFile(item, dir.FullName);

                FileVersion fileVersion = new FileVersion { Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path), Hash = hash, Time = item.LastWriteTime };

                var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);
                if (files.BusinessLicense is null)
                    files.BusinessLicense = new VersionedFileInfo { Name = nameof(BusinessLicense), Files = new() };
                files!.BusinessLicense.Files.Add(fileVersion);
                db.GetCollection<InstitutionFiles>().Update(files);
            }
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems is not null)
        {
            using var db = new BaseDatabase();
            var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);


            foreach (FileInfo item in e.OldItems)
            {
                var rp = Path.GetRelativePath(Directory.GetCurrentDirectory(), item.FullName);
                var file = files!.BusinessLicense?.Files.FirstOrDefault(x => rp == x.Path || x.Path == item.FullName);
                if (file is not null)
                    files.BusinessLicense!.Files.Remove(file);
            }

            db.GetCollection<InstitutionFiles>().Update(files!);
        }

    }
    private void BusinessLicense2_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems is not null)
        {
            using var db = new BaseDatabase();

            foreach (FileInfo item in e.NewItems)
            {
                string hash = item.ComputeHash()!;

                // 保存副本
                var dir = Directory.CreateDirectory("Manager");
                var tar = FileHelper.CopyFile(item, dir.FullName);

                FileVersion fileVersion = new FileVersion { Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path), Hash = hash, Time = item.LastWriteTime };

                var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);
                if (files.BusinessLicense2 is null)
                    files.BusinessLicense2 = new VersionedFileInfo { Name = nameof(BusinessLicense2), Files = new() };
                files!.BusinessLicense2.Files.Add(fileVersion);
                db.GetCollection<InstitutionFiles>().Update(files);
            }
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems is not null)
        {
            using var db = new BaseDatabase();
            var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);


            foreach (FileInfo item in e.OldItems)
            {
                var rp = Path.GetRelativePath(Directory.GetCurrentDirectory(), item.FullName);
                var file = files!.BusinessLicense2?.Files.FirstOrDefault(x => rp == x.Path || x.Path == item.FullName);
                if (file is not null)
                    files.BusinessLicense2!.Files.Remove(file);
            }

            db.GetCollection<InstitutionFiles>().Update(files!);
        }

    }
    private void AccountOpeningLicense_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems is not null)
        {
            using var db = new BaseDatabase();

            foreach (FileInfo item in e.NewItems)
            {
                string hash = item.ComputeHash()!;

                // 保存副本
                var dir = Directory.CreateDirectory("Manager");
                var tar = FileHelper.CopyFile(item, dir.FullName);

                FileVersion fileVersion = new FileVersion { Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path), Hash = hash, Time = item.LastWriteTime };

                var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);
                if (files.AccountOpeningLicense is null)
                    files.AccountOpeningLicense = new VersionedFileInfo { Name = nameof(AccountOpeningLicense), Files = new() };
                files!.AccountOpeningLicense.Files.Add(fileVersion);
                db.GetCollection<InstitutionFiles>().Update(files);
            }
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems is not null)
        {
            using var db = new BaseDatabase();
            var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);


            foreach (FileInfo item in e.OldItems)
            {
                var rp = Path.GetRelativePath(Directory.GetCurrentDirectory(), item.FullName);
                var file = files!.AccountOpeningLicense?.Files.FirstOrDefault(x => rp == x.Path || x.Path == item.FullName);
                if (file is not null)
                    files.AccountOpeningLicense!.Files.Remove(file);
            }

            db.GetCollection<InstitutionFiles>().Update(files!);
        }

    }
    private void CharterDocument_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems is not null)
        {
            using var db = new BaseDatabase();

            foreach (FileInfo item in e.NewItems)
            {
                string hash = item.ComputeHash()!;

                // 保存副本
                var dir = Directory.CreateDirectory("Manager");
                var tar = FileHelper.CopyFile(item, dir.FullName);

                FileVersion fileVersion = new FileVersion { Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path), Hash = hash, Time = item.LastWriteTime };

                var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);
                if (files.CharterDocument is null)
                    files.CharterDocument = new VersionedFileInfo { Name = nameof(CharterDocument), Files = new() };
                files!.CharterDocument.Files.Add(fileVersion);
                db.GetCollection<InstitutionFiles>().Update(files);
            }
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems is not null)
        {
            using var db = new BaseDatabase();
            var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);


            foreach (FileInfo item in e.OldItems)
            {
                var rp = Path.GetRelativePath(Directory.GetCurrentDirectory(), item.FullName);
                var file = files!.CharterDocument?.Files.FirstOrDefault(x => rp == x.Path || x.Path == item.FullName);
                if (file is not null)
                    files.CharterDocument!.Files.Remove(file);
            }

            db.GetCollection<InstitutionFiles>().Update(files!);
        }

    }
    private void LegalPersonIdCard_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems is not null)
        {
            using var db = new BaseDatabase();

            foreach (FileInfo item in e.NewItems)
            {
                string hash = item.ComputeHash()!;

                // 保存副本
                var dir = Directory.CreateDirectory("Manager");
                var tar = FileHelper.CopyFile(item, dir.FullName);

                FileVersion fileVersion = new FileVersion { Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path), Hash = hash, Time = item.LastWriteTime };

                var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);
                if (files.LegalPersonIdCard is null)
                    files.LegalPersonIdCard = new VersionedFileInfo { Name = nameof(LegalPersonIdCard), Files = new() };
                files!.LegalPersonIdCard.Files.Add(fileVersion);
                db.GetCollection<InstitutionFiles>().Update(files);
            }
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems is not null)
        {
            using var db = new BaseDatabase();
            var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);


            foreach (FileInfo item in e.OldItems)
            {
                var rp = Path.GetRelativePath(Directory.GetCurrentDirectory(), item.FullName);
                var file = files!.LegalPersonIdCard?.Files.FirstOrDefault(x => rp == x.Path || x.Path == item.FullName);
                if (file is not null)
                    files.LegalPersonIdCard!.Files.Remove(file);
            }

            db.GetCollection<InstitutionFiles>().Update(files!);
        }

    }

    internal void SetLogo(string v)
    {
        var db = new BaseDatabase();
        db.FileStorage.Upload("icon.main", v);
        if (db.FileStorage.Exists("icon.main"))
        {
            using var ms = new MemoryStream();
            db.FileStorage.Download("icon.main", ms);
            BitmapImage bitmapSource = new BitmapImage();
            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.OnLoad;
            bitmapSource.StreamSource = ms;
            bitmapSource.EndInit();
            MainLogo = bitmapSource;

            App.Current.MainWindow.Icon = MainLogo;
        }
    }
    #endregion
}
