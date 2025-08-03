using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee.JsonCITICS;

#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

public class BankBalanceJson : JsonBase
{
    /// <summary>
    /// �˺�
    /// </summary>
    [JsonPropertyName("YHZH")]
    public string AccountNumber { get; set; }

    /// <summary>
    /// �˻����
    /// </summary>
    [JsonPropertyName("ZHYE")]
    public string AccountBalance { get; set; }

    /// <summary>
    /// �˻�����
    /// </summary>
    [JsonPropertyName("KHHM")]
    public string AccountHolderName { get; set; }

    /// <summary>
    /// �������ࣺ
    /// HKD-�۱ң�
    /// RMB-����ң�
    /// USD-��Ԫ
    /// </summary>
    [JsonPropertyName("JSBZ")]
    public string CurrencyType { get; set; }

    /// <summary>
    /// ����ѯʱ�䣬��ʽ��YYYY-MM-DD HH:MM:SS
    /// </summary>
    [JsonPropertyName("CXSJ")]
    public string QueryTime { get; set; }

    /// <summary>
    /// �˻��������
    /// </summary>
    [JsonPropertyName("ZHKYYE")]
    public string AvailableBalance { get; set; }

    /// <summary>
    /// ��������
    /// 0-�ɹ���
    /// -2-�����У�
    /// ����ֵ����ʧ��
    /// </summary>
    [JsonPropertyName("CLJG")]
    public string ProcessingResult { get; set; }

    /// <summary>
    /// ����˵��
    /// </summary>
    [JsonPropertyName("CLSM")]
    public string ProcessingDescription { get; set; }

    /// <summary>
    /// ������
    /// </summary>
    [JsonPropertyName("KHYH")]
    public string OpeningBank { get; set; }

    public BankBalance ToObject()
    {
        return new BankBalance
        {
            AccountName = AccountHolderName,
            AccountNo = AccountNumber,
            Name = OpeningBank,
            Balance = ParseDecimal(AccountBalance),
            Currency = CurrencyType,
            Time = DateTime.ParseExact(QueryTime, "yyyy-MM-dd HH:mm:ss", null)
        };
    }
}


#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��