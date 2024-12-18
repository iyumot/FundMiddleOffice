using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using Microsoft.Playwright;
using Serilog;
using System.Text;
using System.Text.RegularExpressions;


namespace FMO.IO.AMAC;

public static class AmacAssist
{

    /// <summary>
    /// 通过关键字获取在AMAC中的管理人名称
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    public static async Task<ManagerInfo[]?> GetInstitutionInfoFromAmac(string search)
    {

        try
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36 Edg/126.0.0.0");

            var resp = await client.GetAsync("https://gs.amac.org.cn/amac-infodisc/res/pof/manager/managerList.html");
            if (!resp.IsSuccessStatusCode)
            {
                Log.Warning("请检查网络");
                return null;
            }

            StringContent content = new StringContent("{\"keyword\":\"sssss\",\"regiProvinceFsc\":\"province\",\"offiProvinceFsc\":\"province\",\"establishDate\":{\"from\":\"1900-01-01\",\"to\":\"9999-01-01\"},\"registerDate\":{\"from\":\"1900-01-01\",\"to\":\"9999-01-01\"}}".
                Replace("sssss", search), Encoding.UTF8, "application/json");


            var request = new HttpRequestMessage();
            request.RequestUri = new Uri("https://gs.amac.org.cn/amac-infodisc/api/pof/manager/query?&page=0&size=20");
            request.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            request.Headers.Add("Origin", "https://gs.amac.org.cn");
            request.Headers.Add("Referer", "https://gs.amac.org.cn/amac-infodisc/res/pof/manager/managerList.html");
            request.Method = HttpMethod.Post;
            request.Content = content;


            resp = await client.SendAsync(request); ;// await client.PostAsync("https://gs.amac.org.cn/amac-infodisc/api/pof/manager/query?&page=0&size=20", content);
            if (resp.IsSuccessStatusCode)
            {
                var s = await resp.Content.ReadAsStringAsync();
                s = Regex.Replace(s, "</*em>", "");

                var root = System.Text.Json.JsonSerializer.Deserialize<QueryManagers>(s);// await resp.Content.ReadFromJsonAsync<AmacQuery.QueryManagers>();

                return root?.Content.ToArray();
            }

            Log.Error($"GetInstitutionInfoFromAmac Net Error {resp.StatusCode}");
            return null;
        }
        catch (Exception e)
        {
            Log.Error(e, $"GetInstitutionInfoFromAmac");

            return null;
        }
    }

    public static async ValueTask<Dictionary<string, string>> CrawlFundInfo(string url, IPage page)
    {
        //
        if (string.IsNullOrWhiteSpace(url) || page is null)
            return new Dictionary<string, string>();

        await page.GotoAsync(url);

        var trs = page.Locator("//td[contains(text(),'基金名称')]/../../tr");
        var dict = new Dictionary<string, string>();
        foreach (var item in await trs.AllAsync())
        {
            var nlc = item.Locator("td");
            var cnt = await nlc.CountAsync();
            if (cnt > 3) continue;

            var ls = new List<string>();
            foreach (var value in await item.Locator("td").AllAsync())
                ls.Add(await value.InnerTextAsync());

            if (ls.Count > 1)
                dict.Add(ls[0], ls[1]);
        }

        return dict;
    }


    private static async Task<FundBasicInfo[]> ExtractFund(IPage page)
    {
        var lc = page.Locator("//*[contains(text(),'产品信息')]/../..");

        // type a
        var lf = lc.Locator("//td[text()='暂行办法实施前成立的基金']/..").Locator("tbody > tr");
        var lf2 = lc.Locator("//td[text()='暂行办法实施后成立的基金']/..").Locator("tbody > tr");
        var lf3 = lc.Locator("//td[text()='投资顾问类产品\t']/..").Locator("tbody > tr");
        int cnt = await lf.CountAsync() + await lf2.CountAsync() + await lf3.CountAsync();
        double step = 50.0 / cnt;

        List<FundBasicInfo> list = new List<FundBasicInfo>();

        foreach (var item in await lf.AllAsync())
        {
            LocatorInnerTextOptions options = new LocatorInnerTextOptions { Timeout = 100 };
            var name = await item.Locator("td > a").First.InnerTextAsync(options);
            if (string.IsNullOrWhiteSpace(name))
            {
                Log.Error($"CrawleManagerInfo. Fund Name is Empty");
                return Array.Empty<FundBasicInfo>();
            }
            var url = await item.Locator("td > a").First.GetAttributeAsync("href");
            if (string.IsNullOrWhiteSpace(url))
            {
                Log.Error($"CrawleManagerInfo. Fund Url is Empty");
                return Array.Empty<FundBasicInfo>();
            }

            list.Add(new FundBasicInfo { Name = name, Url = url, IsPreRule = true });
        }
        foreach (var item in await lf2.AllAsync())
        {
            LocatorInnerTextOptions options = new LocatorInnerTextOptions { Timeout = 100 };
            var name = await item.Locator("td > a").First.InnerTextAsync(options);
            if (string.IsNullOrWhiteSpace(name))
            {
                Log.Error($"CrawleManagerInfo. Fund Name is Empty");
                return Array.Empty<FundBasicInfo>();
            }
            var url = await item.Locator("td > a").First.GetAttributeAsync("href");
            if (string.IsNullOrWhiteSpace(url))
            {
                Log.Error($"CrawleManagerInfo. Fund Url is Empty");
                return Array.Empty<FundBasicInfo>();
            }

            list.Add(new FundBasicInfo { Name = name, Url = url, IsPreRule = false });
        }
        foreach (var item in await lf3.AllAsync())
        {
            LocatorInnerTextOptions options = new LocatorInnerTextOptions { Timeout = 100 };
            var name = await item.Locator("td > a").First.InnerTextAsync(options);
            if (string.IsNullOrWhiteSpace(name))
            {
                Log.Error($"CrawleManagerInfo. Fund Name is Empty");
                return Array.Empty<FundBasicInfo>();
            }
            var url = await item.Locator("td > a").First.GetAttributeAsync("href");
            if (string.IsNullOrWhiteSpace(url))
            {
                Log.Error($"CrawleManagerInfo. Fund Url is Empty");
                return Array.Empty<FundBasicInfo>();
            }

            list.Add(new FundBasicInfo { Name = name, Url = url, IsAdvisor = true });
        }

        return list.ToArray();
    }


    public static async Task<bool> CrawleManagerInfo(string name, string amacid)
    {
        try
        {
            using IPlaywright pw = await Playwright.CreateAsync();
            var browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge" });
            var page = await browser.NewPageAsync();
            await page.GotoAsync($"https://gs.amac.org.cn/amac-infodisc/res/pof/manager/{amacid}.html");

            object InitProgress = 20;
            WeakReferenceMessenger.Default.Send(InitProgress, "AMAC.CrawleManagerInfo");


            var sec = page.Locator(".section");
            var basic = page.Locator(".section[0]");
            var fvs = await page.Locator(".section").First.Locator(".table > tbody > tr").AllAsync();

            var dict = new Dictionary<string, string>();
            LocatorInnerTextOptions options = new LocatorInnerTextOptions { Timeout = 100 };

            foreach (var item in fvs)
            {
                var nlc = item.Locator("td");
                var cnt = await nlc.CountAsync();
                if (cnt > 3) continue;

                dict.Add(await nlc.Nth(0).InnerTextAsync(), await nlc.Nth(1).InnerTextAsync());
            }

            var id = dict.First(x => x.Key.Contains("组织机构代码")).Value;
            var regno = dict.First(x => x.Key.Contains("登记编号")).Value;

            Manager manager = new Manager() { Name = name, AmacId = amacid, RegisterNo = regno, Id = id };

            manager.RegisterCapital = decimal.Parse(dict.FirstOrDefault(x => x.Key.Contains("注册资本")).Value);
            manager.RealCapital = decimal.Parse(dict.FirstOrDefault(x => x.Key.Contains("实缴资本")).Value);
            manager.Advisorable = dict.FirstOrDefault(x => x.Key.Contains("提供投资建议")).Value?.Contains("是") ?? false;
            manager.ScaleRange = dict.FirstOrDefault(x => x.Key.Contains("管理规模区间")).Value;
            InitProgress = 30;
            WeakReferenceMessenger.Default.Send(InitProgress, "AMAC.CrawleManagerInfo");


            /////////////////////////////////////////////////////////////////////////////

            var nus = await ExtractFund(page);

        }
        catch (Exception e)
        {
            Log.Error(e, $"CrawleManagerInfo. Fund Url is Empty");
            return false;
        }
        return true;
    }
}
