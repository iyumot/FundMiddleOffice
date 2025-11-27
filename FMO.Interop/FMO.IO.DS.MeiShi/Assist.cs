using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using Microsoft.Playwright;
using Serilog;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FMO.DS.MeiShi;

public class Assist : AssistBase
{
    public override string Identifier => "meishi";

    public override string Name => "易私募";

    public override string Domain => "https://vipfunds.simu800.com/";



    public override Regex HoldingCheck { get; init; } = new Regex(@"vipfunds\.simu800\.com|sso\.simu800\.com");

    private string? token { get; set; }

    public string? ManagerCode { get; private set; }
    public string? TokenId { get; private set; }

    IPlaywright? playwright { get; set; }

    IBrowserContext? BrowserContext { get; set; }






    public override async Task<bool> PrepareLoginAsync(IPage page)
    {
        await base.PrepareLoginAsync(page);

        try
        {
            if (!string.IsNullOrWhiteSpace(UserID))
                await page.Locator("#userName").FillAsync(UserID);
            else return true;

            if (!string.IsNullOrWhiteSpace(Password))
                await page.Locator("#password").FillAsync(Password);
            else return true;

            var box = page.Locator("#account_drag").Locator("..");

            var sliderBoundingBox = await box.BoundingBoxAsync();
            if (sliderBoundingBox != null)
            {
                // 计算滑块最右边的位置
                var rightX = sliderBoundingBox.X + sliderBoundingBox.Width;
                var centerY = sliderBoundingBox.Y + sliderBoundingBox.Height / 2;

                // 移动鼠标到滑块上
                await page.Mouse.MoveAsync(sliderBoundingBox.X + 5, centerY);

                // 按下鼠标左键
                await page.Mouse.DownAsync();

                // 拖动滑块到最右边
                await page.Mouse.MoveAsync(rightX, centerY);

                // 释放鼠标左键
                await page.Mouse.UpAsync();
            }

            var loc = page.Locator("div.antd-pro-pages-login-style-loginContainer >> form > button.ant-btn.ant-btn-primary");
            if (await loc.CountAsync() == 1)
                await loc.ClickAsync();

        }
        catch (Exception e)
        {
            Log.Error($"Login {Domain} Set User Password Error : {e.Message}");
            return false;
        }

        return true;
    }


    public override async Task<bool> LoginValidationOverrideAsync(IPage page)
    {
        return (await page.GetByText("易直销").CountAsync() > 0);
    }

    public override async Task<bool> EndLoginAsync(IPage page)
    {
        var m = Regex.Match(page.Url, @"ascription=([\w%]+)");
        if (m.Success) token = m.Groups[1].Value;

        IsLogedIn = true;

        var cookies = await page.Context.CookiesAsync(["https://vipfunds.simu800.com", "https://sso.simu800.com"]);
        ManagerCode = cookies.FirstOrDefault(x => x.Name == "manager_externalCompanyCode")?.Value;

        return await base.EndLoginAsync(page);
    }

