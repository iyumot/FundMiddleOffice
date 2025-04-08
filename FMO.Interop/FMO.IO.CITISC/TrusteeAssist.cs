//using FMO.IO.Trustee.Json.FundRasing;
using FMO.IO.Trustee.CITISC;
using FMO.Models;
using FMO.Utilities;
using Microsoft.Playwright;
using Serilog;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace FMO.IO.Trustee;

/// <summary>
/// 托管接口
/// </summary>
public class CSTISCAssist : TrusteeAssistBase
{

    public override string Identifier => "cstisc";

    public override string Name => "中信证券股份有限公司";


    public override string Domain => "https://iservice.citics.com/";

    public override Regex HoldingCheck { get; init; } = new Regex("citics.com");



    /// <summary>
    /// 获取每页数据条数
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    private ILocator ItemCountPerPageLocator(IPage page) => page.Locator("div.main-page-box.main-select-content.ivu-row >> div.ivu-select-selection >> ivu-select-selected-value");







    public override async Task<bool> LoginValidationOverrideAsync(IPage page)
    {

        if (await page.GetByText("首页").CountAsync() > 0)
            return true;

        return false;
    }



    #region 同步募集户流水

    /// <summary>
    /// 获取对应时间内的流水
    /// </summary>
    /// <param name="page"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    private async Task<BankTransaction[]?> FundRaisingRecordAsync(IPage page, DateTime start, DateTime end)
    {
        try
        {
            List<BankTransaction> data = new();

            var from = start.ToString("yyyy-MM-dd");
            var to = end.ToString("yyyy-MM-dd");

            // 设置日期
            var loc = page.GetByPlaceholder("开始日期");
            if (await loc.CountAsync() == 0)
            {
                Log.Warning($"{nameof(CSTISCAssist)}.{nameof(FundRaisingRecordAsync)}->未找到日期设置框");
                return null;
            }

            await loc.EvaluateAsync("node => node.setAttribute('type', 'text')");
            await loc.First.EvaluateAsync("element =>{element.removeAttribute('readonly');}");
            await loc.First.FillAsync(from);

            loc = page.GetByPlaceholder("结束日期");
            if (await loc.CountAsync() == 0)
            {
                Log.Warning($"{nameof(CSTISCAssist)}.{nameof(FundRaisingRecordAsync)}->未找到日期设置框");
                return null;
            }

            await loc.EvaluateAsync("node => node.setAttribute('type', 'text')");
            await loc.First.EvaluateAsync("element =>{element.removeAttribute('readonly');}");
            await loc.First.FillAsync(to);


            loc = page.Locator("[id=\"__qiankun_microapp_wrapper_for_iservice__\"]").GetByText("100 条/页");
            var lcnt = await loc.CountAsync();
            if (lcnt > 1)
                loc = page.Locator("form.ivu-form.ivu-form-label-right.ivu-form-inline > div.kr-tableFilter >> span.search-button > button");//GetByRole(AriaRole.Button, new() { NameRegex = new Regex( "查询") });
            else
                await page.Locator("[id=\"__qiankun_microapp_wrapper_for_iservice__\"] span").Filter(new() { HasText = "条/页" }).ClickAsync();


            //lcnt = await loc.CountAsync();

            IResponse? response = null;

            /// 尝试3次
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await page.RunAndWaitForResponseAsync(async () => await loc.ClickAsync(), resp =>
                    {
                        if (resp.Url != "https://iservice.citics.com/api/sys/midPlatformCommonMethod?funcId=307") return false;
                        response = resp;
                        return true;
                    }, new PageRunAndWaitForResponseOptions { Timeout = 5000 });
                }
                catch { }

                if (response is not null) break;
            }



