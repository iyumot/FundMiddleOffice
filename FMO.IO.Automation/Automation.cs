using Microsoft.Playwright;

namespace FMO.IO;




public static class Automation
{
    static IPlaywright? playwright;
    static IBrowser? browser;
    static IBrowserContext? context;

    /// <summary>
    /// 5分钟无变动，保存一次
    /// </summary>
    static Debouncer debouncer = new Debouncer(async () => await SaveContextAsync(), 1000 * 60 * 5);

 
    /// <summary>
    /// 打开一个新标签
    /// </summary>
    /// <returns></returns>
    public static async Task<IPage> NewPageAsync()
    {
        if (playwright is null)
            playwright = await Playwright.CreateAsync();

        if (browser is null)
            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = false });

        if (context is null)
            context = !File.Exists("context") ? await browser.NewContextAsync() : await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = "context" });

        var page = await context.NewPageAsync();

        page.Response += Page_Response;

        return page;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void Page_Response(object? sender, IResponse e)
    {
        debouncer.Invoke();
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
        if (context is not null)
            await context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = "context" });
    }




}


