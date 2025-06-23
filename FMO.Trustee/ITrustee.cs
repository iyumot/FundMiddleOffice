
using FMO.Models;

namespace FMO.Trustee;











/// <summary>
/// 
/// </summary>
public interface ITrustee
{
    /// <summary>
    /// https://xxx.com no /
    /// </summary>
    string TestDomain { get; }

    /// <summary>
    /// https://xxx.com no /
    /// </summary>
    string Domain { get; }

    bool IsValid { get; }

    Task<bool> Prepare();

    /// <summary>
    /// 获取募集户流水
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Task<BankTransaction[]?> GetRaisingAccountRecords(DateOnly begin, DateOnly end);

    /// <summary>
    /// 获取托管户流水
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Task<BankTransaction[]?>? GetCustodyAccountRecords(DateOnly begin, DateOnly end);


    Task<TransferRequest[]?> GetTransferRequests(DateOnly begin, DateOnly end);

    /// <summary>
    /// 获取交易确认记录
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Task<ReturnWrap<TransferRecord>> QueryTransferRecords(DateOnly begin, DateOnly end);

    /// <summary>
    /// 获取基金固定费用
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Task<ReturnWrap<FundDailyFee>> QueryFundFeeDetail(DateOnly begin, DateOnly end);


    /// <summary>
    /// 托管户流水
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Task<ReturnWrap<BankTransaction>> QueryCustodialAccountTransction(DateOnly begin, DateOnly end = default);


    /// <summary>
    /// 募集户流水
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Task<ReturnWrap<BankTransaction>> QueryRaisingAccountTransction(DateOnly begin, DateOnly end);



    /// <summary>
    /// 获取分级产品映射关系
    /// </summary>
    /// <returns></returns>
    Task<ReturnWrap<SubjectFundMapping>> SyncSubjectFundMappings();
}



public interface IAPIConfig
{
    public string Id { get; }
}
