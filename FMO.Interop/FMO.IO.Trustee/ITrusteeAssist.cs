using LiteDB;
using Microsoft.Playwright;


namespace FMO.IO.Trustee;


/// <summary>
/// 托管接口
/// 必须指定标识，唯一
/// 必须指定名称
/// 可以设置logo，放在主文件夹内，命名logo.xxx，生成类型为嵌入的资源
/// </summary>
public interface ITrusteeAssist : IDisposable
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
    /// 同步募集流水，单向
    /// </summary>
    /// <returns></returns>
    Task<bool> SynchronizeFundRaisingRecord();



    /// <summary>
    /// 从托管外包机构同步客户资料，单向
    /// </summary>
    /// <returns></returns>
    Task<bool> SynchronizeCustomerAsync();


    /// <summary>
    /// 同步TA
    /// </summary>
    /// <returns></returns>
    Task<bool> SynchronizeTAAsync();






}


public class TrusteeSynchronizeTime
{
    [BsonId]
    public required string Identifier { get; set; }


    /// <summary>
    /// 客户同步时间
    /// </summary>
    public DateTime Customer { get; set; }

    /// <summary>
    /// TA同步时间
    /// </summary>
    public DateTime TA { get; set; }
}