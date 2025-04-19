using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.PDF;
using FMO.Utilities;
using Microsoft.Win32;
using Serilog;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

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
        var sas = db.GetCollection<SecurityCard>().Find(x => x.FundId == fundId);
        if (sas is not null) SecurityCards = new(sas.Select(x => new SecurityCardViewModel(x)));


        var fa = db.GetCollection<FundAccounts>().FindById(FundId);
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
                db.GetCollection<FundAccounts>().Update(fa);
            }
        }
    }

    public int FundId { get; }
    public string Code { get; }
    public string[] Names { get; }

    [ObservableProperty]
    public partial ObservableCollection<SecurityCardViewModel>? SecurityCards { get; set; }


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





    /// <summary>
    /// 添加股卡
    /// </summary>
    [RelayCommand]
    public void AddSecurityCard()
    {
        var fd = new OpenFileDialog();
        fd.Filter = "PDF|*.pdf";
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


                HandyControl.Controls.Growl.Info($"已解析{cnt}个股卡{(list.Count < cnt ? $"，{cnt-list.Count}个不属于本产品":"")}"); 
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
}


public partial class SecurityCardViewModel : ObservableObject
{
    public SecurityCardViewModel(SecurityCard x)
    {
        Id = x.Id;
        FundId = x.FundId;
        Name = x.Name;
        SerialNo = x.SerialNo;
        CardNo = x.CardNo;
        UniversalNo = x.UniversalNo;
        FundCode = x.FundCode;
        Date = x.Date;
        Tag = x.Type switch { SecurityCardType.ShangHai => "沪", SecurityCardType.ShenZhen => "深", _ => x.CardNo.StartsWith('B') ? "沪" : "深" };
        File = new FileInfo(@$"files\accounts\security\{SerialNo}-{CardNo}.pdf");
    }

    public int Id { get; set; }

    public int FundId { get; set; }

    /// <summary>
    /// 流水号
    /// </summary>
    [ObservableProperty] public partial string? SerialNo { get; set; }

    /// <summary>
    /// 子账户号
    /// </summary>
    [ObservableProperty] public partial string? CardNo { get; set; }

    /// <summary>
    /// 一码通
    /// </summary>
    [ObservableProperty] public partial string? UniversalNo { get; set; }

    [ObservableProperty] public partial string? Name { get; set; }

    [ObservableProperty] public partial string? FundCode { get; set; }

    /// <summary>
    /// 申请日期
    /// </summary>
    public DateOnly Date { get; set; }

    public string Tag { get; set; }

    public FileInfo File { get; set; }

    [RelayCommand]
    public void View()
    {
        if (File?.Exists ?? false)
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(File.FullName) { UseShellExecute = true }); } catch { }
    }

    [RelayCommand]
    public void Print()
    {
        if (File is null || !File.Exists) return;


        PrintDialog printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            // 获取默认打印机名称
            string printerName = printDialog.PrintQueue.Name;

            // 使用系统默认的PDF阅读器打印PDF文档
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = File.FullName;
            process.StartInfo.Verb = "print";
            process.Start();

            // 等待打印任务完成
            process.WaitForExit();
        }
    }


    [RelayCommand]
    public void SaveAs()
    {
        if (File is null || !File.Exists) return;

        try
        {
            var d = new SaveFileDialog();
            d.FileName = File.Name;
            d.DefaultExt = ".pdf";
            d.Filter = "Pdf文件|*.pdf";
            if (d.ShowDialog() == true)
                System.IO.File.Copy(File.FullName, d.FileName);
        }
        catch (Exception ex)
        {
            Log.Error($"文件另存为失败: {ex.Message}");
        }
    }

    [RelayCommand]
    public void Copy()
    {
        Clipboard.SetDataObject(new DataObject(CardNo));
    }
}