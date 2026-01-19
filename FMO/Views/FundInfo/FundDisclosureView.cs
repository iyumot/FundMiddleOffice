using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.AMAC.Direct;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using HandyControl.Controls;
using LiteDB;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;

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


public partial class FundDisclosureViewModel : ObservableObject, IRecipient<FundPeriodicReport>
{
    public FundDisclosureViewModel(int fid)
    {
        FundId = fid;

        // 获取公告列表
        using var db = DbHelper.Base();
        var data = db.GetCollection<FundAnnouncement>().Find(x => x.FundId == fid).ToArray();

        Announcements = [.. data.Select(x => new AnnouncementViewModel(x))];

        PeriodicDisclosure = [.. db.GetCollection<FundPeriodicReport>().Find(x => x.FundId == fid).Select(x => new FundPeriodicReportViewModel(x))];
        QuarterlyDisclosure = [.. db.GetCollection<FundQuarterlyUpdate>().Find(x => x.FundId == fid).Select(x => new FundQuarterlyUpdateViewModel(x))];


        //if (PeriodicDisclosure.Count == 0) PeriodicDisclosure = [new FundPeriodicReport { FundId = FundId, Type = PeriodicReportType.MonthlyReport }, new FundQuarterlyUpdate { FundId = FundId }];


        Monthly.Source = PeriodicDisclosure;
        Monthly.Filter += (s, e) => e.Accepted = e.Item switch { FundPeriodicReportViewModel r => r.Type == PeriodicReportType.MonthlyReport, _ => false };

        Quarterly.Source = PeriodicDisclosure;
        Quarterly.Filter += (s, e) => e.Accepted = e.Item switch { FundPeriodicReportViewModel r => r.Type == PeriodicReportType.QuarterlyReport, _ => false };

        SemiAnnually.Source = PeriodicDisclosure;
        SemiAnnually.Filter += (s, e) => e.Accepted = e.Item switch { FundPeriodicReportViewModel r => r.Type == PeriodicReportType.SemiAnnualReport, _ => false };

        Annually.Source = PeriodicDisclosure;
        Annually.Filter += (s, e) => e.Accepted = e.Item switch { FundPeriodicReportViewModel r => r.Type == PeriodicReportType.AnnualReport, _ => false };

        QuarterlyUpdate.Source = QuarterlyDisclosure;
    }

    public int FundId { get; }


    public ObservableCollection<AnnouncementViewModel> Announcements { get; init; }

    public CollectionViewSource Monthly { get; } = new();
    public CollectionViewSource Quarterly { get; } = new();
    public CollectionViewSource SemiAnnually { get; } = new();
    public CollectionViewSource Annually { get; } = new();
    public CollectionViewSource QuarterlyUpdate { get; } = new();


    public ObservableCollection<FundPeriodicReportViewModel> PeriodicDisclosure { get; }

    public ObservableCollection<FundQuarterlyUpdateViewModel> QuarterlyDisclosure { get; }

    [RelayCommand]
    public void AddAnnouncement()
    {
        using var db = DbHelper.Base();
        var obj = new FundAnnouncement { FundId = FundId };
        db.GetCollection<FundAnnouncement>().Insert(obj);

        Announcements?.Add(new(obj));
    }

    public void Receive(FundPeriodicReport message)
    {
    }
}





public partial class FundPeriodicReportViewModel : ObservableObject
{
    private readonly FundPeriodicReport report;

    public FundPeriodicReportViewModel(FundPeriodicReport report)
    {
        Id = report.Id;
        Type = report.Type;
        PeriodEnd = report.PeriodEnd;
        Word = new(report.Word);
        Excel = new(report.Excel);
        Pdf = new(report.Pdf);
        Xbrl = new(report.Xbrl);
        this.report = report;
    }

    public int Id { get; }

    public PeriodicReportType Type { get; }

    public string Title => Type switch
    {
        PeriodicReportType.QuarterlyReport => $"{PeriodEnd:yy} {PeriodEnd.Month switch { < 4 => "Q1", < 7 => "Q2", < 10 => "Q3", _ => "Q4" }}",
        PeriodicReportType.SemiAnnualReport => $"{PeriodEnd:yy} {PeriodEnd.Month switch { < 7 => "上半年", _ => "下半年" }}",
        PeriodicReportType.AnnualReport => $"{PeriodEnd:yy}",
        _ => $"{PeriodEnd:yy/MM}",
    };


    public string? FundName { get; set; }

    public DateOnly PeriodEnd { get; }

    public SimpleFileViewModel Word { get; }

    public SimpleFileViewModel Excel { get; }

    public SimpleFileViewModel Xbrl { get; }

    public SimpleFileViewModel Pdf { get; }


