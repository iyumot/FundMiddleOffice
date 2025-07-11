﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExcelLibrary.SpreadSheet;
using FMO.IO.AMAC;
using FMO.Utilities;
using Microsoft.Playwright;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial string? UserName { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial string? Password { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial string? VerifyCode { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowVerifyBox))]
    public partial ImageSource? VerifyImage { get; set; }


    [ObservableProperty]
    public partial bool IsLogin { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<LearnInfo> History { get; set; } = new();


    [ObservableProperty]
    public partial LearnInfo? Selected { get; set; }


    [ObservableProperty]
    public partial int TargetHour { get; set; } = 15;

    [ObservableProperty]
    public partial int TargetHour2 { get; set; } = 5;

    [ObservableProperty]
    public partial bool OnlyFree { get; set; } = true;




    public ClassInfo[]? AllClasses { get; set; }







    public bool ShowVerifyBox => VerifyImage is not null;

    public bool CanLogin => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(VerifyCode);


    IPlaywright? Operator { get; set; }
    IBrowser? Browser { get; set; }
    IBrowserContext? Context { get; set; }

    public MainWindowViewModel()
    {
        Directory.CreateDirectory("data");
        Directory.CreateDirectory("files\\peixun");


        if (File.Exists(@"files\peixun\records.json"))
        {
            using var sr = new StreamReader(@"files\peixun\records.json");
            var json = sr.ReadToEnd();
            History = JsonSerializer.Deserialize<ObservableCollection<LearnInfo>>(Encoding.UTF8.GetString(Convert.FromBase64String(json)));
        }

        // 加载缓存 
        var fi = new FileInfo(@"files\peixun\classes.csv");
        if (fi.Exists && (DateTime.Now - fi.LastWriteTime).TotalHours < 72)
        {
            Toast.Success("加载课程列表成功");
            ParseClasses(@"files\peixun\classes.csv");
        }
        else Toast.Warning(fi.LastWriteTime.ToString());

        Init();
    }

    private void Init()
    {
        using var db = DbHelper.Base();

        var acc = db.GetCollection<AmacAccount>().FindById("peixun");

        if (acc is not null)
        {
            UserName = acc.Name;
            Password = acc.Password;
        }

        Task.Run(async () =>
        {
            Operator = await Playwright.CreateAsync();
            Browser = await Operator.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = true });

            if (File.Exists("files\\peixun\\web.json"))
            {
                Context = await Browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = "files\\peixun\\web.json" });
            }
            else
                Context = await Browser.NewContextAsync();
            var page = await Context.NewPageAsync();
            await page.GotoAsync("https://peixun.amac.org.cn/team/");

            // 验证
            try
            {
                var ele = await page.WaitForSelectorAsync("span:has-text('培训查询')", new PageWaitForSelectorOptions { Timeout = 10000 });

                IsLogin = ele is null ? false : await ele.IsVisibleAsync();
            }
            catch { }

            if (!IsLogin)
                await UpdateVerify(page);
            else
                await DownloadData(page);
        });
    }


    private async Task UpdateVerify(IPage page)
    {
        var tmp = Path.GetTempFileName();
        File.Move(tmp, tmp + ".png");
        tmp = tmp + ".png";
        var locator = page.Locator("img#verify");
        await locator.ScreenshotAsync(new LocatorScreenshotOptions { Path = tmp });

        var fs = new FileStream(tmp, FileMode.Open);
        MemoryStream ms = new MemoryStream();
        fs.CopyTo(ms);
        fs.Close();
        File.Delete(tmp);

        await App.Current.Dispatcher.BeginInvoke(() =>
        {
            // 加载 
            var bm = new BitmapImage();
            bm.BeginInit();
            bm.StreamSource = ms;
            bm.CacheOption = BitmapCacheOption.Default;
            bm.EndInit();

            VerifyImage = bm;
        });

    }



    [RelayCommand]
    public async Task RefreshVerify()
    {
        if (Context?.Pages?.LastOrDefault() is not IPage page)
            return;

        try
        {
            await page.Locator("img#verify").ClickAsync();

            await page.WaitForTimeoutAsync(1000);
            await UpdateVerify(page);
        }
        catch (Exception) { }
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    public async Task Login()
    {
        if (Context?.Pages?.LastOrDefault() is not IPage page)
            return;

        var locator = page.Locator("#user_name");
        if (!await locator.IsVisibleAsync())
            Toast.Warning("无法登录，请使用最近版本");

        await locator.FillAsync(UserName!);

        locator = page.Locator("input#password");
        if (!await locator.IsVisibleAsync())
            Toast.Warning("无法登录，请使用最近版本");

        await locator.FillAsync(Password!);

        locator = page.Locator("input#code");
        if (!await locator.IsVisibleAsync())
            Toast.Warning("无法登录，请使用最近版本");

        await locator.FillAsync(VerifyCode!);

        locator = page.Locator("span").GetByText("登录");
        if (!await locator.IsVisibleAsync())
            Toast.Warning("无法登录，请使用最近版本");

        await locator.ClickAsync();

        // 验证
        try
        {
            var ele = await page.WaitForSelectorAsync("span:has-text('培训查询')", new PageWaitForSelectorOptions { Timeout = 2000 });

            IsLogin = ele is null ? false : await ele.IsVisibleAsync();
        }
        catch (Exception e)
        {
            Toast.Warning("登录失败");
            return;
        }



        if (IsLogin)
        {
            await Context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = "files\\peixun\\web.json" });


            Toast.Success("登录成功");

            await DownloadData(page);
        }
        else Toast.Warning("登录失败");
    }



    private async Task DownloadData(IPage page)
    {
        //


        //if (History?.Count < 1)
        {
            Toast.Info("开始下载学习历史");
            await SyncLearnHistory(page);
            Toast.Success("学习历史 下载完成");
        }

        if (AllClasses is null || AllClasses.Length < 300)
        {
            Toast.Info("开始下载所有课程");
            await GetAllClass(page);
            Toast.Success("所有课程 下载完成");
        }
        else
        {
            foreach (var p in History)
            {
                await App.Current.Dispatcher.BeginInvoke(() =>
                 {
                     p.ApplyInfo = (p.Record is null ? AllClasses : AllClasses.Where(x => !p.Record.Any(y => y.Id == x.Id))).Select(x => new ClassApply { Class = x }).ToArray();
                     p.ApplyClass.Source = p.ApplyInfo;
                 });
            }
        }
    }

    [RelayCommand]
    public async Task GetLearnHistory()
    {
        if (Context?.Pages?.FirstOrDefault() is not IPage page)
            return;

        await SyncLearnHistory(page);
    }

    [RelayCommand]
    public async Task GetAllClass()
    {

        if (Context?.Pages?.LastOrDefault() is not IPage page)
            return;

        await GetAllClass(page);
    }


    [RelayCommand]
    public void RandomChoose()
    {
        // 清空
        foreach (var per in History)
        {
            if (per.ApplyInfo is null) continue;
            foreach (var x in per.ApplyInfo)
                x.IsSelected = false;
        }

        // 优先选同样的课
        // 获取所有人都未选的课
        int year = DateTime.Today.Year;
        var sel = History.Where(x => x.Record is not null).SelectMany(x => x.Record!.Where(y => y.PayTime.Year != year).Select(y => y.Id)!).Distinct();
        var cansel = AllClasses?.Where(x=>!x.Name.Contains("?") && !x.Name.Contains("？"))?.ExceptBy(sel, x => x.Id)?.ToArray() ?? [];


        // 选够职业道德 
        List<string> lawids = new(), otherids = new();
        {
            var cl = cansel.Where(x => x.Type == "职业道德").Select(x => (x, rank: Random.Shared.Next())).OrderBy(x => x.rank).Select(x => x.x).ToArray();
            decimal sum = 0;
            foreach (var item in cl)
            {
                if (OnlyFree && item.Price > 0) continue;

                lawids.Add(item.Id);

                sum += item.Hour;
                if (sum >= TargetHour2)
                    break;
            }


            cl = cansel.Where(x => x.Type != "职业道德").Select(x => (x, rank: Random.Shared.Next())).OrderBy(x => x.rank).Select(x => x.x).ToArray();
            decimal sum2 = 0;
            foreach (var item in cl)
            {
                if (OnlyFree && item.Price > 0) continue;

                otherids.Add(item.Id);

                sum2 += item.Hour;
                if (sum2 + sum >= TargetHour)
                    break;
            }


        }

        // 选课
        foreach (var per in History)
        {
            if (per.ApplyInfo is null) continue;


            // 选够法律与道德 
            var cl = per.ApplyInfo.Where(x => x.Class.Type == "职业道德").Select(x => (x, rank: lawids.Contains(x.Class.Id) ? -1 : Random.Shared.Next())).OrderBy(x => x.rank).Select(x => x.x).ToArray();
            decimal sum = per.Record?.Where(x => x.Type == "职业道德" && x.PayTime.Year == year).Sum(x => x.Hour) ?? 0;
            foreach (var item in cl)
            {
                if (sum >= TargetHour2)
                    break;

                if (OnlyFree && item.Class.Price > 0) continue;

                if (per.Record?.Any(x => x.Id == item.Class.Id) ?? false) continue;

                // 最佳匹配，如果选它，课时超过太多，就路过
                if (sum + item.Class.Hour >= TargetHour2 + 1)
                    continue;

                item.IsSelected = true;
                sum += item.Class.Hour;
            }

            if (sum == 0)
            {
                Toast.Warning($"{per.Name} 未找到足够的 职业道德");
                return;
            }

            cl = per.ApplyInfo.Where(x => x.Class.Type != "职业道德").Select(x => (x, rank: Random.Shared.Next())).OrderBy(x => x.rank).Select(x => x.x).ToArray();
            decimal sum2 = per.Record?.Where(x => x.Type != "职业道德" && x.PayTime.Year == year).Sum(x => x.Hour) ?? 0;
            foreach (var item in cl)
            {
                if (sum2 + sum >= TargetHour)
                    break;

                if (OnlyFree && item.Class.Price > 0) continue;

                if (per.Record?.Any(x => x.Id == item.Class.Id) ?? false) continue;

                // 最佳匹配，如果选它，课时超过太多，就路过
                if (sum + sum2 + item.Class.Hour >= TargetHour + 1)
                    continue;


                item.IsSelected = true;

                sum2 += item.Class.Hour;
            }

            per.ApplyClass.View.Refresh();
        }

    }


    [RelayCommand]
    public void Generate()
    {
        var path = GenerateFileFree(); GenerateFilePay();

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(new FileInfo(path)!.Directory!.FullName) { UseShellExecute = true });
    }

    private async Task SyncLearnHistory(IPage page)
    {
        try
        {
            var locator = page.Locator("div.left-main").GetByText("培训查询");
            if (!await locator.IsVisibleAsync())
                await page.GotoAsync("https://peixun.amac.org.cn/team");


            locator = page.Locator("div.left-main").GetByText("培训查询");
            if (!await locator.IsVisibleAsync())
            {
                Toast.Warning($"操作异常 {LineNumber()}");
                return;
            }

            await locator.ClickAsync();

            await page.Locator("div.left-main >> span.sub-title").GetByText("学员学习情况").ClickAsync();

            await page.WaitForTimeoutAsync(1000);

            await page.Locator("a.btn.btn-success").Filter(new LocatorFilterOptions { HasText = "搜索" }).ClickAsync();

            locator = page.Locator("div.container-fluid >> div.tab >> table");

            await page.WaitForSelectorAsync("div.container-fluid >> div.tab >> table", new PageWaitForSelectorOptions { Timeout = 2000 });

            // 获取列表
            List<(string a, string b, string c, decimal d, decimal e, string? f)> list = new();
            var rows = locator.Locator("tr");
            foreach (var r in await rows.AllAsync())
            {
                var cells = await r.Locator("td").AllAsync();
                if (cells.Count < 8) continue;

                list.Add((await cells[1].InnerTextAsync(), await cells[2].InnerTextAsync(), await cells[3].InnerTextAsync(),
                   decimal.Parse(await cells[6].InnerTextAsync()),
                   decimal.Parse(await cells[8].InnerTextAsync()),
                   await cells[^1].Locator("a").GetAttributeAsync("href")));


            }

            await App.Current.Dispatcher.BeginInvoke(() => History = new(list.Select(x => new LearnInfo
            {
                Name = x.a,
                IdType = x.b,
                IdNumber = x.c,
                Apply = x.d,
                Learned = x.e,
                Url = x.f,
            })));


        }
        catch (Exception e)
        {
            Toast.Warning($"{e}");
        }


        if (History?.Count == 0) return;

        Toast.Info("开始下载个人学习历史");

        var semaphore = new SemaphoreSlim(5); // 最多 3 个并发
        var tasks = History!.Select(async item =>
        {
            await semaphore.WaitAsync();
            try { await GetDetail(item); }
            finally { semaphore.Release(); }
        });

        await Task.WhenAll(tasks);


        // 保存  
        var json = JsonSerializer.Serialize(History);
        using var fs = new StreamWriter("files\\peixun\\records.json");
        fs.Write(Convert.ToBase64String(Encoding.UTF8.GetBytes(json)));
        fs.Flush();

        if (AllClasses is not null)
            foreach (var p in History!)
            {
                await App.Current.Dispatcher.BeginInvoke(() =>
                 {
                     p.ApplyInfo = (p.Record is null ? AllClasses : AllClasses.Where(x => !p.Record.Any(y => y.Id == x.Id))).Select(x => new ClassApply { Class = x }).ToArray();
                     p.ApplyClass.Source = p.ApplyInfo;
                 });
            }
    }


    private void ParseClasses(string path)
    {
        // 解析
        using var sr = new StreamReader(path, Encoding.GetEncoding("GB2312"));
        var head = sr.ReadLine()?.Split(',');
        if (head?.Length < 8 || head![0] != "课程编号" || head![1] != "课程名称" || head![2] != "课程属性" ||
            !head![5].Contains("课程价格") || head![4] != "学时数" || head[7] != "上线时间")
        {
            Toast.Warning("无法识别课程信息，请更新");
            return;
        }

        List<ClassInfo> classes = new();
        while (!sr.EndOfStream)
        {
            var values = sr.ReadLine()?.Split(',');
            if (values is null || values?.Length < 8) continue;

            classes.Add(new ClassInfo
            {
                Id = values![0],
                Name = values[1],
                Type = values[2],
                Hour = decimal.Parse(values[4]),
                Price = decimal.Parse(values[5]),
                Time = DateTime.Parse(values[7])
            });
        }

        AllClasses = classes.ToArray();

        foreach (var p in History)
        {
            App.Current.Dispatcher.BeginInvoke(() =>
            {
                p.ApplyInfo = (p.Record is null ? classes : classes.Where(x => !p.Record.Any(y => y.Id == x.Id))).Select(x => new ClassApply { Class = x }).ToArray();
                p.ApplyClass.Source = p.ApplyInfo;
            });
        }
    }

    private async Task GetAllClass(IPage page)
    {
        try
        {
            await page.Locator("div.left-main >> div.subNav").GetByText("课程查询").ClickAsync();
            await page.Locator("div.left-main >> div.sBox >> a").GetByText("课程查询").ClickAsync();

            var locator = page.Locator("a#async-export-btn");//#async-export-btn

            // 监听下载事件
            var downloadTask = page.WaitForDownloadAsync();

            await locator.ClickAsync();
            var download = await downloadTask;

            // 保存下载的文件
            string path = $"files\\peixun\\classes.csv";
            await download.SaveAsAsync(path);

            // 解析
            ParseClasses(path);
        }
        catch (Exception ex)
        {

        }
    }


    private async Task GetDetail(LearnInfo per)
    {
        var p = await Context!.NewPageAsync();
        await p.WaitForTimeoutAsync(500);

        await p.GotoAsync("https://peixun.amac.org.cn/team/" + per.Url);

        var locator = p.Locator("div.container-fluid >> div.tab >> table");

        var tt = await locator.InnerHTMLAsync();

        // 获取列表
        List<ClassLearnInfo> list = new();
        var rows = locator.Locator("tr");
        foreach (var r in await rows.AllAsync())
        {
            var cells = await r.Locator("td").AllAsync();
            if (cells.Count < 8) continue;


            var info = new ClassLearnInfo
            {
                Id = await cells[1].InnerTextAsync(),
                Name = await cells[2].InnerTextAsync(),
                Type = await cells[3].InnerTextAsync(),
                Hour = decimal.Parse(await cells[4].InnerTextAsync()),
                PayTime = DateTime.Parse(await cells[5].InnerTextAsync()),
                LearnTime = DateTime.TryParse(await cells[6].InnerTextAsync(), out var d) ? d : default,
                IsLearned = "已通过" == await cells[8].InnerTextAsync()
            };

            list.Add(info);
        }

        await App.Current.Dispatcher.BeginInvoke(() => per.Record = list.ToArray());

        if (AllClasses is not null)
            per.ApplyInfo = (per.Record is null ? AllClasses : AllClasses.Where(x => !per.Record.Any(y => y.Id == x.Id))).Select(x => new ClassApply { Class = x }).ToArray();
        await p.CloseAsync();
    }


    private string GenerateFileFree()
    {
        var stream = App.GetResourceStream(new Uri("pack://application:,,,/tpl.xls", UriKind.RelativeOrAbsolute)).Stream;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        ms.Seek(0, SeekOrigin.Begin);

        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"批量报课-免费.xls");

        ExcelLibrary.SpreadSheet.Workbook workbook = ExcelLibrary.SpreadSheet.Workbook.Load(ms);

        var sheet = workbook.Worksheets[0];

        int row = 2;
        for (int i = 0; i < History.Count; i++)
        {
            var apply = History[i].ApplyInfo?.Where(x => x.IsSelected && x.Class.Price == 0).Select(x => x.Class).ToArray() ?? [];

            for (int j = 0; j < apply.Length; j++, row++)
            {
                sheet.Cells[row, 1] = new Cell(History[i].Name);
                sheet.Cells[row, 2] = new Cell(History[i].IdType);
                sheet.Cells[row, 3] = new Cell(History[i].IdNumber);
                sheet.Cells[row, 4] = new Cell(apply[j].Id);
                sheet.Cells[row, 5] = new Cell(apply[j].Name);
            }


            // 保存为 XLS 格式
        }
        workbook.Save(path);

        return path;
    }
    private string GenerateFilePay()
    {
        var stream = App.GetResourceStream(new Uri("pack://application:,,,/tpl.xls", UriKind.RelativeOrAbsolute)).Stream;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        ms.Seek(0, SeekOrigin.Begin);

        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"批量报课-收费.xls");

        ExcelLibrary.SpreadSheet.Workbook workbook = ExcelLibrary.SpreadSheet.Workbook.Load(ms);

        var sheet = workbook.Worksheets[0];

        bool any = false;
        int row = 2;
        for (int i = 0; i < History.Count; i++)
        {
            var apply = History[i].ApplyInfo?.Where(x => x.IsSelected && x.Class.Price > 0).Select(x => x.Class).ToArray() ?? [];

            for (int j = 0; j < apply.Length; j++, row++)
            {
                sheet.Cells[row, 1] = new Cell(History[i].Name);
                sheet.Cells[row, 2] = new Cell(History[i].IdType);
                sheet.Cells[row, 3] = new Cell(History[i].IdNumber);
                sheet.Cells[row, 4] = new Cell(apply[j].Id);
                sheet.Cells[row, 5] = new Cell(apply[j].Name);

                any = true;
            }


            // 保存为 XLS 格式
        }
        if (any)
            workbook.Save(path);

        return path;
    }

    private int LineNumber([CallerLineNumber] int line = 0) => line;

    public void Dispose()
    {
        Operator?.Dispose();
    }
}
