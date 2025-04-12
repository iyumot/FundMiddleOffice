using FMO.Models;
using FMO.Utilities;
using Microsoft.Playwright;
using Serilog;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                await using var page = await Automation.AcquirePage(Identifier);
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
                //var url = $"https://i.cmschina.com/public/tgfw-view/index.html#/iisp/share-register/customer-transaction-info/history-trading-detail?mid=50100";
                var locator = page.Locator("span", new PageLocatorOptions { HasText = "直销与TA" });
                if (await locator.CountAsync() == 0)
                {
                    Log.Error($"{nameof(CMSChinaAssist)}: {fun} 未找到 直销与TA");
                    return false;
                }

                var box = await locator.BoundingBoxAsync();
                await page.Mouse.MoveAsync(box!.X + box.Width / 2, box.Y + box.Height / 2);

                locator = page.GetByRole(AriaRole.Button, new() { Name = "交易确认" });
                if (await locator.CountAsync() == 0)
                {
                    Log.Error($"{nameof(CMSChinaAssist)}: {fun} 未找到 交易确认");
                    return false;
                }
                box = await locator.BoundingBoxAsync();
                await page.Mouse.MoveAsync(box!.X + box.Width / 2, box.Y + box.Height / 2);

                locator = page.GetByText("历史交易确认明细"); 
                box = await locator.BoundingBoxAsync();
                //await page.Mouse.MoveAsync(box!.X + box.Width / 2, box.Y + box.Height / 2); 

                IResponse? response = null;
                await page.RunAndWaitForResponseAsync(async () => await page.Mouse.ClickAsync(box!.X + box.Width / 2, box.Y + box.Height / 2), resp =>
                {
                    if (resp.Url != $"https://i.cmschina.com/tgfw-ta/hisTranConfirm/hisTranConfirm/getHisTranConfirmList") return false;
                    response = resp;
                    return true;
                });

                // 关闭提示框
                locator = page.Locator("i.anticon.anticon-close");
                try { await locator.ClickAsync(new LocatorClickOptions { Timeout = 2000 }); } catch { }

                // 设置为最大条数
                locator = page.Locator("span", new PageLocatorOptions { HasText = "条/页" });
                locator = await FirstVisible(locator);
                await locator.ClickAsync();

                locator = page.Locator("div.rc-virtual-list >> div").Filter(new LocatorFilterOptions { HasTextRegex = new Regex(@"\d+\s*条\s*/\s*页") });
                locator = await GetMaxPageOption(locator);
                var itemcnt = await locator.Last.InnerTextAsync();
                Log.Debug($"选中{itemcnt}");
                await locator.Last.ClickAsync();


                var (refresh, oldb, olde) = await SetDateAsync(page, DateOnly.FromDateTime(last.Time), DateOnly.FromDateTime(DateTime.Now));

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
                        return false;
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
                    IEnumerable<Json.TransferRecordJson.Item> rows = obj!.rows.Where(x => x.VC_YWLX != "认购确认");
                    data.AddRange(rows.Select(x => x.ToObject(Identifier)));
                    response = null;

                    // 判断一下页
                    locator = page.GetByRole(AriaRole.Button, new() { Name = "right" });
                    locator = await FirstVisible(locator);
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
                var oids = db.GetCollection<TransferRecord>().Find(x => x.Source == Identifier).Select(x => new { x.Id, x.ExternalId });
                foreach (var item in data)
                    item.Id = oids.FirstOrDefault(x => x.ExternalId == item.ExternalId)?.Id ?? 0;

                db.GetCollection<TransferRecord>().EnsureIndex(x => new { x.Source, x.ExternalId }, true);
                db.GetCollection<TransferRecord>().Upsert(data);
                db.Dispose();


                // 更新同步记录
                db = DbHelper.Platform();
                last.Time = DateTime.Now;
                if (last.Id == 0)
                    last.Id = db.GetCollection<PlatformSynchronizeTime>().FindOne(x => x.Identifier == Identifier && x.Method == fun)?.Id ?? 0;
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

        public async Task<(bool result, DateOnly oldBegin, DateOnly oldEnd)> SetDateAsync(IPage page, DateOnly begin, DateOnly end)
        {
            try
            {
                var locator = page.GetByPlaceholder("开始日期");
                locator = await FirstVisible(locator);
                DateOnly.TryParseExact(await locator.InputValueAsync(), "yyyy-MM-dd", out DateOnly oldbegin);
                if (oldbegin > begin)
                {
                    await locator.ClickAsync();
                    await Task.Delay(500);

                    await locator.First.EvaluateAsync("element =>{element.removeAttribute('readonly');}");
                    await locator.FillAsync(begin.ToString("yyyy-MM-dd"), new LocatorFillOptions { Force = true, Timeout = 2000 });


                }
                locator = page.GetByPlaceholder("结束日期");
                locator = await FirstVisible(locator);
                DateOnly.TryParseExact(await locator.InputValueAsync(), "yyyy-MM-dd", out DateOnly oldend);
                if (oldend < end)
                {
                    await locator.ClickAsync();
                    await Task.Delay(500);

                    await locator.First.EvaluateAsync("element =>{element.removeAttribute('readonly');}");
                    await locator.FillAsync(end.ToString("yyyy-MM-dd"), new LocatorFillOptions { Timeout = 2000 });

                }

                await Task.Delay(500);
                await page.Keyboard.PressAsync("Enter");
                await page.Keyboard.PressAsync("Enter");
                await page.Keyboard.PressAsync("Enter");



                // 验证
                locator = page.GetByPlaceholder("开始日期");
                locator = await FirstVisible(locator);
                if (!DateOnly.TryParseExact(await locator.InputValueAsync(), "yyyy-MM-dd", out DateOnly tmp) || tmp != begin)
                {
                    // 失败
                    return default;
                }
                // 验证
                locator = page.GetByPlaceholder("结束日期");
                locator = await FirstVisible(locator);
                if (!DateOnly.TryParseExact(await locator.InputValueAsync(), "yyyy-MM-dd", out tmp) || tmp != end)
                {
                    // 失败 
                    return default;
                }
                return (true, oldbegin, oldend);
            }
            catch (Exception e)
            {
                Log.Error($"设置日期失败，{e.Message}");
                return (false, default, default);
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
                await using var page = await Automation.AcquirePage(Identifier);
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
                //var url = $"https://i.cmschina.com/public/tgfw-view/index.html#/iisp/share-register/customer-transaction-info/historical-transaction-query?mid=50103";

                var locator = page.Locator("span", new PageLocatorOptions { HasText = "直销与TA" });
                if (await locator.CountAsync() == 0)
                {
                    Log.Error($"{nameof(CMSChinaAssist)}: {fun} 未找到 直销与TA");
                    return false;
                }

                int lcnt = await locator.CountAsync();
                var box = await locator.BoundingBoxAsync();
                await page.Mouse.MoveAsync(box!.X + box.Width / 2, box.Y + box.Height / 2);

                //await locator.ClickAsync();

                locator = page.GetByRole(AriaRole.Button, new() { Name = "交易申请", Exact = true });
                if (await locator.CountAsync() == 0)
                {
                    Log.Error($"{nameof(CMSChinaAssist)}: {fun} 未找到 交易申请");
                    return false;
                }
                box = await locator.BoundingBoxAsync();
                await page.Mouse.MoveAsync(box!.X + box.Width / 2, box.Y + box.Height / 2);

                locator = page.GetByText("历史交易申请");
                box = await locator.BoundingBoxAsync();
                //await page.Mouse.MoveAsync(box!.X + box.Width / 2, box.Y + box.Height / 2); 

                IResponse? response = null;
                await page.RunAndWaitForResponseAsync(async () => await page.Mouse.ClickAsync(box!.X + box.Width / 2, box.Y + box.Height / 2), resp =>
                {
                    if (resp.Url != $"https://i.cmschina.com/tgfw-ta/hisTranApply/hisTranApplyList/getList") return false;
                    response = resp;
                    return true;
                });

                // 设置为最大条数
                locator = page.Locator("span", new PageLocatorOptions { HasText = "条/页" });
                locator = await FirstVisible(locator);
                await locator.ClickAsync();

                locator = page.Locator("div", new PageLocatorOptions { HasTextRegex = new Regex(@"\d+\s*条\s*/\s*页") });
                var itemcnt = await locator.Last.InnerTextAsync();
                Log.Debug($"选中{itemcnt}");
                await locator.Last.ClickAsync();

                //bool refresh = false;
                // 检查日期

                var (refresh, oldb, olde) = await SetDateAsync(page, DateOnly.FromDateTime(last.Time), DateOnly.FromDateTime(DateTime.Now));

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
                    var rows = obj!.rows.SkipLast(1).Where(x => !x.VC_YWLX?.Contains("分红方式") ?? true);
                    data.AddRange(rows.Select(x => x.ToObject(Identifier)));
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


                // 更新同id数据
                var oids = db.GetCollection<TransferRequest>().Find(x => x.Source == Identifier).Select(x => new { x.Id, x.ExternalId });
                foreach (var item in data)
                    item.Id = oids.FirstOrDefault(x => x.ExternalId == item.ExternalId)?.Id ?? 0;

                db.GetCollection<TransferRequest>().EnsureIndex(x => new { x.Source, x.ExternalId }, true);
                db.GetCollection<TransferRequest>().Upsert(data);
                db.Dispose();


                // 更新同步记录
                db = DbHelper.Platform();
                last.Time = DateTime.Now;
                if (last.Id == 0)
                    last.Id = db.GetCollection<PlatformSynchronizeTime>().FindOne(x => x.Identifier == Identifier && x.Method == fun)?.Id ?? 0;
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


        public async Task<ILocator> GetMaxPageOption(ILocator locator)
        {
            var data = await locator.AllAsync();

            Regex regex = new Regex(@"\d+");
            ILocator max = locator.First;
            int cnt = 0;
            foreach (var d in data)
            {
                var text = await d.InnerTextAsync();
                var m = regex.Matches(text);
                if(m.Count == 1)
                {
                    int v = int.Parse(m[0].Value);
                    if(v >= cnt)
                    {
                        cnt = v; 

                        if (await d.IsVisibleAsync())
                            max = d;
                    }
                }
            }

            return max;
        }
    }


}
