using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using FMO.Utilities;
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



    public HomePageViewModel()
    {

        Task.Run(async () =>
        {
            await Task.Delay(2000);

            IsSynchronizing = true;

            await Task.Delay(22222);

            IsSynchronizing = false;
            //Initialize();
        });
    }



    /// <summary>
    /// 数据自检
    /// </summary>
    public void DataSelfTest()
    {
        var db = new BaseDatabase();
        var c = db.GetCollection<Fund>().FindAll().ToArray();
        db.Dispose();
        foreach (var f in c)
        {
            //if(f.PublicDisclosureSynchronizeTime == default)
        }









    }


}