using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.PDF;
using FMO.Shared;
using FMO.Utilities;
using Serilog;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Data;

namespace FMO;

/// <summary>
/// AddOrderWindow.xaml 的交互逻辑
/// </summary>
public partial class AddOrderWindow : Window
{
    public AddOrderWindow()
    {
        InitializeComponent();
    }


}



public abstract partial class AddOrderWindowViewModelBase : ObservableObject
{
    public AddOrderWindowViewModelBase()
    {
        Contract = new()
        {
            Label = "基金合同",
            OnSetFile = (x, y) => SetFile(x, y, (a, b) => a.Contract = b),
            OnDeleteFile = x => DeleteFile(x),
            FileChanged = () => Check()
        };
        RiskDisclosure = new()
        {
            Label = "风险揭示书",
            OnSetFile = (x, y) => SetFile(x, y, (a, b) => a.RiskDiscloure = b),
            OnDeleteFile = x => DeleteFile(x),
            FileChanged = () => Check()
        };
        OrderFile = new()
        {
            Label = "认申赎单",
            OnSetFile = (x, y) => SetFile2(x, y, (a, b) => a.OrderSheet = b),
            OnDeleteFile = x => DeleteFile(x),
            FileChanged = () => Check()
        };
        Video = new()
        {
            Label = "双录",
            OnSetFile = (x, y) => SetFile(x, y, (a, b) => a.Videotape = b),
            OnDeleteFile = x => DeleteFile(x),
            FileChanged = () => Check()
        };


        RiskPair = new()
        {
            Label = "风险匹配告知书",
            OnSetFile = (x, y) => SetFile(x, y, (a, b) => a.RiskPair = b),
            OnDeleteFile = x => DeleteFile(x),
            FileChanged = () => Check()
        };

        Review = new()
        {
            Label = "回访",
            OnSetFile = (x, y) => SetFile(x, y, (a, b) => a.Review = b),
            OnDeleteFile = x => DeleteFile(x),
            FileChanged = () => Check()
        };
    }


    public int Id { get; set; }

