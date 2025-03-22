using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FMO.IO.AMAC;
using FMO.Models;
using FMO.Schedule;
using FMO.Utilities;
using Serilog;
using System.IO;
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
            MissionSchedule.Init();

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
        var db = DbHelper.Base();
        var c = db.GetCollection<Fund>().FindAll().ToArray();
        db.Dispose();



        var ned = c.Where(x => x.Status >= FundStatus.Normal && x.PublicDisclosureSynchronizeTime == default).ToArray();
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

                    int finished = i * 50 + set.Length;
                    if (finished < ned.Length)
                        HandyControl.Controls.Growl.Success($"已同步{finished}个基金，剩余{ned.Length - finished}个");
                    
                    db = DbHelper.Base();
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



        // 基金文件夹
        var dis = new DirectoryInfo(@"files\funds").GetDirectories();
        foreach (var f in c)
        {
            if (f.Code?.Length > 4)
            {
                var di = dis.FirstOrDefault(x => x.Name.StartsWith(f.Code));

                var name = $"{f.Code}.{f.Name}";

                string folder = $"files\\funds\\{name}";

                if (di is null)
                {
                    Directory.CreateDirectory(folder);
                    continue;
                }
                if (di.Name != name)
                {
                    Directory.Move(di.FullName, folder);
                    Log.Warning($"基金 {f.Code} 名称已更新 [{di.Name}] -> [{f.Name}]");
                }

                FundHelper.Map(f, folder);
            }
        }





        IsSelfTesting = false;
    }


}