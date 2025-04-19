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
        if (sas is not null) SecurityCards = new (sas);
    }

    public int FundId { get; }
    public string Code { get; }
    public string[] Names { get; }

    [ObservableProperty]
    public partial ObservableCollection<SecurityCard>? SecurityCards { get; set; }






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

                    m = Regex.Match(c.text, @"客户名称\s*[:：]\s*(\w+)");
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

                    m = Regex.Match(c.text, @"(?:开立|申请|受理)日期\s*[:：]\s*([\w\s]+)");
                    if (!m.Success)
                    {
                        Log.Error($"解析股卡失败:\n {c.text}");
                        continue;
                    }
                    var g = m.Groups[1].Value;
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

                    // 保存文件
                    using var outf = new FileStream(@$"files\accounts\security\{a}-{e}.pdf", FileMode.Create);
                    outf.Write(c.page);
                    outf.Flush();

                    if (FindFund(sa))
                    {
                        using var db = DbHelper.Base();
                        db.GetCollection<SecurityCard>().Insert(sa);
                    }

                    if (sa.FundId == FundId)                    
                        list.Add(sa);
                    
                }

                if(list.Count > 0)
                {
                    if (SecurityCards is null)
                        SecurityCards = new(list);
                    else
                        foreach (var l in list)
                            SecurityCards.Add(l);
                }
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
        }

        Log.Error($"股卡{sa.SerialNo}-{sa.CardNo}未找到对应的基金");
        return false;
    }
}