    [ObservableProperty]
    public partial bool IsReadOnly { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    public partial DateTime? Date { get; set; }

    [ObservableProperty]
    public partial decimal? Number { get; set; }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSell))]
    public partial TransferOrderType? SelectedType { get; set; }


    public virtual TransferOrderType[] Types { get; } = [TransferOrderType.Buy, TransferOrderType.Share, TransferOrderType.Amount, TransferOrderType.RemainAmout];




    public SingleFileViewModel Contract { get; }

    public SingleFileViewModel RiskDisclosure { get; }


    public SingleFileViewModel OrderFile { get; }


    public SingleFileViewModel Video { get; }


    public SingleFileViewModel RiskPair { get; }


    public SingleFileViewModel Review { get; }



    //partial void OnDateChanged(DateTime? value)
    //{
    //    if (!SupplementaryMode) return;

    //    var tip = "签约日期晚于申请日期";
    //    var max = SelectedRecords?.Any() ?? false ? SelectedRecords.Max(x => x.RequestDate) : RecordSource.View.Cast<TransferRecord>().Max(x => x.RequestDate);

    //    bool need = value is not null && DateOnly.FromDateTime(value.Value) > max;
    //    if (!need)
    //        Tips.Remove(tip);
    //    else if (!Tips.Contains(tip))
    //        Tips.Add(tip);
    //}



    //partial void OnSelectedRecordsChanged(IEnumerable<TransferRecord>? value)
    //{
    //    if (!SupplementaryMode || Date is null) return;

    //    var tip = "签约日期晚于申请日期";
    //    var max = SelectedRecords?.Any() ?? false ? SelectedRecords.Max(x => x.RequestDate) : RecordSource.View.Cast<TransferRecord>().Max(x => x.RequestDate);

    //    bool need = value is not null && DateOnly.FromDateTime(Date.Value) > max;
    //    if (!need)
    //        Tips.Remove(tip);
    //    else if (!Tips.Contains(tip))
    //        Tips.Add(tip);
    //}







    public virtual bool CanConfirm => true;


    [ObservableProperty]
    public partial string? Tips { get; set; }


    public bool IsSell => SelectedType != TransferOrderType.Buy;


    protected abstract void Check();



    [RelayCommand(CanExecute = nameof(CanConfirm))]
    public void Confirm(Window window)
    {
        ConfirmOverride();

        window.DialogResult = true;
        window.Close();
    }


    protected abstract void ConfirmOverride();


    private void DeleteFile(FileStorageInfo x)
    {
        if (Id == 0) //未保存
            return;

        using var db = DbHelper.Base();
        if (db.GetCollection<TransferOrder>().FindById(Id) is TransferOrder o)
        {
            if (Contract.File == x)
                o.Contract = null;
            else if (RiskDisclosure.File == x)
                o.RiskDiscloure = null;
            else if (RiskPair.File == x)
                o.RiskPair = null;
            else if (Review.File == x)
                o.Review = null;
            else if (Video.File == x)
                o.Videotape = null;
            else if (OrderFile.File == x)
                o.OrderSheet = null;

            db.GetCollection<TransferOrder>().Update(o);
        }
    }


    protected FileStorageInfo? SetFile(System.IO.FileInfo fi, string title, Action<TransferOrder, FileStorageInfo> func)
    {
        // 如果文件名中有日期
        if (Date is null && DateTimeHelper.TryFindDate(fi.Name) is DateOnly date)
            Date = new DateTime(date, default);


        if (Id == 0) //新增加的
        {
            return new FileStorageInfo(fi.FullName, "", DateTime.Now);
        }
        else
        {
            string hash = fi.ComputeHash()!;

            // 保存副本
            var dir = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "files", "order", Id.ToString()));

            var tar = FileHelper.CopyFile2(fi, dir.FullName);
            if (tar is null)
            {
                Log.Error($"保存文件出错，{fi.Name}");
                HandyControl.Controls.Growl.Error($"无法保存{fi.Name}，文件名异常或者存在过多重名文件");
                return null;
            }

            var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar);

            using var db = DbHelper.Base();
            var q = db.GetCollection<TransferOrder>().FindById(Id);

            FileStorageInfo fsi = new()
            {
                Title = title,
                Path = path,
                Hash = hash,
                Time = DateTime.Now
            };
            func(q, fsi);
            db.GetCollection<TransferOrder>().Update(q);
            return fsi;
        }
    }


    protected FileStorageInfo? SetFile2(FileInfo fi, string title, Action<TransferOrder, FileStorageInfo> func)
    {
        var fsi = SetFile(fi, title, func);

        // 如果是pdf，解析日期
        try
        {
            var texts = PdfHelper.GetTexts(fi.FullName);
            foreach (var txt in texts)
            {
                if(DateTimeHelper.TryFindDate(txt) is DateOnly d)
                {
                    Date = new DateTime(d, default);
                }
            }
        }
        catch { }
        return fsi;
    }

    protected FileStorageInfo? Move(FileStorageInfo? fsi)
    {
        if (fsi?.Path is null) return null;

        var fi = new FileInfo(fsi.Path);
        string hash = fi.ComputeHash()!;

        // 保存副本
        var dir = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "files", "order", Id.ToString()));

        var tar = FileHelper.CopyFile2(fi, dir.FullName);
        if (tar is null)
        {
            Log.Error($"保存文件出错，{fi.Name}");
            HandyControl.Controls.Growl.Error($"无法保存{fi.Name}，文件名异常或者存在过多重名文件");
            return null;
        }

        var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar);
        return new() { Title = "", Path = path, Hash = hash, Time = DateTime.Now };
    }

}


public partial class AddOrderWindowViewModel : AddOrderWindowViewModelBase
{
    public TransferOrderViewModel? Order { get; internal set; }


