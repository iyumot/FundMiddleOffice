using LiteDB;
using Microsoft.Playwright;
using Serilog;
using System.Collections.Concurrent;

namespace FMO.IO;


public static class PlatformDatabase
{
    private const string connectionString = @"FileName=data\platform.db;Password=891uiu89f41uf9dij432u89;Connection=Shared";


    public static LiteDatabase Instance()
    {


        return new LiteDatabase(connectionString, null);
    }
     
}

public class WebPageInfo
{
    public required IPage Page { get; set; }

    public bool IsUsing { get; set; }

    public bool IsMajor { get; set; }
}

public class PlatformUsingInfo
{
    public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);

    public required string Identifier { get; set; }

    public List<WebPageInfo> Pages { get; } = new();

}


/// <summary>
/// 使用持久化方案，有头
/// </summary>
public static class Automation
{
    // 信号量，初始计数为 1，最大计数为 1
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    static IPlaywright? _playwright;
    static IBrowser? _browser;
    static IBrowserContext? _context;


    static ConcurrentDictionary<string, PlatformUsingInfo> _pages = new();

    /// <summary>
    /// 初始化资源 
    /// </summary>
    /// <returns></returns>
    private static async Task EnsureResourcesInitialized()
    {
        // 等待信号量，确保同一时间只有一个线程可以进入临界区
        await _semaphore.WaitAsync();
        try
        {
            if (_playwright == null)
                _playwright = await Playwright.CreateAsync();


            if (_context == null)
                _context = await _playwright.Chromium.LaunchPersistentContextAsync(Path.Combine(Directory.GetCurrentDirectory(), "config\\platform"),
                    new BrowserTypeLaunchPersistentContextOptions { Channel = "msedge", Headless = false, Args = new[] { "--disable-gpu" } });

            _context.Close += (s,e) => _context = null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 申请页面，有空闲时返回主页面，否则新申请
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public static async Task<IPageWraper> AcquirePage(string identifier)
    {
        var info = _pages.GetOrAdd(identifier, new PlatformUsingInfo { Identifier = identifier });
        await info.Semaphore.WaitAsync();
        try
        {
            foreach (var item in info.Pages)
            {
                if (!item.IsUsing && !item.Page.IsClosed)
                {
                    item.IsUsing = true;
                    return new IPageWraper(identifier, item.Page);
                }
            }

            await EnsureResourcesInitialized();

            var page = await _context!.NewPageAsync();

            info.Pages.Add(new WebPageInfo { Page = page, IsUsing = true, IsMajor = info.Pages.Count == 0 });
            //if (info.Pages.Count > 1)
            //    await page.GotoAsync(info.Pages[0].Page.Url);

            return new IPageWraper(identifier, page);
        }
        finally { info.Semaphore.Release(); }
    }


    public static bool ReleasePage(IPage page, string? identifier = null)
    {
        PlatformUsingInfo? info = null;

        if (identifier is not null)
            _pages.TryGetValue(identifier, out info);

        if (info is null)
            info = _pages.Values.FirstOrDefault(y => y.Pages.Any(x => x.Page == page));

        if (info is null)
            return false;

        var bi = info.Pages.FirstOrDefault(x => x.Page == page);
        if (bi is null)
            return false;

        if (!bi.IsMajor && info.Pages.Count > 1)
            info.Pages.Remove(bi);
        else bi.IsUsing = false;

        return true;
    }


    /// <summary>
    /// 打开一个新标签
    /// </summary>
    /// <returns></returns>
    public static async Task<IPage> NewPageAsync()
    {
        await EnsureResourcesInitialized();

        try
        {
            var page = await _context!.NewPageAsync();
            return page;
        }
        catch (PlaywrightException e)
        {
            //await _context!.DisposeAsync();

            Log.Error($"Plawright Error , {e.Message}");

            _context = await _playwright!.Chromium.LaunchPersistentContextAsync(Path.Combine(Directory.GetCurrentDirectory(), "config\\platform"),
                        new BrowserTypeLaunchPersistentContextOptions { Channel = "msedge", Headless = false, Args = new[] { "--disable-gpu" } });

            var page = await _context!.NewPageAsync();
            return page;

        }

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


    public static async Task DisposeAsync()
     {
        if (_browser is not null)
            await _browser.DisposeAsync();
        _playwright?.Dispose();
    }



}


