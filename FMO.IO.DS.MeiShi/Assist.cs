using FMO.Models;
using FMO.Utilities;
using Microsoft.Playwright;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FMO.IO.DS.MeiShi;

public class Assist : AssistBase
{
    public override string Identifier => "meishi";

    public override string Name => "易私募";

    public override string Domain => "https://vipfunds.simu800.com/";

      
  
    public override async Task<bool> LoginValidationOverrideAsync(IPage page, int wait_seconds = 5)
    {
        for (var i = 0; i < wait_seconds; i++)
        {
            await Task.Delay(1000);

            if (await page.GetByText("易直销").CountAsync() > 0)
                return true;
        }
        return false;
    }

    public override async Task<bool> SynchronizeCustomerAsync()
    {
        // 判断登陆状态
        if (!IsLogedIn && !await LoginAsync())
            return false;

        var page = await GetPageAsync();

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
}