    public Fund[] Funds { get; set; }

    public CollectionViewSource FundSource { get; } = new();


    [ObservableProperty]
    public partial string? ShareClass { get; set; }

    [ObservableProperty]
    public partial string? SearchFundKey { get; set; }


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    public partial Fund? SelectedFund { get; set; }



    public Investor[] Investors { get; set; }

    public CollectionViewSource InvestorSource { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    public partial string? SearchInvestorKey { get; set; }


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    public partial Investor? SelectedInvestor { get; set; }




    public AddOrderWindowViewModel()
    {
        using var db = DbHelper.Base();
        Funds = db.GetCollection<Fund>().FindAll().ToArray();

        FundSource.Source = Funds;
        FundSource.Filter += (s, e) => e.Accepted = string.IsNullOrWhiteSpace(SearchFundKey) || SearchFundKey == SelectedFund?.Name ? true : e.Item switch { Fund f => f.Name.Contains(SearchFundKey), _ => true };


        Investors = db.GetCollection<Investor>().FindAll().ToArray();
        InvestorSource.Source = Investors;
        InvestorSource.Filter += (s, e) =>
        {
            e.Accepted = (string.IsNullOrWhiteSpace(SearchInvestorKey) || SearchInvestorKey == SelectedInvestor?.Name ? true : e.Item switch { Investor f => f.Name.Contains(SearchInvestorKey), _ => true });
        };




    }

    partial void OnSearchFundKeyChanged(string? value) => FundSource.View.Refresh();

    partial void OnSearchInvestorKeyChanged(string? value) => InvestorSource.View.Refresh();

    public override bool CanConfirm => SelectedFund is not null && SelectedInvestor is not null && Date is not null && SelectedType is not null && Number is not null;

    protected override void ConfirmOverride()
    {

        using var db = DbHelper.Base();
        db.BeginTrans();
        try
        {
            // 如果是新增加的
            if (Id == 0)
            {
                var obj = new TransferOrder();
                db.GetCollection<TransferOrder>().Insert(obj);
                Id = obj.Id;

                // 移动文件
                Contract.File = Move(Contract.File);
                RiskDisclosure.File = Move(RiskDisclosure.File);
                OrderFile.File = Move(OrderFile.File);
                Video.File = Move(Video.File);
                Contract.File = Move(Contract.File);
            }


            TransferOrder order = new TransferOrder
            {
                Id = Id,
                Date = DateOnly.FromDateTime(Date ?? default),
                FundId = SelectedFund!.Id,
                FundName = SelectedFund!.Name,
                ShareClass = ShareClass,
                InvestorId = SelectedInvestor!.Id,
                InvestorName = SelectedInvestor.Name,
                InvestorIdentity = SelectedInvestor.Identity?.Id,
                Type = SelectedType!.Value,
                Number = Number ?? 0,
                Contract = Contract.File,
                RiskDiscloure = RiskDisclosure.File,
                OrderSheet = OrderFile.File,
                Videotape = Video.File,
                RiskPair = RiskPair.File,
                Review = Review.File,
            };
            db.GetCollection<TransferOrder>().Upsert(order);
            db.Commit();

            WeakReferenceMessenger.Default.Send(order);
        }
        catch (Exception e)
        {
            db.Rollback();
            Log.Error($"添加交易订单失败，{e}");
        }
    }


    protected override void Check()
    {
        var tip = "";

        // 判断是否是首次
        bool need = false;
        bool needvideo = false;
        DateOnly date = Date is null ? default : DateOnly.FromDateTime(Date.Value);
        if (SelectedType == TransferOrderType.Buy)
        {
            using var db = DbHelper.Base();
            int fundid = SelectedFund?.Id ?? 0;
            var cid = SelectedInvestor?.Id ?? 0;
            var early = db.GetCollection<TransferRecord>().Find(x => x.FundId == fundid && x.CustomerId == cid).Min(x => x.RequestDate);

            if (Date is not null && early >= date) need = true;

            var q = db.GetCollection<InvestorQualification>().Find(x => x.InvestorId == cid).Where(x => x.Date <= date).OrderBy(x => x.Date).LastOrDefault();
            if (q is null || q.Result == QualifiedInvestorType.Normal) needvideo = true;
        }

        if (Date is not null && date < DateOnly.FromDateTime(Date.Value))
            tip += "签约日期晚于申请日期";

        if (need && !Contract.Exists)
            tip += " 缺少合同";

        if (need && !RiskDisclosure.Exists)
            tip += " 缺少风险揭示书";

        if (needvideo && !Video.Exists)
            tip += " 缺少双录";


        Tips = tip;
    }
}

public partial class SupplementaryOrderWindowViewModel : AddOrderWindowViewModelBase
{
    public SupplementaryOrderWindowViewModel(TransferRecordViewModel record)
    {
        Record = record;

        SelectedType = record.Type switch { TransferRecordType.Redemption or TransferRecordType.ForceRedemption => GetSellType(record), _ => TransferOrderType.Buy };


        Number = record.RequestAmount > 0 ? record.RequestAmount : record.RequestShare;


        IsSellTypeForzen = SelectedType == TransferOrderType.Share;


        // 检查是否存在已存在
        using var db = DbHelper.Base();
        var order = db.GetCollection<TransferOrder>().FindById(record.OrderId);

        if (order is not null)
        {
            Id = order.Id;
            Date = new DateTime(order.Date, default);
            Contract.File = order.Contract;
            OrderFile.File = order.OrderSheet;
            RiskDisclosure.File = order.RiskDiscloure;
            RiskPair.File = order.RiskPair;
            Video.File = order.Videotape;
            Review.File = order.Review;

        }

        if (SelectedType == TransferOrderType.Amount || SelectedType == TransferOrderType.RemainAmout)
            Types = [TransferOrderType.Amount, TransferOrderType.RemainAmout];
        else Types = [TransferOrderType.Share, TransferOrderType.Amount, TransferOrderType.RemainAmout];

        Check();
    }

