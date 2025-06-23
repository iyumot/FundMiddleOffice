
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
    /// ��ȡļ������ˮ
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Task<BankTransaction[]?> GetRaisingAccountRecords(DateOnly begin, DateOnly end);

    /// <summary>
    /// ��ȡ�йܻ���ˮ
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Task<BankTransaction[]?>? GetCustodyAccountRecords(DateOnly begin, DateOnly end);


    Task<TransferRequest[]?> GetTransferRequests(DateOnly begin, DateOnly end);

    /// <summary>
    /// ��ȡ����ȷ�ϼ�¼
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Task<ReturnWrap<TransferRecord>> QueryTransferRecords(DateOnly begin, DateOnly end);

    /// <summary>
    /// ��ȡ����̶�����
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Task<ReturnWrap<FundDailyFee>> QueryFundFeeDetail(DateOnly begin, DateOnly end);


    /// <summary>
    /// �йܻ���ˮ
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Task<ReturnWrap<BankTransaction>> QueryCustodialAccountTransction(DateOnly begin, DateOnly end = default);


    /// <summary>
    /// ļ������ˮ
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Task<ReturnWrap<BankTransaction>> QueryRaisingAccountTransction(DateOnly begin, DateOnly end);



    /// <summary>
    /// ��ȡ�ּ���Ʒӳ���ϵ
    /// </summary>
    /// <returns></returns>
    Task<ReturnWrap<SubjectFundMapping>> SyncSubjectFundMappings();
}



public interface IAPIConfig
{
    public string Id { get; }
}
