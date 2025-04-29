using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;

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


public partial class ManagerPageViewModel : EditableControlViewModelBase<Manager>
{
    private int FilesId;

    public string AmacPageUrl { get; set; }

    /// <summary>
    /// 管理人名称
    /// </summary>  
    public ChangeableViewModel<Manager, string> ManagerName { get; }

    /// <summary>
    /// 实控人
    /// </summary>
    public ChangeableViewModel<Manager, string> ArtificialPerson { get; }


    public ChangeableViewModel<Manager, string> RegisterNo { get; }

    /// <summary>
    /// 注册资本
    /// </summary>
    public ChangeableViewModel<Manager, decimal?> RegisterCapital { get; }

    /// <summary>
    /// 实缴
    /// </summary>
    public ChangeableViewModel<Manager, decimal?> RealCapital { get; }

    public ChangeableViewModel<Manager, DateTime?> SetupDate { get; }

    public ChangeableViewModel<Manager, BooleanDate?> ExpireDate { get; }



    public ChangeableViewModel<Manager, DateTime?> RegisterDate { get; }


    /// <summary>
    /// 电话
    /// </summary> 
    public ChangeableViewModel<Manager, string> Telephone { get; }

    /// <summary>
    /// 传真
    /// </summary> 
    public ChangeableViewModel<Manager, string> Fax { get; }

    /// <summary>
    /// 统一信用代码
    /// </summary>  
    public ChangeableViewModel<Manager, string> InstitutionCode { get; }

    /// <summary>
    /// 注册地址
    /// </summary> 
    public ChangeableViewModel<Manager, string> RegisterAddress { get; }



    /// <summary>
    /// 办公地址
    /// </summary> 
    public ChangeableViewModel<Manager, string> OfficeAddress { get; }


    /// <summary>
    /// 经营范围
    /// </summary> 
    public ChangeableViewModel<Manager, string> BusinessScope { get; }


    /// <summary>
    /// 官网
    /// </summary> 
    public ChangeableViewModel<Manager, string> WebSite { get; }

    [ObservableProperty]
    public partial ImageSource? MainLogo { get; set; }


    /// <summary>
    /// 法人
    /// </summary> 
    public ChangeableViewModel<Manager, PersonViewModel> LegalPerson { get; }

    /// <summary>
    /// 实控人
    /// </summary>
    public ChangeableViewModel<Manager, PersonViewModel> ActualController { get; }

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