    [RelayCommand]
    public async Task Upload()
    {
        // 获取 账号
        using var db = DbHelper.Base();
        var acc = db.GetCollection<AmacReportAccount>().FindOne(x => x.Id == "pof");

        if (acc is null || string.IsNullOrWhiteSpace(acc.Name) || string.IsNullOrWhiteSpace(acc.Password) || string.IsNullOrWhiteSpace(acc.Key))
        {
            HandyControl.Controls.Growl.Info("请先在[平台]中设置信批账号");
            return;
        }

        var manager = db.GetCollection<Manager>().Query().First();

        var result = await DirectReporter.UploadReport(report, acc);

        if (result.UploadCode != 0)
        {
            HandyControl.Controls.Growl.Info($"上传文件失败:{result.UploadError}");
            return;
        }

        HandyControl.Controls.Growl.Info($"上传报告成功，请等待校验结果");
        await Task.Delay(20 * 1000);

        await DirectReporter.QueryResult(result, acc);

        if (result.ResultInfo?.Count > 0)
            HandyControl.Controls.Growl.Info($"{result.ResultInfo[0].Message}");
        else
            HandyControl.Controls.Growl.Info($"校验异常");

        if (result.ValidateCode == 0)
        {
            await Task.Delay(2000);
            await DirectReporter.Submit(result, manager.Name, acc);
            HandyControl.Controls.Growl.Info($"报告提交:{result.SubmitError}");
        }
    }
}

public partial class FundQuarterlyUpdateViewModel : ObservableObject
{
    private readonly FundQuarterlyUpdate report;

    public FundQuarterlyUpdateViewModel(FundQuarterlyUpdate report)
    {
        Id = report.Id;
        Type = report.Type;
        PeriodEnd = report.PeriodEnd;
        Investor = new(report.Investor);
        Operation = new(report.Operation);
        this.report = report;
    }

    public int Id { get; }
    public PeriodicReportType Type { get; }
    public string Title => $"{PeriodEnd:yy} {PeriodEnd.Month switch { < 4 => "Q1", < 7 => "Q2", < 10 => "Q3", _ => "Q4" }}";


    public DateOnly PeriodEnd { get; }

    public SimpleFileViewModel Investor { get; }

    public SimpleFileViewModel Operation { get; }


    [RelayCommand]
    public async Task UploadOperation()
    {
        // 获取 账号
        using var db = DbHelper.Base();
        var acc = db.GetCollection<AmacReportAccount>().FindOne(x => x.Id == "pmg");

        if (acc is null || string.IsNullOrWhiteSpace(acc.Name) || string.IsNullOrWhiteSpace(acc.Password) || string.IsNullOrWhiteSpace(acc.Key))
        {
            HandyControl.Controls.Growl.Info("请先在[平台]中设置信批账号");
            return;
        }

        var manager = db.GetCollection<Manager>().Query().First();

        // 检查是否有上传记录
        var result = db.GetCollection<AmacProcessResult>().FindById(Id);
        if (result is null)
        {
            result = await DirectReporter.UploadReport(report, acc);
            if (result.UploadCode != 0)
            {
                HandyControl.Controls.Growl.Info($"上传文件失败:{result.UploadError}");
                return;
            }

            HandyControl.Controls.Growl.Info($"上传报告成功，请等待校验结果");
            await Task.Delay(20 * 1000);
        }
        else HandyControl.Controls.Growl.Info("存在上传记录，继续查询结果");

        await DirectReporter.QueryResult(result, acc);

        if (result.ResultInfo?.Count > 0)
            HandyControl.Controls.Growl.Info($"{result.ResultInfo[0].Message}");
        else
            HandyControl.Controls.Growl.Info($"校验异常");

        if (result.ValidateCode == 0 || MessageBox.Show($"季度更新存在警告或错误", "是否强制提交", button: System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
        {
            await Task.Delay(50000);
            await DirectReporter.Submit(result, manager.Name, acc);
            HandyControl.Controls.Growl.Info($"报告提交, Code:{result.SubmitCode},{result.SubmitError}");
        }
       
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
            if (Id == 0) return; // 新建时不保存
            using var db = DbHelper.Base();
            db.GetCollection<FundAnnouncement>().UpdateMany(BsonMapper.Global.ToDocument(new { File = f }).ToString(), $"_id={Id}");
        };
    }

    public DualFileViewModel File { get; }


    public int FundId { get; }


    public ChangeableViewModel<FundAnnouncement, string?> Title { get; }

    public ChangeableViewModel<FundAnnouncement, DateTime?> Date { get; }


    protected override FundAnnouncement InitNewEntity() => new FundAnnouncement { FundId = FundId };
}