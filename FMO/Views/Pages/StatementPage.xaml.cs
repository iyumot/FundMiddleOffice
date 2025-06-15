using System.IO;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.TPL;
using FMO.Utilities;

namespace FMO;

/// <summary>
/// StatementPage.xaml 的交互逻辑
/// </summary>
public partial class StatementPage : UserControl
{
    public StatementPage()
    {
        InitializeComponent();
    }
}


public partial class StatementPageViewModel : ObservableObject
{


    [RelayCommand]
    public void GenerateReport()
    {
        try
        {
            using var db = DbHelper.Base();
            var funds = db.GetCollection<Fund>().Find(x=>x.Status == FundStatus.Normal).ToArray();

            var ds = funds.Select(x => new { f = x, d = db.GetDailyCollection(x.Id).FindAll().Where(x => x?.NetValue > 0).MaxBy(x => x.Date) }).ToArray();

            var obj = new
            {
                Funds = ds.Select(x => new
                {
                    Name = x.f.Name,
                    Code = x.f.Code,
                    LastDate = x.d!.Date,
                    NetAsset = x.d.NetAsset / 10000,
                    NetValue = x.d.NetValue
                })
            };

            Tpl.Generate(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "summary.xlsx"), Tpl.GetPath("nv_summary.xlsx"), obj);
            //ExcelTpl.GenerateFromTemplate(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "summary.xlsx"), "nv_summary.xlsx", obj);
        }
        catch (Exception e)
        { 
        }
    }
}