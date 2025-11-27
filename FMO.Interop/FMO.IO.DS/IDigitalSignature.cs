using CommunityToolkit.Mvvm.Messaging;
using FMO.IO;
using FMO.Utilities;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace FMO.DS;


/// <summary>
/// 电签平台
/// </summary>
public interface IDigitalSignature : IExternPlatform
{

    /// <summary>
    /// 从托管外包机构同步客户资料，单向
    /// </summary>
    /// <returns></returns>
    Task<bool> SynchronizeCustomerAsync();
    Task<bool> SynchronizeQualificatoinAsync();

    Task<bool> SynchronizeOrderAsync();
}



/// <summary>
/// 提供部分实现的基类
/// </summary>
public abstract class AssistBase : IDigitalSignature
{
    //public IPage? MainPage { get; protected set; }


    public abstract string Identifier { get; }

    public abstract string Name { get; }


    /// <summary>
    /// 主页
    /// </summary>
    public abstract string Domain { get; }



    public bool IsLogedIn { get; protected set; }
     

    public abstract Regex HoldingCheck { get; init; }

    public string? UserID { get; set; }

    public string? Password { get; set; }

    protected AssistBase()
    { 
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

        if (!b) WeakReferenceMessenger.Default.Send($"{Name}平台登陆失败", "toast");//WeakReferenceMessenger.Default.Send(Identifier, "DigitalSignature.LogOut");
        return b;
    }

    public abstract Task<bool> LoginValidationOverrideAsync(IPage page);


    //protected async Task<IPage> GetPageAsync()
    //{
    //    if (MainPage is null || MainPage.IsClosed)
    //    {
    //        MainPage = await Automation.NewPageAsync();
    //        await MainPage.GotoAsync(Domain);
    //    }
    //    return MainPage;
    //}


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

    //public virtual async Task<bool> LoginAsync()
    //{
    //    try
    //    {
    //        WeakReferenceMessenger.Default.Send("请手动登陆平台", "toast");

    //        IsLogedIn = await Automation.LoginAsync(Identifier, PrepareLoginAsync, LoginValidationOverrideAsync, 60);

    //    }
    //    catch (Exception e) { IsLogedIn = false; Log.Error($"Platform Login Failed : {e.Message}"); }

    //    return IsLogedIn;
    //}


    /// <summary>
    /// 从托管外包机构同步客户资料
    /// </summary>
    /// <returns></returns>
    public abstract Task<bool> SynchronizeCustomerAsync();


    public abstract Task<bool> SynchronizeQualificatoinAsync();

    protected async Task<ILocator?> FirstVisible(ILocator loc)
    {
        var lcnt = await loc.CountAsync(); 

        for (int i = 0; i < lcnt; i++)
        {
            var l = loc.Nth(i);
            if (await l.IsVisibleAsync())
                return l;
        }
        return null;
    }



    public abstract Task<bool> SynchronizeOrderAsync();
  

}