            if (response is null)
            {
                Log.Warning($"{nameof(CSTISCAssist)}.{nameof(FundRaisingRecordAsync)}->获取json数据异常，可能托管修改了流程");
                return null;
            }
            var json = await response.TextAsync();

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new DateTimeConverterUsingDateTimeParse());

            try
            {
                var obj = JsonSerializer.Deserialize<CITISC.Json.FundRasing.JsonRootDto>(json, options);
                if (obj?.Data?.Total == 0)
                {
                    Log.Information($"{nameof(CSTISCAssist)}.{nameof(FundRaisingRecordAsync)}->未解析到流水");
                    return Array.Empty<BankTransaction>();
                }

                // 中信开发真逆天
                foreach (var item in obj!.Data!.List)
                {
                    data.Add(item.ToTransaction(Identifier));
                }

                /// 下一页的数据
                while (obj.Data.HasNextPage)
                {
                    await Task.Delay(500);

                    loc = page.GetByRole(AriaRole.Listitem, new() { Name = "下一页" }).Locator("a");

                    await page.RunAndWaitForResponseAsync(async () => await loc.ClickAsync(), resp =>
                    {
                        if (resp.Url != "https://iservice.citics.com/api/sys/midPlatformCommonMethod?funcId=307") return false;
                        response = resp;
                        return true;
                    });

                    if (response is null)
                    {
                        Log.Warning($"{nameof(CSTISCAssist)}.{nameof(FundRaisingRecordAsync)}->获取json数据异常，可能托管修改了流程");
                        return null;
                    }

                    json = await response.TextAsync();


                    obj = JsonSerializer.Deserialize<CITISC.Json.FundRasing.JsonRootDto>(json, options);
                    if (obj?.Data?.Total == 0)
                    {
                        Log.Information($"{nameof(CSTISCAssist)}.{nameof(FundRaisingRecordAsync)}->未解析“下一页”中的流水");
                        return null;
                    }

                    foreach (var item in obj!.Data!.List)
                    {
                        data.Add(item.ToTransaction(Identifier));
                    }
                }


                return data.ToArray();
            }
            catch (JsonException ex)
            {
                Log.Warning($"{nameof(CSTISCAssist)}.{nameof(FundRaisingRecordAsync)}->json解析异常，可能托管修改了模型：{ex.Message}");
                return null;
            }




        }
        catch (Exception e)
        {
            Log.Warning($"{nameof(CSTISCAssist)}.{nameof(FundRaisingRecordAsync)}->设置日期异常：{e.Message}");
            return null;
        }

    }

    /// <summary>
    /// 同步募集户流水
    /// </summary>
    /// <returns></returns>
    public override async Task<bool> SynchronizeFundRaisingRecord()
    {
        // 判断登陆状态
        if (!IsLogedIn && !await ((IExternPlatform)this).LoginAsync())
            return false;

        // 获取已保存的流水最新日期
        var db = DbHelper.Base();
        var last = db.GetCollection<BankTransaction>().Query().Where(x => x.Origin == Identifier).OrderByDescending(x => x.Time).FirstOrDefault();
        // var bankaccs = db.GetCollection<BankAccount>().FindAll();
        db.Dispose();
        if (last is null)
            Log.Information($"{nameof(CSTISCAssist)}.{nameof(FundRaisingRecordAsync)}->初始建档");

        string url = $"https://iservice.citics.com/iservice/mjzj/mjzhlscx?refresh={DateTime.Now.TimeStampBySeconds()}";

        using var page = await Automation.AcquirePage(Identifier);


        IResponse? response = null;
        await page.RunAndWaitForResponseAsync(async () => await page.GotoAsync(url), resp =>
        {
            if (resp.Url != "https://iservice.citics.com/api/sys/midPlatformCommonMethod?funcId=307") return false;
            response = resp;
            return true;
        });

        if (response is null)
        {
            Log.Warning($"{nameof(CSTISCAssist)}.{nameof(FundRaisingRecordAsync)}->获取json数据异常，可能托管修改了流程");
            return false;
        }


        //
        DateTime ori = last?.Time ?? new DateTime(1970, 1, 1), start = DateTime.Today.AddYears(-1), end = DateTime.Today;

        int empty_year = 0;
        List<BankTransaction> transactions = new List<BankTransaction>();
        while (end >= ori)
        {
            if (ori > start)
                start = ori;

            var vals = await FundRaisingRecordAsync(page, start, end);
            if (vals is not null)
            {
                transactions.AddRange(vals);

                if (vals.Length == 0) ++empty_year;
            }

            if (empty_year > 2) break;

            end = start.AddDays(-1);
            start = end.AddYears(-1);
        }

        // 保存到数据库
        db = DbHelper.Base();
        db.GetCollection<BankTransaction>().Insert(transactions);
        db.Dispose();
        await page.CloseAsync();

        return true;
    }


    #endregion

    /// <summary>
    /// 从托管外包机构同步客户资料
    /// </summary>
    /// <returns></returns>
    public override async Task<bool> SynchronizeCustomerAsync()
    {
        // 判断登陆状态
        if (!IsLogedIn && !await ((IExternPlatform)this).LoginAsync())
            return false;

        using var page = await Automation.AcquirePage(Identifier);

        IResponse? response = null;
        await page.RunAndWaitForResponseAsync(async () => await page.GotoAsync($"https://iservice.citics.com/ds/account/searchcustomer?refresh={DateTime.Now.TimeStampBySeconds()}"), x =>
        {
            if (x.Request.PostData?.Contains("queryCustInfoList") ?? false)
            {
                response = x;
                return true;
            }
            return false;
        });

        if (response is null)
        {
            Log.Error($"{Identifier}.{nameof(SynchronizeCustomerAsync)} 未获取到数据json的response");
            return false;
        }
        var json = await response.TextAsync();

        var obj = JsonSerializer.Deserialize<CITISC.Json.Customer.JsonRootDto>(json);
        if (obj is null)
        {
            Log.Error($"{Identifier}.{nameof(SynchronizeCustomerAsync)} json数据解析失败");
            return false;
        }

        try
        {
            var data = obj.Data.Select(x => x.ToCustomer()).ToArray();

            using var db = DbHelper.Base();

            // 获取已存在的
            var exist_ids = db.GetCollection<Investor>().Query().Select(x => new { id = x.Id, identity = x.Identity }).ToList();//.Where(x => data.Any(y => y.Item1.Identity == x.Identity)).ToArray();
            var accounts = db.GetCollection<BankAccount>().FindAll();
            //
            foreach (var item in data)
            {
                int idx = exist_ids.FindIndex(0, x => x.identity == item.customer.Identity);

                if (idx == -1)
                {
                    db.GetCollection<Investor>().Insert(item.customer);
                    item.account.OwnerId = item.customer.Id;
                    db.GetCollection<BankAccount>().Insert(item.account);
                }
                else
                {
                    item.account.OwnerId = exist_ids[idx].id;
                    if (accounts.Any(x => x.Bank == item.account.Bank && x.Number == item.account.Number))
                        continue;

                    db.GetCollection<BankAccount>().Insert(item.account);
                }
            }


        }
        catch (Exception e)
        {
            Log.Error($"{Identifier}.{nameof(SynchronizeCustomerAsync)} 数据转换失败：{e.Message}");
        }


        //await page.CloseAsync();
        return true;
    }


    /// <summary>
    /// 获取对应json数据
    /// </summary>
    /// <param name="page"></param>
    /// <param name="func"></param>
    /// <param name="funcid"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    private async Task<TD[]?> GetFuncResultAsync<T, TD>(IPage page, string func, int funcid, DateOnly start, DateOnly end, Func<T, int> getdatacount, Func<T, IEnumerable<TD>> getdata, Func<T, bool> hasnext)
    {

        List<TD> data = new();
        Regex text = new Regex(@"100\s*条/页");

        var from = start.ToString("yyyy-MM-dd");
        var to = end.ToString("yyyy-MM-dd");

        // 设置日期
        var loc = page.GetByPlaceholder("开始日期");
        if (await loc.CountAsync() == 0)
        {
            //Log.Warning($"{nameof(CSTISCAssist)}.{func}->未找到日期设置框");
            //return null;
            throw new Exception($"{nameof(CSTISCAssist)}.{func}->未找到日期设置框");
        }

        await loc.First.EvaluateAsync("node => node.setAttribute('type', 'text')");
        await loc.First.EvaluateAsync("element =>{element.removeAttribute('readonly');}");
        await loc.First.FillAsync(from);

        loc = page.GetByPlaceholder("结束日期");
        if (await loc.CountAsync() == 0)
        {
            throw new Exception($"{nameof(CSTISCAssist)}.{func}->未找到日期设置框");
            //Log.Warning($"{nameof(CSTISCAssist)}.{func}->未找到日期设置框");
            //return null;
        }

        await loc.First.EvaluateAsync("node => node.setAttribute('type', 'text')");
        await loc.First.EvaluateAsync("element =>{element.removeAttribute('readonly');}");
        await loc.First.FillAsync(to);

        /// 检查每页条数，设为100条#app > div > div > div.main-container.ivu-row.ivu-row-flex > div > div.newTabStyle.new-container > div:nth-child(2) > div > div > div.main-page-box.main-select-content.ivu-row > div > ul > div > div > div > div.ivu-select-selection > div > span
        loc = page.Locator("div.main-page-box.main-select-content.ivu-row >> div.ivu-select-selection >> .ivu-select-selected-value");//("[id=\"__qiankun_microapp_wrapper_for_iservice__\"]").GetByText("100 条/页");
        loc = await FirstVisible(loc);
        var lcnt = await loc.CountAsync();

        if (lcnt == 1 && text.IsMatch(await loc.InnerTextAsync()))
            loc = page.Locator("form.ivu-form.ivu-form-label-right.ivu-form-inline > div.kr-tableFilter >> span.search-button > button");//.GetByText("查询");//GetByRole(AriaRole.Button, new() { NameRegex = new Regex( "查询") });
        else
        {
            loc = page.Locator("span.ivu-select-selected-value").Filter(new() { HasText = "条/页" });
            loc = await FirstVisible(loc);
            await loc.ClickAsync();
            loc = page.Locator("div.main-page-box.main-select-content.ivu-row >> div.ivu-select-dropdown >> ul.ivu-select-dropdown-list").GetByText(text);
        }

        IResponse? response = null;

        /// 尝试3次
        for (int i = 0; i < 3; i++)
        {
            try
            {
                loc = await FirstVisible(loc);
                await page.RunAndWaitForResponseAsync(async () => await loc.ClickAsync(), resp =>
                {
                    if (resp.Url != $"https://iservice.citics.com/api/sys/midPlatformCommonMethod?funcId={funcid}") return false;
                    response = resp;
                    return true;
                }, new PageRunAndWaitForResponseOptions { Timeout = 5000 });
            }
            catch { }

            if (response is not null) break;
        }


        if (response is null)
        {
            throw new Exception($"{nameof(CSTISCAssist)}.{func}->获取json数据异常，可能托管修改了流程");
            //Log.Warning($"{nameof(CSTISCAssist)}.{func}->获取json数据异常，可能托管修改了流程");
            //return null;
        }
        var json = await response.TextAsync();


        JsonSerializerOptions options = new JsonSerializerOptions();
        options.Converters.Add(new DateTimeConverterUsingDateTimeParse());

        T? obj = JsonSerializer.Deserialize<T>(json, options);

        if (obj is null)
        {
            throw new Exception($"{nameof(CSTISCAssist)}.{func}->json 空对象");
            //Log.Information($"{nameof(CSTISCAssist)}.{func}->json 空对象");
            //return Array.Empty<TD>();
        }


        if (getdatacount(obj) == 0)
        {
            //throw new Exception($"{nameof(CSTISCAssist)}.{func}->未解析到数据");
            Log.Information($"{nameof(CSTISCAssist)}.{func}->未解析到数据");
            return Array.Empty<TD>();
        }

        data.AddRange(getdata(obj));

        //var hasnext = //Regex.IsMatch(json, "\"hasNextPage\":\\s+true");
        /// 下一页的数据
        while (hasnext(obj))
        {
            await Task.Delay(500);

            loc = page.GetByRole(AriaRole.Listitem, new() { Name = "下一页" }).Locator("a");

            await page.RunAndWaitForResponseAsync(async () => await loc.ClickAsync(), resp =>
            {
                if (resp.Url != $"https://iservice.citics.com/api/sys/midPlatformCommonMethod?funcId={funcid}") return false;
                response = resp;
                return true;
            });

            if (response is null)
            {
                throw new Exception($"{nameof(CSTISCAssist)}.{func}->获取json数据异常，可能托管修改了流程");
                //Log.Warning($"{nameof(CSTISCAssist)}.{func}->获取json数据异常，可能托管修改了流程");
                //return null;
            }

            json = await response.TextAsync();

            obj = JsonSerializer.Deserialize<T>(json, options);
            if (obj is null)
            {
                throw new Exception($"{nameof(CSTISCAssist)}.{func}->json 空对象");
                //Log.Information($"{nameof(CSTISCAssist)}.{func}->json 空对象");
                //return Array.Empty<TD>();
            }

            data.AddRange(getdata(obj));
        }


        return data.ToArray();
    }



    public async override Task<bool> SynchronizeTransferRequestAsync()
    {
        // 判断登陆状态
        if (!IsLogedIn && !await ((IExternPlatform)this).LoginAsync())
            return false;

        int fid = 291;
        using var page = await Automation.AcquirePage(Identifier);
        if (page.IsNew) await page.GotoAsync(Domain);

        if (!await LoginValidationAsync(page, 5))
        {
            IsLogedIn = false;
            return false;
        }

        await page.GotoAsync($"https://iservice.citics.com/iservice/zcdj/jyqrcx?refresh={DateTime.Now.TimeStampBySeconds()}");


        try
        {
            var db = DbHelper.Base();
            var last = db.GetCollection<TransferRequest>().Query().OrderByDescending(x => x.RequestDate).FirstOrDefault();
            db.Dispose();

            if (last is null)
                Log.Information($"{nameof(CSTISCAssist)}.{nameof(SynchronizeTransferRequestAsync)}->初始建档");

            string url = $"https://iservice.citics.com/iservice/zcdj/jysqcx?refresh={DateTime.Now.TimeStampBySeconds()}";

            IResponse? response = null;
            await page.RunAndWaitForResponseAsync(async () => await page.GotoAsync(url), resp =>
            {
                if (resp.Url != $"https://iservice.citics.com/api/sys/midPlatformCommonMethod?funcId={fid}") return false;
                response = resp;
                return true;
            });

            if (response is null)
            {
                Log.Warning($"{nameof(CSTISCAssist)}.{nameof(SynchronizeTransferRequestAsync)}->获取json数据异常，可能托管修改了流程");
                return false;
            }


            //
            DateOnly ori = last?.RequestDate ?? new DateOnly(1970, 1, 1), start = DateOnly.FromDateTime(DateTime.Today.AddYears(-5)), end = DateOnly.FromDateTime(DateTime.Today);
            if (ori != end)
            {
                await page.GetByText("历史交易申请查询").First.ClickAsync();
            }


            int empty_year = 0;
            List<TransferRequest> data = new();
            while (end >= ori)
            {
                if (ori > start)
                    start = ori;

                var vals = await GetFuncResultAsync<CITISC.Json.TransferRequest.JsonRootDto, CITISC.Json.TransferRequest.DataItem>(page, nameof(SynchronizeTransferRequestAsync), fid, start, end, x => x.Data.Total, x => x.Data.List, x => x.Data.HasNextPage);
                if (vals is not null)
                {

                    data.AddRange(vals.Select(x => x.ToObject(Identifier)));


                    if (vals.Length == 0) ++empty_year;
                }

                if (empty_year > 0) break;

                end = start.AddDays(-1);
                start = end.AddYears(-5);
            }

            data = data.OrderBy(x => x.CustomerIdentity).ToList();

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

            // 排除同id数据
            var hasids = data.Select(x => x.ExternalId);
            db.GetCollection<TransferRequest>().DeleteMany(x => x.Source == Identifier && hasids.Contains(x.ExternalId));
            db.GetCollection<TransferRequest>().Insert(data);
            db.Dispose();

            return true;
        }
        catch (JsonException e)
        {
            Log.Error($"{Identifier}.{nameof(SynchronizeTransferRequestAsync)} 同步失败：Json 解析异常 {e.Path} {e.InnerException?.Message}");
            return false;
        }
        catch (Exception e)
        {
            Log.Error($"{Identifier}.{nameof(SynchronizeTransferRequestAsync)} 同步失败：{e.Message}");
            return false;
        }



        return true;
    }



    public async override Task<bool> SynchronizeTransferRecordAsync()
    {
        // 判断登陆状态
        if (!IsLogedIn && !await ((IExternPlatform)this).LoginAsync())
            return false;

        int fid = 105;
        using var page = await Automation.AcquirePage(Identifier);
        if (page.IsNew) await page.GotoAsync(Domain);

        if (!await LoginValidationAsync(page, 5))
        {
            IsLogedIn = false;
            return false;
        }

        // 网址https://iservice.citics.com/iservice/zcdj/jyqrcx?refresh=1744077069249
        await page.GotoAsync($"https://iservice.citics.com/iservice/zcdj/jyqrcx?refresh={DateTime.Now.TimeStampBySeconds()}");


        try
        {
            var db = DbHelper.Base();
            var last = db.GetCollection<TransferRecord>().Query().OrderByDescending(x => x.RequestDate).FirstOrDefault();
            db.Dispose();

            if (last is null)
                Log.Information($"{nameof(CSTISCAssist)}.{nameof(SynchronizeTransferRecordAsync)}->初始建档");

            string url = $"https://iservice.citics.com/iservice/zcdj/jyqrcx?refresh={DateTime.Now.TimeStampBySeconds()}";

            IResponse? response = null;
            await page.RunAndWaitForResponseAsync(async () => await page.GotoAsync(url), resp =>
            {
                if (resp.Url != $"https://iservice.citics.com/api/sys/midPlatformCommonMethod?funcId={fid}") return false;
                response = resp;
                return true;
            });

            if (response is null)
            {
                Log.Warning($"{nameof(CSTISCAssist)}.{nameof(SynchronizeTransferRecordAsync)}->获取json数据异常，可能托管修改了流程");
                return false;
            }


            //
            DateOnly ori = last?.RequestDate ?? new DateOnly(1970, 1, 1), start = DateOnly.FromDateTime(DateTime.Today.AddYears(-5)), end = DateOnly.FromDateTime(DateTime.Today);



            int empty_year = 0;
            List<TransferRecord> data = new();
            while (end >= ori)
            {
                if (ori > start)
                    start = ori;

                var vals = await GetFuncResultAsync<CITISC.Json.TransferRecord.Root, CITISC.Json.TransferRecord.List>(page, nameof(SynchronizeTransferRecordAsync), fid, start, end, x => x.data.total, x => x.data.list, x => x.data.hasNextPage);
                if (vals is not null)
                {

                    data.AddRange(vals.Select(x => x.ToObject(Identifier)));


                    if (vals.Length == 0) ++empty_year;
                }

                if (empty_year > 0) break;

                end = start.AddDays(-1);
                start = end.AddYears(-5);
            }

            data = data.OrderBy(x => x.CustomerIdentity).ToList();

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

            var ids2 = db.GetCollection<TransferRequest>().Find(x => x.Source == Identifier).Select(x => (x.Id, x.ExternalId)).ToList();
            foreach (var item in data)
                item.RequestId = ids2.Find(x => x.ExternalId == item.ExternalRequestId).Id;

            // 排除同id数据
            var hasids = data.Select(x => x.ExternalId);
            db.GetCollection<TransferRecord>().DeleteMany(x => x.Source == Identifier && hasids.Contains(x.ExternalId));
            db.GetCollection<TransferRecord>().Insert(data);
            db.Dispose();

            return true;
        }
        catch (JsonException e)
        {
            Log.Error($"{Identifier}.{nameof(SynchronizeTransferRecordAsync)} 同步失败：Json 解析异常 {e.Path} {e.InnerException?.Message}");
            return false;
        }
        catch (Exception e)
        {
            Log.Error($"{Identifier}.{nameof(SynchronizeTransferRecordAsync)} 同步失败：{e.Message}");
            return false;
        }



        return true;
    }
}