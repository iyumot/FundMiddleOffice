using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.IO.AMAC;
using FMO.Models;
using FMO.Utilities;
using Microsoft.Win32;
using Serilog;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// CustomerPage.xaml 的交互逻辑
/// </summary>
public partial class CustomerPage : UserControl
{




    public CustomerPage()
    {
        InitializeComponent();
    }
}

public partial class CustomerPageViewModel : ObservableRecipient, IRecipient<Investor>
{

    public ObservableCollection<InvestorReadOnlyViewModel> Customers { get; }

    [ObservableProperty]
    public partial InvestorReadOnlyViewModel? Selected { get; set; }


    [ObservableProperty]
    public partial CustomerViewModel? Detail { get; set; }




    public CustomerPageViewModel()
    {
        IsActive = true;

        using var db = DbHelper.Base();

        var cusomers = db.GetCollection<Investor>().FindAll().ToArray();

        //if (cusomers.Length == 0)
        //    cusomers = [
        //        new Investor { Id = 1, Name = "张三", EntityType = EntityType.Natural},
        //        new Investor { Id = 2, Name = "某公司", EntityType = EntityType.Institution},
        //        new Investor { Id = 3, Name = "某产品", EntityType = EntityType.Product},
        //    ];

        Customers = new(cusomers.Select(x => new InvestorReadOnlyViewModel(x)));// new(cusomers.Select(x => new CustomerViewModel(x)));




    }


    [RelayCommand]
    public void AddInvestor(DataGrid grid)
    {
        InvestorReadOnlyViewModel item = new(new Investor { Name = "" });
        Customers.Add(item);
        grid.ScrollIntoView(item);
    }

