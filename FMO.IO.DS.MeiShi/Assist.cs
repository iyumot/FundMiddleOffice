using FMO.Models;
using FMO.Utilities;
using Microsoft.Playwright;
using Serilog;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FMO.IO.DS.MeiShi;

public class Assist : AssistBase
{
    public override string Identifier => "meishi";

    public override string Name => "易私募";

    public override string Domain => "https://vipfunds.simu800.com/";



    public override Regex HoldingCheck { get; } = new Regex(@"vipfunds\.simu800\.com|sso\.simu800\.com");


    public override async Task<bool> PrepareLoginAsync(IPage page)
    {
        await base.PrepareLoginAsync(page);

        try
        {
            UserID = "sj@seesunfund.com"; Password = "Ss123789";
            if (!string.IsNullOrWhiteSpace(UserID))
                await page.Locator("#userName").FillAsync(UserID);

            if (!string.IsNullOrWhiteSpace(Password))
                await page.Locator("#password").FillAsync(Password);

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
        }

        return true;
    }


    public override async Task<bool> LoginValidationOverrideAsync(IPage page)
    {
        return (await page.GetByText("易直销").CountAsync() > 0);
    }

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

        IResponse? response = null;
        await page.RunAndWaitForResponseAsync(async () => await locator.First.ClickAsync()/* await page.GotoAsync($"https://vipfunds.simu800.com/vipmanager/investorManagement/customerInfo/:timestamp?v={DateTime.Now.TimeStampBySeconds()}")*/, x =>
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

        var obj = JsonSerializer.Deserialize<MeiShi.Json.Customer.Root>(json);
        if (obj is null)
        {
            Log.Error($"{Identifier}.{nameof(SynchronizeCustomerAsync)} json数据解析失败");
            return false;
        }

        try
        {
            var data = obj.data.list.Select(x => x.ToCustomer()).ToArray();

            using var db = new BaseDatabase();

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


        }
        catch (Exception e)
        {
            Log.Error($"{Identifier}.{nameof(SynchronizeCustomerAsync)} 数据转换失败：{e.Message}");
        }


        //await page.CloseAsync();
        return true;
    }
}