    public ManagerPageViewModel()
    {
        var db = DbHelper.Base();
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

        ManagerName = new ChangeableViewModel<Manager, string>
        {
            Label = "管理人",
            InitFunc = x => x.Name,
            UpdateFunc = (x, y) => x.Name = y ?? "",
            ClearFunc = x => x.Name = string.Empty,
        };
        ManagerName.Init(manager);

        ArtificialPerson = new ChangeableViewModel<Manager, string>
        {
            Label = "实控人",
            InitFunc = x => x.ArtificialPerson,
            UpdateFunc = (x, y) => x.ArtificialPerson = y,
            ClearFunc = x => x.ArtificialPerson = null,
        };
        ArtificialPerson.Init(manager);

        LegalPerson = new()
        {
            Label = "法人代表/委派代表",
            InitFunc = x => new(x.LegalAgent),
            UpdateFunc = (x, y) => x.LegalAgent = y!.Build(),
            ClearFunc = x => x.LegalAgent = null, 
        };
        LegalPerson.Init(manager);

        ActualController = new()
        {
            Label = "实控人",
            InitFunc = x => new(x.ActualController),
            UpdateFunc = (x, y) => x.ActualController = y!.Build(),
            ClearFunc = x => x.ActualController = null,
        };
        ActualController.Init(manager);

        RegisterNo = new ChangeableViewModel<Manager, string>
        {
            Label = "编码",
            InitFunc = x => x.RegisterNo,
            UpdateFunc = (x, y) => throw new Exception(),
            ClearFunc = x => throw new Exception(),
        };
        RegisterNo.Init(manager);


        RegisterCapital = new ChangeableViewModel<Manager, decimal?>
        {
            Label = "注册资本",
            InitFunc = x => x.RegisterCapital,
            UpdateFunc = (x, y) => x.RegisterCapital = y ?? 0,
            ClearFunc = x => x.RegisterCapital = 0,
            DisplayFunc = x => $"{x}万元"
        };
        RegisterCapital.Init(manager);

        RealCapital = new ChangeableViewModel<Manager, decimal?>
        {
            Label = "实缴资本",
            InitFunc = x => x.RealCapital,
            UpdateFunc = (x, y) => x.RealCapital = y ?? 0,
            ClearFunc = x => x.RealCapital = 0,
            DisplayFunc = x => $"{x}万元"
        };
        RealCapital.Init(manager);

        SetupDate = new ChangeableViewModel<Manager, DateTime?>
        {
            Label = "成立日期",
            InitFunc = x => new DateTime(x.SetupDate, default),
            UpdateFunc = (x, y) => x.SetupDate = y is null ? default : DateOnly.FromDateTime(y.Value),
            ClearFunc = x => x.SetupDate = default,
            DisplayFunc = x => x?.ToString("yyyy-MM-dd")
        };
        SetupDate.Init(manager);


        ExpireDate = new ChangeableViewModel<Manager, BooleanDate?>
        {
            Label = "核销日期",
            InitFunc = x => new BooleanDate { IsLongTerm = x.ExpireDate == DateOnly.MaxValue, Date = x.ExpireDate == default || x.ExpireDate == DateOnly.MaxValue ? null : new DateTime(x.ExpireDate, default) },
            UpdateFunc = (x, y) => x.ExpireDate = y is null || y.Date is null ? default : (y.IsLongTerm ? DateOnly.MaxValue : DateOnly.FromDateTime(y.Date.Value)),
            ClearFunc = x => x.ExpireDate = default,
            DisplayFunc = x => x.IsLongTerm ? "长期" : x?.Date?.ToString("yyyy-MM-dd")
        };
        ExpireDate.Init(manager);

        RegisterDate = new ChangeableViewModel<Manager, DateTime?>
        {
            Label = "登记日期",
            InitFunc = x => new DateTime(x.RegisterDate, default),
            UpdateFunc = (x, y) => x.RegisterDate = y is null ? default : DateOnly.FromDateTime(y.Value),
            ClearFunc = x => x.RegisterDate = default,
            DisplayFunc = x => x?.ToString("yyyy-MM-dd")
        };
        RegisterDate.Init(manager);


        Telephone = new ChangeableViewModel<Manager, string>
        {
            Label = "固定电话",
            InitFunc = x => x.Telephone,
            UpdateFunc = (x, y) => x.Telephone = y,
            ClearFunc = x => x.Telephone = null,
        };
        Telephone.Init(manager);

        Fax = new ChangeableViewModel<Manager, string>
        {
            Label = "传真",
            InitFunc = x => x.Fax,
            UpdateFunc = (x, y) => x.Fax = y,
            ClearFunc = x => x.Fax = null,
        };
        Fax.Init(manager);



        InstitutionCode = new ChangeableViewModel<Manager, string>
        {
            Label = "统一信用代码",
            InitFunc = x => x.Id,
            UpdateFunc = (x, y) => throw new Exception(),
            ClearFunc = x => throw new Exception(),
        };
        InstitutionCode.Init(manager);



        RegisterAddress = new ChangeableViewModel<Manager, string>
        {
            Label = "注册地址",
            InitFunc = x => x.RegisterAddress,
            UpdateFunc = (x, y) => x.RegisterAddress = y,
            ClearFunc = x => x.RegisterAddress = null,
        };
        RegisterAddress.Init(manager);



        OfficeAddress = new ChangeableViewModel<Manager, string>
        {
            Label = "办公地址",
            InitFunc = x => x.OfficeAddress,
            UpdateFunc = (x, y) => x.OfficeAddress = y,
            ClearFunc = x => x.OfficeAddress = null,
        };
        OfficeAddress.Init(manager);



        BusinessScope = new ChangeableViewModel<Manager, string>
        {
            Label = "经营范围",
            InitFunc = x => x.BusinessScope,
            UpdateFunc = (x, y) => x.BusinessScope = y,
            ClearFunc = x => x.BusinessScope = null,
        };
        BusinessScope.Init(manager);

        WebSite = new ChangeableViewModel<Manager, string>
        {
            Label = "官网",
            InitFunc = x => x.WebSite,
            UpdateFunc = (x, y) => x.WebSite = y,
            ClearFunc = x => x.WebSite = null,
        };
        WebSite.Init(manager);





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


    public override Manager EntityOverride(LiteDB.ILiteDatabase db)
    {
        return db.GetCollection<Manager>().FindOne(x => x.IsMaster);
    }

    [RelayCommand]
    public void OpenLink()
    {
        if (!string.IsNullOrWhiteSpace(WebSite.OldValue))
            try { Process.Start(new ProcessStartInfo(WebSite.OldValue) { UseShellExecute = true }); } catch { }
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
            using var db = DbHelper.Base();

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
            using var db = DbHelper.Base();
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
            using var db = DbHelper.Base();

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
            using var db = DbHelper.Base();
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
            using var db = DbHelper.Base();

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
            using var db = DbHelper.Base();
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
            using var db = DbHelper.Base();

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
            using var db = DbHelper.Base();
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
            using var db = DbHelper.Base();

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
            using var db = DbHelper.Base();
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
        var db = DbHelper.Base();
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

    protected override Manager InitNewEntity()
    {
        using var db = DbHelper.Base();
        var manager = db.GetCollection<Manager>().FindOne(x => x.IsMaster);
        return manager;
    }
    #endregion
}


//[AutoChangeableViewModel(typeof(Person))]
public partial class PersonViewModel : ObservableObject, IEquatable<PersonViewModel>
{
    public PersonViewModel(FMO.Models.Person? instance)
    {
        if (instance is FMO.Models.Person obj)
        {
            Id = obj.Id;
            Name = obj.Name;
            IDType = obj.IDType;
            Title = obj.Title;
            Cellphone = obj.Cellphone;
            Phone = obj.Phone;
            Address = obj.Address;
            Email = obj.Email;
            Profile = obj.Profile;
        }
    }
    public PersonViewModel()
    {
    }


    [ObservableProperty]
    public partial string? Name { get; set; }


    [ObservableProperty]
    public partial string? Id { get; set; }

    [ObservableProperty]
    public partial IDType IDType { get; set; } = IDType.IdentityCard;

    [ObservableProperty]
    public partial string? Title { get; set; }

    [ObservableProperty]
    public partial string? Cellphone { get; set; }

    [ObservableProperty]
    public partial string? Phone { get; set; }

    [ObservableProperty]
    public partial string? Address { get; set; }

    [ObservableProperty]
    public partial string? Email { get; set; }

    [ObservableProperty]
    public partial string? Profile { get; set; }

    public bool Equals(PersonViewModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return EqualityComparer<string?>.Default.Equals(Id, other.Id) &&
            EqualityComparer<string?>.Default.Equals(Name, other.Name)&&
            EqualityComparer<string?>.Default.Equals(Title, other.Title)
            && EqualityComparer<string?>.Default.Equals(Cellphone, other.Cellphone)
            && EqualityComparer<string?>.Default.Equals(Phone, other.Phone)
            && EqualityComparer<string?>.Default.Equals(Address, other.Address)
            && EqualityComparer<string?>.Default.Equals(Email, other.Email)
            && EqualityComparer<string?>.Default.Equals(Profile, other.Profile);
    }


    public FMO.Models.Person? Build()
    {
        if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(Name))
            return null;

        var result = new FMO.Models.Person()
        {
            Title = Title,
            Id = Id,
            Address = Address,
            Email = Email,
            Profile = Profile,
            Name = Name,
            Cellphone = Cellphone,
            Phone = Phone
        };
        return result;
    }
}