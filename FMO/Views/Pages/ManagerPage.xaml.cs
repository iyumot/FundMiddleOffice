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
using static FMO.OwnershipMapViewModel;

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
    // private int FilesId;




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
    //[ObservableProperty]
    //public partial ObservableCollection<FileInfo>? BusinessLicense { get; set; }


    public MultipleFileViewModel BusinessLicense { get; }

    /// <summary>
    /// 营业执照副本
    /// </summary>
    public MultipleFileViewModel BusinessLicense2 { get; }

    /// <summary>
    /// 开户许可证
    /// </summary>
    public MultipleFileViewModel AccountOpeningLicense { get; }


    /// <summary>
    /// 章程
    /// </summary>
    public MultipleFileViewModel CharterDocument { get; }

    /// <summary>
    /// 法人身份证
    /// </summary>
    public MultipleFileViewModel LegalPersonIdCard { get; }


    [ObservableProperty]
    public partial ObservableCollection<ParticipantViewModel> Members { get; set; }


    public ObservableCollection<RelationViewModel> ShareRelations { get; }


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

    /// <summary>
    /// 股权与注册资本不一致
    /// </summary>
    [ObservableProperty]
    public partial bool ShareNotPair { get; set; }


    public ObservableCollection<ManagerFlowViewModel> Flows { get; }

    public ManagerPageViewModel()
    {
        WeakReferenceMessenger.Default.Register<ParticipantChangedMessage>(this);

        var db = DbHelper.Base();
        Manager manager = db.GetCollection<Manager>().FindById(1)!;
        var id = manager?.Identity?.Id;
        //var mfile = db.GetCollection<InstitutionFiles>().FindOne(x => x.InstitutionId == id);
        //if (mfile is null)
        //{
        //    mfile = new InstitutionFiles { InstitutionId = id };
        //    db.GetCollection<InstitutionFiles>().Insert(mfile);
        //}
        //FilesId = mfile.Id;


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


        var rel = db.GetCollection<Ownership>().Find(x => x.InstitutionId == 0).ToArray();
        var entities = db.GetCollection<IEntity>().FindAll().ToArray();
        var relations = rel.Select(x => new RelationViewModel
        {
            Id = x.Id,
            Holder = entities.FirstOrDefault(y => y.Id == x.HolderId),
            Institution = manager!,
            Share = x.Share,
            Ratio = x.Ratio == 0 ? x.Share / manager!.RegisterCapital : x.Ratio
        }).ToArray();
        ShareRelations = [.. relations.Where(x => x.Institution == manager)];
        ShareNotPair = rel.Sum(x => x.Share) != manager.RegisterCapital;


        db.Dispose();

        AmacPageUrl = $"https://gs.amac.org.cn/amac-infodisc/res/pof/manager/{manager!.AmacId}.html";

        #region MyRegion
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
            InitFunc = x => x.Identity?.Id,
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


        var cef = db.GetCollection<InstitutionCertifications>().FindById(manager.Identity!.Id);
        if (cef is null)
        {
            cef = new() { Id = manager.Identity.Id };
            db.GetCollection<InstitutionCertifications>().Insert(cef);
        }
        BusinessLicense = new()
        {
            Label = "营业执照正本",
            Files = [.. (cef.BusinessLicense ?? new())],
            OnAddFile = (x, y) => SetFile(x => x.BusinessLicense ??= new(), x),
            OnDeleteFile = x => DeleleFile(x => x.BusinessLicense, x),
        };

        BusinessLicense2 = new()
        {
            Label = "营业执照副本",
            Files = [.. (cef.BusinessLicense2 ?? new())],
            OnAddFile = (x, y) => SetFile(x => x.BusinessLicense2 ??= new(), x),
            OnDeleteFile = x => DeleleFile(x => x.BusinessLicense2, x),
        };


        AccountOpeningLicense = new()
        {
            Label = "开户许可证",
            Files = [.. (cef.AccountOpeningLicense ?? new())],
            OnAddFile = (x, y) => SetFile(x => x.AccountOpeningLicense ??= new(), x),
            OnDeleteFile = x => DeleleFile(x => x.AccountOpeningLicense, x),
        };


        CharterDocument = new()
        {
            Label = "章程/合伙协议",
            Files = [.. (cef.CharterDocument ?? new())],
            OnAddFile = (x, y) => SetFile(x => x.CharterDocument ??= new(), x),
            OnDeleteFile = x => DeleleFile(x => x.CharterDocument, x),
        };


        LegalPersonIdCard = new()
        {
            Label = "法人/委派代表身份证",
            Files = [.. (cef.LegalPersonIdCard ?? new())],
            OnAddFile = (x, y) => SetFile(x => x.LegalPersonIdCard ??= new(), x),
            OnDeleteFile = x => DeleleFile(x => x.LegalPersonIdCard, x),
        };
        #endregion


        Flows = [.. db.GetCollection<ManagerFlow>().FindAll().Select(x => new ManagerFlowViewModel(x))];

        //BusinessLicense = mfile?.BusinessLicense is null ? new() : new ObservableCollection<FileInfo>(mfile.BusinessLicense.Files.Select(x => new FileInfo(x.Path)));
        //BusinessLicense.CollectionChanged += BusinessLicense_CollectionChanged;


        //BusinessLicense2 = mfile?.BusinessLicense2 is null ? new() : new ObservableCollection<FileInfo>(mfile.BusinessLicense2.Files.Select(x => new FileInfo(x.Path)));
        //BusinessLicense2.CollectionChanged += BusinessLicense2_CollectionChanged;


        //AccountOpeningLicense = mfile?.AccountOpeningLicense is null ? new() : new ObservableCollection<FileInfo>(mfile.AccountOpeningLicense.Files.Select(x => new FileInfo(x.Path)));
        //AccountOpeningLicense.CollectionChanged += AccountOpeningLicense_CollectionChanged;

        //CharterDocument = mfile?.CharterDocument is null ? new() : new ObservableCollection<FileInfo>(mfile.CharterDocument.Files.Select(x => new FileInfo(x.Path)));
        //CharterDocument.CollectionChanged += CharterDocument_CollectionChanged;


        //LegalPersonIdCard = mfile?.LegalPersonIdCard is null ? new() : new ObservableCollection<FileInfo>(mfile.LegalPersonIdCard.Files.Select(x => new FileInfo(x.Path)));
        //LegalPersonIdCard.CollectionChanged += LegalPersonIdCard_CollectionChanged;
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
        var db = DbHelper.Base();
        var manager = db.GetCollection<Manager>().FindById(1);

        var wnd = new AddOrModifyShareHolderWindow();
        wnd.DataContext = new AddOrModifyShareHolderWindowViewModel(manager);
        wnd.Owner = App.Current.MainWindow;
        wnd.ShowDialog();

        var rel = db.GetCollection<Ownership>().Find(x => x.InstitutionId == 0).ToArray();
        var entities = db.GetCollection<IEntity>().FindAll().ToArray();
        foreach (var x in rel.ExceptBy(ShareRelations.Select(x => x.Holder?.Id), x => x.HolderId))
        {
            ShareRelations.Add(new RelationViewModel
            {
                Id = x.Id,
                Holder = entities.FirstOrDefault(y => y.Id == x.HolderId),
                Institution = manager,
                Share = x.Share,
                Ratio = x.Ratio == 0 ? x.Share / manager!.RegisterCapital : x.Ratio
            });
        }


        ShareNotPair = ShareRelations.Sum(x => x.Share) != manager.RegisterCapital;

    }

    [RelayCommand]
    public void AddPerson()
    {
        var wnd = new AddOrModifyPersonWindow();
        wnd.DataContext = new PersonViewModel(new Person { Name = "" });
        wnd.Owner = App.Current.MainWindow;
        wnd.ShowDialog();
    }


    [RelayCommand]
    public void RemoveShareHolder(RelationViewModel value)
    {
        if (HandyControl.Controls.MessageBox.Show($"是否确认删除 {value.Holder!.Name}？", button: MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            using var db = DbHelper.Base();
            db.GetCollection<Ownership>().Delete(value.Id);

            ShareRelations.Remove(value);

            ShareNotPair = ShareRelations.Sum(x => x.Share) != RegisterCapital.OldValue;
        }
    }

    [RelayCommand]
    public void EditShareHolder(RelationViewModel value)
    {
        var db = DbHelper.Base();
        var manager = db.GetCollection<Manager>().FindById(1);
        var wnd = new AddOrModifyShareHolderWindow();
        AddOrModifyShareHolderWindowViewModel obj = new(manager);
        obj.Holder = obj.Entities.FirstOrDefault(x => x.Id == value.Holder!.Id);
        obj.HolderName = value.Holder!.Name;
        obj.Institution = value.Institution;
        obj.ShareAmount = value.Share;
        wnd.DataContext = obj;
        wnd.Owner = App.Current.MainWindow;
        wnd.ShowDialog();

    }


    [RelayCommand]
    public void OpenOwnership()
    {
        var wnd = new Window();
        EquityStructureDiagram dig = new();
        wnd.Content = dig;
        dig.SetBinding(EquityStructureDiagram.NodesProperty, new Binding(nameof(EquityViewModel.CompanyNodes)));
        wnd.DataContext = new EquityViewModel();
        wnd.Owner = App.Current.MainWindow;
        wnd.ShowDialog();


        using var db = DbHelper.Base();
        var per = db.GetCollection<IEntity>().FindAll().ToList();
        per.Insert(0, db.GetCollection<Manager>().FindById(1));


        var os = db.GetCollection<Ownership>().FindAll().ToArray();
        // 解析股权结构图
        List<OwnershipItem> data = new();

        var ins = per.FirstOrDefault(x => x.Id == 1);
        var oi = new OwnershipItem { Name = ins!.Name };
        Parse(1, per, os, oi);




    }

    [RelayCommand]
    public void AddFlow()
    {
        Flows.Add(new ManagerFlowViewModel(new()));
    }

    [RelayCommand]
    public void DeleteFlow(ManagerFlowViewModel v)
    {
        if (v.Id != 0 && HandyControl.Controls.MessageBox.Show($"确认删除【{v.Title.NewValue}】吗？，无法恢复", button: MessageBoxButton.YesNo) == MessageBoxResult.No)
            return;

        using var db = DbHelper.Base();
        db.GetCollection<ManagerFlow>().Delete(v.Id);
        Flows.Remove(v);
    }


    private OwnershipItem Parse(int institutionId, IList<IEntity> per, IEnumerable<Ownership> os, OwnershipItem oi)
    {
        foreach (var item in os.Where(x => x.InstitutionId == institutionId))
        {
            var ins = per.FirstOrDefault(x => x.Id == item.InstitutionId);
            var holder = per.FirstOrDefault(x => x.Id == item.HolderId);

            if (holder is Person p)
            {
                oi.Childs.Add(new OwnershipItem { Name = p.Name, Ratio = item.Ratio });
                continue;
            }
            else if (holder is Institution)
                oi.Childs.Add(Parse(holder.Id, per, os, new OwnershipItem { Name = holder.Name, Ratio = item.Ratio }));


        }

        return oi;
    }


    #region MyRegion





    //private void BusinessLicense_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    //{
    //    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems is not null)
    //    {
    //        using var db = DbHelper.Base();

    //        foreach (FileInfo item in e.NewItems)
    //        {
    //            string hash = item.ComputeHash()!;

    //            // 保存副本
    //            var dir = Directory.CreateDirectory("manager");
    //            var tar = FileHelper.CopyFile(item, dir.FullName);

    //            FileVersion fileVersion = new FileVersion { Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path), Hash = hash, Time = item.LastWriteTime };

    //            var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);
    //            if (files.BusinessLicense is null)
    //                files.BusinessLicense = new VersionedFileInfo { Name = nameof(BusinessLicense), Files = new() };
    //            files!.BusinessLicense.Files.Add(fileVersion);
    //            db.GetCollection<InstitutionFiles>().Update(files);
    //        }
    //    }
    //    else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems is not null)
    //    {
    //        using var db = DbHelper.Base();
    //        var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);


    //        foreach (FileInfo item in e.OldItems)
    //        {
    //            var rp = Path.GetRelativePath(Directory.GetCurrentDirectory(), item.FullName);
    //            var file = files!.BusinessLicense?.Files.FirstOrDefault(x => rp == x.Path || x.Path == item.FullName);
    //            if (file is not null)
    //                files.BusinessLicense!.Files.Remove(file);
    //        }

    //        db.GetCollection<InstitutionFiles>().Update(files!);
    //    }

    //}
    //private void BusinessLicense2_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    //{
    //    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems is not null)
    //    {
    //        using var db = DbHelper.Base();

    //        foreach (FileInfo item in e.NewItems)
    //        {
    //            string hash = item.ComputeHash()!;

    //            // 保存副本
    //            var dir = Directory.CreateDirectory("manager");
    //            var tar = FileHelper.CopyFile(item, dir.FullName);

    //            FileVersion fileVersion = new FileVersion { Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path), Hash = hash, Time = item.LastWriteTime };

    //            var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);
    //            if (files.BusinessLicense2 is null)
    //                files.BusinessLicense2 = new VersionedFileInfo { Name = nameof(BusinessLicense2), Files = new() };
    //            files!.BusinessLicense2.Files.Add(fileVersion);
    //            db.GetCollection<InstitutionFiles>().Update(files);
    //        }
    //    }
    //    else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems is not null)
    //    {
    //        using var db = DbHelper.Base();
    //        var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);


    //        foreach (FileInfo item in e.OldItems)
    //        {
    //            var rp = Path.GetRelativePath(Directory.GetCurrentDirectory(), item.FullName);
    //            var file = files!.BusinessLicense2?.Files.FirstOrDefault(x => rp == x.Path || x.Path == item.FullName);
    //            if (file is not null)
    //                files.BusinessLicense2!.Files.Remove(file);
    //        }

    //        db.GetCollection<InstitutionFiles>().Update(files!);
    //    }

    //}
    //private void AccountOpeningLicense_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    //{
    //    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems is not null)
    //    {
    //        using var db = DbHelper.Base();

    //        foreach (FileInfo item in e.NewItems)
    //        {
    //            string hash = item.ComputeHash()!;

    //            // 保存副本
    //            var dir = Directory.CreateDirectory("manager");
    //            var tar = FileHelper.CopyFile(item, dir.FullName);

    //            FileVersion fileVersion = new FileVersion { Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path), Hash = hash, Time = item.LastWriteTime };

    //            var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);
    //            if (files.AccountOpeningLicense is null)
    //                files.AccountOpeningLicense = new VersionedFileInfo { Name = nameof(AccountOpeningLicense), Files = new() };
    //            files!.AccountOpeningLicense.Files.Add(fileVersion);
    //            db.GetCollection<InstitutionFiles>().Update(files);
    //        }
    //    }
    //    else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems is not null)
    //    {
    //        using var db = DbHelper.Base();
    //        var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);


    //        foreach (FileInfo item in e.OldItems)
    //        {
    //            var rp = Path.GetRelativePath(Directory.GetCurrentDirectory(), item.FullName);
    //            var file = files!.AccountOpeningLicense?.Files.FirstOrDefault(x => rp == x.Path || x.Path == item.FullName);
    //            if (file is not null)
    //                files.AccountOpeningLicense!.Files.Remove(file);
    //        }

    //        db.GetCollection<InstitutionFiles>().Update(files!);
    //    }

    //}
    //private void CharterDocument_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    //{
    //    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems is not null)
    //    {
    //        using var db = DbHelper.Base();

    //        foreach (FileInfo item in e.NewItems)
    //        {
    //            string hash = item.ComputeHash()!;

    //            // 保存副本
    //            var dir = Directory.CreateDirectory("manager");
    //            var tar = FileHelper.CopyFile(item, dir.FullName);

    //            FileVersion fileVersion = new FileVersion { Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path), Hash = hash, Time = item.LastWriteTime };

    //            var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);
    //            if (files.CharterDocument is null)
    //                files.CharterDocument = new VersionedFileInfo { Name = nameof(CharterDocument), Files = new() };
    //            files!.CharterDocument.Files.Add(fileVersion);
    //            db.GetCollection<InstitutionFiles>().Update(files);
    //        }
    //    }
    //    else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems is not null)
    //    {
    //        using var db = DbHelper.Base();
    //        var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);


    //        foreach (FileInfo item in e.OldItems)
    //        {
    //            var rp = Path.GetRelativePath(Directory.GetCurrentDirectory(), item.FullName);
    //            var file = files!.CharterDocument?.Files.FirstOrDefault(x => rp == x.Path || x.Path == item.FullName);
    //            if (file is not null)
    //                files.CharterDocument!.Files.Remove(file);
    //        }

    //        db.GetCollection<InstitutionFiles>().Update(files!);
    //    }

    //}
    //private void LegalPersonIdCard_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    //{
    //    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems is not null)
    //    {
    //        using var db = DbHelper.Base();

    //        foreach (FileInfo item in e.NewItems)
    //        {
    //            string hash = item.ComputeHash()!;

    //            // 保存副本
    //            var dir = Directory.CreateDirectory("manager");
    //            var tar = FileHelper.CopyFile(item, dir.FullName);

    //            FileVersion fileVersion = new FileVersion { Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar.Path), Hash = hash, Time = item.LastWriteTime };

    //            var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);
    //            if (files.LegalPersonIdCard is null)
    //                files.LegalPersonIdCard = new VersionedFileInfo { Name = nameof(LegalPersonIdCard), Files = new() };
    //            files!.LegalPersonIdCard.Files.Add(fileVersion);
    //            db.GetCollection<InstitutionFiles>().Update(files);
    //        }
    //    }
    //    else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems is not null)
    //    {
    //        using var db = DbHelper.Base();
    //        var files = db.GetCollection<InstitutionFiles>().FindById(FilesId);


    //        foreach (FileInfo item in e.OldItems)
    //        {
    //            var rp = Path.GetRelativePath(Directory.GetCurrentDirectory(), item.FullName);
    //            var file = files!.LegalPersonIdCard?.Files.FirstOrDefault(x => rp == x.Path || x.Path == item.FullName);
    //            if (file is not null)
    //                files.LegalPersonIdCard!.Files.Remove(file);
    //        }

    //        db.GetCollection<InstitutionFiles>().Update(files!);
    //    }

    //}

    internal void SetLogo(string v)
    {
        var db = DbHelper.Base();
        db.FileStorage.Upload("icon.main", v);
        if (db.FileStorage.Exists("icon.main"))
        {
            try
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
            catch (Exception e)
            {
                Log.Error($"SetLogo Error {e}");
            }
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


    private FileStorageInfo? SetFile(Func<InstitutionCertifications, List<FileStorageInfo>> func, FileInfo fi)
    {
        string hash = fi.ComputeHash()!;

        // 保存副本
        var dir = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "files", "manager"));

        var tar = FileHelper.CopyFile2(fi, dir.FullName);
        if (tar is null)
        {
            Log.Error($"保存文件出错，{fi.Name}");
            HandyControl.Controls.Growl.Error($"无法保存{fi.Name}，文件名异常或者存在过多重名文件");
            return null;
        }

        var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar);

        using var db = DbHelper.Base();
        var q = db.GetCollection<InstitutionCertifications>().FindById(InstitutionCode.OldValue);
        var l = func(q);
        FileStorageInfo fsi = new()
        {
            Title = "",
            Path = path,
            Hash = hash,
            Time = DateTime.Now
        };
        l!.Add(fsi);
        db.GetCollection<InstitutionCertifications>().Update(q);
        return fsi;
    }


    private void DeleleFile(Func<InstitutionCertifications, List<FileStorageInfo>?> func, FileStorageInfo file)
    {
        using var db = DbHelper.Base();
        var q = db.GetCollection<InstitutionCertifications>().FindById(InstitutionCode.OldValue);

        var l = func(q);
        if (l is null) return;
        var old = l.Find(x => x.Path is not null && file.Path is not null && Path.GetFullPath(x.Path) == Path.GetFullPath(file.Path));
        if (old is not null)
        {
            l.Remove(old);

            try { File.Delete(old.Path!); } catch (Exception e) { Log.Error($"delete file failed {e}"); }
        }
        db.GetCollection<InstitutionCertifications>().Update(q);
    }


    public partial class RelationViewModel : ObservableObject
    {
        public int Id { get; set; }

        /// <summary>
        /// 持有人
        /// </summary>
        public IEntity? Holder { get; set; }

        /// <summary>
        /// 持股的机构、公司
        /// 0 表示 管理人
        /// </summary>
        public required Institution Institution { get; set; }


        public string? InstitutionName { get; set; }

        public decimal Share { get; set; }

        public decimal Ratio { get; set; }


        public ObservableCollection<RelationViewModel>? Children { get; set; }

        [RelayCommand]
        public void AddShareHolder()
        {
            var wnd = new AddOrModifyShareHolderWindow();
            wnd.DataContext = new AddOrModifyShareHolderWindowViewModel(Institution);
            wnd.Owner = App.Current.MainWindow;
            wnd.ShowDialog();

        }

        [RelayCommand]
        public void AddPerson()
        {
            var wnd = new AddOrModifyPersonWindow();
            wnd.DataContext = new PersonViewModel(new Person { Name = "" });
            wnd.Owner = App.Current.MainWindow;
            wnd.ShowDialog();
        }


        [RelayCommand]
        public void RemoveShareHolder(RelationViewModel value)
        {
            if (HandyControl.Controls.MessageBox.Show($"是否确认删除 {value.Holder!.Name}？", button: MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using var db = DbHelper.Base();
                db.GetCollection<Ownership>().Delete(value.Id);

                Children?.Remove(value);
            }
        }

        [RelayCommand]
        public void EditShareHolder(RelationViewModel value)
        {
            var wnd = new AddOrModifyShareHolderWindow();
            AddOrModifyShareHolderWindowViewModel obj = new(Institution);
            obj.Holder = value.Holder;
            obj.Institution = value.Institution;
            obj.ShareAmount = value.Share;
            wnd.DataContext = obj;
            wnd.Owner = App.Current.MainWindow;
            wnd.ShowDialog();

        }


        [RelayCommand]
        public void OpenEntity()
        {
            if (Holder is Institution institution)
            {
                try
                {
                    var wnd = new InstitutionWindow();
                    var context = new InstitutionWindowViewModel(Holder.Id);
                    wnd.DataContext = context;
                    wnd.Owner = App.Current.MainWindow;
                    wnd.ShowDialog();
                }
                catch (Exception e) { Log.Error($"{e}"); }
            }
        }

    }
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


public partial class ManagerFlowViewModel : EditableControlViewModelBase<ManagerFlow>
{

    public ChangeableViewModel<ManagerFlow, DateOnly?> Date { get; }

    public ChangeableViewModel<ManagerFlow, string> Title { get; }

    public ChangeableViewModel<ManagerFlow, string> Description { get; }


    [ObservableProperty]
    public partial bool IsTemple { get; set; }


    public bool IsFinished { get; set; }

    public ManagerFlowViewModel(ManagerFlow flow)
    {
        Id = flow.Id;

        Date = new()
        {
            Label = "日期",
            InitFunc = x => x.Date == default ? null : x.Date,
            UpdateFunc = (x, y) => { IsTemple = false; x.Date = y ?? default; },
            ClearFunc = x => x.Date = DateOnly.MinValue,
        };
        Date.Init(flow);

        Title = new()
        {
            Label = "标题",
            InitFunc = x => x.Title,
            UpdateFunc = (x, y) => { IsTemple = false; x.Title = y; },
            ClearFunc = x => x.Title = string.Empty,
        };
        Title.Init(flow);

        Description = new()
        {
            Label = "描述",
            InitFunc = x => x.Description,
            UpdateFunc = (x, y) => { IsTemple = false; x.Description = y; },
            ClearFunc = x => x.Description = string.Empty,
        };
        Description.Init(flow);

        IsTemple = Id == 0;


    }



    [RelayCommand]
    public void OpenNormal() => OpenFolder(@$"manager\flow\{Id}\normal");


    [RelayCommand]
    public void OpenSealed() => OpenFolder(@$"manager\flow\{Id}\sealed");


    [RelayCommand]
    public void AddSealedFile(DragEventArgs args) => SaveFile(args, @$"manager\flow\{Id}\sealed");


    [RelayCommand]
    public void AddNormalFile(DragEventArgs args) => SaveFile(args, @$"manager\flow\{Id}\normal");

    private void SaveFile(DragEventArgs e, string folder)
    {
        if(e.Data.GetData(DataFormats.FileDrop) is string[] s)
        {
            foreach (var item in s)
            {
                File.Copy(item, Path.Combine(folder, Path.GetFileName(item)), true);
            }
        }
    }


    private void OpenFolder(string folder)
    {
        try
        {
            DirectoryInfo di = new DirectoryInfo(folder);
            if (!di.Exists)
                di.Create();
            Process.Start(new ProcessStartInfo
            {
                FileName = di.FullName,
                UseShellExecute = true
            });
        }
        catch (Exception e)
        {
            Log.Error($"OpenFolder Error {e}");
            WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Warning, "打开文件夹失败"));
        }
    }



    protected override ManagerFlow InitNewEntity() => new ManagerFlow();
}


