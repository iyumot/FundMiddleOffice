using LiteDB;
using Microsoft.Playwright;
using Serilog;
using System.Collections.Concurrent;

namespace FMO.IO;


public class PlatformDatabase : LiteDatabase
{
    private const string connectionString = @"FileName=data\platform.db;Password=891uiu89f41uf9dij432u89;Connection=Shared";

    public PlatformDatabase() : base(connectionString, null)
    {
    }
}

public class WebContext
{
    public required string Id { get; set; }

    public required string Context { get; set; }
}


public static class Automation
{
    // 信号量，初始计数为 1，最大计数为 1
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    static IPlaywright? _playwright;
    static IBrowser? _browser;
    static IBrowserContext? _context;


    static ConcurrentDictionary<string, IBrowserContext> _contexts = new();

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
            if (_playwright == null)
                _playwright = await Playwright.CreateAsync();


            //if (_context == null)
            //    _context = await _playwright.Chromium.LaunchPersistentContextAsync(Path.Combine(Directory.GetCurrentDirectory(), "config\\platform"), new BrowserTypeLaunchPersistentContextOptions { Channel = "msedge", Headless = true, Args = new[] { "--disable-gpu" } });
            if (_browser is null)
                _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = false,/* Args = new[] { "--disable-gpu" }*/ });


            //if (_context == null)
            //{
            //    _context = await browser.NewContextAsync();

            //    using var db = new PlatformDatabase();
            //    var cookies = db.GetCollection<Cookie>().FindAll().ToArray();
            //    await _context.AddCookiesAsync(cookies);

            //    var cc = await _context.CookiesAsync();
            //}
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
    /// 打开一个新标签
    /// </summary>
    /// <returns></returns>
    public static async Task<IPage> NewPageAsync(string identify)
    {
        await EnsureResourcesInitialized();

        await _semaphore.WaitAsync();

        if (!_contexts.ContainsKey(identify))
            _contexts.TryAdd(identify,await GetContext(identify));
        
        _semaphore.Release();

        if (!_contexts.TryGetValue(identify, out IBrowserContext? context))
             context = await _browser!.NewContextAsync();

        context = await _browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = Path.Combine(Directory.GetCurrentDirectory(), "ddd.json") });

        var page = await context!.NewPageAsync();

        //page.Response += Page_Response;

        return page;
    }

    private static async Task<IBrowserContext> GetContext(string identify)
    {
        using var db = new PlatformDatabase();
        var obj = db.GetCollection<WebContext>().FindById(identify);

        return obj?.Context is null ? await _browser!.NewContextAsync() : await _browser!.NewContextAsync(new BrowserNewContextOptions { StorageState = obj.Context });
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

    public static async Task<bool> LoginAsync(string identifier, PrepareDelegate loginProcess, ValidationDelegate validation, int time_out = 10, FinalDelegate? final = null)
    {
        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = false });

        // var context = await browser.NewContextAsync();

        //await using var context = await playwright.Chromium.LaunchPersistentContextAsync(Path.Combine(Directory.GetCurrentDirectory(), "config\\platform"),
        //    new BrowserTypeLaunchPersistentContextOptions
        //    {
        //        Channel = "msedge",
        //        Headless = false, 
        //        ServiceWorkers  = ServiceWorkerPolicy.Block,
        //    }); //await browser.NewContextAsync();

        var db = new PlatformDatabase();
        var obj = db.GetCollection<WebContext>().FindById(identifier);
       // db.Dispose();
        var context = obj?.Context is null ? await browser!.NewContextAsync() : await browser!.NewContextAsync(new BrowserNewContextOptions { StorageState = obj.Context });

        var page = await context.NewPageAsync();

        await loginProcess(page);

        bool b = false;
        for (var i = 0; i < time_out; i++)
        {
            await Task.Delay(1000);

            try { b = await validation(page); } catch { }
            if (b) break;
        }

        await Task.Delay(1000);

        if (b && final is not null)
            await final(page);

        var domain = Uri.TryCreate(page.Url, UriKind.Absolute, out var uri) ? uri.Host : "unk";


        if (!b) Log.Error($"{domain} Login Failed ");
        else Log.Information($"{domain} Login Succuss ");

      //  using var db = new PlatformDatabase();
        string storage = await context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = Path.Combine(Directory.GetCurrentDirectory(), "ddd.json")});
        db.GetCollection<WebContext>().Upsert(new WebContext { Id = identifier, Context = storage });


        _contexts.TryRemove(identifier, out IBrowserContext? cc);
        _contexts.TryAdd(identifier, await _browser!.NewContextAsync(new BrowserNewContextOptions { StorageState = storage }));

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
    //public static async Task<bool> CheckAsync(PrepareDelegate loginProcess, ValidationDelegate validation)
    //{
    //    var page = await NewPageAsync();

    //    await loginProcess(page);

    //    bool b = false;
    //    for (var i = 0; i < 2; i++)
    //    {
    //        await Task.Delay(1000);

    //        try { b = await validation(page); } catch { }
    //        if (b) break;
    //    }
    //    return b;
    //}


    public static async Task DisposeAsync()
    {
        if (_browser is not null)
            await _browser.DisposeAsync();
        _playwright?.Dispose();
    }



}


