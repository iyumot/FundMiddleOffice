using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.IO.AMAC;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using Microsoft.Win32;
using Serilog;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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


public partial class ManagerPageViewModel : EditableControlViewModelBase<Manager>, IRecipient<ParticipantChangedMessage>
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
    public partial ObservableCollection<ParticipantViewModel> Members { get; set; }

    public CollectionViewSource MemberSource { get; } = new();

    [ObservableProperty]
    public partial Participant? SelectedMember { get; set; }

    [ObservableProperty]
    public partial ManagerMemberViewModel? MemberContext { get; set; }

    /// <summary>
    /// 修改成员信息
    /// </summary>
    [ObservableProperty]
    public partial bool ShowMemberPopup { get; set; }

    public ManagerPageViewModel()
    {
        WeakReferenceMessenger.Default.Register<ParticipantChangedMessage>(this);

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

        Members = new(db.GetCollection<Participant>().FindAll().ToArray().Select(x => new ParticipantViewModel(x))/*.Select(x => new PersonViewModel(x)*/);
        MemberSource.Source = Members;
        //if (manager.LegalAgent is not null)
        //    Members.Add( (manager.LegalAgent));

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


    [RelayCommand]
    public void AddMember()
    {
        Participant obj = new();
        Members.Add(new(obj));
        MemberContext = new ManagerMemberViewModel(obj);
        ShowMemberPopup = true;
    }


    [RelayCommand]
    public void RemoveMember(ParticipantViewModel participant)
    {
        if (HandyControl.Controls.MessageBox.Show($"是否确认删除 {participant.Name}？", button: MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            using var db = DbHelper.Base();
            db.GetCollection<Participant>().Delete(participant.Id);

            Members.Remove(participant);
        }
    }

    [RelayCommand]
    public void EditMember(ParticipantViewModel participant)
    {
        using var db = DbHelper.Base();
        var obj = db.GetCollection<Participant>().FindById(participant.Id);
        MemberContext = new ManagerMemberViewModel(obj);
        ShowMemberPopup = true;
    }

    partial void OnSelectedMemberChanged(Participant? value)
    {
        MemberContext = value is null ? null : new ManagerMemberViewModel(value);
    }

    [RelayCommand]
    public async Task LoadMemberFromAmac()
    {
        try
        {
            // 检查有没有账号
            using var db = DbHelper.Base();
            var acc = db.GetCollection<AmacAccount>().FindById("human");
            if (acc is null || string.IsNullOrWhiteSpace(acc.Name) || string.IsNullOrWhiteSpace(acc.Password))
            {
                HandyControl.Controls.Growl.Error($"获取管理人成员失败，没有有效的账号密码");
                return;
            }

            var result = await AmacHuman.GetParticipants(acc.Name, acc.Password);

            switch (result.Code)
            {
                case AmacReturn.AccountError:
                    HandyControl.Controls.Growl.Error($"获取管理人成员失败，账号密码错误");
                    return;
                case AmacReturn.Browser:
                case AmacReturn.InvalidResponse:
                    HandyControl.Controls.Growl.Error($"获取管理人成员失败，请查看log");
                    return;
                default:
                    break;
            }


            // 处理数据
            var data = db.GetCollection<Participant>().FindAll().ToArray();
            var np = result.Data.Where(x => data.All(y => y.Name != x.Name && y.Identity?.Id != x.Identity?.Id));
            if (np.Any())
            {
                db.GetCollection<Participant>().Insert(np);

                foreach (var item in np)
                    Members.Add(new ParticipantViewModel(item));
            }


            HandyControl.Controls.Growl.Success($"获取管理人成员成功");
        }
        catch (Exception ex)
        {
            Log.Error($"获取管理人成员失败，{ex}");
            HandyControl.Controls.Growl.Error($"获取管理人成员失败，请查看log");
        }
    }


    [RelayCommand]
    public void AddShareHolder()
    {

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
                var dir = Directory.CreateDirectory("manager");
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
                var dir = Directory.CreateDirectory("manager");
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
                var dir = Directory.CreateDirectory("manager");
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
                var dir = Directory.CreateDirectory("manager");
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
                var dir = Directory.CreateDirectory("manager");
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
            ms.Seek(0, SeekOrigin.Begin);
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

    public void Receive(ParticipantChangedMessage message)
    {
        using var db = DbHelper.Base();
        var obj = db.GetCollection<Participant>().FindById(message.Id);


        if (obj is not null && (Members.FirstOrDefault(x => x.Id == message.Id) ?? Members.LastOrDefault(x => x.Id == 0)) is ParticipantViewModel old)
        {
            old.UpdateFrom(obj);
            MemberSource.View.Refresh();
        }
    }
    #endregion
}


public partial class ParticipantViewModel : ObservableObject
{
    public ParticipantViewModel(FMO.Models.Participant? instance)
    {
        if (instance is FMO.Models.Participant obj)
        {
            Id = obj.Id;
            Name = obj.Name;
            Role = obj.Role;
            Identity = obj.Identity;
            Title = obj.Title;
            Phone = obj.Phone;
            Address = obj.Address;
            Email = obj.Email;
            Profile = obj.Profile;
        }
    }

    [ObservableProperty]
    public partial string? Name { get; set; }


    public int Id { get; set; }

    [ObservableProperty]
    public partial Identity Identity { get; set; }

    [ObservableProperty]
    public partial PersonRole Role { get; set; }

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

    [ObservableProperty]
    public partial FileInfo? IdFile { get; set; }


    [ObservableProperty]
    public partial FileInfo? SealedIdFile { get; set; }


    partial void OnIdentityChanged(Identity value)
    {
        var path = $@"manager\members\{Id}.{Identity.Id}";
        var di = new DirectoryInfo($@"manager\members");

        if (!di.Exists) return;
        var ff = di.GetFiles();
        IdFile = ff.LastOrDefault(x => x.Name.StartsWith($@"{Id}.{Identity.Id}"));
        SealedIdFile = ff.LastOrDefault(x => x.Name.StartsWith($@"sealed.{Id}.{Identity.Id}"));
    }


    [RelayCommand]
    public void View()
    {
        if (IdFile?.Exists ?? false)
            try { System.Diagnostics.Process.Start(new ProcessStartInfo { FileName = IdFile.FullName, UseShellExecute = true }); } catch { }
    }

    [RelayCommand]
    public void SetFile()
    {
        var dlg = new OpenFileDialog();
        var r = dlg.ShowDialog();

        if (r is null || !r.Value) return;

        var di = new DirectoryInfo($@"manager\members");

        if (!di.Exists) di.Create();

        string tar = Path.Combine("manager", "members", $"{Id}.{Identity.Id}{Path.GetExtension(dlg.FileName)}");
        try { File.Copy(dlg.FileName, tar); IdFile = new FileInfo(tar); } catch { }
    }





}