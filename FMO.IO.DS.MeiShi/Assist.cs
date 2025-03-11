using FMO.Models;
using FMO.Utilities;
using Microsoft.Playwright;
using Serilog;
using System.Diagnostics;
using System.Text.Json;

namespace FMO.IO.DS.MeiShi;

public class Assist : AssistBase
{
    public override string Identifier => "meishi";

    public override string Name => "易私募";

    public override string Domain => "https://vipfunds.simu800.com/";



    public override async Task<bool> LoginValidationOverrideAsync(IPage page)
    {
        return (await page.GetByText("易直销").CountAsync() > 0);
    }

    public override async Task<bool> SynchronizeCustomerAsync()
    {
        // 判断登陆状态
        if (!IsLogedIn && !await LoginAsync())
            return false;

        var page = await GetPageAsync();

        var locator = page.Locator("div.antd-pro-components-header-index-tabWarp").GetByText("易运营");
        await locator.HoverAsync();
        locator = page.GetByText("投资者综合信息");

        IResponse? response = null;
        await page.RunAndWaitForResponseAsync(async () => await locator.First.ClickAsync()/* await page.GotoAsync($"https://vipfunds.simu800.com/vipmanager/investorManagement/customerInfo/?v={DateTime.Now.TimeStampBySeconds()}")*/, x =>
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
                    db.GetCollection<Investor>().Insert(item);
                }
                else
                {
                    var old = exist_ids[idx];
                    if (string.IsNullOrWhiteSpace(old.Phone)) old.Phone = item.Phone;
                    if (string.IsNullOrWhiteSpace(old.Email)) old.Email = item.Email;

                    if (old.Identity.Type == IDType.Unknown)
                        old.Identity = item.Identity;

                    if (old.RiskLevel == default) old.RiskLevel = item.RiskLevel;
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
