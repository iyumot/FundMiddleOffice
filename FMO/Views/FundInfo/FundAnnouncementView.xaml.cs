using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using Microsoft.Win32;

namespace FMO;

/// <summary>
/// FundAnnouncementView.xaml 的交互逻辑
/// </summary>
public partial class FundAnnouncementView : UserControl
{
    public FundAnnouncementView()
    {
        InitializeComponent();
    }
}


public partial class FundAnnouncementViewModel : ObservableObject
{
    public FundAnnouncementViewModel(int fid)
    {
        FundId = fid;

        // 获取公告列表
        using var db = DbHelper.Base();
        var data = db.GetCollection<FundAnnouncement>().Find(x => x.FundId == fid).ToArray();

        Announcements = [.. data.Select(x => new AnnouncementViewModel(x))];
    }

    public int FundId { get; }


    public ObservableCollection<AnnouncementViewModel> Announcements { get; init; }




    [RelayCommand]
    public void AddAnnouncement()
    {
        using var db = DbHelper.Base();
        var obj = new FundAnnouncement { FundId = FundId };
        db.GetCollection<FundAnnouncement>().Insert(obj);

        Announcements?.Add(new(obj));
    }
}









public partial class AnnouncementViewModel : EditableControlViewModelBase<FundAnnouncement>, IFileSetter
{
    public AnnouncementViewModel(FundAnnouncement obj)
    {
        Id = obj.Id;
        FundId = obj.FundId;

        Title = new()
        {
            InitFunc = x => x.Title,
            UpdateFunc = (x, y) => x.Title = y,
            ClearFunc = x => x.Title = null
        };
        Title.Init(obj);

        Date = new()
        {
            InitFunc = x => x.Date == default ? null : new DateTime(x.Date, default),
            UpdateFunc = (x, y) => x.Date = y is null ? default : DateOnly.FromDateTime(y.Value),
            ClearFunc = x => x.Date = default
        };
        Date.Init(obj);

        File = new()
        {
            SaveFolder = FundHelper.GetFolder(obj.FundId, "Announcement"),
            GetProperty = x => x.File,
            SetProperty = (x, y) => x.File = y,
        };
        File.Init(obj);

        Sealed = new()
        {
            SaveFolder = FundHelper.GetFolder(obj.FundId, "Announcement"),
            GetProperty = x => x.Sealed,
            SetProperty = (x, y) => x.Sealed = y,
        };
        Sealed.Init(obj);
    }

    public FileViewModel<FundAnnouncement> File { get; }
    public FileViewModel<FundAnnouncement> Sealed { get; }
    public int FundId { get; }


    public ChangeableViewModel<FundAnnouncement, string?> Title { get; }

    public ChangeableViewModel<FundAnnouncement, DateTime?> Date { get; }

    [RelayCommand]
    public void ChooseFile(FileViewModel<FundAnnouncement> file)
    {
        var fd = new OpenFileDialog();
        fd.Filter = file?.Filter;
        if (fd.ShowDialog() != true)
            return;

        SetFile(file, fd.FileName);
    }


    public void SetFile(IFileViewModel? file, string path)
    {
        if (file is FileViewModel<FundAnnouncement> ff)
        {
            FileStorageInfo? tar = ff.Build(path);
            if (tar?.Path is not null)
                ff.File = new FileInfo(tar.Path);

            using var db = DbHelper.Base();
            var obj = db.GetCollection<FundAnnouncement>().FindById(Id);
            if (obj is not null)
            {
                ff.SetProperty(obj, tar);
                db.GetCollection<FundAnnouncement>().Update(obj);
            }
        }
    }




    [RelayCommand]
    public void Clear(FileViewModel<FundAnnouncement> file)
    {
        if (file is null) return;

        var r = HandyControl.Controls.MessageBox.Show("是否删除文件", "提示", MessageBoxButton.YesNoCancel);
        if (r == MessageBoxResult.Cancel) return;

        if (r == MessageBoxResult.Yes)
        {
            try
            {
                file.File?.Delete();
            }
            catch (Exception e)
            {
                HandyControl.Controls.Growl.Warning("文件已打开，无法删除，请先关闭文件");
                return;
            }
        }


        using var db = DbHelper.Base();
        var obj = db.GetCollection<FundAnnouncement>().FindById(Id);

        if (obj is not null)
        {
            file.SetProperty(obj, null);
            db.GetCollection<FundAnnouncement>().Update(obj);
            file.File = null;
        }
    }

    protected override FundAnnouncement InitNewEntity() => new FundAnnouncement { FundId = FundId };
}