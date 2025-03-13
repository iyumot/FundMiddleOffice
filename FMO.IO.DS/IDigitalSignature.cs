using CommunityToolkit.Mvvm.Messaging;
using FMO.Utilities;
using Microsoft.Playwright;
using Serilog;

namespace FMO.IO.DS;


/// <summary>
/// 电签平台
/// </summary>
public interface IDigitalSignature : IDisposable
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

    /// <summary>
    /// 是否已登录
    /// </summary>
    bool IsLogedIn { get; }


    /// <summary>
    /// 验证登录的方法
    /// </summary>
    /// <param name="page"></param>
    /// <param name="wait_seconds">最大等待时间（秒）</param>
    /// <returns></returns>
    Task<bool> LoginValidationAsync(IPage page, int wait_seconds = 5);

    /// <summary>
    /// 登录
    /// </summary>
    /// <returns></returns>
    Task<bool> LoginAsync();





    /// <summary>
    /// 从托管外包机构同步客户资料，单向
    /// </summary>
    /// <returns></returns>
    Task<bool> SynchronizeCustomerAsync();



    Task<bool> PrepareLoginAsync(IPage page);
}



/// <summary>
/// 提供部分实现的基类
/// </summary>
public abstract class AssistBase : IDigitalSignature, IDisposable
{
    private IPage? _mainPage;


    public abstract string Identifier { get; }

    public abstract string Name { get; }


    /// <summary>
    /// 主页
    /// </summary>
    public abstract string Domain { get; }



    public bool IsLogedIn { get; protected set; }


    public SynchronizeTime SynchronizeTime { get; init; }


    protected AssistBase()
    {
        using var db = new DSDatabase();
        SynchronizeTime = db.GetCollection<SynchronizeTime>().FindById(Identifier) ?? new SynchronizeTime { Identifier = Identifier };
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


    protected async Task<IPage> GetPageAsync()
    {
        if (_mainPage is null || _mainPage.IsClosed)
        {
            _mainPage = await Automation.NewPageAsync(Identifier);
            await _mainPage.GotoAsync(Domain);
        }
        return _mainPage;
    }


    public async Task<bool> PrepareLoginAsync(IPage page)
    {
        return (await page.GotoAsync(Domain))?.Ok ?? false;
    }

    public virtual async Task<bool> LoginAsync()
    {
        try
        {
            WeakReferenceMessenger.Default.Send("请手动登陆平台", "toast");

            IsLogedIn = await Automation.LoginAsync(Identifier, PrepareLoginAsync, LoginValidationOverrideAsync, 60);

        }
        catch (Exception e) { IsLogedIn = false; Log.Error($"Platform Login Failed : {e.Message}"); }

        return IsLogedIn;
    }


    /// <summary>
    /// 从托管外包机构同步客户资料
    /// </summary>
    /// <returns></returns>
    public abstract Task<bool> SynchronizeCustomerAsync();



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


    public async void Dispose()
    {
        if (_mainPage is not null)
            await _mainPage.CloseAsync();
    }


}