    public TransferRecordViewModel Record { get; }


    public TransferRecord[]? SameDay { get; set; }

    public override TransferOrderType[] Types { get; }

    [ObservableProperty]
    public partial bool IsSellTypeForzen { get; set; }

    /// <summary>
    /// 同日的确认合并一个订单
    /// </summary>
    [ObservableProperty]
    public partial bool MergeOrderBySameDay { get; set; }

    public override bool CanConfirm => Date is not null && Number > 0 && SelectedType is not null;

    protected override void ConfirmOverride()
    {
        using var db = DbHelper.Base();
        db.BeginTrans();
        try
        {
            // 如果是新增加的
            if (Id == 0)
            {
                var obj = new TransferOrder();
                db.GetCollection<TransferOrder>().Insert(obj);
                Id = obj.Id;

                // 移动文件
                Contract.File = Move(Contract.File);
                RiskDisclosure.File = Move(RiskDisclosure.File);
                OrderFile.File = Move(OrderFile.File);
                Video.File = Move(Video.File);
                Review.File = Move(Review.File);
                RiskPair.File = Move(RiskPair.File);
            }


            TransferOrder order = new TransferOrder
            {
                Id = Id,
                Date = DateOnly.FromDateTime(Date ?? default),
                FundId = Record.FundId ?? 0,
                FundName = Record.FundName,
                ShareClass = Record.ShareClass,
                InvestorId = Record.CustomerId ?? 0,
                InvestorIdentity = Record.CustomerIdentity,
                InvestorName = Record.CustomerName,
                Type = SelectedType!.Value,
                Number = Number ?? 0,
                Contract = Contract.File,
                RiskDiscloure = RiskDisclosure.File,
                OrderSheet = OrderFile.File,
                Videotape = Video.File,
                RiskPair = RiskPair.File,
                Review = Review.File,
            };
            // 同日订单
            if (MergeOrderBySameDay)
            {
                var same = db.GetCollection<TransferRecord>().Find(x => x.ConfirmedDate == Record.ConfirmedDate && x.FundId == Record.FundId && x.CustomerId == Record.CustomerId).ToArray();
                foreach (var item in same)
                    item.OrderId = Id;

                DataTracker.LinkOrder(same);
                db.GetCollection<TransferRecord>().Update(same);
            }
            else
            {
                var rec = db.GetCollection<TransferRecord>().FindById(Record.Id);
                rec.OrderId = Id;
                db.GetCollection<TransferRecord>().Update(rec);
                DataTracker.LinkOrder(rec);
            }


            db.GetCollection<TransferOrder>().Upsert(order);
            db.Commit();

            WeakReferenceMessenger.Default.Send(order);
        }
        catch (Exception e)
        {
            db.Rollback();
            Log.Error($"添加交易订单失败，{e}");
        }
    }


