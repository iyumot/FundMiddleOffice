using CommunityToolkit.Mvvm.Messaging;
using FMO.Logging;
using FMO.Models;
using FMO.Utilities;
using LiteDB;
using Microsoft.Playwright;

namespace FMO.IO.AMAC;

public static class AmbersAssist
{


    public static async Task<(IPlaywright pw, IBrowser browser, IPage page)> Prepare(bool load = false)
    {
        var pw = await Playwright.CreateAsync();
        var browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = false });

        var fi = new FileInfo(@"config\ambers.ck");
        var context = load && fi.Exists && (DateTime.Now - fi.LastWriteTime).TotalHours < 1 ? await browser.NewContextAsync(new BrowserNewContextOptions { StorageStatePath = fi.FullName }) : await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync("https://ambers.amac.org.cn/");


        return (pw, browser, page);
    }


    public static async Task<bool> Login(IPage page, string username, string password)
    {
        await page.FillAsync("#accountName", username);
        await page.FillAsync("#password", password);
        await page.ClickAsync("#btn-login1");

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // 安全登录弹窗
        var loc = page.Locator("#mb_btn_ok");
        if (await loc.CountAsync() == 1 && await loc.IsVisibleAsync())
            await loc.ClickAsync();

        await page.Context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = Path.GetFullPath(@"config\ambers.ck") });


        loc = page.Locator("#log_yh1").GetByText("密码错误");
        return !await loc.IsVisibleAsync();
    }



    public static async Task<bool> UploadQuartlyInvestor(params FundQuarterlyUpdate[] reports)
    {
        using var db = DbHelper.Base();
        var acc = db.GetCollection<AmacAccount>().FindById("ambers");
        if (string.IsNullOrWhiteSpace(acc?.Name) || string.IsNullOrWhiteSpace(acc?.Password))
        {
            LogEx.Error("AMAC账号信息不完整，请检查数据库");
            return false;
        }

        var dic = db.GetCollection<Fund>().Query().Where(x => x.Code != null).Select(x => new { x.Code, x.AmacID }).ToArray().ToDictionary(x => x.Code!, x => x.AmacID);


        var (pw, browser, page) = await Prepare(true);

        try
        {
            // 登录
            var loginResult = await Login(page, acc.Name, acc.Password);
            if (!loginResult)
            {
                LogEx.Error("AMAC登录失败，请检查账号信息");
                return false;
            }

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);


            await page.GotoAsync("https://ambers.amac.org.cn/web/app.html#/product/quarterUpdate");

            var data = await CrawlAllFundReportAsync(page);

            var query = from r in reports
                        join p in data on r.FundCode equals p.ProductCode
                        select new
                        {
                            Code = r.FundCode,
                            FundId = r.FundId,
                            IsInvestorFilled = p.IsInvestorFilled,
                            IsSubmitted = p.IsSubmited,
                            File = r.Investor,
                            Date = r.PeriodEnd
                        };

            foreach (var item in query)
            {
                if (item.IsInvestorFilled) continue;

                if (!dic.TryGetValue(item.Code, out var id) || id!.Length < 10)
                {
                    LogEx.Warning($"{item.Code} 上报季度更新时没有找到id，请先在基金总览页点击更新");
                    continue;
                }
                if (item.File?.File is null)
                {
                    LogEx.Warning($"{item.Code} 上报季度更新时，投资者信息表不存在");
                    continue;
                }

                await page.GotoAsync($"https://ambers.amac.org.cn/web/app.html#/investorInfoEdit/{id}/{item.Date.Year}/{item.Date.Month / 3}");

                var sheet = $"temp\\{item.File.File.Name}";
                File.Copy(item.File.File.GetFullPath(), sheet);

                var ta = db.GetCollection<TransferRecord>().Find(x => x.FundId == item.FundId && x.ConfirmedDate < item.Date).ToArray();

                // 排除已全部赎回的
                var groupd = ta.GroupBy(x => x.InvestorId).Select(x => (id: x.Key, share: x.Sum(y => y.ShareChange()), saler: x.First().Agency)).Where(x => x.share > 0).ToDictionary(x => x.id, x => x);
                var ids = groupd.Keys.Select(x => new BsonValue(x));
                var investors = db.GetCollection<Investor>().Find(Query.In("_id", new BsonArray(ids))).ToList();
                var fundName = db.FindFund(item.Code)?.Name ?? $"未知产品{item.Code}";

                // 打包风揭
                var packs = PackDiscloseSheets(investors, item.FundId, fundName, item.Date);

                await UploadInvestorSheetAndZip(page, sheet, packs);





            }



            Console.WriteLine();


            return true;
        }
        catch (Exception e)
        {
            LogEx.Error(e);
            return false;
        }
        finally
        {
            pw.Dispose();
        }

    }

    /// <summary>
    /// 打包风险揭示书
    /// </summary>
    private static string[] PackDiscloseSheets(List<Investor> data, int FundId, string FundName, DateOnly PeriodEnd)
    {
        using var db = DbHelper.Base();
        var orders = db.GetCollection<TransferOrder>().Find(x => x.FundId == FundId && x.Date < PeriodEnd).OrderByDescending(x => x.Date).ToArray();

        var ids = data.Select(x => x.Id).ToList();

        var d = orders.Where(x => x.RiskDiscloure?.File is not null).GroupBy(x => x.InvestorId).
            Where(x => ids.Contains(x.Key)).Select(x => x.First()).Select(x => (x.InvestorId, File: x.RiskDiscloure!.File!)).ToList();

        if (d.Count != data.Count)
        {
            LogEx.Warning($"{FundName} 生成季度更新风险揭示书数据不匹配，应有{data.Count}，实际{d.Count}");
            WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Warning, "风险揭示书数量不全"));
        }
        return ZipSplitter.CreateSplitZip(d.Select(x => x.File).ToArray(), "temp", $"{FundName}_风险揭示书", 20 * 1024 * 1024);
    }

    /// <summary>
    /// 删除现有的投资人数据
    /// </summary>
    /// <param name="page"></param>
    /// <param name="delayMs"></param>
    /// <returns></returns>
    private static async Task BatchDeleteAllWithScrollAsync(IPage page, int delayMs = 200)
    {
        // 精准定位你的删除按钮
        var deleteLocator = page.Locator("table >> a[ng-click^='remove(investor)']:has-text('删除')");
        var count = await deleteLocator.CountAsync();

        if (count == 0)
        {
            return;
        }


        // 必须倒序删除！！！
        for (int i = count - 1; i >= 0; i--)
        {
            try
            {
                var delBtn = deleteLocator.Nth(i);

                // ====================== 关键：自动滚动到元素 ======================
                await delBtn.ScrollIntoViewIfNeededAsync();
                await page.WaitForTimeoutAsync(200); // 滚动后小停顿，更稳定

                // 等待按钮可见可用
                await delBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

                // 点击删除
                await delBtn.ClickAsync();

                // 如果有确认弹窗，打开下面这段，注释上面 Click
                /*
                await page.RunAndWaitForDialogAsync(async () =>
                {
                    await delBtn.ClickAsync();
                }, dialog =>
                {
                    dialog.AcceptAsync();
                    return Task.CompletedTask;
                });
                */

                Console.WriteLine($"已删除第 {i + 1} 条");
                await page.WaitForTimeoutAsync(delayMs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除第 {i + 1} 条失败：{ex.Message}");
            }
        }

        Console.WriteLine("✅ 批量删除全部完成！");
    }


    private static async Task<bool> UploadInvestorSheetAndZip(IPage page, string sheet, string[] zip)
    {
        try
        {
            // ==============================================
            // 步骤1：点击【模板导入】并上传 Excel
            // ==============================================
            var importBtn = page.GetByRole(AriaRole.Button, new() { Name = "模板导入" });
            await importBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
            //await importBtn.ClickAsync();

            // 定位隐藏的 file input
            var excelFileInput = page.Locator("#excelFile");
            await excelFileInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached, Timeout = 3000 });

            // 上传 Excel
            await excelFileInput.SetInputFilesAsync(sheet);
            await page.WaitForTimeoutAsync(1000); // 等待上传响应

            importBtn = page.Locator("#importBtn");
            await importBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
            await importBtn.ClickAsync();

            var errmsg = page.Locator("div.tab-content.m-info-table >> div.erroMess-content");
            if (await errmsg.IsVisibleAsync())
            {
                // 上传失败

                return false;
            }



            // ==============================================
            // 步骤2：删除风险揭示书现有所有附件
            // ==============================================
            if (zip.Length > 0)
            {
                var riskFileList = page.Locator("ul.data-list li:visible");
                var deleteButtons = riskFileList.Locator("button", new() { HasText = "删除" });

                if (await deleteButtons.CountAsync() > 0)
                {
                    var deleteCount = await deleteButtons.CountAsync();
                    for (int i = 0; i < deleteCount; i++)
                    {
                        // 每次都重新获取，避免DOM刷新导致元素失效
                        await deleteButtons.First.ClickAsync();
                        await page.WaitForTimeoutAsync(500);
                    }
                }

                // ==============================================
                // 步骤3：上传多个风险揭示书文件（zip/pdf等）
                // ==============================================
                var riskUploadInput = page.Locator("#valid_PA008");
                await riskUploadInput.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached, Timeout = 5000 });

                // 批量上传
                await riskUploadInput.SetInputFilesAsync(zip);
                await page.WaitForTimeoutAsync(2000);
            }

            return true;
        }
        catch (Exception ex)
        {
            LogEx.Error(ex);
            return false;
        }
    }


    /// <summary>
    /// 自动翻页采集基金报表数据（直到找到过期数据/无下一页）
    /// </summary>
    public static async Task<List<PrivateFundQuarterReport>> CrawlAllFundReportAsync(IPage page)
    {
        var allData = new List<PrivateFundQuarterReport>();

        while (true)
        {
            // 等待表格加载
            await page.WaitForSelectorAsync("tbody tr.ng-scope", new() { Timeout = 5000 });

            // 解析当前页
            var currentPageData = await ParseFundReportTableAsync(page);
            allData.AddRange(currentPageData);

            // 判断：本页是否【全部都是未来截止日】→ 需要翻页
            bool allFuture = currentPageData.All(x => x.ReportDeadline > DateTime.Now);
            if (!allFuture)
                break;


            // ============== 定位【下一页】 ==============
            var nextPageItem = page.Locator("li:has(a:contains('下一页'))");

            // ============== 判断是否禁用（最后一页） ==============
            bool isDisabled = (await nextPageItem.GetAttributeAsync("class"))?.Contains("disabled") ?? true;

            if (isDisabled)
                break;

            // ============== 点击下一页 ==============
            await nextPageItem.Locator("a").ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(800); // 等待 Angular 渲染
        }

        return allData;
    }

    /// <summary>
    /// 解析AMAC私募产品季度报告表格
    /// </summary>
    /// <param name="page">Playwright Page对象</param>
    /// <returns>解析后的表格数据列表</returns>
    public static async Task<List<PrivateFundQuarterReport>> ParseFundReportTableAsync(IPage page)
    {
        var reportList = new List<PrivateFundQuarterReport>();
        var rows = await page.Locator("tbody tr.ng-scope").AllAsync();

        foreach (var row in rows)
        {
            var cells = await row.Locator("td").AllAsync();
            if (cells.Count < 11) continue;

            var report = new PrivateFundQuarterReport();

            // 1. 序号
            report.SerialNumber = int.TryParse(await cells[0].TextContentAsync(), out int sn) ? sn : 0;
            // 2. 产品名称
            report.ProductName = (await cells[1].TextContentAsync())?.Trim() ?? string.Empty;
            // 3. 产品编码
            report.ProductCode = (await cells[2].TextContentAsync())?.Trim() ?? string.Empty;

            // 4. 投资者信息更新：状态(bool) + 链接
            var investorCell = cells[3];
            var investorLink = investorCell.Locator("a");
            report.InvestorUpdateUrl = await investorLink.GetAttributeAsync("href") ?? string.Empty;
            var investorText = await investorLink.TextContentAsync() ?? string.Empty;
            report.IsInvestorFilled = investorText.Contains("已填报");

            // 5. 运行信息更新：状态(bool) + 链接
            var operationCell = cells[4];
            var operationLink = operationCell.Locator("a");
            report.OperationUpdateUrl = await operationLink.GetAttributeAsync("href") ?? string.Empty;
            var operationText = await operationLink.TextContentAsync() ?? string.Empty;
            report.IsOperationFilled = operationText.Contains("已填报");

            // 6. 报告基准日
            report.ReportBaseDate = (await cells[5].TextContentAsync())?.Trim() ?? string.Empty;
            // 7. 报送截止日
            report.ReportDeadline = DateTime.TryParse(await cells[6].TextContentAsync(), out DateTime deadline)
                ? deadline
                : DateTime.MinValue;
            // 8. 提交次数
            report.SubmitCount = int.TryParse(await cells[7].TextContentAsync(), out int count) ? count : 0;
            // 9. 倒计时
            report.CountdownDays = (await cells[8].TextContentAsync())?.Trim() ?? string.Empty;
            // 10. 填报日期
            report.FillDate = (await cells[9].TextContentAsync())?.Trim() ?? string.Empty;
            // 11. 提交状态
            report.IsSubmited = (await cells[10].TextContentAsync())?.Trim() switch { "已提交" => true, _ => false };
            // 12. 操作按钮
            var operationBtn = cells[11].Locator("a");
            report.OperationText = await operationBtn.CountAsync() > 0
                ? (await operationBtn.TextContentAsync())?.Trim() ?? string.Empty
                : string.Empty;

            reportList.Add(report);
        }
        return reportList;
    }
}