    /// <summary>
    /// 同步客户数据
    /// </summary>
    /// <returns></returns>
    public override async Task<bool> SynchronizeCustomerAsync()
    {
        // 判断登陆状态
        if (!IsLogedIn && !await ((IExternPlatform)this).LoginAsync())
            return false;

        await using var page = await Automation.AcquirePage(Identifier);
        //page.SetDefaultTimeout(5000);
        //var page = pw.Page;
        await page.Keyboard.PressAsync("Escape");
        await page.Keyboard.PressAsync("Escape");

        var locator = page.Locator("div.antd-pro-components-header-index-tabWarp").GetByText("易运营");
        await locator.HoverAsync();
        locator = page.GetByText("投资者综合信息");

        await locator.First.ClickAsync();

        //切换500条
        locator = page.Locator(" div.ant-table-wrapper.antd-pro-pages-components-m-x-table-styles-MXtable.antd-pro-pages-components-m-x-table-styles-newMXTable >> div.ant-select.ant-select-sm.ant-pagination-options-size-changer.ant-select-single.ant-select-show-arrow > div.ant-select-selector");
        await locator.First.ClickAsync();

        locator = page.Locator("div.rc-virtual-list >> div").Filter(new() { HasTextRegex = new Regex("条\\/页$") });
        var cnt = await locator.CountAsync();

        for (int i = 0; i < 99; i++)
        {
            IResponse? response = null;
            await page.RunAndWaitForResponseAsync(async () => await locator.Last.ClickAsync()/* await page.GotoAsync($"https://vipfunds.simu800.com/vipmanager/investorManagement/customerInfo/:timestamp?v={DateTime.Now.TimeStampBySeconds()}")*/, x =>
            {
                if (x.Request.Url?.Contains("customer/query") ?? false)
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

            var result = HandleCustomerJson(json);
            if (!result) return false;

            // 判断是否有下一页
            locator = page.Locator("li.ant-pagination-next");
            locator = await FirstVisible(locator);
            if (locator is null)
                break;
        }

        return true;
    }



    private bool HandleCustomerJson(string json)
    {
        var obj = JsonSerializer.Deserialize<MeiShi.Json.Customer.Root>(json);
        if (obj is null)
        {
            Log.Error($"{Identifier}.{nameof(SynchronizeCustomerAsync)} json数据解析失败");
            return false;
        }

        try
        {
            var data = obj.data.list.Select(x => x.ToCustomer()).ToArray();

            using var db = DbHelper.Base();

            // 获取已存在的
            var exist_ids = db.GetCollection<Investor>().FindAll().ToList();//.Where(x => data.Any(y => y.Item1.Identity == x.Identity)).ToArray();
            var manager = db.GetCollection<Manager>().FindOne(x => x.IsMaster);

            List<Investor> list = new();
            //
            foreach (var item in data)
            {
                int idx = exist_ids.FindIndex(0, x => x.Identity.Id == item.Identity.Id);

                if (idx == -1)
                {
                    if (exist_ids.Find(x => x.Name == item.Name && x.Identity == default) is Investor old)
                    {
                        if (string.IsNullOrWhiteSpace(old.Phone)) old.Phone = item.Phone;
                        if (string.IsNullOrWhiteSpace(old.Email)) old.Email = item.Email;

                        if (old.Identity.Type == IDType.Unknown)
                            old.Identity = item.Identity;

                        old.RiskEvaluation = item.RiskEvaluation;

                        if (old.Type == default) old.Type = item.Type;
                        list.Add(old);
                        // db.GetCollection<Investor>().Update(old);
                    }
                    else if (!item.Name.Contains("test"))
                        list.Add(item); db.GetCollection<Investor>().Insert(item);
                }
                else
                {
                    var old = exist_ids[idx];
                    if (string.IsNullOrWhiteSpace(old.Phone)) old.Phone = item.Phone;
                    if (string.IsNullOrWhiteSpace(old.Email)) old.Email = item.Email;

                    if (old.Identity.Type == IDType.Unknown)
                        old.Identity = item.Identity;

                    old.RiskEvaluation = item.RiskEvaluation;

                    if (old.Type == default) old.Type = item.Type;
                    if (item.Name == manager?.Name) old.Type = AmacInvestorType.Manager;

                    list.Add(old);
                    // db.GetCollection<Investor>().Update(old);
                }
            }

            db.GetCollection<Investor>().Upsert(list);
            return true;
        }
        catch (Exception e)
        {
            Log.Error($"{Identifier}.{nameof(SynchronizeCustomerAsync)} 数据转换失败：{e.Message}");
            return false;
        }
    }



    /// <summary>
    /// 同步合投数据
    /// </summary>
    /// <returns></returns>
    public override async Task<bool> SynchronizeQualificatoinAsync()
    {
        string log = $"{Identifier} 同步合投资料";
        // 判断登陆状态
        if (!IsLogedIn && !await ((IExternPlatform)this).LoginAsync())
        {
            Log.Error(log + "失败：未登陆");
            return false;
        }


        await using var page = await Automation.AcquirePage(Identifier);
        await page.GotoAsync($"https://vipfunds.simu800.com/vipmanager/investorAppropriatenessManagement/investorsProcessList?ascription={token}&v={(int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMicroseconds}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        //var page = pw.Page; 
        await page.Keyboard.PressAsync("Escape");
        await page.Keyboard.PressAsync("Escape");

        var locator = page.Locator("div.antd-pro-components-header-index-tabWarp").GetByText("易运营");
        try
        {
            await locator.HoverAsync();
            locator = page.GetByText("合格投资者认定");

            await locator.First.ClickAsync();
        }
        catch (Exception e)
        {
            Log.Error(log + $"打开页面失败{e.Message}");
            WeakReferenceMessenger.Default.Send("同步合投资料失败，请查看log", "toast");
            return false;
        }

        // 已完成页
        try
        {
            locator = page.GetByText("已完成");
            await locator.First.ClickAsync();
        }
        catch (Exception e)
        {
            Log.Error(log + $"打开已完成页面失败{e.Message}");
            WeakReferenceMessenger.Default.Send("同步合投资料失败，请查看log", "toast");
            return false;
        }

        //切换500条#ant-page-tabs-vipMeix-panel-\/investorAppropriatenessManagement\/investorsProcessList > div > div.ant-card-body > div.ant-table-wrapper.antd-pro-pages-components-m-x-table-styles-MXtable > div > div > ul > li.ant-pagination-options > div.ant-select.ant-pagination-options-size-changer.ant-select-single.ant-select-show-arrow
        try
        {
            locator = page.Locator("div.ant-select.ant-pagination-options-size-changer.ant-select-single.ant-select-show-arrow");
            await locator.First.ClickAsync();

            locator = page.Locator("div.rc-virtual-list >> div").Filter(new() { HasTextRegex = new Regex("条\\/页$") });
            await locator.Last.ClickAsync();
        }
        catch (Exception e)
        {
            Log.Error(log + $"切换500条/页失败{e.Message}");
            WeakReferenceMessenger.Default.Send("同步合投资料失败，请查看log", "toast");
            return false;
        }

        try
        {
            using var db = DbHelper.Base();
            var data = db.GetCollection<InvestorQualification>();

            for (int i = 0; i < 999; i++)
            {
                var table = page.Locator("div.ant-table-container");
                var rows = await table.Locator("tr").AllAsync();

                // 获取列名
                var th = (await page.Locator("thead.ant-table-thead >> th").AllInnerTextsAsync()).ToList();
                int idn = th.IndexOf("客户名称");
                int idd = th.IndexOf("证件号码");
                int idt = th.IndexOf("认定时间");
                int idty = th.IndexOf("客户类型");

                if (idn < 0 || idd < 0 || idt < 0)
                {
                    Log.Error($"{Identifier}.{nameof(SynchronizeQualificatoinAsync)} 配置错误，无法识别对应列");
                    WeakReferenceMessenger.Default.Send("同步合投资料失败，请查看log", "toast");
                }

                foreach (var r in rows)
                {
                    var cells = await r.Locator("td").AllAsync();
                    if (cells.Count < idt) continue;

                    var name = await cells[idn].InnerTextAsync();
                    var id = await cells[idd].InnerTextAsync();
                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(id)) continue;

                    var finds = db.GetCollection<Investor>().Find(x => x.Identity.Id == id);
                    var investor = finds.Count() == 1 ? finds.First() : finds.FirstOrDefault(x => x.Name == name);

                    string str = await cells[idt].InnerTextAsync();
                    DateOnly.TryParse(str, out DateOnly date);

                    var cusType = idty != -1 ? await cells[idty].InnerTextAsync() : "";

                    try
                    {
                        var q = data.FindOne(x => x.InvestorName == name && x.IdentityCode == id && x.Date == date);

                        if (q?.IsSealed ?? false)
                            continue;

                        if (q is null)
                        {
                            q = new InvestorQualification { InvestorName = name, IdentityCode = id, Date = date, };
                            data.Insert(q);
                        }

                        // 判断下载文件是否存在
                        FileInfo file = new(@$"files\qualification\{q.Id}\pack.zip");
                        if (!file.Exists)
                        {
                            locator = cells[^1].Locator("a").Filter(new LocatorFilterOptions { HasText = "查看" });

                            int n = await locator.CountAsync();
                            var sss = await cells[^3].InnerTextAsync();
                            await locator.ScrollIntoViewIfNeededAsync();

                            await locator.ClickAsync();

                            var frame = page.Locator(" div.ant-card.ant-card-bordered > div.ant-card-body").First;

                            n = await locator.CountAsync();
                            sss = await locator.InnerTextAsync();
                            n = await frame.CountAsync();
                            sss = await frame.InnerTextAsync();
                            var loc = frame.GetByText("适当性类型").Locator(".. >> span").Last;

                            var type = await loc.InnerTextAsync();


                            sss = await loc.InnerTextAsync();

                            var s = await loc.InnerTextAsync();

                            locator = frame.Locator("a").Filter(new LocatorFilterOptions { HasText = "下载" });

                            // 监听下载事件
                            var downloadTask = page.WaitForDownloadAsync();

                            await locator.ClickAsync();
                            var download = await downloadTask;

                            // 保存下载的文件
                            await download.SaveAsAsync(file.FullName);

                            await page.Keyboard.PressAsync("Escape");
                        }

                        using ZipArchive zip = ParseQualificationZip(q, file);

                        //prof = qfiles.Where(x => x.Name.Contains("身份证") && !x.Name.Contains("投资经历"));
                        //if(prof.Any())
                        //{
                        //    // 个人
                        //    if (investor?.EntityType == EntityType.Natural)
                        //    {
                        //        if (investor.Certifications is null)
                        //            investor.Certifications = new() { Name = "证件" };

                        //        investor.Certifications.Files.Add(new FileVersion { Time = fi.LastWriteTime, Path = fi.Name, Hash = FileHelper.ComputeHash(fi)! });
                        //    }
                        //}


                        q.IsSealed = true;
                        q.Source = Identifier;
                        data.Update(q);

                    }
                    catch (Exception e)
                    {
                        Log.Error($"{Identifier}.{nameof(SynchronizeQualificatoinAsync)} 解析 {name} 文件出错 {e.Message}");
                        WeakReferenceMessenger.Default.Send($"同步{name}合投资料失败，请查看log", "toast");
                    }


                    await Task.Delay(300);
                }


                // 判断是否有下一页
                locator = page.Locator("li.ant-pagination-next");
                if (await locator.CountAsync() == 0)
                    break;
            }

            await Task.Delay(300);
        }
        catch (Exception e)
        {
            Log.Error(log + $"解析失败{e.Message}");
            WeakReferenceMessenger.Default.Send("同步合投资料失败，请查看log", "toast");
            return false;
        }

        return true;

    }

    private async Task<bool> HandleQualificationJson(string json)
    {
        var obj = JsonSerializer.Deserialize<MeiShi.Json.QualificationJson.Root>(json);
        if (obj is null)
        {
            Log.Error($"{Identifier}.{nameof(SynchronizeQualificatoinAsync)} json数据解析失败");
            return false;
        }

        var db = DbHelper.Base();
        var co = db.GetCollection<Investor>();
        List<(Json.QualificationJson.ListItem j, InvestorQualification q)> list = new();
        foreach (var item in obj.data.list)
        {
            var id = item.identifyFlowId;

            // 找对应投资人
            var investor = co.FindOne(x => x.Name == item.InvestorName && x.Identity.Id == item.cardNumber);
            if (investor is null)
            {
                Log.Error($"合投资料没有找到对应的投资人数据，{item.InvestorName}-{item.cardNumber} {item.identifyTime}");
                continue;
            }

            InvestorQualification qualification = new InvestorQualification();
            qualification.InvestorId = investor.Id;
            qualification.Date = DateOnly.FromDateTime(DateTime.Parse(item.identifyTime));

            db.GetCollection<InvestorQualification>().Insert(qualification);

            list.Add((item, qualification));
        }
        db.Dispose();

        using var db2 = PlatformDatabase.Instance();
        var history = db2.GetCollection<QualificationSyncHistory>(Identifier).FindAll();

        await using var page = await Automation.AcquirePage(Identifier);

        foreach (var item in list)
        {
            var id = item.j.identifyFlowId;

            if (history.Any(x => x.Id == id))
                continue;

            // 监听下载事件
            var downloadTask = page.WaitForDownloadAsync();

            // 定义 POST 请求的 JSON 数据
            string jsonData = "{\"identifyFlowIds\":[xxxx],\"downloadType\":1,\"codeTypeList\":[109,110,108,120,203,111,201,113,112,114,152,118,116,117,119,608,130]}".Replace("xxxx", id.ToString());

            // 发送 POST 请求
            await page.EvaluateAsync(@"(jsonData) => {
            const xhr = new XMLHttpRequest();
            xhr.open('POST', 'https://vipfunds.simu800.com/vip-manager/identify/flow/downloadIdentifyFlowId', true);
            xhr.setRequestHeader('Content-Type', 'application/json');
            xhr.responseType = 'blob';
            xhr.onload = function() {
                if (xhr.status === 200) {
                    const url = window.URL.createObjectURL(xhr.response);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = 'downloaded_file';
                    a.click();
                    window.URL.revokeObjectURL(url);
                }
            };
            xhr.send(jsonData);
            }", jsonData);

            var download = await downloadTask;

            // 保存下载的文件
            await download.SaveAsAsync(new FileInfo(@$"files\qualification\{item.q.Id}\pack.zip").FullName);


            db2.GetCollection<QualificationSyncHistory>(Identifier).Insert(new QualificationSyncHistory { Id = id, Time = DateTime.Now });
        }

        return true;
    }


    public override async Task<bool> SynchronizeOrderAsync()
    {
        string log = $"{Identifier} 同步订单";
        // 判断登陆状态
        if (!IsLogedIn && !await ((IExternPlatform)this).LoginAsync())
        {
            Log.Error(log + "失败：未登陆");
            return false;
        }


        string url = $"https://vipfunds.simu800.com/vipmanager/investorManagement/signFlowManagement?ascription={token}&v={DateTime.Now.TimeStampByMilliseconds()}";
        await using var page = await Automation.AcquirePage(Identifier);
        await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        //var page = pw.Page; 
        await page.Keyboard.PressAsync("Escape");
        await page.Keyboard.PressAsync("Escape");

        var locator = page.Locator("div.ant-select.ant-pagination-options-size-changer.ant-select-single.ant-select-show-arrow");
        IResponse? response = null;
        try
        {
            await page.RunAndWaitForResponseAsync(async () => await locator.First.ClickAsync(), x =>
            {
                if (x.Request.Url?.Contains("querySignFlowAll") ?? false)
                {
                    response = x;
                    return true;
                }
                return false;
            });

            locator = page.Locator("div.rc-virtual-list >> div").Filter(new() { HasTextRegex = new Regex("条\\/页$") });

            await page.WaitForTimeoutAsync(200);
            foreach (var item in (await locator.AllAsync()).Reverse())
            {
                if (await item.IsVisibleAsync())
                {
                    await item.ClickAsync();
                    break;
                }
            }

        }
        catch (Exception e)
        {
            Log.Error(log + $"切换500条/页失败{e.Message}");
            WeakReferenceMessenger.Default.Send("同步合投资料失败，请查看log", "toast");
            //return false;
        }

        if (response is null) return false;
        var json = await response.TextAsync();

        var data = JsonSerializer.Deserialize<Json.Order.ApiResponse>(json);

        if (data?.data?.list is null) return false;

        // 判断是否有下一页




        // 解析
        using var pdb = DbHelper.Platform();
        var cache = pdb.GetCollection<MeiShiOrderCache>().FindAll().ToArray();
        using var db = DbHelper.Base();
        var funds = db.GetCollection<Fund>().FindAll().ToArray();
        var customers = db.GetCollection<Investor>().FindAll().ToArray();


        foreach (var item in data.data.list.ExceptBy(cache.Select(x => x.Id), x => x.signFlowId))
        {
            var id = item.signFlowId;
            var fundname = item.productName;
            var cname = item.InvestorName;
            var cid = item.cardNumber;
            //var type = item.signType;
            //var openday = item.openDay;
            var number = item.tradeMoney ?? item.redemptionMoney;

            if (fundname is null) continue;
            if (item.signType != 1) continue;

            // 查找对应的基金
            var (fund, sc) = funds.FindByName(fundname);
            if (fund is null)
            {
                Log.Error($"{Identifier}.{nameof(SynchronizeOrderAsync)} 未找到基金 {fundname}");
                continue;
            }

            var cus = customers.FirstOrDefault(x => x.Name == cname && x.Identity?.Id == cid);
            if (cus is null)
            {
                Log.Error($"{Identifier}.{nameof(SynchronizeOrderAsync)} 未找到客户 {cname} {cid}");
                continue;
            }


            var od = await GetOrderInfo(id, item.signType, page);
            if (od is null)
            {
                Log.Error($"{Identifier}.{nameof(SynchronizeOrderAsync)} 获取订单详情失败 {id}");
                continue;
            }

            var stime = DateTime.TryParse(od?.sealedDocuments?.FirstOrDefault()?.sealTime, out var dt) ? DateOnly.FromDateTime(dt) : default;

            TransferOrderType type = TransferOrderType.Buy;
            if (item.signType == 3 && item?.redemptionType < 3)
                type = od?.redemptionType switch
                {
                    0 => TransferOrderType.Share,
                    1 => TransferOrderType.Amount,
                    2 => TransferOrderType.RemainAmout,
                };

            var sid = $"{id}";
            TransferOrder order = new TransferOrder
            {
                FundId = fund.Id,
                FundName = fund.Name,
                ShareClass = sc,
                InvestorName = cname,
                InvestorId = cus.Id,
                InvestorIdentity = cid,
                Date = stime,
                Type = type,
                Number = number ?? 0,
                Source = Identifier,
                ExternalId = $"{id}",
            };

            // 检查是否已存在
            var exist = db.GetCollection<TransferOrder>().FindOne(x => x.ExternalId == sid);
            if (exist is null) exist = db.GetCollection<TransferOrder>().FindOne(x => x.FundId == fund.Id && x.InvestorId == cus.Id && x.Date == stime && x.Type == type && x.Number == number);
            db.BeginTrans();
            try
            {

                if (exist is not null)
                    order.Id = exist.Id;
                else db.GetCollection<TransferOrder>().Insert(order);

                // 下载文件
                var di = new DirectoryInfo($"files\\order\\{order.Id}");
                di.Create();
                foreach (var ff in od.sealedDocuments!)
                {
                    var fdown = await page.APIRequest.GetAsync(ff.sealedUrl!);

                    if (response.Status != 200)
                        continue;


                    DateTime time = DateTime.Parse(ff.sealTime!);

                    if (ff.documentName.Length > 10 && ff.codeType == 123)
                        ff.documentName = "基金合同";

                    var filepath = Path.Combine(di.FullName, $"{order.FundName}-{order.InvestorName}-{time.ToLocalTime():yyyyMMdd}-{ff.documentName}.pdf");

                    // 保存下载的文件   
                    var buffer = await fdown.BodyAsync();
                    using (var fileStream = File.Create(filepath))
                    {
                        fileStream.Write(buffer, 0, buffer.Length);
                        fileStream.Flush();
                    }

                    if (ff.codeType is null)
                    {
                        if (ff.documentName.Contains("申请"))
                            ff.codeType = 125;
                    }

                    switch (ff.codeType)
                    {
                        case 121:
                            order.RiskPair = new SimpleFile { Label = ff.documentName, File = FileMeta.Create(filepath) with { Time = time } };
                            break;

                        case 122:
                            order.RiskDiscloure = new SimpleFile { Label = ff.documentName, File = FileMeta.Create(filepath) with { Time = time } };
                            break;
                        case 123:
                            order.Contract = new SimpleFile { Label = ff.documentName, File = FileMeta.Create(filepath) with { Time = time } };
                            break;
                        case 125:
                            order.OrderSheet = new SimpleFile { Label = ff.documentName, File = FileMeta.Create(filepath) with { Time = time } };
                            break;

                        default:
                            break;
                    }

                }

                // 双录
                url = od.doubleRecordingUrl!;
                if (url is not null)
                {
                    var fdown = await page.APIRequest.GetAsync(url!);

                    if (response.Status == 200)
                    {
                        // 保存下载的文件   
                        var buffer = await fdown.BodyAsync();
                        var filepath = Path.Combine(di.FullName, $"{fundname}-{cname}-双录.mp4");
                        using (var fileStream = File.Create(filepath))
                        {
                            fileStream.Write(buffer, 0, buffer.Length);
                            fileStream.Flush();
                        }

                        order.Videotape = new SimpleFile { Label = "双录", File = FileMeta.Create(filepath) };
                        if (order.OrderSheet?.File?.Time is DateTime t)
                            order.Videotape.File = order.Videotape.File with { Time = t }; // 双录时间与订单时间一致
                    }
                }

                db.GetCollection<TransferOrder>().Update(order);

                // 找ta
                DateTimeHelper.TryParse(item.openDay, out var openday);
                var sameday = db.GetCollection<TransferRecord>().Find(x => x.FundId == order.FundId && x.InvestorId == order.InvestorId && openday == x.RequestDate).ToArray();
                foreach (var ta in sameday)
                {
                    if (ta.OrderId == 0) ta.OrderId = order.Id;
                }
                db.GetCollection<TransferRecord>().Update(sameday);

                pdb.GetCollection<MeiShiOrderCache>().Upsert(new MeiShiOrderCache { Id = id, OrderId = order.Id });
                db.Commit();
            }
            catch (Exception)
            {
                db.Rollback();
            }
        }



        return true;
    }

    //public async Task<string> GetOrderDetail(long id, IPage page)
    //{
    //    var url = $"https://vipfunds.simu800.com/vip-manager/manager/signFlowController/downloadSignFlowId?&signFlowIdList={id}&companyCode={ManagerCode}&downloadType=2&codeTypeList=123,124,609,122,125,127,121,129,126,610";
    //    await page.GotoAsync(url);
    //    var json = await page.ContentAsync();

    //    var root = JsonSerializer.Deserialize<Json.OrderDetail.ApiResponse>(json);
    //    if (root is null) return null;

    //}


    public async Task<Json.OrderDetail.ResponseData?> GetOrderInfo(long id, int? signType, IPage page)
    {
        try
        {

            var type = signType switch { 0 => "productProcessDetails", 1 => "applyPurchase", _ => "Redeming" };
            IResponse? response = null;
            var url = $"https://vipfunds.simu800.com/vipmanager/investorManagement/signFlowManagement/{type}/{id}?ascription={token}&isDelete=0&key=null&v={DateTime.Now.TimeStampByMilliseconds()}";
            await page.RunAndWaitForResponseAsync(async () => await page.GotoAsync(url), req =>
            {
                if (req.Url.Contains("codeValue=4040"))
                {
                    response = req;
                    return true;
                }
                return false;
            });

            if (response is null) return null;

            //await page.GotoAsync($"https://vipfunds.simu800.com/vip-manager/manager/flowProcess/queryProcessByFlowId?flowId={id}&t={DateTime.Now.TimeStampByMilliseconds()}");
            //var url = $"https://vipfunds.simu800.com/vip-manager/manager/signFlowController/querySignFlowInfo?flowId={id}&codeValue=2070&t={DateTime.Now.TimeStampByMilliseconds()}";
            //await page.GotoAsync(url);

            var json = await response.TextAsync();

            var root = JsonSerializer.Deserialize<Json.OrderDetail.ApiResponse>(json);
            if (root is null) return null;

            return root.data;
        }
        catch (Exception e)
        {
            return null;
        }
    }





    public async Task<bool> DownloadSignFile(long id, IPage page)
    {
        var url = $"https://vipfunds.simu800.com/vip-manager/manager/signFlowController/downloadSignFlowId?&signFlowIdList={id}&companyCode={ManagerCode}&downloadType=1&codeTypeList=123,124,609,122,125,127,121,129,126,610";
        var downloadTask = page.WaitForDownloadAsync();
        await page.GotoAsync(url);

        var download = await downloadTask;

        if (await download.FailureAsync() is string err)
        {
            Log.Error($"下载签署文件失败：{err}");
            return false;
        }

        await download.SaveAsAsync(@$"cache\meishi\order\{id}.zip");

        return true;
    }


    static ZipArchive ParseQualificationZip(InvestorQualification q, FileInfo file)
    {
        // 解压文件
        var zip = ZipFile.OpenRead(file.FullName);
        foreach (var item in zip.Entries)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || item.Name.EndsWith("/")) continue;

            using var fs = new FileStream(@$"files\qualification\{q.Id}\{item.Name}", FileMode.Create);
            using var stream = item.Open();
            stream.CopyTo(fs);
            fs.Flush();
        }

        var qfiles = file.Directory!.GetFiles().Where(x => x.Extension.ToLower() != ".zip").ToList();

        var fi = qfiles.FirstOrDefault(x => x.Name.Contains("合格投资者承诺函"));
        if (fi is not null)
            q.CommitmentLetter = new SimpleFile { Label = fi.Name, File = FileMeta.Create(fi.FullName) };


        fi = qfiles.FirstOrDefault(x => x.Name.Contains("基本信息表"));
        if (fi is not null)
        {
            if (fi.Name.Contains("_专业投资者_"))
            {
                q.Result = QualifiedInvestorType.Professional;
            }
            q.InfomationSheet = new SimpleFile { Label = fi.Name, File = FileMeta.Create(fi.FullName) };
            qfiles.Remove(fi);
        }

        fi = qfiles.FirstOrDefault(x => x.Name.Contains("告知书"));
        if (fi is not null)
        {
            q.Notice = new SimpleFile { Label = fi.Name, File = FileMeta.Create(fi.FullName) };
            qfiles.Remove(fi);
        }

        fi = qfiles.FirstOrDefault(x => x.Name.Contains("税收居民身份声明"));
        if (fi is not null)
        {
            q.TaxDeclaration = new SimpleFile { Label = fi.Name, File = FileMeta.Create(fi.FullName) };
            qfiles.Remove(fi);
        }

        fi = qfiles.FirstOrDefault(x => x.Name.Contains("经办人身份证件") || x.Name.Contains("法人"));
        if (fi is not null)
        {
            q.Agent = new SimpleFile { Label = fi.Name, File = FileMeta.Create(fi.FullName) };
            qfiles.Remove(fi);
        }




        fi = qfiles.FirstOrDefault(x => x.Name.Contains("授权委托书"));
        if (fi is not null)
        {
            q.Authorization = new SimpleFile { Label = fi.Name, File = FileMeta.Create(fi.FullName) };
            qfiles.Remove(fi);
        }

        fi = qfiles.FirstOrDefault(x => x.Name.Contains("投资经历"));
        if (fi is not null)
        {
            q.ProofOfExperience = new SimpleFile { Label = fi.Name, File = FileMeta.Create(fi.FullName) };
            qfiles.Remove(fi);
        }

        var prof = qfiles.Where(x => x.Name.Contains("证明材料") && !x.Name.Contains("投资经历")).ToArray();
        if (prof.Any())
        {
            if (q.CertificationFiles is null) q.CertificationFiles = new();

            foreach (var item in prof)
            {
                q.CertificationFiles.Files.Add(FileMeta.Create(item.Name));
                qfiles.Remove(item);
            }
        }

        // 记录未处理的文件
        if (qfiles.Any())
        {
            Log.Warning($"{q.Id} {q.InvestorName}的合投文件存在未处理项目：{string.Join(',', qfiles.Select(x => x.Name))}");
        }


        return zip;
    }
}

public class QualificationSyncHistory
{
    public int Id { get; set; }


    public DateTime Time { get; set; }

}

public class MeiShiOrderCache
{
    /// <summary>
    /// signFlowId 
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 生成的order
    /// </summary>
    public int OrderId { get; set; }





}