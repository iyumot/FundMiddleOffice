using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using Microsoft.Playwright;
using Serilog;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FMO.IO.DS.MeiShi;

public class Assist : AssistBase
{
    public override string Identifier => "meishi";

    public override string Name => "易私募";

    public override string Domain => "https://vipfunds.simu800.com/";



    public override Regex HoldingCheck { get; init; } = new Regex(@"vipfunds\.simu800\.com|sso\.simu800\.com");

    private string? token { get; set; }

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

    public override Task<bool> EndLoginAsync(IPage page)
    {
        var m = Regex.Match(page.Url, @"ascription=([\w%]+)");
        if (m.Success) token = m.Groups[1].Value;
        return base.EndLoginAsync(page);
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

        using var page = await Automation.AcquirePage(Identifier);
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

        for (int i = 0; i < 999; i++)
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
            if (await locator.CountAsync() == 0)
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

                        old.RiskLevel = item.RiskLevel;

                        if (old.Type == default) old.Type = item.Type;
                        db.GetCollection<Investor>().Update(old);
                    }
                    else if (!item.Name.Contains("test"))
                        db.GetCollection<Investor>().Insert(item);
                }
                else
                {
                    var old = exist_ids[idx];
                    if (string.IsNullOrWhiteSpace(old.Phone)) old.Phone = item.Phone;
                    if (string.IsNullOrWhiteSpace(old.Email)) old.Email = item.Email;

                    if (old.Identity.Type == IDType.Unknown)
                        old.Identity = item.Identity;

                    old.RiskLevel = item.RiskLevel;

                    if (old.Type == default) old.Type = item.Type;
                    db.GetCollection<Investor>().Update(old);
                }
            }
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


        using var page = await Automation.AcquirePage(Identifier);
        await page.GotoAsync($"https://vipfunds.simu800.com/vipmanager/investorAppropriatenessManagement/investorsProcessList?ascription={token}&v={(int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMicroseconds}");
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
            var cnt = await locator.CountAsync();
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
                        FileInfo file = new(@$"files\qualification\{q.Id}\pack.zip");
                        await download.SaveAsAsync(file.FullName);


                        await page.Keyboard.PressAsync("Escape");


                        // 解压文件
                        using var zip = ZipFile.OpenRead(file.FullName);
                        foreach (var item in zip.Entries)
                        {
                            using var fs = new FileStream(@$"files\qualification\{q.Id}\{item.Name}", FileMode.Create);
                            using var stream = item.Open();
                            stream.CopyTo(fs);
                            fs.Flush();


                            if (item.Name.Contains("合格投资者承诺函"))
                            {
                                q.CommitmentLetter = new FileStorageInfo { Name = item.Name, Time = item.LastWriteTime.LocalDateTime, Path = fs.Name, Hash = FileHelper.ComputeHash(fs) };
                            }
                            else if (item.Name.Contains("基本信息表"))
                            {
                                q.InfomationSheet = new FileStorageInfo { Name = item.Name, Time = item.LastWriteTime.LocalDateTime, Path = fs.Name, Hash = FileHelper.ComputeHash(fs) };
                            }
                            else if (item.Name.Contains("告知书"))
                            {
                                q.Notice = new FileStorageInfo { Name = item.Name, Time = item.LastWriteTime.LocalDateTime, Path = fs.Name, Hash = FileHelper.ComputeHash(fs) };
                            }
                            else if (item.Name.Contains("税收居民身份声明"))
                            {

                                q.TaxDeclaration = new FileStorageInfo { Name = item.Name, Time = item.LastWriteTime.LocalDateTime, Path = fs.Name, Hash = FileHelper.ComputeHash(fs) };
                            }
                            else if (item.Name.Contains("经办人身份证件"))
                            {
                                q.Agent = new FileStorageInfo { Name = item.Name, Time = item.LastWriteTime.LocalDateTime, Path = fs.Name, Hash = FileHelper.ComputeHash(fs) };
                            }
                            else if (item.Name.Contains("授权委托书"))
                            {
                                q.Authorization = new FileStorageInfo { Name = item.Name, Time = item.LastWriteTime.LocalDateTime, Path = fs.Name, Hash = FileHelper.ComputeHash(fs) };
                            }
                        }


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
            var investor = co.FindOne(x => x.Name == item.customerName && x.Identity.Id == item.cardNumber);
            if (investor is null)
            {
                Log.Error($"合投资料没有找到对应的投资人数据，{item.customerName}-{item.cardNumber} {item.identifyTime}");
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

        using var page = await Automation.AcquirePage(Identifier);

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
}

public class QualificationSyncHistory
{
    public int Id { get; set; }


    public DateTime Time { get; set; }

}