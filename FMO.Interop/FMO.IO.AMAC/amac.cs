using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using Microsoft.Playwright;
using Serilog;
using System.Text;
using System.Text.RegularExpressions;


namespace FMO.IO.AMAC;


/// <summary>
/// 程序初始化的第2步
/// </summary>
public class InitStep2Info
{
    /// <summary>
    /// 进度
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// 管理规模
    /// </summary>
    public string? CurrentScale { get; set; }

    public int? PreRuleCount { get; set; }

    public int? NormalCount { get; set; }

    public int? AdviseCount { get; set; }
}


public static class AmacAssist
{
    public const string MessageToken = "AMAC.CrawleManagerInfo";


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


    /// <summary>
    /// 从基金公示信息同步数据 
    /// </summary>
    /// <param name="fund"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    public static async Task<bool> SyncFundInfoAsync(Fund fund, HttpClient client)
    {
        //
        if (string.IsNullOrWhiteSpace(fund.Url))
            return false;

        var content = await client.GetStringAsync(fund.Url);

        var idx = content.IndexOf("基金名称");
        if (idx < 0)
        {
            Log.Error("获取基金公示信息错误 1");
            return default;
        }

        var s = content.LastIndexOf("tbody", idx);
        var e = content.IndexOf("tbody", idx);
        if (e <= s)
        {
            Log.Error("获取基金公示信息错误 2");
            return default;
        }

        var table = content.Substring(s - 1, e - s + 7);

        var ms = Regex.Matches(table, "<tr>.*?</tr>", RegexOptions.Singleline);

        foreach (Match m in ms)
        {
            var tds = Regex.Matches(m.Value, "<td.*?>.*?</td>", RegexOptions.Singleline).Select(x => x.Value).ToArray();

            if (tds.Length < 2)
            {
                Log.Error("获取基金公示信息错误 3");
                return default;
            }

            var match = Regex.Match(tds[0], ">.*?<");
            if (!match.Success)
            {
                Log.Error("获取基金公示信息错误 4");
                return default;
            }

            var field = match.Value[1..^1];

            match = Regex.Match(tds[1], "(?s)>.*?<");
            if (!match.Success)
            {
                Log.Error("获取基金公示信息错误 5");
                return default;
            }
            var value = match.Value[1..^1].Trim();


            Fill(fund, field, value);

        }

        fund.PublicDisclosureSynchronizeTime = DateTime.Now;
        return true;
    }

    private static void Fill(Fund fund, string field, string value)
    {


        switch (field)
        {
            case string s when s.Contains("基金名称"):
                if (value != fund.Name)
                {
                    fund.Name = value;
                    fund.ShortName = Fund.GetDefaultShortName(value);
                }
                //   throw new Exception($"从基金公示信息同步数据错误，基金名称不匹配[{fund.Name.Value}]，公示为[{value}] {fund.Url}");
                break;
            case string s when s.Contains("成立时间"):
                if (DateOnly.TryParse(value, out DateOnly d))
                    fund.SetupDate = d;
                else throw new Exception($"未识别的日期格式：{value}");
                break;
            case string s when s.Contains("备案时间"):
                if (DateOnly.TryParse(value, out d))
                    fund.AuditDate = d;
                else throw new Exception($"未识别的日期格式：{value}");
                break;

            case string s when s.Contains("基金编号"):
                fund.Code = value.Trim();
                break;

            case string s when s.Contains("基金类型"):
                fund.Type = EnumHelper.FromDescription<FundType>(value.Trim());
                break;

            case string s when s.Contains("管理类型"):
                fund.ManageType = EnumHelper.FromDescription<ManageType>(value.Trim());
                break;

            case string s when s.Contains("托管人名称"):
                fund.Trustee = value.Trim();
                break;

            case string s when s.Contains("运作状态"):
                fund.Status = EnumHelper.FromDescription<FundStatus>(value.Trim());
                break;


            case string s when s.Contains("基金信息最后更新时间"):
                if (DateTime.TryParse(value, out DateTime dt))
                    fund.LastUpdate = dt;
                break;


            default:
                break;
        }

    }

    private static async Task<FundBasicInfo[]> ExtractFund(IPage page, InitStep2Info info)
    {
        var lc = page.Locator("//*[contains(text(),'产品信息')]/../..");

        // type a
        var lf = lc.Locator("//td[text()='暂行办法实施前成立的基金']/..").Locator("tbody > tr");
        var lf2 = lc.Locator("//td[text()='暂行办法实施后成立的基金']/..").Locator("tbody > tr");
        var lf3 = lc.Locator("//td[text()='投资顾问类产品']/..").Locator("tbody > tr");

        info.PreRuleCount = await lf.CountAsync();
        info.NormalCount = await lf2.CountAsync();
        info.AdviseCount = await lf3.CountAsync();

        int cnt = info.PreRuleCount.Value + info.NormalCount.Value + info.AdviseCount.Value;
        double step = 50.0 / Math.Max(1, cnt);
        info.Progress = 50;
        WeakReferenceMessenger.Default.Send(info, MessageToken);
        info.PreRuleCount = null;
        info.NormalCount = null;
        info.NormalCount = null;

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

            list.Add(new FundBasicInfo { Name = name, Url = url[2..], IsPreRule = true });

            info.Progress += step;
            WeakReferenceMessenger.Default.Send(info, MessageToken);
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

            list.Add(new FundBasicInfo { Name = name, Url = url[2..], IsPreRule = false });
            info.Progress += step;
            WeakReferenceMessenger.Default.Send(info, MessageToken);
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

            list.Add(new FundBasicInfo { Name = name, Url = url[2..], IsAdvisor = true });
            info.Progress += step;
            WeakReferenceMessenger.Default.Send(info, MessageToken);
        }


