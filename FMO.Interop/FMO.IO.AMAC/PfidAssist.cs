using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.Playwright;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FMO.IO.AMAC;

public static class PfidAssist
{


    public static async Task<(bool Success, string Message)> CheckAndLogin(IPage page)
    {
        var locator = page.Locator("#menu-product");

        // 登陆
        if (await page.Locator("#username").IsVisibleAsync() || !await locator.IsVisibleAsync())
        {
            using var db = DbHelper.Base();
            var amacc = db.GetCollection<AmacAccount>().FindById("xinpi2");

            // 设置账号密码
            await page.Locator("#username").FillAsync(amacc.Name);
            await page.Locator("#password").FillAsync(amacc.Password);
            await page.Locator("#captcha").FillAsync("");

            locator = page.Locator("#img_captcha");
            var buf = await locator.ScreenshotAsync();


            VerifyMessage verify = new VerifyMessage { Title = "请输入验证码", Type = VerifyType.Captcha, Image = buf, TimeOut = 1000 * 30  };
            try
            {
                WeakReferenceMessenger.Default.Send(verify);


                // 等待验证码，1分钟
                //if (!verify.Waiter.WaitOne(new TimeSpan(0, 1, 0)))
                //    return (false, "加载验证码失败");

                // 空
                if (string.IsNullOrWhiteSpace(verify.Code))
                {
                    WeakReferenceMessenger.Default.Send(new VerifyResultMessage(verify.Id, false, "未输入验证码"));
                    return (false, "未输入验证码");
                }

                // 验证登录
                await page.Locator("#captcha").FillAsync(verify.Code);
                await page.Locator("#btnLogin").ClickAsync();



                // 验证码错误
                locator = page.Locator("#message_login");
                if (await locator.IsVisibleAsync())
                {
                    WeakReferenceMessenger.Default.Send(new VerifyResultMessage(verify.Id, false, await locator.InnerTextAsync()));
                    return (false, "验证码错误");
                }
                return (true, "");
            }
            finally { verify.Waiter.Dispose(); }
        }
        else return (false, "未找到登陆界面");
    }

    
    public static async Task<(bool IsSuccess, IList<PendingReportInfo> Pendings)> Query()
    {
        using var pw = await Playwright.CreateAsync();
        var browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge" });

        var fi = new FileInfo(@"config\pfid.ck");
        var context = fi.Exists ? await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = fi.FullName }) : await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync("https://pfid.amac.org.cn/");


        // 等待页面完全加载（包括图片、CSS、JS 等）
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // 检查及登陆
        var login = await CheckAndLogin(page);


        return default;
    }





    //{"iTotalRecords":400,"aaData":[{"disc_date":"2025-08-07 00:00:00","error_info":"0","fund_type":"证券投资基金","fund_name":"鑫享世宸量化选股1号私募证券投资基金","compName":"宁波鑫享世宸投资管理合伙企业（有限合伙）","statusClass":"label-success","report_id":"7248209","today":"2025-08-20 12:34:38","statusText":"已上报","report_name":"鑫享世宸量化选股1号私募证券投资基金2025年7月月度报告","create_date":"2025-08-01 01:59:49","submit_date":"2025-08-06 09:42:09","stock_code":"SGM937","status":"4"},{"disc_date":"2025-08-07 00:00:00","error_info":"0","fund_type":"证券投资基金","fund_name":"鑫享世宸5号私募证券投资基金","compName":"宁波鑫享世宸投资管理合伙企业（有限合伙）","statusClass":"label-success","report_id":"7248210","today":"2025-08-20 12:34:38","statusText":"已上报","report_name":"鑫享世宸5号私募证券投资基金2025年7月月度报告","create_date":"2025-08-01 01:59:49","submit_date":"2025-08-06 09:41:30","stock_code":"SJJ152","status":"4"},{ "disc_date":"2025-07-31 00:00:00","error_info":"0","fund_type":"证券投资基金","fund_name":"鑫享世宸吉祥1号私募证券投资基金","compName":"宁波鑫享世宸投资管理合伙企业（有限合伙）","statusClass":"label-success","report_id":"7112574","today":"2025-08-20 12:34:38","statusText":"已上报","report_name":"鑫享世宸吉祥1号私募证券投资基金2025年2季度报告","create_date":"2025-07-01 08:35:01","submit_date":"2025-07-15 16:55:18","stock_code":"SAUJ47","status":"4"},{ "disc_date":"2025-07-31 00:00:00","error_info":"0","fund_type":"证券投资基金","fund_name":"鑫享世宸量化选股1号私募证券投资基金","compName":"宁波鑫享世宸投资管理合伙企业（有限合伙）","statusClass":"label-success","report_id":"7112577","today":"2025-08-20 12:34:38","statusText":"已上报","report_name":"鑫享世宸量化选股1号私募证券投资基金2025年2季度报告","create_date":"2025-07-01 08:35:01","submit_date":"2025-07-15 16:25:04","stock_code":"SGM937","status":"4"},{ "disc_date":"2025-07-31 00:00:00","error_info":"0","fund_type":"证券投资基金","fund_name":"鑫享世宸量化CTA1号私募证券投资基金","compName":"宁波鑫享世宸投资管理合伙企业（有限合伙）","statusClass":"label-success","report_id":"7112576","today":"2025-08-20 12:34:38","statusText":"已上报","report_name":"鑫享世宸量化CTA1号私募证券投资基金2025年2季度报告","create_date":"2025-07-01 08:35:01","submit_date":"2025-07-15 16:24:25","stock_code":"SGA032","status":"4"},{ "disc_date":"2025-07-31 00:00:00","error_info":"0","fund_type":"证券投资基金","fund_name":"鑫享世宸5号私募证券投资基金","compName":"宁波鑫享世宸投资管理合伙企业（有限合伙）","statusClass":"label-success","report_id":"7112578","today":"2025-08-20 12:34:38","statusText":"已上报","report_name":"鑫享世宸5号私募证券投资基金2025年2季度报告","create_date":"2025-07-01 08:35:01","submit_date":"2025-07-15 16:23:49","stock_code":"SJJ152","status":"4"},{ "disc_date":"2025-07-31 00:00:00","error_info":"0","fund_type":"证券投资基金","fund_name":"鑫享万象稳健一号私募证券投资基金","compName":"宁波鑫享世宸投资管理合伙企业（有限合伙）","statusClass":"label-success","report_id":"7112581","today":"2025-08-20 12:34:38","statusText":"已上报","report_name":"鑫享万象稳健一号私募证券投资基金2025年2季度报告","create_date":"2025-07-01 08:35:03","submit_date":"2025-07-15 16:22:19","stock_code":"SST963","status":"4"},{ "disc_date":"2025-07-31 00:00:00","error_info":"0","fund_type":"证券投资基金","fund_name":"鑫享世宸1号私募证券投资基金","compName":"宁波鑫享世宸投资管理合伙企业（有限合伙）","statusClass":"label-success","report_id":"7112575","today":"2025-08-20 12:34:38","statusText":"已上报","report_name":"鑫享世宸1号私募证券投资基金2025年2季度报告","create_date":"2025-07-01 08:35:01","submit_date":"2025-07-15 16:14:26","stock_code":"SEV943","status":"4"},{ "disc_date":"2025-07-31 00:00:00","error_info":"0","fund_type":"证券投资基金","fund_name":"鑫享世宸2号私募证券投资基金","compName":"宁波鑫享世宸投资管理合伙企业（有限合伙）","statusClass":"label-success","report_id":"7112579","today":"2025-08-20 12:34:38","statusText":"已上报","report_name":"鑫享世宸2号私募证券投资基金2025年2季度报告","create_date":"2025-07-01 08:35:02","submit_date":"2025-07-15 16:14:23","stock_code":"SJU169","status":"4"},{ "disc_date":"2025-07-31 00:00:00","error_info":"0","fund_type":"证券投资基金","fund_name":"鑫享世宸量化CTA1号A期私募证券投资基金","compName":"宁波鑫享世宸投资管理合伙企业（有限合伙）","statusClass":"label-success","report_id":"7112580","today":"2025-08-20 12:34:38","statusText":"已上报","report_name":"鑫享世宸量化CTA1号A期私募证券投资基金2025年2季度报告","create_date":"2025-07-01 08:35:03","submit_date":"2025-07-15 16:03:53","stock_code":"SQE018","status":"4"}],"iTotalDisplayRecords":400,"sEcho":2}



    /// <summary>
    /// 获取投资人账号
    /// </summary>
    /// <param name="amacc"></param>
    /// <returns></returns>
    public static async Task<PfidAccount[]> QueryInvestorAccounts(AmacAccount amacc)
    {
        // 获取账号
        //using var db = DbHelper.Base();
        //if( db.GetCollection<AmacAccount>().FindById("xinpi") is not AmacAccount amacc)
        //{
        //    return [];
        //} 

        using var pw = await Playwright.CreateAsync();
        var browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge" });

        var fi = new FileInfo(@"config\pfid.ck");
        var context = fi.Exists ? await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = fi.FullName }) : await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync("https://pfid.amac.org.cn/");

        // 等待页面完全加载（包括图片、CSS、JS 等）
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // 检查登陆
        var locator = page.Locator("#menu-product");

        var login = await CheckAndLogin(page);

        if (!login.Success) return [];
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

            if (response is null) return [];

            var json = await response.JsonAsync();


            var str = await response.TextAsync();
            var node = JsonNode.Parse(str);

            if (node?["total"] is JsonValue tot && tot.GetValue<int>() is int total && total > 0 && node?["aaData"] is JsonArray arr)
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