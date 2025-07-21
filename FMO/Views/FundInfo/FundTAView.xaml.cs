using System.IO;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.TPL;
using FMO.Utilities;
using Microsoft.Win32;

namespace FMO;

/// <summary>
/// FundTAView.xaml 的交互逻辑
/// </summary>
public partial class FundTAView : UserControl
{
    public FundTAView()
    {
        InitializeComponent();
    }
}



public partial class FundTAViewModel : ObservableObject
{
    public FundTAViewModel(int fundId)
    {
        FundId = fundId;

        using var db = DbHelper.Base();
        Fund fund = db.GetCollection<Fund>().FindById(fundId);
        FundName = fund.Name;

        IsCleared = fund.Status > FundStatus.StartLiquidation;
        var ta = db.GetCollection<TransferRecord>().Find(x => x.FundId == fundId).ToList();
        var ts = db.GetCollection<FundShareRecord>().Find(x => x.FundId == fundId).OrderBy(x => x.Date).ToList();
        Records = ta;

        // customeid = 0
        foreach (var item in ta.Where(x=>x.CustomerId == 0)) 
            item.CustomerId = item.CustomerIdentity.GetHashCode();

        // 对齐开放日净值
        var ds = db.GetDailyCollection(fundId).FindAll().OrderByDescending(x => x.Date);
        //foreach (var item in ta)
        //{
        //    var d = ds.FirstOrDefault(x => x.Date < item.ConfirmedDate);
        //}

        var daily = ds.FirstOrDefault(x => x.NetValue > 0);
        var nv = daily?.NetValue ?? 0;
        NetValueDate = daily?.Date;
        Daily = daily;

        if (ta.Count > 0)
        {
            // 按投资人分
            var cur = ta.GroupBy(x => x.CustomerId).Select(x => new { Id = x.Key, Name = x.First().CustomerName, Record = x, Share = x.Sum(y => y.ShareChange()) }).OrderByDescending(x => x.Share);
            InvestorCount = cur.Count(x => x.Share > 0);
            CurrentTotalShare = cur.Sum(x => x.Share);
            CurrentShareDate = ta.Max(x => x.ConfirmedDate);
            CurrentShares = cur.Select(x => new InvestorShareViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Share = x.Share,
                Asset = x.Share * nv,
                Deposit = x.Record.Where(x => x.Type switch { TransferRecordType.Subscription or TransferRecordType.Purchase or TransferRecordType.MoveIn => true, _ => false }).Sum(x => x.ConfirmedNetAmount),
                Withdraw = x.Record.Where(x => x.Type switch { TransferRecordType.Redemption or TransferRecordType.Redemption or TransferRecordType.MoveOut or TransferRecordType.Distribution => true, _ => false }).Sum(x => x.ConfirmedNetAmount),
                Proportion = x.Share == 0 ? null : x.Share / CurrentTotalShare
                //Profit = x.Record.Sum(y => y.AmountChange()) + x.Share * nv
            }).ToList();
            CurrentTotalProfit = CurrentShares.Sum(x => x.Profit);
        }

        if (DataTracker.FundTips.Any(x => x.FundId == fundId && x.Type == TipType.FundShareNotPair))
            CurrentShareNotPair = true;

        // 认购时份额
        var subscription = ta.Where(x => x.Type == TransferRecordType.Subscription);
        InitialShares = new(subscription.Select(x => new InvestorShareViewModel { Id = x.Id, Name = x.CustomerName, Share = x.ConfirmedShare }));
        InitialTotalShare = subscription.Sum(x => x.ConfirmedShare);
    }

    public int FundId { get; }

    public IList<TransferRecord> Records { get; set; }

    public string FundName { get; }

    /// <summary>
    /// 清盘
    /// </summary>
    [ObservableProperty]
    public partial bool IsCleared { get; set; }

    [ObservableProperty]
    public partial int InvestorCount { get; set; }

    [ObservableProperty]
    public partial decimal CurrentTotalShare { get; set; }


    [ObservableProperty]
    public partial decimal CurrentTotalProfit { get; set; }

    [ObservableProperty]
    public partial DateOnly CurrentShareDate { get; set; }

    [ObservableProperty]
    public partial List<InvestorShareViewModel>? CurrentShares { get; set; }

    [ObservableProperty]
    public partial bool CurrentShareNotPair { get; set; }

    [ObservableProperty]
    public partial DateOnly? NetValueDate { get; set; }

    public DailyValue? Daily { get; set; }


    [ObservableProperty]
    public partial List<InvestorShareViewModel>? InitialShares { get; set; }


    [ObservableProperty]
    public partial decimal InitialTotalShare { get; set; }


    [ObservableProperty]
    public partial bool InvestorDetailIsOpen { get; set; }

    [ObservableProperty]
    public partial IEnumerable<TransferRecord>? InvestorDetail { get; set; }

    [RelayCommand]
    public void ShowInvestorDetail(InvestorShareViewModel v)
    {
        InvestorDetailIsOpen = true;
        InvestorDetail = Records.Where(x => x.CustomerId == v.Id);
    }


    [RelayCommand]
    public void Export()
    {
        if (CurrentShares is null || Daily is null) return;


        // 默认模板
        var tplfile = "invester_share.xlsx";
        if (Tpl.IsExists(tplfile))
        {
            tplfile = Tpl.GetPath(tplfile);
        }
        else
        {
            var dlg = new OpenFileDialog();
            dlg.Title = "选择表格模板";
            dlg.Filter = "Excel|*.xlsx";
            if (dlg.ShowDialog() switch { false or null => true, _ => false })
                return;

            tplfile = dlg.FileName;
        }
        try
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"{FundName}-份额.xlsx");

            using var db = DbHelper.Base();
            var customers = db.GetCollection<Investor>().FindAll().ToArray();

            var gend= CurrentShares.OrderByDescending(x=> x.Share).Take(10).Join(customers, x => x.Id, x => x.Id, (x, y) => new
            {
                Name = x.Name,
                ID = y.Identity.Id,
                Amount = x.Asset,
                Portion = x.Proportion,
                Phone = y.Phone,
                Addr = y.Address
            });


            Tpl.Generate(path, tplfile, new { ii = gend });

        }
        catch (Exception)
        {
        }
    }


    [RelayCommand]
    public void GoToTA()
    {
        WeakReferenceMessenger.Default.Send(new OpenPageMessage("TA"));
        WeakReferenceMessenger.Default.Send(new PageTAMessage(2, FundName));
    }
}

public class InvestorShareViewModel
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public decimal Share { get; set; }

    /// <summary>
    /// 市值
    /// </summary>
    public decimal Asset { get; set; }


    public decimal Deposit { get; set; }

    public decimal Withdraw { get; set; }

    /// <summary>
    /// 当前份额占比
    /// </summary>
    public decimal? Proportion { get; set; }

    public decimal Profit => Asset + Withdraw - Deposit;
}
