using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using LiteDB;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// FundAnnouncementView.xaml 的交互逻辑
/// </summary>
public partial class FundDisclosureView : UserControl
{
    public FundDisclosureView()
    {
        InitializeComponent();
    }
}


public partial class FundDisclosureViewModel : ObservableObject
{
    public FundDisclosureViewModel(int fid)
    {
        FundId = fid;

        // 获取公告列表
        using var db = DbHelper.Base();
        var data = db.GetCollection<FundAnnouncement>().Find(x => x.FundId == fid).ToArray();

        Announcements = [.. data.Select(x => new AnnouncementViewModel(x))];

        PeriodicDisclosure = [..db.GetCollection<FundPeriodicReport>().Find(x => x.FundId == fid).ToList<IPeriodical>().Union(db.GetCollection<FundQuarterlyUpdate>().Find(x=>x.FundId == fid))];


    }

    public int FundId { get; }


    public ObservableCollection<AnnouncementViewModel> Announcements { get; init; }

    public ObservableCollection<IPeriodical> PeriodicDisclosure { get;  }


    [RelayCommand]
    public void AddAnnouncement()
    {
        using var db = DbHelper.Base();
        var obj = new FundAnnouncement { FundId = FundId };
        db.GetCollection<FundAnnouncement>().Insert(obj);

        Announcements?.Add(new(obj));
    }
}









public partial class AnnouncementViewModel : EditableControlViewModelBase<FundAnnouncement>
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
            ClearFunc = x => x.Date = default,
            DisplayFunc = x => x?.ToString("yyyy-MM-dd")
        };
        Date.Init(obj);

        File = new(obj.File);
        File.FileChanged += f =>
        {
            if(Id == 0) return; // 新建时不保存
            using var db = DbHelper.Base();
            db.GetCollection<FundAnnouncement>().UpdateMany(BsonMapper.Global.ToDocument(f).ToString(), $"_id={Id}");
        };
    }

    public DualFileViewModel File { get; }


    public int FundId { get; }


    public ChangeableViewModel<FundAnnouncement, string?> Title { get; }

    public ChangeableViewModel<FundAnnouncement, DateTime?> Date { get; }

   
    protected override FundAnnouncement InitNewEntity() => new FundAnnouncement { FundId = FundId };
}