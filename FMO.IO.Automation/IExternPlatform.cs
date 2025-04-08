using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace FMO.IO;

public interface IExternPlatform //: IDisposable
{
    /// <summary>
    /// 标识 
    /// 不可重复
    /// cstisc 中信证券
    /// </summary>
    string Identifier { get; }


    /// <summary>
    /// 公司名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 主页
    /// </summary>
    string Domain { get; }


    string? UserID { get; set; }

    string? Password { get; set; }

    /// <summary>
    /// 是否已登录
    /// </summary>
    bool IsLogedIn { get; }

    //IPage? MainPage { get; }


    Regex HoldingCheck { get; }


    Task<bool> LoginValidationOverrideAsync(IPage page);

    /// <summary>
    /// 验证登录的方法
    /// </summary>
    /// <param name="page"></param>
    /// <param name="wait_seconds">最大等待时间（秒）</param>
    /// <returns></returns>
    async Task<bool> LoginValidationAsync(IPage page, int wait_seconds = 5)
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

    /// <summary>
    /// 登录
    /// </summary>
    /// <returns></returns>
    async Task<bool> LoginAsync()
    {
        using var page = await Automation.AcquirePage(Identifier);

        if (page.IsNew) await page.GotoAsync(Domain);

        if (page is not null && await LoginValidationOverrideAsync(page))
            return true;


        await PrepareLoginAsync(page!);

        bool b = false;
        int time_out = 30;
        for (var i = 0; i < time_out; i++)
        {
            await Task.Delay(1000);

            if (page!.IsClosed) break;

            try { b = await LoginValidationOverrideAsync(page!); } catch { }
            if (b) break;
        }

        try { await EndLoginAsync(page!); } catch { }


        //Automation.ReleasePage(page!, Identifier);
        return b;
    }



    /// <summary>
    /// 登陆前工作
    /// 如果可以全自动登陆，也写在这里
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    Task<bool> PrepareLoginAsync(IPage page);

    /// <summary>
    /// 登陆后工作
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    Task<bool> EndLoginAsync(IPage page);
}

/// <summary>
/// 
/// </summary>
public class PlatformAccount
{
    public required string Id { get; set; }

    public string? UserId { get; set; }

    public string? Password { get; set; }


}