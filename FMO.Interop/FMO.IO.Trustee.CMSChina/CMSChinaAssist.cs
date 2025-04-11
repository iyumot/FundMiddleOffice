using FMO.Models;
using FMO.Utilities;
using Microsoft.Playwright;
using Serilog;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FMO.IO.Trustee.CMSChina
{
    public class CMSChinaAssist : TrusteeAssistBase
    {
        public override string Identifier => "cmschina";

        public override string Name => "招商证券股份有限公司";

        public override string Domain => "https://i.cmschina.com";

        public override Regex HoldingCheck { get; init; } = new Regex("cmschina.com");

        public override Task<(string Code, ManageFeeDetail[] Fee)[]> GetManageFeeDetails(DateOnly start, DateOnly end)
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> LoginValidationOverrideAsync(IPage page)
        {
            return await page.GetByText("产品管理").CountAsync() > 0;
        }

        public override Task<bool> SynchronizeCustomerAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> SynchronizeDistributionAsync()
        {
            return Task.FromResult(true);
        }

        public override Task<bool> SynchronizeFundRaisingRecord()
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> SynchronizeTransferRecordAsync()
        {
            const string fun = nameof(SynchronizeTransferRecordAsync);
            try
            {
                // 判断登陆状态
                if (!IsLogedIn && !await ((IExternPlatform)this).LoginAsync())
                    return false;

            }
            catch (Exception e)
            {
                Log.Error($"{fun} 登陆出错 {e.Message}");
                return false;
            }

            try
            {
                using var page = await Automation.AcquirePage(Identifier);
                if (page.IsNew) await page.GotoAsync(Domain);


                LiteDB.ILiteDatabase db = DbHelper.Platform();
                var last = db.GetCollection<PlatformSynchronizeTime>().FindOne(x => x.Identifier == Identifier && x.Method == fun);
                db.Dispose();

                if (last is null)
                {
                    Log.Information($"{nameof(CMSChinaAssist)}.{fun}->初始建档");

                    // 获取最早日期
                    db = DbHelper.Base();
                    var date = db.GetCollection<Fund>().FindAll().Min(x => x.SetupDate);
                    last = new PlatformSynchronizeTime { Identifier = Identifier, Method = fun, Time = new DateTime(date.AddYears(-1), default) };
                    db.Dispose();
                }


                // 跳转
                var url = $"https://i.cmschina.com/public/tgfw-view/index.html#/iisp/share-register/customer-transaction-info/history-trading-detail?mid=50100";

                IResponse? response = null;
                await page.RunAndWaitForResponseAsync(async () => await page.GotoAsync(url), resp =>
                {
                    if (resp.Url != $"https://i.cmschina.com/tgfw-ta/hisTranConfirm/hisTranConfirm/getHisTranConfirmList") return false;
                    response = resp;
                    return true;
                });

                // 设置为最大条数
                var locator = page.Locator("span", new PageLocatorOptions { HasText = "条/页" });
                locator = await FirstVisible(locator);
                await locator.ClickAsync();

                locator = page.Locator("div", new PageLocatorOptions { HasTextRegex = new Regex(@"\d+\s*条\s*/\s*页") });
                var itemcnt = await locator.Last.InnerTextAsync();
                Log.Debug($"选中{itemcnt}");
                await locator.Last.ClickAsync();

                bool refresh = false;
                // 检查日期
                locator = page.GetByPlaceholder("开始日期");
                DateOnly.TryParseExact(await locator.InputValueAsync(), "yyyy-MMM-dd", out DateOnly begin);
                var oldbegin = DateOnly.FromDateTime(last.Time);
                if (oldbegin < begin)
                {
                    refresh = true;
                    await locator.FillAsync(oldbegin.ToString("yyyy-MMM-dd"));
                }
                locator = page.GetByPlaceholder("结束日期");
                DateOnly.TryParseExact(await locator.InputValueAsync(), "yyyy-MMM-dd", out DateOnly end);
                if (end != DateOnly.FromDateTime(DateTime.Today))
                {
                    refresh = true;
                    await locator.FillAsync(DateTime.Today.ToString("yyyy-MMM-dd"));
                }

                if (refresh)
                {
                    try
                    {
                        await page.RunAndWaitForResponseAsync(async () => await page.GetByRole(AriaRole.Button, new() { Name = "查 询" }).ClickAsync(), resp =>
                        {
                            if (resp.Url != $"https://i.cmschina.com/tgfw-ta/hisTranConfirm/hisTranConfirm/getHisTranConfirmList") return false;
                            response = resp;
                            return true;
                        });
                    }
                    catch (Exception e)
                    {
                        Log.Error($"{fun} 未找到查询按钮或者未获取到json {e.Message}");
                    }
                }

                // 解析json
                List<TransferRecord> data = new();

                for (int i = 0; i < 99; i++)
                {
                    if (response is null) break;

                    var json = await response.TextAsync();
                    var obj = JsonSerializer.Deserialize<CMSChina.Json.TransferRecordJson.Root>(json);

                    ///////////////////////
                    data.AddRange(obj!.rows.Select(x => x.ToObject(Identifier)));
                    response = null;

                    // 判断一下页
                    locator = page.GetByRole(AriaRole.Button, new() { Name = "right" });
                    if (await locator.IsDisabledAsync())
                        break;

                    await page.RunAndWaitForResponseAsync(async () => await locator.ClickAsync(), resp =>
                    {
                        if (resp.Url != $"https://i.cmschina.com/tgfw-ta/hisTranConfirm/hisTranConfirm/getHisTranConfirmList") return false;
                        response = resp;
                        return true;
                    });
                }

                // 判断是否有数据
                if (data.Count == 0) return false;
                // 保存到数据库
                db = DbHelper.Base();

                // 客户映射
                var ids = data.Select(x => x.CustomerIdentity).Distinct().ToList();

                var customers = db.GetCollection<Investor>().Query().ToArray();
                Investor? lastc = null;
                foreach (var item in data)
                {
                    if (lastc is not null && lastc.Name == item.CustomerName && lastc.Identity.Id == item.CustomerIdentity)
                    {
                        item.CustomerId = lastc.Id;
                        continue;
                    }

                    lastc = customers.FirstOrDefault(x => x.Name == item.CustomerName && x.Identity.Id == item.CustomerIdentity);
                    if (lastc is not null)
                        item.CustomerId = lastc.Id;
                    else Log.Error("");
                }

                // fundid
                var ids3 = db.GetCollection<Fund>().Query().Select(x => new { x.Id, x.Code, x.Name }).ToList();
                foreach (var item in data) // 存在子份额名称与基金不一致
                    item.FundId = (ids3.FirstOrDefault(x => x.Code == item.FundCode) ?? ids3.FirstOrDefault(x => item.FundName == x.Name))?.Id ?? 0;


                // 设置idnex
                db.GetCollection<TransferRecord>().EnsureIndex(x => new { x.Source, x.ExternalId }, true);
                db.GetCollection<TransferRecord>().Upsert(data);
                db.Dispose();


                // 更新同步记录
                db = DbHelper.Platform();
                last.Time = DateTime.Now;
                db.GetCollection<PlatformSynchronizeTime>().Upsert(last);
                db.Dispose();
                return true;

            }
            catch (Exception e)
            {
                Log.Error($"{fun} 获取页面出错 {e.Message}");
                return false;
            }

        }

        public override async Task<bool> SynchronizeTransferRequestAsync()
        {
            const string fun = nameof(SynchronizeTransferRequestAsync);
            try
            {
                // 判断登陆状态
                if (!IsLogedIn && !await ((IExternPlatform)this).LoginAsync())
                    return false;

            }
            catch (Exception e)
            {
                Log.Error($"{fun} 登陆出错 {e.Message}");
                return false;
            }

            try
            {
                using var page = await Automation.AcquirePage(Identifier);
                if (page.IsNew) await page.GotoAsync(Domain);


                LiteDB.ILiteDatabase db = DbHelper.Platform();
                var last = db.GetCollection<PlatformSynchronizeTime>().FindOne(x => x.Identifier == Identifier && x.Method == fun);
                db.Dispose();

                if (last is null)
                {
                    Log.Information($"{nameof(CMSChinaAssist)}.{fun}->初始建档");

                    // 获取最早日期
                    db = DbHelper.Base();
                    var date = db.GetCollection<Fund>().FindAll().Min(x => x.SetupDate);
                    last = new PlatformSynchronizeTime { Identifier = Identifier, Method = fun, Time = new DateTime(date.AddYears(-1), default) };
                    db.Dispose();
                }


                // 跳转
                var url = $"https://i.cmschina.com/public/tgfw-view/index.html#/iisp/share-register/customer-transaction-info/historical-transaction-query?mid=50103";

                IResponse? response = null;
                await page.RunAndWaitForResponseAsync(async () => await page.GotoAsync(url), resp =>
                {
                    if (resp.Url != $"https://i.cmschina.com/tgfw-ta/hisTranApply/hisTranApplyList/getList") return false;
                    response = resp;
                    return true;
                });

                // 设置为最大条数
                var locator = page.Locator("span", new PageLocatorOptions { HasText = "条/页" });
                locator = await FirstVisible(locator);
                await locator.ClickAsync();

                locator = page.Locator("div", new PageLocatorOptions { HasTextRegex = new Regex(@"\d+\s*条\s*/\s*页") });
                var itemcnt = await locator.Last.InnerTextAsync();
                Log.Debug($"选中{itemcnt}");
                await locator.Last.ClickAsync();

                bool refresh = false;
                // 检查日期
                locator = page.GetByPlaceholder("开始日期");
                DateOnly.TryParseExact(await locator.InputValueAsync(), "yyyy-MMM-dd", out DateOnly begin);
                var oldbegin = DateOnly.FromDateTime(last.Time);
                if (oldbegin < begin)
                {
                    refresh = true;
                    await locator.FillAsync(oldbegin.ToString("yyyy-MMM-dd"));
                }
                locator = page.GetByPlaceholder("结束日期");
                DateOnly.TryParseExact(await locator.InputValueAsync(), "yyyy-MMM-dd", out DateOnly end);
                if (end != DateOnly.FromDateTime(DateTime.Today))
                {
                    refresh = true;
                    await locator.FillAsync(DateTime.Today.ToString("yyyy-MMM-dd"));
                }

                if (refresh)
                {
                    try
                    {
                        await page.RunAndWaitForResponseAsync(async () => await page.GetByRole(AriaRole.Button, new() { Name = "查 询" }).ClickAsync(), resp =>
                                   {
                                       if (resp.Url != $"https://i.cmschina.com/tgfw-ta/hisTranApply/hisTranApplyList/getList") return false;
                                       response = resp;
                                       return true;
                                   });
                    }
                    catch (Exception e)
                    {
                        Log.Error($"{fun} 未找到查询按钮或者未获取到json {e.Message}");
                    }
                }

                // 解析json
                List<TransferRequest> data = new();

                for (int i = 0; i < 99; i++)
                {
                    if (response is null) break;

                    var json = await response.TextAsync();
                    var obj = JsonSerializer.Deserialize<CMSChina.Json.TransferRequestJson.Root>(json);
                    data.AddRange(obj!.rows.Select(x => x.ToObject(Identifier)));
                    response = null;

                    // 判断一下页
                    locator = page.GetByRole(AriaRole.Button, new() { Name = "right" });
                    if (await locator.IsDisabledAsync())
                        break;

                    await page.RunAndWaitForResponseAsync(async () => await locator.ClickAsync(), resp =>
                    {
                        if (resp.Url != $"https://i.cmschina.com/tgfw-ta/hisTranApply/hisTranApplyList/getList") return false;
                        response = resp;
                        return true;
                    });
                }

                // 判断是否有数据
                if (data.Count == 0) return false;
                // 保存到数据库
                db = DbHelper.Base();
                // 客户映射
                var ids = data.Select(x => x.CustomerIdentity).Distinct().ToList();

                var customers = db.GetCollection<Investor>().Query().ToArray();
                Investor? lastc = null;
                foreach (var item in data)
                {
                    if (lastc is not null && lastc.Name == item.CustomerName && lastc.Identity.Id == item.CustomerIdentity)
                    {
                        item.CustomerId = lastc.Id;
                        continue;
                    }

                    lastc = customers.FirstOrDefault(x => x.Name == item.CustomerName && x.Identity.Id == item.CustomerIdentity);
                    if (lastc is not null)
                        item.CustomerId = lastc.Id;
                    else Log.Error("");
                }

                // fundid
                var ids3 = db.GetCollection<Fund>().Query().Select(x => new { x.Id, x.Code, x.Name }).ToList();
                foreach (var item in data) // 存在子份额名称与基金不一致
                    item.FundId = (ids3.FirstOrDefault(x => x.Code == item.FundCode) ?? ids3.FirstOrDefault(x => item.FundName.StartsWith(x.Name)))?.Id ?? 0;


                // 排除同id数据
                db.GetCollection<TransferRequest>().EnsureIndex(x => new { x.Source, x.ExternalId }, true);
                db.GetCollection<TransferRequest>().Upsert(data);
                db.Dispose();


                // 更新同步记录
                db = DbHelper.Platform();
                if (last is null) last = new PlatformSynchronizeTime { Identifier = Identifier, Method = fun, Time = DateTime.Now };
                else last.Time = DateTime.Now;
                db.GetCollection<PlatformSynchronizeTime>().Upsert(last);
                db.Dispose();
                return true;

            }
            catch (Exception e)
            {
                Log.Error($"{fun} 获取页面出错 {e.Message}");
                return false;
            }

        }
    }
}
