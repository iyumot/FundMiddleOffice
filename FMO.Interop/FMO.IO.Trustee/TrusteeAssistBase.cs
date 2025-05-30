﻿using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using Microsoft.Playwright;




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

    public string? UserID { get; set; }

    public string? Password { get; set; }

    public abstract Regex HoldingCheck { get; init; }

    protected TrusteeAssistBase()
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

        if (!b) WeakReferenceMessenger.Default.Send($"{Name}平台登陆失败", "toast");
        return b;
    }

    public abstract Task<bool> LoginValidationOverrideAsync(IPage page);

    public virtual async Task<bool> PrepareLoginAsync(IPage page)
    {
        try
        {
            var resp = await page.GotoAsync(Domain);

            return resp?.Ok ?? false;
        }
        catch (Exception)
        {
            return false;
        } 
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

    /// <summary>
    /// 同步交易申请
    /// </summary>
    /// <returns></returns>
    public abstract Task<bool> SynchronizeTransferRequestAsync();


    public abstract Task<bool> SynchronizeTransferRecordAsync();

    public abstract Task<bool> SynchronizeDistributionAsync();




    public abstract Task<(string Code, ManageFeeDetail[] Fee)[]> GetManageFeeDetails(DateOnly start, DateOnly end);

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