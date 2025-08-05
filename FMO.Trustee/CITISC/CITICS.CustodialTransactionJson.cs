using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee.JsonCITICS;

#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

internal class CustodialTransactionJson : JsonBase
{
    /// <summary>
    /// ���д���
    /// </summary>
    [JsonPropertyName("YHDM")]
    public string BankCode { get; set; }

    /// <summary>
    /// ��������
    /// </summary>
    [JsonPropertyName("YHMC")]
    public string BankName { get; set; }

    /// <summary>
    /// �����˺ţ�CLJGΪ-1ʱ���ѯ�ʺţ�
    /// </summary>
    [JsonPropertyName("FKFZH")]
    public string PayerAccountNo { get; set; }

    /// <summary>
    /// ��������
    /// </summary>
    [JsonPropertyName("FKFHM")]
    public string PayerAccountName { get; set; }

    /// <summary>
    /// �շ��˺ţ�CLJGΪ-1ʱ���ѯ�ʺţ�
    /// </summary>
    [JsonPropertyName("SKFZH")]
    public string PayeeAccountNo { get; set; }

    /// <summary>
    /// �շ�����
    /// </summary>
    [JsonPropertyName("SKFHM")]
    public string PayeeAccountName { get; set; }

    /// <summary>
    /// �������
    /// </summary>
    [JsonPropertyName("FSJE")]
    public decimal Amount { get; set; }

    /// <summary>
    /// �������
    /// HKD: �۱�, RMB: �����, USD: ��Ԫ
    /// </summary>
    [JsonPropertyName("JSBZ")]
    public string Currency { get; set; }

    /// <summary>
    /// �����־��
    /// �衢��������=�衢�ۿ�=�衢�տ�=�����ۿ�����տ��
    /// </summary>
    [JsonPropertyName("JDBZ")]
    public string DebitCreditFlag { get; set; }

    /// <summary>
    /// ժҪ����
    /// </summary>
    [JsonPropertyName("ZYMC")]
    public string Summary { get; set; }

    /// <summary>
    /// ����ʱ�䣬��ʽ yyyy-MM-dd HH:mm:ss
    /// </summary>
    [JsonPropertyName("FSSJ")]
    public string OccurTime { get; set; }

    /// <summary>
    /// ������Ϣ
    /// </summary>
    [JsonPropertyName("FHXX")]
    public string ReturnInfo { get; set; }

    /// <summary>
    /// �����ֶ�
    /// </summary>
    [JsonPropertyName("BYZD")]
    public string ReservedField { get; set; }

    /// <summary>
    /// �˻����
    /// </summary>
    [JsonPropertyName("ZHYE")]
    public decimal AccountBalance { get; set; }

    /// <summary>
    /// �������
    /// </summary>
    [JsonPropertyName("KYYE")]
    public decimal AvailableBalance { get; set; }

    /// <summary>
    /// ��ˮ��
    /// </summary>
    [JsonPropertyName("LSH")]
    public string SerialNumber { get; set; }

    /// <summary>
    /// ������
    /// 0 - �ɹ�
    /// -2 - ������
    /// ����ֵ����ʧ��
    /// </summary>
    [JsonPropertyName("CLJG")]
    public string ProcessResult { get; set; }

    /// <summary>
    /// ����˵��
    /// </summary>
    [JsonPropertyName("CLSM")]
    public string ProcessDescription { get; set; }


    public BankTransaction ToObject()
    {
        return new BankTransaction
        {
            AccountBank = BankName,
            AccountName = PayerAccountName,
            AccountNo = PayerAccountNo,
            CounterBank = "",
            CounterName = PayeeAccountName,
            CounterNo = PayeeAccountNo,
            Id = SerialNumber,
            Serial = SerialNumber,
            Amount = Amount,
            Balance = AccountBalance,
            Currency = Currency,
            Direction = DebitCreditFlag switch { string s when s.Contains("����") => TransctionDirection.Cancel, "��" or "���" or "�ۿ�" => TransctionDirection.Pay, _ => TransctionDirection.Receive },
            Remark = Summary,
            Time = DateTime.ParseExact(OccurTime, "yyyy-MM-dd HH:mm:ss", null)
        };
    }

}


#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��