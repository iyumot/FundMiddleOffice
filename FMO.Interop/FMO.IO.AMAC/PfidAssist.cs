using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using Microsoft.Playwright;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FMO.IO.AMAC;

public static class PfidAssist
{


    public static async Task<PfidAccount[]> QueryInvestorAccounts(AmacAccount amacc)
    {
        // 获取账号
        //using var db = DbHelper.Base();
        //if( db.GetCollection<AmacAccount>().FindById("xinpi") is not AmacAccount amacc)
        //{
        //    return [];
        //} 

        using var pw = await Playwright.CreateAsync();
        var browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge"});

        var fi = new FileInfo(@"config\pfid.ck");
        var context = fi.Exists ? await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = fi.FullName }) : await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync("https://pfid.amac.org.cn/");

        // 等待页面完全加载（包括图片、CSS、JS 等）
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // 检查登陆
        var locator = page.Locator("#menu-product");

        // 登陆
        if (await page.Locator("#username").IsVisibleAsync() || !await locator.IsVisibleAsync())
        {
            // 设置账号密码
            await page.Locator("#username").FillAsync(amacc.Name);
            await page.Locator("#password").FillAsync(amacc.Password);
            await page.Locator("#captcha").FillAsync("");

            locator = page.Locator("#img_captcha");
            var tmp = Path.GetTempFileName();
            File.Move(tmp, tmp + ".png");
            tmp = tmp + ".png";
            var buf = await locator.ScreenshotAsync(new LocatorScreenshotOptions { Path = tmp });
            File.Delete(tmp);

            VerifyMessage verify = new VerifyMessage { Title = "请输入验证码", Type = VerifyType.Captcha, Image = buf };
            try
            {
                WeakReferenceMessenger.Default.Send(verify);


                // 等待验证码，1分钟
                if (!verify.Waiter.WaitOne(new TimeSpan(0, 1, 0)))
                    return [];

                // 空
                if (string.IsNullOrWhiteSpace(verify.Code))
                {
                    WeakReferenceMessenger.Default.Send(new VerifyResultMessage(verify.Id, false, "未输入验证码"));
                    return [];
                }

                // 验证登录
                await page.Locator("#captcha").FillAsync(verify.Code);
                await page.Locator("#btnLogin").ClickAsync();



                // 验证码错误
                locator = page.Locator("#message_login");
                if (await locator.IsVisibleAsync())
                    WeakReferenceMessenger.Default.Send(new VerifyResultMessage(verify.Id, false, await locator.InnerTextAsync()));
            }
            finally { verify.Waiter.Dispose(); }
        }

        try
        {

            // 检查登陆
            locator = page.Locator("#menu-product");
            if (!await locator.IsVisibleAsync())
                return [];

            // 登陆成功，保存cookie
            await browser.Contexts[0].StorageStateAsync(new BrowserContextStorageStateOptions { Path = Path.GetFullPath(@"config\pfid.ck") });

            // 定向披露
            var cusloc = page.Locator("#reportInvest");
            if (!await cusloc.IsVisibleAsync())
            {
                locator = page.Locator("aside >> dt").Filter(new LocatorFilterOptions { HasText = "定向披露" });
                await locator.ClickAsync();
            }

            if (!await cusloc.IsVisibleAsync())
                return [];

            await cusloc.ClickAsync();



            // 每页记录
            var frame = page.Frames.Last();
            // 等待页面完全加载（包括图片、CSS、JS 等）
            try { await frame.WaitForLoadStateAsync(LoadState.NetworkIdle); } catch { }

            locator = frame.Locator("select.select");
            var box = await locator.BoundingBoxAsync(new LocatorBoundingBoxOptions { Timeout = 1000 });

            if (!await locator.IsVisibleAsync())
                return [];

            IResponse? response = null;
            await page.RunAndWaitForResponseAsync(async () => await locator.SelectOptionAsync(new SelectOptionValue { Value = "-1" }), x =>
             {
                 if (x.Url.Contains("find-cr-investor-list.do"))
                 {
                     response = x;
                     return true;
                 }
                 return false;
             });

            if(response is null) return [];

            var json = await response.JsonAsync();
    

            var str = await response.TextAsync();
            var node = JsonNode.Parse(str);

            if(node?["total"] is JsonValue tot && tot.GetValue<int>() is int total && total > 0 && node?["aaData"] is JsonArray arr)
            {
                var da = arr.Deserialize<PfidAccountJson[]>();

                using var db = DbHelper.Base();
                var cus = db.GetCollection<Investor>().FindAll().ToArray();

                var res = new List<PfidAccount>(da!.Length);
                foreach (var item in da)
                {
                    if (cus.FirstOrDefault(x => x.Identity is not null && IsLike(x.Name, item.Name!) && IsLike(x.Identity.Id, item.Identity!)) is Investor investor)
                        res.Add(new PfidAccount(investor.Id, item.Account!));
                }

                return res.ToArray();
            }


        }
        catch (Exception e)
        {

        }


        return [];
    }


    private static bool IsLike(string full, string star)
    {
        var arr = star.Split('*');

        return full.StartsWith(arr[0]) && full.EndsWith(arr[^1]);
    }
}

public class PfidAccountJson
{
    [JsonPropertyName("accountName")]
    public string? Account { get; set; }

    [JsonPropertyName("investorName")]
    public string? Name { get; set; }

    [JsonPropertyName("idNumber")]
    public string? Identity { get; set; }
}