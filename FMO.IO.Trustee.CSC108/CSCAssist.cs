using System.Text.RegularExpressions;
using FMO.Models;
using Microsoft.Playwright;
using Serilog;

namespace FMO.IO.Trustee;

public class CSCAssist : TrusteeAssistBase, IExternPlatform
{
    public override string Identifier => "csc108";

    public override string Name => "中信建投证券股份有限公司";

    public override string Domain => "https://tgfw.csc108.com/";

    public override Regex HoldingCheck { get; init; } = new Regex("csc108.com");

    public override Task<(string Code, ManageFeeDetail[] Fee)[]> GetManageFeeDetails(DateOnly start, DateOnly end)
    {
        throw new NotImplementedException();
    }

    public override async Task<bool> LoginValidationOverrideAsync(IPage page)
    {
        return await page.GetByText("首页").CountAsync() > 0;
    }

    public override Task<bool> SynchronizeCustomerAsync()
    {
        throw new NotImplementedException();
    }

    public override Task<bool> SynchronizeDistributionAsync()
    {
        throw new NotImplementedException();
    }

    public override Task<bool> SynchronizeFundRaisingRecord()
    {
        throw new NotImplementedException();
    }

    public override Task<bool> SynchronizeTransferRecordAsync()
    {
        throw new NotImplementedException();
    }

    public override async Task<bool> SynchronizeTransferRequestAsync()
    {
        // 判断登陆状态
        if (!IsLogedIn && !await ((IExternPlatform)this).LoginAsync())
            return false;

        var func_name = nameof(SynchronizeCustomerAsync);

        // 获取页面
        await using var page = await Automation.AcquirePage(Identifier);
        if (page.IsNew) await page.GotoAsync(Domain);

        // 获取
        var locator = page.Locator("#innerSession").GetByText("份额登记");
        locator = await locator.SingleVisible();
        if(locator is null)
        {
            Log.Error($"{func_name} <份额登记> 不可见");
            return false;
        }
        await locator.ClickAsync();

        // 验证是否弹出






        return true;
    }
}
