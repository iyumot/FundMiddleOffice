using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.PDF;
using FMO.Shared;
using FMO.Utilities;
using Microsoft.Win32;
using Serilog;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FMO;

/// <summary>
/// FundAccountsView.xaml 的交互逻辑
/// </summary>
public partial class FundAccountsView : UserControl
{
    public FundAccountsView()
    {
        InitializeComponent();
    }
}


public partial class FundAccountsViewModel : ObservableObject
{
    public FundAccountsViewModel(int fundId, string code, string[] names)
    {
        FundId = fundId;
        Code = code;
        Names = names;

        using var db = DbHelper.Base();
        var sas = db.GetCollection<SecurityCard>().Find(x => x.FundId == fundId).ToArray();
        if (sas is not null)
            SecurityCards = new(sas.Select(x => new SecurityCardViewModel(x)));

        var sa = db.GetCollection<StockAccount>().Find(x => x.FundId == fundId).ToArray();
        if (sa is not null)
            StockAccounts = new(sa.Select(x => new StockAccountViewModel(x)));

        var fas = db.GetCollection<FutureAccount>().Find(x => x.FundId == fundId).ToArray();
        if (fas is not null)
            FutureAccounts = new(fas.Select(x => new FutureAccountViewModel(x)));

        var fa = db.GetCollection<FundSingletonAccounts>().FindById(FundId);
        Singleton = new(fa ?? new() { Id = fundId });
        UniversalNo = fa?.UniversalNo;
        FutureNo = fa?.FutureNo;
        CCDCBondAccount = fa?.CCDCBondAccount;
        SHCBondAccount = fa?.SHCBondAccount;

        if (UniversalNo is null && sas?.Select(x => x.UniversalNo).Distinct().Where(x => !string.IsNullOrWhiteSpace(x)) is IEnumerable<string> s)
        {
            UniversalNo = s.FirstOrDefault();

            if (UniversalNo is not null)
            {
                if (fa is null) fa = new();
                fa.UniversalNo = UniversalNo;
                db.GetCollection<FundSingletonAccounts>().Update(fa);
            }
        }


        SecurityCompanies = new CollectionViewSource();
        try
        {
            if (File.Exists("config\\securitycompany.txt"))
            {
                using var sr = new StreamReader("config\\securitycompany.txt");
                var arr = sr.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);

                var regex = new Regex("(?:股份)*有限(?:责任)*公司");
                SecurityCompanies.Source = arr.Select(x => regex.Replace(x, "")).ToArray();
            }
            else
            {
                var fs = Application.GetResourceStream(new Uri("res/securitycompany.txt", UriKind.Relative));
                using var sr = new StreamReader(fs.Stream);
                var arr = sr.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);

                var regex = new Regex("(?:股份)*有限(?:责任)*公司");
                SecurityCompanies.Source = arr.Select(x => regex.Replace(x, "")).ToArray();
            }

