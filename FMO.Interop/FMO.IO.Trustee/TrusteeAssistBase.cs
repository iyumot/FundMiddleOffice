using FMO.Utilities;
using Microsoft.Playwright;




namespace FMO.IO.Trustee;






/// <summary>
/// 提供部分实现的基类
/// </summary>
public abstract class TrusteeAssistBase : ITrusteeAssist, IDisposable
{
    IPlaywright? playwright;
    IBrowser? browser;
    IBrowserContext? context;
    BrowserContextCache? cache;

    public abstract string Identifier { get; }

    public abstract string Name { get; }



    /// <summary>
    /// 主页
    /// </summary>
    public abstract string Domain { get; }



    public bool IsLogedIn { get; protected set; }

    public async void Dispose()
    {
        // 保存状态 
        await SaveContext();

        playwright?.Dispose();
    }

    /// <summary>
    /// 保存浏览器状态
    /// </summary>
    /// <returns></returns>
    protected async Task SaveContext()
    {
        if (context is not null && await context.StorageStateAsync() is string str)
        {
            if (cache is null) cache = new BrowserContextCache { Id = GetType().FullName! };
            cache.Context = str;

            using (var db = new TrusteeDatabase())
                db.GetCollection<BrowserContextCache>().Upsert(cache);
        }
    }

    /// <summary>
    /// 用于login前的准备，调用缓存 
    /// </summary>
    /// <returns></returns>
    protected async Task<BrowserContextInfo> PrepareLogin()
    {
        BrowserContextCache? cache = null;
        using (var db = new TrusteeDatabase())
            cache = db.GetCollection<BrowserContextCache>().FindById(GetType().FullName);
        bool hascache = cache is not null && !string.IsNullOrWhiteSpace(cache.Context);


        var playwright = await Playwright.CreateAsync();

        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = false });

        var context = !hascache ? await browser.NewContextAsync() : await browser.NewContextAsync(new BrowserNewContextOptions { StorageState = cache!.Context });

        return new BrowserContextInfo(playwright, browser, context, hascache);
    }


    /// <summary>
    /// 获取浏览器context，是否是缓存
    /// </summary>
    /// <returns></returns>
    protected async Task<IBrowserContext> GetBrowserContext()
    {
        if (playwright is null)
            playwright = await Playwright.CreateAsync();

        if (browser is null)
            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = false });

        if (context is null)
        {
            BrowserContextCache? cache = null;
            using (var db = new TrusteeDatabase())
                cache = db.GetCollection<BrowserContextCache>().FindById(GetType().FullName);
            bool hascache = cache is not null && !string.IsNullOrWhiteSpace(cache.Context);
            context = !hascache ? await browser.NewContextAsync() : await browser.NewContextAsync(new BrowserNewContextOptions { StorageState = cache!.Context });
        }
        return context;
    }

    ///// <summary>
    ///// 登陆状态强验证
    ///// 通常为需要跳转的验证方式
    ///// </summary>
    ///// <param name="page"></param>
    ///// <returns></returns>
    //public abstract Task<bool> StrongLoginValidationAsync(IPage page);

    ///// <summary>
    ///// 登陆状态弱验证
    ///// 通常为不需要跳转的验证方式
    ///// </summary>
    ///// <param name="page"></param>
    ///// <returns></returns>
    //public abstract Task<bool> WeakLoginValidationAsync(IPage page, int t);

    /// <summary>
    /// 登陆验证
    /// </summary>
    /// <param name="page"></param>
    /// <param name="wait_seconds"></param>
    /// <returns></returns>
    public abstract Task<bool> LoginValidationAsync(IPage page, int wait_seconds = 5);


    public virtual async Task<bool> LoginAsync()
    {
        // 读取cookie
        using var bci = await PrepareLogin();

        var page = await bci.Context.NewPageAsync();

        // 如果有缓存，直接验证
        if (bci.HasCache && await LoginValidationAsync(page))
            return true;

        await page.GotoAsync(Domain);

        // 验证
        IsLogedIn = await LoginValidationAsync(page, 100);

        /// 保存状态
        if (await bci.Context.StorageStateAsync() is string str)
        {
            if (cache is null) cache = new BrowserContextCache { Id = GetType().FullName! };
            cache.Context = str;

            using (var db = new TrusteeDatabase())
                db.GetCollection<BrowserContextCache>().Upsert(cache);
        }

        return IsLogedIn;
    }

    public abstract Task<bool> SynchronizeFundRaisingRecord();


    /// <summary>
    /// 从托管外包机构同步客户资料
    /// </summary>
    /// <returns></returns>
    public abstract Task<bool> SynchronizeCustomer();
}