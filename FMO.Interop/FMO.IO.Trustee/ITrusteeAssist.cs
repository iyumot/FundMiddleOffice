using FMO.Models;
using LiteDB;
using Microsoft.Playwright;


namespace FMO.IO.Trustee;


/// <summary>
/// 托管接口
/// 必须指定标识，唯一
/// 必须指定名称
/// 可以设置logo，放在主文件夹内，命名logo.xxx，生成类型为嵌入的资源
/// </summary>
public interface ITrusteeAssist : IExternPlatform
{ 
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
    /// 同步交易申请
    /// </summary>
    /// <returns></returns>
    Task<bool> SynchronizeTransferRequestAsync();

    /// <summary>
    /// 同步交易确认
    /// </summary>
    /// <returns></returns>
    Task<bool> SynchronizeTransferRecordAsync();
    Task<(string Code, ManageFeeDetail[] Fee)[]> GetManageFeeDetails(DateOnly start, DateOnly end);
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