/// <summary>
/// 私募证券投资基金季度报告表格实体
/// </summary>
public class PrivateFundQuarterReport
{
    /// <summary>
    /// 序号
    /// </summary>
    public int SerialNumber { get; set; }

    /// <summary>
    /// 产品名称
    /// </summary>
    public string ProductName { get; set; }

    /// <summary>
    /// 产品编码
    /// </summary>
    public string ProductCode { get; set; }

    /// <summary>
    /// 投资者信息更新是否已填报（bool）
    /// </summary>
    public bool IsInvestorFilled { get; set; }

    /// <summary>
    /// 投资者信息更新 填报链接（未填报时保存地址）
    /// </summary>
    public string? InvestorUpdateUrl { get; set; }

    /// <summary>
    /// 运行信息更新是否已填报（bool）
    /// </summary>
    public bool IsOperationFilled { get; set; }

    /// <summary>
    /// 运行信息更新 填报链接（未填报时保存地址）
    /// </summary>
    public string? OperationUpdateUrl { get; set; }

    /// <summary>
    /// 报告基准日
    /// </summary>
    public string ReportBaseDate { get; set; }

    /// <summary>
    /// 报送截止日
    /// </summary>
    public DateTime ReportDeadline { get; set; }

    /// <summary>
    /// 提交次数
    /// </summary>
    public int SubmitCount { get; set; }

    /// <summary>
    /// 倒计时（天）
    /// </summary>
    public string CountdownDays { get; set; }

    /// <summary>
    /// 操作（填报/提交/重报按钮文本）
    /// </summary>
    public string OperationText { get; set; }

    /// <summary>
    /// 提交状态（已提交/未提交）
    /// </summary>
    public bool IsSubmited { get; set; }

    /// <summary>
    /// 填报日期（运行信息更新填报时间）
    /// </summary>
    public string FillDate { get; set; }
}