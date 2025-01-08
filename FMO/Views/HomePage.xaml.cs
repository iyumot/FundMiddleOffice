using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
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
                foreach (var (i, set) in ned.Chunk(50).Index())
                {
                    foreach (var (j, f) in set.Index())
                    {
                        await AmacAssist.SyncFundInfoAsync(f, client);

                        WeakReferenceMessenger.Default.Send(f);

                        await Task.Delay(200);
                    }

                    HandyControl.Controls.Growl.Success($"已同步{i * 50 + set.Length}个基金，剩余{ned.Length - i * 50 - set.Length - 1}个");

                    db = new BaseDatabase();
                    db.GetCollection<Fund>().Update(set);
                    db.Dispose();
                }

                HandyControl.Controls.Growl.Success($"基金同步公示信息完成");
            }
            catch (Exception ex)
            {
                Log.Error($"同步公示信息失败：{ex.Message}");
                HandyControl.Controls.Growl.Error($"同步基金公示信息失败");
            }
        }







        IsSelfTesting = false;
    }


}