    partial void OnMergeOrderBySameDayChanged(bool value)
    {
        if (value && SameDay is null)
        {
            using var db = DbHelper.Base();
            SameDay = db.GetCollection<TransferRecord>().Find(x => x.ConfirmedDate == Record.ConfirmedDate && x.FundId == Record.FundId && x.CustomerId == Record.CustomerId).ToArray();
        }

        if (!value)
            Number = Record.RequestAmount > 0 ? Record.RequestAmount : Record.RequestShare;
        else if (SameDay is not null)
            Number = SameDay.Sum(x => x.RequestAmount) switch { 0 => SameDay.Sum(x => x.RequestShare), var n => n };


    }

    private TransferOrderType GetSellType(TransferRecordViewModel record)
    {
        if (record.RequestAmount is null || record.RequestAmount == 0)
            return TransferOrderType.Share;
        else if (record.RequestAmount is not null && record.ConfirmedNetAmount is not null && Math.Abs(record.RequestAmount.Value - record.ConfirmedNetAmount.Value) < 10)
            return TransferOrderType.Amount;
        else return TransferOrderType.RemainAmout;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(Date):
                Check();
                break;
        }
    }

    protected override void Check()
    {
        var tip = "";

        // 判断是否是首次
        bool need = false;
        bool needvideo = false;
        if (SelectedType == TransferOrderType.Buy)
        {
            using var db = DbHelper.Base();
            var early = db.GetCollection<TransferRecord>().Find(x => x.FundId == Record.FundId && x.CustomerId == Record.CustomerId).Min(x => x.RequestDate);
            if (early == Record.RequestDate) need = true;

            var q = db.GetCollection<InvestorQualification>().Find(x => x.InvestorId == Record.CustomerId).Where(x => x.Date <= Record.RequestDate).OrderBy(x => x.Date).LastOrDefault();
            if (q is null || q.Result == QualifiedInvestorType.Normal) needvideo = true;
        }

        if (Date is not null && Record.RequestDate < DateOnly.FromDateTime(Date.Value))
            tip += "签约日期晚于申请日期";

        if (need && !Contract.Exists)
            tip += " 缺少合同";

        if (need && !RiskDisclosure.Exists)
            tip += " 缺少风险揭示书";

        if (need && needvideo && !Video.Exists)
            tip += " 缺少双录";


        Tips = tip;
    }

    internal void OnBatchFile(string[]? v)
    {
        if (v is null) return;

        if(v.Length == 1 && Path.GetExtension(v[0]) switch { "zip" or "gzip" or "rar" or "7z"=> true ,_=>false})
        {
            using var fs = new FileStream(v[0], FileMode.Open);
            using ZipArchive archive = new ZipArchive(fs);

            var file = archive.Entries.Where(x => x.Name.Contains("基金合同"));


        }
    }
}