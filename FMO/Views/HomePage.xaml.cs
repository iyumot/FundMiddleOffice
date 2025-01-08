using CommunityToolkit.Mvvm.ComponentModel;
using FMO.IO.AMAC;
using FMO.Models;
using FMO.Utilities;
using Serilog;
using System.Net.Http;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// HomePage.xaml 的交互逻辑
/// </summary>
public partial class HomePage : UserControl
{
    public HomePage()
    {
        InitializeComponent();
    }























}



public partial class HomePageViewModel : ObservableObject
{
    /// <summary>
    /// 是否正在同步
    /// </summary>
    [ObservableProperty]
    public partial bool IsSynchronizing { get; set; }


    /// <summary>
    /// 是否正在自检
    /// </summary>
    [ObservableProperty]
    public partial bool IsSelfTesting { get; set; }

    public HomePageViewModel()
    {

        Task.Run(async () =>
        {
            await Task.Delay(2000);

            await DataSelfTest();



        });
    }



    /// <summary>
    /// 数据自检
    /// </summary>
    public async Task DataSelfTest()
    {
        IsSelfTesting = true;
        var db = new BaseDatabase();
        var c = db.GetCollection<Fund>().FindAll().ToArray();
        db.Dispose();


        var ned = c.Where(x => x.PublicDisclosureSynchronizeTime == default).ToArray();
        if (ned.Length > 0)
        {
            try
            {
                HandyControl.Controls.Growl.Warning($"发现{ned.Length}个基金未同步公示信息");

                using HttpClient client = new HttpClient();
                foreach (var (i, f) in c.Index())
                {
                    await AmacAssist.SyncFundInfoAsync(f, client);

                    if (i % 50 == 49)
                        HandyControl.Controls.Growl.Success($"已同步{i + 1}个基金，剩余{ned.Length - i - 1}个");
                    await Task.Delay(200);
                }

                db = new BaseDatabase();
                db.GetCollection<Fund>().Update(ned);

                HandyControl.Controls.Growl.Warning($"基金同步公示信息完成");
            }
            catch (Exception ex)
            {
                Log.Error($"同步公示信息失败：{ex.Message}");
                HandyControl.Controls.Growl.Success($"同步基金公示信息失败");
            }
        }







        IsSelfTesting = false;
    }


}