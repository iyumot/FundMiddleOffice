using Microsoft.Playwright;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.IO.DS.MeiShi;

public class Assist : IDigitalSignature
{
    public string Identifier => "meishi";

    public string Name => "易私募";

    public string Domain => "https://vipfunds.simu800.com/";


    public bool IsLogedIn { get; set; }


    public void Dispose()
    {
         
    }

    public async Task<bool> LoginAsync()
    {
        try
        {
            var playwright = await Playwright.CreateAsync();

            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = false });

            var context = await Automation.LoadContext(browser);

            var page = await context.NewPageAsync();

            await page.GotoAsync(Domain);

            // 验证
            IsLogedIn = await LoginValidationAsync(page, 100);

            await context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = "context" });
            //await Automation.SaveContextAsync();

            await page.CloseAsync();
            await browser.CloseAsync();
            playwright.Dispose();
        }
        catch (Exception e) { IsLogedIn = false; Log.Error($"Trustee Login Failed : {e.Message}"); }

        return IsLogedIn;
    }

    public async Task<bool> LoginValidationAsync(IPage page, int wait_seconds = 5)
    {
        for (var i = 0; i < wait_seconds; i++)
        {
            await Task.Delay(1000);

            if (await page.GetByText("易直销").CountAsync() > 0)
                return true;
        }
        return false;
    }

    public Task<bool> SynchronizeCustomerAsync()
    {
         
    }
}
