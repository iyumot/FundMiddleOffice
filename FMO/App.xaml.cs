using FMO.IO;
using FMO.Models;
using FMO.Utilities;
using Serilog;
using System.IO;
using System.Windows;

namespace FMO;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
#if RELEASE
    Mutex mutex;
#endif

    public App()
    {
#if RELEASE
        // 单例模式
        string mutexName = "FundMiddleOfficeSingleton";

        // 尝试创建一个Mutex
        bool createdNew = false;
        mutex = new Mutex(false, mutexName, out createdNew);

        // 如果Mutex已经存在，说明程序已经在运行
        if (!createdNew)
            this.Shutdown();
#endif

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        // 设置工作目录
        if (!string.IsNullOrWhiteSpace(Config.Default.WorkFolder))
        {
            var di = new DirectoryInfo(Config.Default.WorkFolder);
            if (di.Exists)
                Directory.SetCurrentDirectory(di.FullName);
        }

        //DbHelper.initpassword();

        Log.Logger = new LoggerConfiguration().WriteTo.File("log.txt").CreateLogger();

        Directory.CreateDirectory("data");
        Directory.CreateDirectory("config");
        Directory.CreateDirectory("files\\funds");
        Directory.CreateDirectory("plugins");
        Directory.CreateDirectory("files\\tac");
        Directory.CreateDirectory("files\\accounts");
        Directory.CreateDirectory("files\\accounts\\security");
        Directory.CreateDirectory("files\\accounts\\stock");
        Directory.CreateDirectory("files\\accounts\\future");
        Directory.CreateDirectory("files\\accounts\\fund");
        Directory.CreateDirectory("files\\accounts\\other");

        ///数据库自检等操作
        DatabaseAssist.SystemValidation();

        if (CheckIsFirstRun())
            StartupUri = new Uri("InitWindow.xaml", UriKind.Relative);
        else
            StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
    }


    private bool CheckIsFirstRun()
    {
        using var db = DbHelper.Base();
        var manager = db.GetCollection<Manager>().FindOne(x => x.IsMaster);

        return (manager is null);
    }


    protected override async void OnExit(ExitEventArgs e)
    {
        await Automation.DisposeAsync();
        base.OnExit(e);
    }

    private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception.Message);
    }
}
