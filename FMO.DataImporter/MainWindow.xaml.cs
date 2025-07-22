using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.PDF;
using FMO.Utilities;
using Microsoft.Win32;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Windows;

namespace FMO.DataImporter;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        Directory.SetCurrentDirectory(@"d:\fmo");
        InitializeComponent();
    }
}



public partial class MainWindowViewModel : ObservableObject
{
    private object cus;

    public MainWindowViewModel()
    {
        Directory.CreateDirectory(@"importreport");
    }

    [RelayCommand]
    public void ImportOrderFromMeishi()
    {
        var fd = new OpenFileDialog();
        fd.Filter = "压缩文件|*.zip;*.rar;*.gzip;*.7z";
        if (fd.ShowDialog() switch { true => false, _ => true })
            return;

        using var sw = new StreamWriter(@$"importreport\{nameof(ImportOrderFromMeishi)}-{DateTime.Now:yyyyMMdd}.txt");

        var path = fd.FileName;
        using var fs = new FileStream(path, FileMode.Open);
        using ZipArchive zip = new ZipArchive(fs);

        // 产品名-投资人 

        Regex regex = new Regex(@"(\w+)-([^/]+)/[^/]+");
        var folders = zip.Entries.Select(x => (x, regex.Match(x.FullName))).Where(x => x.Item2.Success).
            Select(x => new { Item = x.x, FundName = x.Item2.Groups[1].Value, Customer = x.Item2.Groups[2].Value }).ToList();

        using var db = DbHelper.Base();
        var funds = db.GetCollection<Fund>().FindAll().ToArray();
        var customers = db.GetCollection<Investor>().FindAll().ToArray();
        var tas = db.GetCollection<TransferRecord>().FindAll().ToArray();
        var orders = db.GetCollection<TransferOrder>().FindAll().OrderBy(x => x.Date).ToArray();

        // 备份
        db.GetCollection<TransferRecord>($"TransferRecord_bak_{DateTime.Now:yyyyMMdd_HHmmss}").Insert(tas);
        db.GetCollection<TransferOrder>($"TransferOrder_bak_{DateTime.Now:yyyyMMdd_HHmmss}").Insert(orders);

        // first
        foreach (var item in orders.GroupBy(x => x.FundId))
        {
            foreach (var xx in item.GroupBy(x => x.InvestorId))
            {
                TransferOrder o = xx.First();
                if (o.Type == TransferOrderType.Buy)
                    o.Type = TransferOrderType.FirstTrade;
                else if (o.Type != TransferOrderType.FirstTrade)
                    sw.WriteLine($"{o.FundName} {o.InvestorName} {o.Date} {o.Type}  异常");
            }
        }


        foreach (var fundf in folders.GroupBy(x => x.FundName))
        {
            var fn = fundf.Key;

            // 查找对应的基金
            var (fund, sc) = db.FindByName(fn);
            if (fund is null)
            {
                sw.WriteLine($"{fn} 数据库中不存在此产品");
                continue;
            }
            sw.WriteLine($"{fn}");

            foreach (var customerfiles in fundf.GroupBy(x => x.Customer))
            {
                var cn = customerfiles.Key;

                var cuss = customers.Where(x => x.Name == cn).ToArray();
                if (cuss.Length == 0)
                {
                    sw.WriteLine($"{fn} 数据库中不存在此投资人");
                    continue;
                }

                Investor cus;
                if (cuss.Length > 1) //重名
                {
                    // 从ta判断 
                    var cusinfund = tas.Where(x => x.FundId == fund.Id).Select(x => x.CustomerId).Distinct().
                        Join(customers, x => x, x => x.Id, (x, y) => y).Where(x => x.Name == cn).ToArray();

                    if (cusinfund.Length == 1)
                        cus = cusinfund[0];
                    else
                    {
                        sw.WriteLine($"{fn} 投资人重名且无法区分，{cn}");
                        continue;
                    }
                }
                else cus = cuss[0];

                //首次
                var avafiles = customerfiles.ToList();
                var sheet = customerfiles.FirstOrDefault(x => x.Item.Name.Contains("认购申请表"));
                if (sheet is not null)
                {

                    // 获取签名日期
                    using var ss = sheet.Item.Open();
                    using var ms = new MemoryStream();
                    ss.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    var signDate = PdfHelper.GetSignDate(ms);
                    if (signDate is null)
                    {
                        sw.WriteLine($"\t\t{cn} 无法获取文件签约日期");

                        // 保存
                        Directory.CreateDirectory(@"importreport\meishi");
                        using var save = new FileStream(@$"importreport\meishi\{sheet.Item.Name}", FileMode.Create);
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.CopyTo(save);
                        save.Flush();

                        continue;
                    }

                    // 获取文件
                    var texts = PdfHelper.GetTexts(ms);
                    var number = Regex.Matches(texts[0], @"金额为.*?([\d\.,]+)").Select(x => decimal.TryParse(x.Groups[1].Value, out var n) ? n : 0).LastOrDefault(x => x > 0);
                    if (number == 0) number = Regex.Matches(texts[0], @"([\d\.,]+)\s?\w+万").Select(x => decimal.TryParse(x.Groups[1].Value, out var n) ? n : 0).LastOrDefault(x => x > 0);

                    if (number < 10)
                    {
                        sw.WriteLine($"\t\t\t {sheet.Item.Name}金额异常");
                    }
                    sw.WriteLine($"\t\t{cn}");

                    // 检查是否存在
                    var old = orders.FirstOrDefault(x => x.FundId == fund.Id && x.InvestorId == cus.Id && x.Date == signDate && x.Type == TransferOrderType.FirstTrade);

                    TransferOrder order = new TransferOrder
                    {
                        FundId = fund.Id,
                        FundName = fund.Name,
                        InvestorId = cus.Id,
                        InvestorIdentity = cus.Identity.Id,
                        InvestorName = cus.Name,
                        Type = TransferOrderType.FirstTrade,
                        Number = number,
                        ShareClass = sc,
                        Date = signDate.Value,
                        Source = "import"
                    };

                    if (old is null)
                        db.GetCollection<TransferOrder>().Insert(order);
                    else order = old;//order.Id = old.Id;

                    // 保存文件
                    var di = Directory.CreateDirectory(@$"files\order\{order.Id}");
                    order.OrderSheet = SaveTo(ms, "申请表", Path.Combine(di.FullName, sheet.Item.Name));

                    var file = customerfiles.FirstOrDefault(x => x.Item.Name.Contains("合同"));
                    if (file is not null)
                    {
                        using (var sss = file.Item.Open())
                            order.Contract = SaveTo(sss, "基金合同", Path.Combine(di.FullName, file.Item.Name));
                        avafiles.Remove(file);
                    }

                    file = customerfiles.FirstOrDefault(x => x.Item.Name.Contains("风险揭示书"));
                    if (file is not null)
                    {
                        using (var sss = file.Item.Open())
                            order.RiskDiscloure = SaveTo(sss, "风险揭示书", Path.Combine(di.FullName, file.Item.Name));
                        avafiles.Remove(file);
                    }

                    file = customerfiles.FirstOrDefault(x => Regex.IsMatch(x.Item.Name, "风险匹配|风险告知"));
                    if (file is not null)
                    {
                        using (var sss = file.Item.Open())
                            order.RiskPair = SaveTo(sss, "风险匹配告知书", Path.Combine(di.FullName, file.Item.Name));
                        avafiles.Remove(file);
                    }

                    file = customerfiles.FirstOrDefault(x => x.Item.Name.Contains("双录"));
                    if (file is not null)
                    {
                        using (var sss = file.Item.Open())
                            order.Videotape = SaveTo(sss, "双录", Path.Combine(di.FullName, file.Item.Name));
                        avafiles.Remove(file);
                    }

                    file = customerfiles.FirstOrDefault(x => x.Item.Name.Contains("回访"));
                    if (file is not null)
                    {
                        using (var sss = file.Item.Open())
                            order.Review = SaveTo(sss, "回访", Path.Combine(di.FullName, file.Item.Name));
                        avafiles.Remove(file);
                    }

                    avafiles.Remove(sheet);
                    db.GetCollection<TransferOrder>().Update(order);
                }
                // 追加 、 赎回

                avafiles = avafiles.Where(x => !x.Item.Name.Contains("打款") && !x.Item.Name.Contains("签署流程")).ToList();

                foreach (var nf in avafiles)
                {
                    if (Regex.IsMatch(nf.Item.Name, "追加申购单|申购申请表"))
                    {
                        using var sss = nf.Item.Open();
                        using var mss = new MemoryStream();
                        sss.CopyTo(mss);
                        mss.Seek(0, SeekOrigin.Begin);

                        var signDate = PdfHelper.GetSignDate(mss);
                        if (signDate is null)
                        {
                            sw.WriteLine($"\t\t{nf.Item.Name} 无法获取文件签约日期");
                            // 保存
                            Directory.CreateDirectory(@"importreport\meishi");
                            using var save = new FileStream(@$"importreport\meishi\{nf.Item.Name}", FileMode.Create);
                            mss.Seek(0, SeekOrigin.Begin);
                            mss.CopyTo(save);
                            save.Flush();
                            continue;
                        }
                        // 获取文件
                        var texts = PdfHelper.GetTexts(mss);
                        var number = Regex.Matches(texts[0], @"金额为.*?([\d\.,]+)").Select(x => decimal.TryParse(x.Groups[1].Value, out var n) ? n : 0).LastOrDefault(x => x > 0);
                        if (number < 100)
                        {
                            sw.WriteLine($"\t\t\t {nf.Item.Name}金额异常");
                        }

                        var order = new TransferOrder
                        {
                            FundId = fund.Id,
                            FundName = fund.Name,
                            InvestorId = cus.Id,
                            InvestorIdentity = cus.Identity.Id,
                            InvestorName = cus.Name,
                            Type = TransferOrderType.Buy,
                            Number = number,
                            ShareClass = sc,
                            Date = signDate.Value,
                            Source = "import"
                        };
                        var old = orders.FirstOrDefault(x => x.FundId == fund.Id && x.InvestorId == cus.Id && x.Date == signDate && x.Type == TransferOrderType.Buy);
                        if (old is null)
                            db.GetCollection<TransferOrder>().Insert(order);
                        else continue;//order.Id = old.Id;

                        var di = Directory.CreateDirectory(@$"files\order\{order.Id}");
                        order.OrderSheet = SaveTo(mss, "申请表", Path.Combine(di.FullName, nf.Item.Name));
                        db.GetCollection<TransferOrder>().Update(order);
                    }
                    else if (Regex.IsMatch(nf.Item.Name, "赎回申请表"))
                    {
                        using var sss = nf.Item.Open();
                        using var mss = new MemoryStream();
                        sss.CopyTo(mss);
                        mss.Seek(0, SeekOrigin.Begin);

                        var signDate = PdfHelper.GetSignDate(mss);
                        if (signDate is null)
                        {
                            sw.WriteLine($"\t\t{nf.Item.Name} 无法获取文件签约日期");
                            // 保存
                            Directory.CreateDirectory(@"importreport\meishi");
                            using var save = new FileStream(@$"importreport\meishi\{nf.Item.Name}", FileMode.Create);
                            mss.Seek(0, SeekOrigin.Begin);
                            mss.CopyTo(save);
                            save.Flush();
                            continue;
                        }
                        // 获取文件
                        var texts = PdfHelper.GetTexts(mss);

                        // 类型
                        var rt = texts[0] switch
                        {
                            string s when s.Contains("赎回至指定金额") => TransferOrderType.RemainAmout,
                            string s when s.Contains("赎回方式：金额") => TransferOrderType.Amount,
                            _ => TransferOrderType.Share
                        };

                        var number = Regex.Matches(texts[0], @"：[\d\.,]+").Select(x => decimal.TryParse(x.Value[1..], out var n) ? n : 0).LastOrDefault(x => x > 0);

                        if (number < 100)
                        {
                            sw.WriteLine($"\t\t\t {nf.Item.Name}金额异常");
                        }

                        var order = new TransferOrder
                        {
                            FundId = fund.Id,
                            FundName = fund.Name,
                            InvestorId = cus.Id,
                            InvestorIdentity = cus.Identity.Id,
                            InvestorName = cus.Name,
                            Type = rt,
                            ShareClass = sc,
                            Number = number,
                            Date = signDate.Value,
                            Source = "import"
                        };
                        var old = orders.FirstOrDefault(x => x.FundId == fund.Id && x.InvestorId == cus.Id && x.Date == signDate && x.Type == rt);
                        if (old is null)
                            db.GetCollection<TransferOrder>().Insert(order);
                        else continue;//order.Id = old.Id;

                        var di = Directory.CreateDirectory(@$"files\order\{order.Id}");
                        order.OrderSheet = SaveTo(mss, "申请表", Path.Combine(di.FullName, nf.Item.Name));
                        db.GetCollection<TransferOrder>().Update(order);
                    }
                }
            }

            sw.WriteLine("=========================================");
        }

        sw.Flush();
    }


    private FileStorageInfo? SaveTo(Stream ms, string title, string path)
    {
        using var fs = new FileStream(path, FileMode.Create);
        if (ms.CanSeek)
            ms.Seek(0, SeekOrigin.Begin);
        ms.CopyTo(fs);
        fs.Flush();

        fs.Seek(0, SeekOrigin.Begin);
        return new FileStorageInfo
        {
            Title = "",
            Path = path,
            Hash = FileHelper.ComputeHash(fs),
            Time = DateTime.Now
        };
    }

}