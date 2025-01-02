using Microsoft.Playwright;
using Serilog;




namespace FMO.IO.Trustee;






/// <summary>
/// 提供部分实现的基类
/// </summary>
public abstract class TrusteeAssistBase : ITrusteeAssist, IDisposable
{
    private IPage? _mainPage;


    public abstract string Identifier { get; }

    public abstract string Name { get; }


    /// <summary>
    /// 主页
    /// </summary>
    public abstract string Domain { get; }



    public bool IsLogedIn { get; protected set; }




    /// <summary>
    /// 登陆验证
    /// </summary>
    /// <param name="page"></param>
    /// <param name="wait_seconds"></param>
    /// <returns></returns>
    public abstract Task<bool> LoginValidationAsync(IPage page, int wait_seconds = 5);


    protected async Task<IPage> GetPageAsync()
    {
        if (_mainPage is null)
            _mainPage = await Automation.NewPageAsync();

        return _mainPage;
    }


    public virtual async Task<bool> LoginAsync()
    {
        try
        {
            var playwright = await Playwright.CreateAsync();

            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = false });

            var context = await Automation.LoadContect(browser);

            var page = await context.NewPageAsync();

            await page.GotoAsync(Domain);

            // 验证
            IsLogedIn = await LoginValidationAsync(page, 100);


            await Automation.SaveContextAsync();

            await page.CloseAsync();
            await browser.CloseAsync();
            playwright.Dispose();
        }
        catch (Exception e) { IsLogedIn = false; Log.Error($"Trustee Login Failed : {e.Message}"); }

        return IsLogedIn;
    }

    public abstract Task<bool> SynchronizeFundRaisingRecord();


    /// <summary>
    /// 从托管外包机构同步客户资料
    /// </summary>
    /// <returns></returns>
    public abstract Task<bool> SynchronizeCustomer();

    public async void Dispose()
    {
        if (_mainPage is not null)
            await _mainPage.CloseAsync();
    }
}