using LiteDB;
using Microsoft.Playwright;
using Serilog;

namespace FMO.IO;


public class PlatformDatabase : LiteDatabase
{
    private const string connectionString = @"FileName=data\platform.db;Password=891uiu89f41uf9dij432u89;Connection=Shared";

    public PlatformDatabase() : base(connectionString, null)
    {
    }
}


public static class Automation
{
    // 信号量，初始计数为 1，最大计数为 1
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    static IPlaywright? playwright;
    static IBrowser? browser;
    static IBrowserContext? _context;

    /// <summary>
    /// 5分钟无变动，保存一次
    /// </summary>
    //static Debouncer debouncer = new Debouncer(async () => await SaveContextAsync(), 1000 * 60 * 5);

    private static async Task EnsureResourcesInitialized()
    {
        // 等待信号量，确保同一时间只有一个线程可以进入临界区
        await _semaphore.WaitAsync();
        try
        {
            if (playwright == null)
                playwright = await Playwright.CreateAsync();


            if (browser == null)
                browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = true });


            if (_context == null)
            {
                _context = await browser.NewContextAsync();

                using var db = new PlatformDatabase();
                var cookies = db.GetCollection<Cookie>().FindAll().ToArray();
                await _context.AddCookiesAsync(cookies);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 打开一个新标签
    /// </summary>
    /// <returns></returns>
    public static async Task<IPage> NewPageAsync()
    {
        await EnsureResourcesInitialized();

        var page = await _context!.NewPageAsync();

        //page.Response += Page_Response;

        return page;
    }



    /// <summary>
    /// 使用独立程序时
    /// </summary>
    /// <param name="browser"></param>
    /// <returns></returns>
    public static async Task<IBrowserContext> LoadContext(IBrowser browser)
    {
        return !File.Exists("context") ? await browser.NewContextAsync() : await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = "context" });
    }

    /// <summary>
    /// 保存状态
    /// </summary>
    /// <returns></returns>
    public static async Task SaveContextAsync()
    {
        if (_context is not null)
            await _context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = "context" });
    }


    public delegate Task<bool> PrepareDelegate(IPage page);

    public delegate Task<bool> ValidationDelegate(IPage page);

    public delegate Task FinalDelegate(IPage page);

    public static async Task<bool> LoginAsync(PrepareDelegate loginProcess, ValidationDelegate validation, int time_out = 10, FinalDelegate? final = null)
    {
        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = false });

        await using var context = await browser.NewContextAsync();

        var page = await context.NewPageAsync();

        await loginProcess(page);

        bool b = false;
        for (var i = 0; i < time_out; i++)
        {
            await Task.Delay(1000);

            try { b = await validation(page); } catch { }
            if (b) break;
        }

        if (b && final is not null)
            await final(page);

        var domain = Uri.TryCreate(page.Url, UriKind.Absolute, out var uri) ? uri.Host : "unk";


        if (!b) Log.Error($"{domain} Login Failed ");
        else Log.Information($"{domain} Login Succuss ");


        var cookie_save = await context.CookiesAsync();

        using var db = new PlatformDatabase();
        db.GetCollection<Cookie>().DeleteMany(x => x.Domain == domain);
        IEnumerable<Cookie> cookies = cookie_save.Select(x => new Cookie { Domain = x.Domain, Expires = x.Expires, HttpOnly = x.HttpOnly, Name = x.Name, Path = x.Path, SameSite = x.SameSite, Secure = x.Secure, Value = x.Value });
        db.GetCollection<Cookie>().Upsert(cookies);

        if (_context is not null)
            await _context.AddCookiesAsync(cookies);

        await page.CloseAsync();
        return b;
    }


    /// <summary>
    /// 检查是否登陆
    /// </summary>
    /// <param name="loginProcess"></param>
    /// <param name="validation"></param>
    /// <param name="final"></param>
    /// <returns></returns>
    public static async Task<bool> CheckAsync(PrepareDelegate loginProcess, ValidationDelegate validation)
    {
        var page = await NewPageAsync();

        await loginProcess(page);

        bool b = false;
        for (var i = 0; i < 2; i++)
        {
            await Task.Delay(1000);

            try { b = await validation(page); } catch { }
            if (b) break;
        }
        return b;
    }


    public static async Task DisposeAsync()
    {
        if (browser is not null)
            await browser.DisposeAsync();
        playwright?.Dispose();
    }
}