        info.Progress = 100;
        WeakReferenceMessenger.Default.Send(info, MessageToken);
        return list.ToArray();
    }


    //public static async Task<(Manager, FundBasicInfo[])?> CrawleManagerInfo(string name, string amacid)
    //{
    //    try
    //    {
    //        using IPlaywright pw = await Playwright.CreateAsync();
    //        var browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge" });
    //        var page = await browser.NewPageAsync();
    //        await page.GotoAsync($"https://gs.amac.org.cn/amac-infodisc/res/pof/manager/{amacid}.html");

    //        object InitProgress = 20;
    //        WeakReferenceMessenger.Default.Send(InitProgress, MessageToken);


    //        var sec = page.Locator(".section");
    //        var basic = page.Locator(".section[0]");
    //        var fvs = await page.Locator(".section").First.Locator(".table > tbody > tr").AllAsync();

    //        var dict = new Dictionary<string, string>();
    //        LocatorInnerTextOptions options = new LocatorInnerTextOptions { Timeout = 100 };

    //        foreach (var item in fvs)
    //        {
    //            var nlc = item.Locator("td");
    //            var cnt = await nlc.CountAsync();
    //            if (cnt > 3) continue;

    //            dict.Add(await nlc.Nth(0).InnerTextAsync(), await nlc.Nth(1).InnerTextAsync());
    //        }

    //        var id = dict.First(x => x.Key.Contains("组织机构代码")).Value;
    //        var regno = dict.First(x => x.Key.Contains("登记编号")).Value;

    //        Manager manager = new Manager() { Name = name, AmacId = amacid, RegisterNo = regno, Id = id };

    //        manager.RegisterCapital = decimal.Parse(dict.FirstOrDefault(x => x.Key.Contains("注册资本")).Value);
    //        manager.RealCapital = decimal.Parse(dict.FirstOrDefault(x => x.Key.Contains("实缴资本")).Value);
    //        manager.Advisorable = dict.FirstOrDefault(x => x.Key.Contains("提供投资建议")).Value?.Contains("是") ?? false;
    //        manager.ScaleRange = dict.FirstOrDefault(x => x.Key.Contains("管理规模区间")).Value;
    //        InitProgress = 30;
    //        WeakReferenceMessenger.Default.Send(InitProgress, MessageToken);


    //        /////////////////////////////////////////////////////////////////////////////

    //        var nus = await ExtractFund(page);

    //        return (manager, nus);
    //    }
    //    catch (Exception e)
    //    {
    //        Log.Error(e, $"CrawleManagerInfo. Fund Url is Empty");
    //        return null;
    //    } 
    //}

    public static async Task<FundBasicInfo[]> CrawleManagerInfo(Manager manager)
    {
        try
        {
            using IPlaywright pw = await Playwright.CreateAsync();
            var browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge" });
            var page = await browser.NewPageAsync();
            await page.GotoAsync($"https://gs.amac.org.cn/amac-infodisc/res/pof/manager/{manager.AmacId}.html");

            InitStep2Info info = new();
            info.Progress = 20;
            WeakReferenceMessenger.Default.Send(info, MessageToken);


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

            //Manager manager = new Manager() { Name = name, AmacId = amacid, RegisterNo = regno, Id = id };
            manager.Id = id;
            manager.RegisterCapital = decimal.Parse(dict.FirstOrDefault(x => x.Key.Contains("注册资本")).Value);
            manager.RealCapital = decimal.Parse(dict.FirstOrDefault(x => x.Key.Contains("实缴资本")).Value);
            manager.Advisorable = dict.FirstOrDefault(x => x.Key.Contains("提供投资建议")).Value?.Contains("是") ?? false;
            manager.ScaleRange = dict.FirstOrDefault(x => x.Key.Contains("管理规模区间")).Value;
            info.Progress = 30;
            info.CurrentScale = manager.ScaleRange;
            WeakReferenceMessenger.Default.Send(info, MessageToken);
            info.CurrentScale = null;

            /////////////////////////////////////////////////////////////////////////////

            var nus = await ExtractFund(page, info);

            return nus;
        }
        catch (Exception e)
        {
            Log.Error(e, $"CrawleManagerInfo. Fund Url is Empty");
            return Array.Empty<FundBasicInfo>();
        }
    }





}
