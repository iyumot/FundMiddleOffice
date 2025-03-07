using Microsoft.Playwright;
using static System.Net.Mime.MediaTypeNames;

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


}