using FMO.Utilities;
using LiteDB;
using Microsoft.Playwright;
using Serilog;

namespace FMO.IO.Trustee;

/// <summary>
/// 托管接口
/// </summary>
public class CSTISCAssist : ITrusteeAssist
{
    IPlaywright? playwright;


    public void Dispose()
    {
        if(playwright is not null)
            playwright.Dispose();
    }

    public async Task<bool> LoginAsync()
    {
        // 读取账号
        using var db = new TrusteeDatabase();
        var user = db.GetCollection<TrusteeCredential>().FindById( GetType().FullName);
        if(user is null)
        {
            Log.Error($"{nameof(CSTISCAssist)}.{nameof(LoginAsync)} no credential");
            return false;
        }

        if (playwright is null)
            playwright = await Playwright.CreateAsync();

        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = false });

        var page = await browser.NewPageAsync();

        await page.GotoAsync("https://iservice.citics.com/");

        var lc = page.Locator("#userName > imput");
        if (await lc.CountAsync() == 0)
        {
            Log.Error($"{nameof(CSTISCAssist)}.{nameof(LoginAsync)} patten error: username");
            return false;
        }

        await lc.First.FillAsync(user.Name);




        return true;
    }
}