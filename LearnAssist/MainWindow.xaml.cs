using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LearnAssist;
using Microsoft.Playwright;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace FMO.LearnAssist;

using Toast = HandyControl.Controls.Growl;


/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{


    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        DataContext = new MainWindowViewModel();
    }
}




public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    public partial string? Name { get; set; }


    [ObservableProperty]
    public partial bool IsCheckingLogin { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowLoginTip { get; set; }

    [ObservableProperty]
    public partial bool ShowLoginError { get; set; }


    [ObservableProperty]
    public partial bool IsLogin { get; set; }

    [ObservableProperty]
    public partial int CurrentClass { get; set; }

    [ObservableProperty]
    public partial int CurrentChapter { get; set; }

    [ObservableProperty]
    public partial int? CountDown { get; set; }


    [ObservableProperty]
    public partial bool IsLoadingClasses { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    public partial bool CanStartLearn { get; set; }

    public ObservableCollection<ClassLearnInfo> Classes { get; } = new();

    private bool IsInitialized = false;


    IPlaywright? Operator { get; set; }
    IBrowser? Browser { get; set; }
    IBrowserContext? Context { get; set; }

    [ObservableProperty]
    public partial decimal? TotalHour { get; private set; }

    [ObservableProperty]
    public partial decimal? MoralHour { get; private set; }

    [ObservableProperty]
    public partial decimal? LawHour { get; private set; }

    [ObservableProperty]
    public partial decimal? SkillHour { get; private set; }


    [ObservableProperty]
    public partial int Year { get; set; }

    public MainWindowViewModel()
    {
        Directory.CreateDirectory("data");
        Directory.CreateDirectory("files\\peixun");


        Init();

        IsInitialized = true;
    }

    private void Init()
    {

        Task.Run(async () =>
        {
            Operator = await Playwright.CreateAsync();
            Browser = await Operator.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = true });

            if (File.Exists("files\\peixun\\learn.json"))
            {
                Context = await Browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = "files\\peixun\\learn.json" });
            }
            else
                Context = await Browser.NewContextAsync();
            var page = await Context.NewPageAsync();
            await page.GotoAsync("https://peixun.amac.org.cn/");

            await CheckLogin(page);

            IsCheckingLogin = false;

            if (!IsLogin)
                await ManualLogin();

            IsLoadingClasses = true;
            page = Context.Pages.First();

            await GetClassInfo(page);

            await GetCurrentYearLearn(page);
            IsLoadingClasses = false;





            await App.Current.Dispatcher.BeginInvoke(() => CanStartLearn = true);

            // 居中
            await App.Current.Dispatcher.BeginInvoke(() =>
            {
                var wnd = Application.Current.MainWindow;
                var screen = SystemParameters.WorkArea;
                wnd.Left = (screen.Width - wnd.Width) / 2 + screen.Left;
                wnd.Top = (screen.Height - wnd.Height) / 2 + screen.Top;
            });

        });
    }


    private async Task CheckLogin(IPage page, int timeout = 2000)
    {
        // 验证
        try
        {
            var ele = await page.WaitForSelectorAsync("span.myname", new PageWaitForSelectorOptions { Timeout = timeout });

            IsLogin = ele is null ? false : await ele.IsVisibleAsync();

            if (IsLogin)
            {
                Name = await ele!.InnerTextAsync();

                await page.Context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = "files\\peixun\\learn.json" });
            }
        }
        catch (TimeoutException e)
        {
            //Toast.Warning("登录超时");
            //await App.Current.Dispatcher.BeginInvoke(() => App.Current.Shutdown());
        }
        catch (Exception e)
        {
            Toast.Warning("登录失败");
            return;
        }
    }


    private async Task ManualLogin()
    {
        ShowLoginTip = true;
        double bot = 100;
        await App.Current.Dispatcher.BeginInvoke(() =>
        {
            var wnd = App.Current.MainWindow;
            wnd.Top = 100;
            bot = wnd.Top + wnd.Height;
        });

        using var pw = await Playwright.CreateAsync();
        var browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Channel = "msedge",
            Headless = false,
            Args = new[]
        {
            $"--window-position=100,{bot+5}",
            "--window-size=600,600"
        }
        });
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync("https://peixun.amac.org.cn/");

        CountDown = 300;

        using var t = new Timer((x) => CountDown = Math.Max(1, CountDown.Value - 1), null, 0, 1000);


        await CheckLogin(page, (int)CountDown * 1000);

        t.Dispose();
        CountDown = null;

        if (!IsLogin)
            await App.Current.Dispatcher.BeginInvoke(() => App.Current.Shutdown());

        await page.CloseAsync();
        await browser.CloseAsync();
        pw.Dispose();

        await Context!.DisposeAsync();
        Context = await Browser!.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = "files\\peixun\\learn.json" });

        page = await Context.NewPageAsync();
        await page.GotoAsync("https://peixun.amac.org.cn/");
        await CheckLogin(page, 4 * 1000);

        ShowLoginTip = false;
        IsCheckingLogin = false;

        if (!IsLogin)
        {
            ShowLoginError = true;
        }
    }


    [RelayCommand(CanExecute = nameof(CanStartLearn))]
    public async Task Start()
    {
        if (Classes?.Count == 0)
        {
            Toast.Warning("没有找到任何课程");
            return;
        }


        foreach (var current in Classes!)
        {
            try
            {
                // 打开课程页
                Toast.Info($"开始学习 【{current.Name}】");
                var page = Context?.Pages.FirstOrDefault();
                if (page is null) return;

                await page.GotoAsync("https://peixun.amac.org.cn/" + current.Url);
                await page.WaitForTimeoutAsync(2000);
                await SkipTips(page);

                // 获取所有章节
                var locator = page.Locator("#tab-content >> .tt2 >> div.xue-tt");
                List<ChapterInfo> cs = new();
                foreach (var ch in await locator.AllAsync())
                {
                    var name = await ch.Locator("a").First.InnerTextAsync(new LocatorInnerTextOptions { Timeout = 1000 });
                    var state = await ch.Locator("div.right >> a").First.InnerTextAsync();
                    var url = await ch.Locator("div.right >> a").First.GetAttributeAsync("data-href");

                    cs.Add(new ChapterInfo { Name = name, Learned = state == "已学习", Url = url });
                }

                current.Chapters = cs.ToArray();


                for (int i = 0; i < cs.Count; i++)
                {
                    var ch = cs[i];

                    if (ch.Learned) continue;

                    await LearnSingleClass(ch, page);
                }
            }
            catch (Exception e)
            {
                Toast.Error($"{e}");
            }
        }
    }

    [RelayCommand]
    public async Task Test()
    {
        try
        {
            var page = Context.Pages[0];


            var locator = page.Locator("div.cont-wrap >> div.video-box");
            var vi = await locator.IsVisibleAsync();


        }
        catch (Exception e)
        {

        }
    }


    private async Task GetCurrentYearLearn(IPage page)
    {
        var locator = page.Locator("#yearid");
        var year = DateTime.Today.Year;
        Year = year;
        await locator.ClickAsync(new LocatorClickOptions { Timeout = 2000 });
        locator = page.Locator($"ul.year-pulldown >> li:has-text('{year}')");
        await locator.ClickAsync(new LocatorClickOptions { Timeout = 2000 });

        await page.WaitForTimeoutAsync(2000);

        TotalHour = decimal.Parse(await page.Locator("#sum-hour-total").InnerTextAsync());
        MoralHour = decimal.Parse(await page.Locator("#sum-hour-type1").InnerTextAsync());
        LawHour = decimal.Parse(await page.Locator("#sum-hour-type2").InnerTextAsync());
        SkillHour = decimal.Parse(await page.Locator("#sum-hour-type3").InnerTextAsync());

    }

    private async Task GetClassInfo(IPage page)
    {
        var locator = page.Locator("div.fixed_top").GetByText("我的学习");

        if (!await locator.IsVisibleAsync())
        {
            Toast.Warning($"Error {LineNumber()}");
            return;
        }
        await locator.ClickAsync();

        // 未学习 div.title >> ul.studyState.clearfix
        // 学习中

        // 获取未学习
        locator = page.Locator("div.title >> ul.studyState.clearfix").GetByText("学习中");
        await locator.ClickAsync();

        try
        {
            await page.WaitForSelectorAsync("#userclass_list", new PageWaitForSelectorOptions { Timeout = 10000 });
            await page.WaitForTimeoutAsync(2000);


            var classes = await page.Locator("#userclass_list >> div.mycourse_list_cont").AllAsync();
            foreach (var c in classes)
            {
                // 课程名
                var name = await c.Locator("h4").InnerTextAsync(new LocatorInnerTextOptions { Timeout = 2000 });
                var url = await c.Locator("h4 >> a").GetAttributeAsync("href", new LocatorGetAttributeOptions { Timeout = 2000 });
                var ratio = await c.Locator("span.mycourse_list_bar_text >> span").InnerTextAsync(new LocatorInnerTextOptions { Timeout = 2000 });

                var learn = c.GetByText("继续学习");

                await App.Current.Dispatcher.BeginInvoke(() => Classes.Add(new ClassLearnInfo { Name = name, Url = url, Progress = decimal.Parse(ratio) }));

            }
        }
        catch { }

        locator = page.Locator("div.title >> ul.studyState.clearfix").GetByText("未学习");
        await locator.ClickAsync();

        try
        {
            locator = page.GetByText("本年度暂无未学习课程");
            if (await locator.CountAsync() > 0 && await locator.First.IsVisibleAsync()) return;

            await page.WaitForSelectorAsync("#userclass_list", new PageWaitForSelectorOptions { Timeout = 10000 });
            await page.WaitForTimeoutAsync(2000);

            var classes = await page.Locator("#userclass_list >> div.mycourse_list_cont").AllAsync();
            foreach (var c in classes)
            {
                // 课程名
                var name = await c.Locator("h4").InnerTextAsync(new LocatorInnerTextOptions { Timeout = 2000 });
                var url = await c.Locator("h4 >> a").GetAttributeAsync("href", new LocatorGetAttributeOptions { Timeout = 2000 });
                var ratio = await c.Locator("span.mycourse_list_bar_text >> span").InnerTextAsync(new LocatorInnerTextOptions { Timeout = 2000 });

                var learn = c.GetByText("继续学习");

                await App.Current.Dispatcher.BeginInvoke(() => Classes.Add(new ClassLearnInfo { Name = name, Url = url, Progress = decimal.Parse(ratio) }));

            }
        }
        catch { }
    }


    private async Task PlayVideo(IPage page)
    {
        var locator = page.Locator("div.cont-wrap >> div.video-box");
        var vi = await locator.IsVisibleAsync();

        var box = await locator.BoundingBoxAsync(new LocatorBoundingBoxOptions { Timeout = 1000 });
        await page.Mouse.ClickAsync(box!.X + box.Width / 2, box.Y + box.Height / 2);
    }

    private async Task LearnSingleClass(ChapterInfo ch, IPage page)
    {
        Toast.Info($"学习 【{ch.Name}】");

        await page.GotoAsync("https://peixun.amac.org.cn/" + ch.Url);

        await SkipTips(page);

        await PlayVideo(page);

        // 等待视频加载
        for (int j = 0; j < 10; j++)
        {
            if (page.Frames.Count < 2)
                await page.WaitForTimeoutAsync(3000);
        }

        int noneplay = 0;
        double last = 0;

        var iframe = page.Frames.FirstOrDefault(x => x.Url.Contains("player"));
        if (iframe is not null)
        {
            var video = await iframe.WaitForSelectorAsync("video", new FrameWaitForSelectorOptions { Timeout = 2000, State = WaitForSelectorState.Visible });

            if (video is not null)
            {
                var du = await video.EvaluateAsync<double>("video => video.duration");

                var dur = TimeSpan.FromSeconds(du);

                for (int j = 0; j < 60 * 60; j++)//1小时
                {
                    await Task.Delay(1000);

                    var pro = await video.EvaluateAsync<double>("video => video.currentTime");

                    ch.VideoProgress = pro / du * 100;
                    ch.VideoTime = double.IsNaN(pro) ? "-" : @$"{TimeSpan.FromSeconds(pro):hh\:mm\:ss} / {dur:hh\:mm\:ss}";
                    if (pro < last)
                    {
                        ch.VideoProgress = 0;
                        ch.Learned = true;
                        ch.VideoTime = "";
                        Toast.Info($"学习 【{ch.Name}】 结束");
                        break;
                    }


                    if (last == pro)
                    {
                        ++noneplay;
                        Toast.Info("播放进度异常");
                    }

                    if (noneplay > 10)
                    {
                        Toast.Error("播放进度异常, 即将重新播放");

                        await LearnSingleClass(ch, page);

                        return;
                    }
                    last = pro;
                }
            }
        }
    }




    private async Task SkipTips(IPage page)
    {
        for (int i = 0; i < 10; i++)
        {
            var locator = page.Locator("span.next");
            foreach (var item in await locator.AllAsync())
            {
                try
                {
                    if (await item.IsVisibleAsync())
                    {
                        await item.ScrollIntoViewIfNeededAsync(new LocatorScrollIntoViewIfNeededOptions { Timeout = 1000 });

                        await page.WaitForTimeoutAsync(300);
                        await item.ClickAsync();
                        break;
                    }
                }
                catch { }
            }
        }
    }

    private int LineNumber([CallerLineNumber] int line = 0) => line;



    public void Dispose()
    {
        Operator?.Dispose();
    }
}




public partial class ClassLearnInfo : ObservableObject
{
    public required string Name { get; set; }


    [ObservableProperty]
    public partial decimal Progress { get; set; }

    [ObservableProperty]
    public partial ChapterInfo[] Chapters { get; set; } = [];

    public string? Url { get; internal set; }
}


public partial class ChapterInfo : ObservableObject
{
    public required string Name { get; set; }

    [ObservableProperty]
    public partial bool Learned { get; set; }


    [ObservableProperty]
    public partial double VideoProgress { get; set; }


    [ObservableProperty]
    public partial string? VideoTime { get; set; }

    public string? Url { get; internal set; }

}