    [RelayCommand]
    public void RemoveInvestor()
    {
        if (Selected is null) return;

        if (Selected.Id != 0)
        {
            if (Selected.Name?.Length > 1 && Selected.Identity != default && HandyControl.Controls.MessageBox.Show($"是否删除投资人 【{Selected.Name}】资料", "提示", button: System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.No)
                return;

            using var db = DbHelper.Base();
            db.GetCollection<Investor>().Delete(Selected.Id);
        }

        Application.Current.Dispatcher.BeginInvoke((() => { Customers.Remove(Selected); Selected = null; }));

    }

    [RelayCommand]
    public async Task GeneratePfidSheet()
    {
        try
        {
            // 加载模板
            var file = new FileInfo(@"files\tpl\pfid_investor.xlsx");
            if (!file.Exists)
            {
                HandyControl.Controls.Growl.Error("未找到模板文件");
                return;
            }

            using FileStream stream = file.OpenRead();
            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheet("投资者信息");
            if (sheet is null)
                return;

            using var db = DbHelper.Base();
            var customer = db.GetCollection<Investor>().FindAll().ToList();
            var ta = db.GetCollection<TransferRecord>().FindAll().ToList();
            var pfmap = db.GetCollection<PfidAccount>();


            if (pfmap.Count() == 0)
            {
                var acc = db.GetCollection<AmacAccount>().FindById("xinpi");
                var ad = await PfidAssist.QueryInvestorAccounts(acc);
                pfmap.Upsert(ad);
            }


            int row = 2;

            foreach (var c in customer)
            {
                // 检查有无仓位 
                var cta = ta.Where(x => x.CustomerId == c.Id).GroupBy(x => x.FundCode);
                cta = cta.Where(x => x.Sum(y => y.ShareChange()) > 0);
                var hasposition = cta.Any();

                if (c.Identity is not null)
                {
                    if (pfmap.FindById(c.Id) is PfidAccount pac)
                        sheet.Cell(row, 1).Value = pac.Account;
                    else if (!hasposition) continue; // 如果无仓位，也没有账号，跳过

                    else sheet.Cell(row, 1).Value = c.EntityType switch
                    {
                        EntityType.Product => c.Identity.Id[^6..], //S 码
                        EntityType.Institution => c.Identity.Id[^9..], // 组织机构码
                        _ => string.IsNullOrWhiteSpace(c.Phone) ? c.Email : c.Phone // 手机或邮箱
                    };// $"xxsc{c.Identity.Id[^6..]}";

                    sheet.Cell(row, 2).Value = c.Name;
                    sheet.Cell(row, 3).Value = EnumDescriptionTypeConverter.GetEnumDescription(c.Type);
                    sheet.Cell(row, 4).Value = c.Identity.Type == IDType.IdentityCard ? "身份证" : EnumDescriptionTypeConverter.GetEnumDescription(c.Identity.Type);
                    if (c.Identity.Type == IDType.Other)
                        sheet.Cell(row, 5).Value = c.Identity.Other;

                    sheet.Cell(row, 6).Value = c.Identity.Id;
                    sheet.Cell(row, 8).Value = c.Email;
                    if (string.IsNullOrWhiteSpace(c.Email))
                        sheet.Cell(row, 7).Value = c.Phone;
                    sheet.Cell(row, 9).Value = hasposition ? "启用" : "关闭";
                    sheet.Cell(row, 10).Value = string.Join(",", cta.Select(x => x.Key));
                    ++row;
                }
                //foreach (var t in cta)
                //{
                //    var fundc = t.Key;

                //    if (t.Sum(x => x.ShareChange()) > 0)
                //    {
                //        has = true;

                //        sheet.Cell(row, 1).Value = $"xxsc{c.Identity.Id[^6..]}";
                //        sheet.Cell(row, 2).Value = c.Name;
                //        sheet.Cell(row, 3).Value = EnumDescriptionTypeConverter.GetEnumDescription(c.Type);
                //        sheet.Cell(row, 4).Value = c.Identity.Type == IDType.IdentityCard ? "身份证": EnumDescriptionTypeConverter.GetEnumDescription(c.Identity.Type);
                //        if (c.Identity.Type == IDType.Other)
                //            sheet.Cell(row, 5).Value = c.Identity.Other;

                //        sheet.Cell(row, 6).Value = c.Identity.Id; 
                //        sheet.Cell(row, 8).Value = c.Email;
                //        if(string.IsNullOrWhiteSpace(c.Email))
                //            sheet.Cell(row, 7).Value = c.Phone;
                //        sheet.Cell(row, 9).Value = "启用";
                //        sheet.Cell(row, 10).Value = fundc;
                //        ++row;
                //    }
                //}  
            }

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "投资者账号.xlsx");
            workbook.SaveAs(path);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(new FileInfo(path).Directory!.FullName) { UseShellExecute = true });
        }
        catch (Exception e)
        {
            HandyControl.Controls.Growl.Error($"生成失败：{e.Message}");
        }
    }


    [RelayCommand]
    public async Task ImportQualificationData()
    {
        var fd = new OpenFileDialog();
        if (!fd.ShowDialog() ?? true) return;

        var file = fd.FileName;

        //await Task.Run(() =>
        //{
            using var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read);

            var info = zip.Entries.Where(x => string.IsNullOrWhiteSpace(x.Name)).Select(x => (x, Regex.Match(x.FullName, @"投资者数据库列表文件/([\w\(\)（）]+)/([^/]+)/$"))).
                            Where(x => x.Item2.Success).Select(x => (Fund: x.Item2.Groups[1].Value, Investor: x.Item2.Groups[2].Value, Folder: x.x.FullName));

        foreach (var fund in info.GroupBy(x=>x.Fund))
        {
            foreach (var customer in fund)
            {
                // 风险测评材料/
                var riskf = zip.Entries.Where(x => x.Name?.Length > 0 && x.FullName.StartsWith(customer.Folder + "风险测评材料/"));

                

            }
        }  

        //});
    }

    partial void OnSelectedChanged(InvestorReadOnlyViewModel? oldValue, InvestorReadOnlyViewModel? newValue)
    {
        Detail = newValue is null ? null : new CustomerViewModel(newValue.Investor);
    }

    public void Receive(Investor message)
    {
        Application.Current.Dispatcher.BeginInvoke((() =>
        {

            try
            {
                for (int i = 0; i < Customers.Count; i++)
                {
                    if (Customers[i].Id == message.Id)
                    {
                        Customers[i].Update(message);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"更新investor出错 {e.Message}");
            }

        }));
    }
}


/// <summary>
/// 用于列表展示
/// </summary>
public partial class InvestorReadOnlyViewModel : ObservableObject
{
    public int Id { get; }


    [ObservableProperty]
    public partial string? Name { get; set; }

    [ObservableProperty]
    public partial AmacInvestorType Type { get; set; }

    [ObservableProperty]
    public partial Identity Identity { get; set; }


    [ObservableProperty]
    public partial DateEfficient Efficient { get; set; }

    [ObservableProperty]
    public partial RiskLevel RiskLevel { get; set; }


    [ObservableProperty]
    public partial bool LackEmail { get; set; }

    [ObservableProperty]
    public partial bool LackPhone { get; set; }


    public Investor Investor { get; set; }

    public InvestorReadOnlyViewModel(Investor investor)
    {
        Id = investor.Id;
        Name = investor.Name;
        Type = investor.Type;

        Identity = investor.Identity;
        Efficient = investor.Efficient;

        RiskLevel = investor.RiskLevel;

        LackPhone = string.IsNullOrWhiteSpace(investor.Phone);
        LackEmail = string.IsNullOrWhiteSpace(investor.Email);
        Investor = investor;
    }

    public void Update(Investor investor)
    {
        Name = investor.Name;
        Type = investor.Type;

        Identity = investor.Identity;
        Efficient = investor.Efficient;

        RiskLevel = investor.RiskLevel;
        LackPhone = string.IsNullOrWhiteSpace(investor.Phone);
        LackEmail = string.IsNullOrWhiteSpace(investor.Email);
        Investor = investor;
    }

}
