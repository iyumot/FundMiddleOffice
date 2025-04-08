using CommunityToolkit.Mvvm.Messaging;
using FMO.Utilities;
using Microsoft.Playwright;
using System.Text.RegularExpressions;




namespace FMO.IO.Trustee;






/// <summary>
/// 提供部分实现的基类
/// </summary>
public abstract class TrusteeAssistBase : ITrusteeAssist
{
    //private IPage? _mainPage;


    public abstract string Identifier { get; }

    public abstract string Name { get; }


    /// <summary>
    /// 主页
    /// </summary>
    public abstract string Domain { get; }



    public bool IsLogedIn { get; protected set; }


    public TrusteeSynchronizeTime TrusteeSynchronizeTime { get; init; }

    public string? UserID { get; set; }

    public string? Password { get; set; }

    public abstract Regex HoldingCheck { get; init; }

    protected TrusteeAssistBase()
    {
        using var db = DbHelper.Trustee();
        TrusteeSynchronizeTime = db.GetCollection<TrusteeSynchronizeTime>().FindById(Identifier) ?? new TrusteeSynchronizeTime { Identifier = Identifier };
    }

    /// <summary>
    /// 登陆验证
    /// </summary>
    /// <param name="page"></param>
    /// <param name="wait_seconds"></param>
    /// <returns></returns>
    public async Task<bool> LoginValidationAsync(IPage page, int wait_seconds = 5)
    {
        bool b = false;
        for (var i = 0; i < wait_seconds; i++)
        {
            await Task.Delay(1000);

            try { b = await LoginValidationOverrideAsync(page); } catch { }
            if (b) break;
        }

        if (!b) WeakReferenceMessenger.Default.Send($"{Name}平台登陆失败", "toast");
        return b;
    }

    public abstract Task<bool> LoginValidationOverrideAsync(IPage page);

    public virtual async Task<bool> PrepareLoginAsync(IPage page)
    {
        return (await page.GotoAsync(Domain))?.Ok ?? false;
    }

    public virtual async Task<bool> EndLoginAsync(IPage page)
    {
        await page!.Keyboard.PressAsync("Escape");
        await page.Keyboard.PressAsync("Escape");
        return true;
    }
     

    public abstract Task<bool> SynchronizeFundRaisingRecord();


    /// <summary>
    /// 从托管外包机构同步客户资料
    /// </summary>
    /// <returns></returns>
    public abstract Task<bool> SynchronizeCustomerAsync();

    public abstract Task<bool> SynchronizeTransferRequestAsync();
     
    public abstract Task<bool> SynchronizeTransferRecordAsync();


    protected async Task<ILocator> FirstVisible(ILocator loc)
    {
        var lcnt = await loc.CountAsync();
        if (lcnt <= 1) return loc;

        for (int i = 0; i < lcnt; i++)
        {
            var l = loc.Nth(i);
            if (await l.IsVisibleAsync())
                return l;
        }
        return loc;
    }



}