            SecurityCompanies.Filter += (s, e) => e.Accepted = string.IsNullOrWhiteSpace(SecurityCompanyKeyword) ? true : e.Item switch { string ss => ss.Contains(SecurityCompanyKeyword), _ => true };


        }
        catch (Exception e)
        {
            Log.Error($"加载证券公司列表失败{e.Message}");
        }



        FutureCompanies = new CollectionViewSource();
        try
        {
            if (File.Exists("config\\Futurecompany.txt"))
            {
                using var sr = new StreamReader("config\\Futurecompany.txt");
                var arr = sr.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);

                var regex = new Regex("(?:股份)*有限(?:责任)*公司");
                FutureCompanies.Source = arr.Select(x => regex.Replace(x, "")).ToArray();
            }
            else
            {
                var fs = Application.GetResourceStream(new Uri("res/Futurecompany.txt", UriKind.Relative));
                using var sr = new StreamReader(fs.Stream);
                var arr = sr.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);

                var regex = new Regex("(?:股份)*有限(?:责任)*公司");
                FutureCompanies.Source = arr.Select(x => regex.Replace(x, "")).ToArray();
            }

            FutureCompanies.Filter += (s, e) => e.Accepted = string.IsNullOrWhiteSpace(FutureCompanyKeyword) ? true : e.Item switch { string ss => ss.Contains(FutureCompanyKeyword), _ => true };


        }
        catch (Exception e)
        {
            Log.Error($"加载期货公司列表失败{e.Message}");
        }


    }

    public int FundId { get; }
    public string Code { get; }
    public string[] Names { get; }

    [ObservableProperty]
    public partial ObservableCollection<SecurityCardViewModel>? SecurityCards { get; set; }


    #region Stock

    [ObservableProperty]
    public partial bool ShowChooseStockCompany { get; set; }

    public CollectionViewSource SecurityCompanies { get; set; }



    [ObservableProperty]
    public partial string? SecurityCompanyKeyword { get; set; }

    private AutoResetEvent ChooseStockCompanyEvent { get; } = new(false);

    [ObservableProperty]
    public partial string? SelectedSecurityCompany { get; set; }


    [ObservableProperty]
    public partial ObservableCollection<StockAccountViewModel>? StockAccounts { get; set; }

    #endregion

    #region Future

    public CollectionViewSource FutureCompanies { get; set; }

    [ObservableProperty]
    public partial bool ShowChooseFutureCompany { get; set; }
     

    [ObservableProperty]
    public partial string? FutureCompanyKeyword { get; set; }

    private AutoResetEvent ChooseFutureCompanyEvent { get; } = new(false);

    [ObservableProperty]
    public partial string? SelectedFutureCompany { get; set; }


    [ObservableProperty]
    public partial ObservableCollection<FutureAccountViewModel>? FutureAccounts { get; set; }

    #endregion




    [ObservableProperty]
    public partial string? UniversalNo { get; set; }

    /// <summary>
    /// 统一开户编码
    /// </summary>
    [ObservableProperty]
    public partial string? FutureNo { get; set; }

    /// <summary>
    /// 中债登债券账户
    /// </summary>
    [ObservableProperty]
    public partial string? CCDCBondAccount { get; set; }

    /// <summary>
    /// 上清所债券账户
    /// </summary>
    [ObservableProperty]
    public partial string? SHCBondAccount { get; set; }

    [ObservableProperty]
    public partial FundSingletonAccountsViewModel Singleton { get; set; }



    /// <summary>
    /// 添加股卡
    /// </summary>
    [RelayCommand]
    public void AddSecurityCard()
    {
        var fd = new OpenFileDialog();
        fd.Filter = "PDF|*.pdf";
        fd.Multiselect = true;
        var dr = fd.ShowDialog();
        if (dr is null || !dr.Value) return;

        List<SecurityCard> list = new();
        int cnt = 0;
        foreach (var f in fd.FileNames)
        {
            try
            {
                using var fs = new FileStream(f, FileMode.Open);

                var result = PdfHelper.GetSecurityAccounts(fs);
                if (result is null) continue;

                // 解析
                foreach (var c in result)
                {
                    var m = Regex.Match(c.text, @"流水号\s*[:：]\s*(\w+)");
                    if (!m.Success)
                    {
                        Log.Error($"解析股卡失败:\n {c.text}");
                        continue;
                    }
                    var a = m.Groups[1].Value;

                    m = Regex.Match(c.text, @"一码通\w*号码\s*[:：]\s*(\w+)");
                    if (!m.Success)
                    {
                        Log.Error($"解析股卡失败:\n {c.text}");
                        continue;
                    }
                    var b = m.Groups[1].Value;

                    m = Regex.Match(c.text, @"客户名称\s*[:：]\s*([\w（）\(\)]+\s*[-－]\w+)");
                    if (!m.Success)
                    {
                        Log.Error($"解析股卡失败:\n {c.text}");
                        continue;
                    }
                    var d = m.Groups[1].Value;

                    m = Regex.Match(c.text, @"\b(?:子账户号码|证券账户号码)\s*[:：]\s*(\w+)");
                    if (!m.Success)
                    {
                        Log.Error($"解析股卡失败:\n {c.text}");
                        continue;
                    }
                    var e = m.Groups[1].Value;

                    m = Regex.Match(c.text, @"(?:开立|受理)日期\s*[:：]\s*([\d年月日\s]+)");
                    if (!m.Success)
                    {
                        Log.Error($"解析股卡失败:\n {c.text}");
                        continue;
                    }
                    var g = m.Groups[1].Value.Trim();
                    if (!DateTimeHelper.TryParse(g, out var date))
                    {
                        Log.Error($"解析股卡失败:\n {c.text}");
                        continue;
                    }

                    var sa = new SecurityCard
                    {
                        SerialNo = a,
                        CardNo = e,
                        Name = d,
                        UniversalNo = b,
                        Date = date
                    };
                    m = Regex.Match(c.text, @"产品编码\s*[:：]\s*(\w+)");
                    if (m.Success)
                        sa.FundCode = m.Groups[1].Value;


                    if (c.text.Contains("沪市A"))
                        sa.Type = SecurityCardType.ShangHai;
                    else if (c.text.Contains("深市A"))
                        sa.Type = SecurityCardType.ShenZhen;

                    // 保存文件
                    using var outf = new FileStream(@$"files\accounts\security\{a}-{e}.pdf", FileMode.Create);
                    outf.Write(c.page);
                    outf.Flush();

                    if (FindFund(sa))
                    {
                        using var db = DbHelper.Base();
                        db.GetCollection<SecurityCard>().EnsureIndex(x => x.SerialNo, true);
                        var old = db.GetCollection<SecurityCard>().FindOne(x => x.SerialNo == sa.SerialNo);
                        if (old is not null) sa.Id = old.Id;
                        db.GetCollection<SecurityCard>().Upsert(sa);

                        ++cnt;
                    }

                    if (sa.FundId == FundId)
                        list.Add(sa);

                }

                if (list.Count > 0)
                {
                    if (SecurityCards is null)
                        SecurityCards = new(list.Select(x => new SecurityCardViewModel(x)));
                    else
                        foreach (var l in list)
                            SecurityCards.Add(new(l));
                }


                HandyControl.Controls.Growl.Info($"已解析{cnt}个股卡{(list.Count < cnt ? $"，{cnt - list.Count}个不属于本产品" : "")}");
            }
            catch (Exception e)
            {

            }
        }

    }

    bool FindFund(SecurityCard sa)
    {
        // 解析对应的基金 
        if (sa.FundCode?.Length > 0)
        {
            if (Code == sa.FundCode)
            {
                sa.FundId = FundId;
                return true;
            }
            else
            {
                using var db = DbHelper.Base();
                var fund = db.GetCollection<Fund>().FindOne(x => x.Code == sa.FundCode);
                if (fund is not null)
                {
                    sa.FundId = fund.Id;
                    return true;
                }
            }
        }

        if (sa.Name?.Length > 10)
        {
            if (Names.Any(x => sa.Name.Contains(x)))
            {
                sa.FundId = FundId;
                return true;
            }
            else
            {
                using var db = DbHelper.Base();
                var fund = db.GetCollection<Fund>().FindOne(x => sa.Name.Contains(x.Name));
                if (fund is not null)
                {
                    sa.FundId = fund.Id;
                    return true;
                }
            }
        }

        Log.Error($"股卡{sa.SerialNo}-{sa.CardNo}未找到对应的基金");
        return false;
    }


    [RelayCommand]
    public void AddStock()
    {
        ShowChooseStockCompany = true;

        ChooseStockCompanyEvent.Reset();
        SelectedSecurityCompany = null;


        Task.Run(() =>
        {
            ChooseStockCompanyEvent.WaitOne();

            ShowChooseStockCompany = false;

            if (!string.IsNullOrWhiteSpace(SelectedSecurityCompany))
                CreateStockAccount(SelectedSecurityCompany.Trim());

        });
    }

    [RelayCommand]
    public void DeleteStock(StockAccountViewModel v)
    {
        using var db = DbHelper.Base();
        db.GetCollection<StockAccount>().Delete(v.Id);
        StockAccounts?.Remove(v);
    }


    #region Stock

    private void CreateStockAccount(string selectedSecurityCompany)
    {
        App.Current.Dispatcher.BeginInvoke(() =>
        {
            var obj = new StockAccount
            {
                FundId = FundId,
                Company = selectedSecurityCompany,
                Common = new OpenAccountEvent { Name = "基本账户" },
                Credit = new OpenAccountEvent { Name = "信用账户" }
            };

            using var db = DbHelper.Base();
            db.GetCollection<StockAccount>().Insert(obj);


            if (StockAccounts is null)
                StockAccounts = [new StockAccountViewModel(obj)];
            else
                StockAccounts.Add(new StockAccountViewModel(obj));
        });
    }

    partial void OnShowChooseStockCompanyChanged(bool value)
    {
        if (!value)
        {
            ChooseStockCompanyEvent.Set();
        }
    }

    partial void OnSelectedSecurityCompanyChanged(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            ChooseStockCompanyEvent.Set();
    }


    partial void OnSecurityCompanyKeywordChanged(string? value)
    {
        SecurityCompanies.View.Refresh();
    }
    #endregion


    [RelayCommand]
    public void AddFuture()
    {
        ShowChooseFutureCompany = true;

        ChooseFutureCompanyEvent.Reset();
        SelectedFutureCompany = null;


        Task.Run(() =>
        {
            ChooseFutureCompanyEvent.WaitOne();

            ShowChooseFutureCompany = false;

            if (!string.IsNullOrWhiteSpace(SelectedFutureCompany))
                CreateFutureAccount(SelectedFutureCompany.Trim());

        });
    }

    [RelayCommand]
    public void DeleteFuture(FutureAccountViewModel v)
    {
        using var db = DbHelper.Base();
        db.GetCollection<FutureAccount>().Delete(v.Id);
        FutureAccounts?.Remove(v);
    }


    #region Future

    private void CreateFutureAccount(string selectedFutureCompany)
    {
        App.Current.Dispatcher.BeginInvoke(() =>
        {
            var obj = new FutureAccount
            {
                FundId = FundId,
                Company = selectedFutureCompany,
                Common = new OpenAccountEvent { Name = "基本账户" }
            };

            using var db = DbHelper.Base();
            db.GetCollection<FutureAccount>().Insert(obj);


            if (FutureAccounts is null)
                FutureAccounts = [new FutureAccountViewModel(obj)];
            else
                FutureAccounts.Add(new FutureAccountViewModel(obj));
        });
    }

    partial void OnShowChooseFutureCompanyChanged(bool value)
    {
        if (!value)
        {
            ChooseFutureCompanyEvent.Set();
        }
    }

    partial void OnSelectedFutureCompanyChanged(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            ChooseFutureCompanyEvent.Set();
    }


    partial void OnFutureCompanyKeywordChanged(string? value)
    {
        FutureCompanies.View.Refresh();
    }
    #endregion




}


