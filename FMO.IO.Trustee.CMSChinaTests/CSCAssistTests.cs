using Microsoft.Playwright;

namespace FMO.IO.Trustee.Tests
{
    [TestClass()]
    public class CSCAssistTests
    {
        [TestMethod()]
        public async Task SynchronizeTransferRequestAsyncTest()
        {
            CSCAssist assist = new();
            await assist.SynchronizeTransferRequestAsync();


        }


        [TestMethod()]
        public async Task TestWeb()
        {
            var pw = await Playwright.CreateAsync();
            var brow = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = false });
            var context = await brow.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36",

                ExtraHTTPHeaders = [new("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7"),
              //  new( "Accept-Language", "zh-CN,zh;q=0.9" ),
                new("Sec-Fetch-Dest", "empty"),
               //new("Sec-Fetch-Dest", "Microsoft Edge";v="135", "Not-A.Brand";v="8", "Chromium";v="135"),
                new("Sec-Fetch-Mode", "cors"),
                new("Sec-Fetch-Site", "same-orgin"),
                new("Sec-Fetch-User", "?1"),
               new( "Upgrade-Insecure-Requests", "1")]
            });

            var page = await context.NewPageAsync();
        await page.GotoAsync("https://tgfw.csc108.com/mgr");








    }
}
}