using CommunityToolkit.Mvvm.ComponentModel;
using FMO.IO.Trustee;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FMO;

/// <summary>
/// TrusteePage.xaml 的交互逻辑
/// </summary>
public partial class TrusteePage : UserControl
{
    public TrusteePage()
    {
        InitializeComponent();

    }
}


public partial class TrusteePageViewModel : ObservableObject
{

    private bool _firstLoad = true;

    public ObservableCollection<TrusteePageViewModelTrustee> Trustees { get; } = new();


    public TrusteePageViewModel()
    {
        /// 读取所有托管插件
        /// 

        if (_firstLoad)
        {

            var files = new DirectoryInfo("plugins").GetFiles("*.dll");


            foreach (var file in files)
            {
                var assembly = Assembly.LoadFile(file.FullName);

                var type = assembly.GetTypes().FirstOrDefault(x => x.GetInterface(typeof(ITrusteeAssist).FullName!) is not null);
                if (type is null) continue;


                ITrusteeAssist trusteeAssist = (ITrusteeAssist)Activator.CreateInstance(type)!;

                Stream? iconStream = null;
                var res = assembly.GetManifestResourceNames();
                var name = res.FirstOrDefault(x => x.Contains(".logo."));
                if (name is not null)
                    iconStream = assembly.GetManifestResourceStream(name);

                var icon = new BitmapImage();
                icon.BeginInit();
                icon.StreamSource = iconStream;
                icon.EndInit();

                Trustees.Add(new TrusteePageViewModelTrustee(trusteeAssist, trusteeAssist.Name, icon));
            }
        }



        using var db = new TrusteeDatabase();


    }
}


public partial class TrusteePageViewModelTrustee : ObservableObject
{
    [SetsRequiredMembers]
    public TrusteePageViewModelTrustee(ITrusteeAssist assist, string name, ImageSource? icon)
    {
        Icon = icon;
        Name = name;
        Assist = assist;


        using var db = new TrusteeDatabase();
        var config = db.GetCollection<TrusteeConfig>().FindOne(x => x.Identifier == Assist.Identifier);
        if (config is not null)
        {
            IsEnabled = config.IsEnabled;
        }
    }


    public ImageSource? Icon { get; set; }

    public required string Name { get; set; }

    public required ITrusteeAssist Assist { get; set; }


    /// <summary>
    /// 是否启用
    /// </summary>
    [ObservableProperty]
    public partial bool IsEnabled { get; set; }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LoginStatus))]
    public partial bool IsLogin { get; set; }


    public string LoginStatus => IsLogin ? "已登陆" : "未登陆";







    partial void OnIsEnabledChanged(bool value)
    {
        using var db = new TrusteeDatabase();
        LiteDB.ILiteCollection<TrusteeConfig> c = db.GetCollection<TrusteeConfig>();

        var config = c.FindOne(x => x.Identifier == Assist.Identifier);

        if (config is null)
            config = new TrusteeConfig { Identifier = Assist.Identifier };

        config.IsEnabled = value;
        c.Upsert(config);


        if (value)
            StartWork();
    }

    /// <summary>
    /// 启动程序
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private async void StartWork()
    {
        if (IsEnabled && !await Assist.LoginAsync())
        {
            IsLogin = false;
            return;
        }


        HandyControl.Controls.Growl.Success($"托管平台【{Assist.Name}】登陆成功");


        /// 同步募集户流水
        //Task.Run(() =>
        //{

        //});




    }
}