public partial class FundSingletonAccountsViewModel : EditableControlViewModelBase<FundSingletonAccounts>
{ 
    public ChangeableViewModel<FundSingletonAccounts, string?> UniversalNo { get; }
    public ChangeableViewModel<FundSingletonAccounts, string?> FutureNo { get; }

    public ChangeableViewModel<FundSingletonAccounts, string?> CCDCBondAccount { get; }

    public ChangeableViewModel<FundSingletonAccounts, string?> SHCBondAccount { get; }


    public FundSingletonAccountsViewModel(FundSingletonAccounts obj)
    {
        Id = obj.Id;

        UniversalNo = new()
        {
            Label = "证券一码通",
            InitFunc = x => x.UniversalNo,
            UpdateFunc = (x, y) => x.UniversalNo = y,
            ClearFunc = x => x.UniversalNo = null
        };
        UniversalNo.Init(obj);

        FutureNo = new()
        {
            Label = "统一开户编码",
            InitFunc = x => x.FutureNo,
            UpdateFunc = (x, y) => x.FutureNo = y,
            ClearFunc = x => x.FutureNo = null
        };
        FutureNo.Init(obj);

        CCDCBondAccount = new()
        {
            Label = "中债登债券账户",
            InitFunc = x => x.CCDCBondAccount,
            UpdateFunc = (x, y) => x.CCDCBondAccount = y,
            ClearFunc = x => x.CCDCBondAccount = null
        };
        CCDCBondAccount.Init(obj);

        SHCBondAccount = new()
        {
            Label = "上清所债券账户",
            InitFunc = x => x.SHCBondAccount,
            UpdateFunc = (x, y) => x.SHCBondAccount = y,
            ClearFunc = x => x.SHCBondAccount = null
        };
        SHCBondAccount.Init(obj);
    }


    protected override FundSingletonAccounts InitNewEntity()
    {
        return new FundSingletonAccounts { Id = Id };
    }
}