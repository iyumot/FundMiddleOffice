using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee.JsonCITICS;

#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

public class CustodialTransactionJson2 : JsonBase
{
    /// <summary>
    /// �����˺�
    /// </summary>
    [JsonPropertyName("fpayerAcctCode")]
    public string PayerAccountCode { get; set; }

    /// <summary>
    /// ��������
    /// </summary>
    [JsonPropertyName("fpayerName")]
    public string PayerName { get; set; }

    /// <summary>
    /// ����������
    /// </summary>
    [JsonPropertyName("fpayerBank")]
    public string PayerBank { get; set; }

    /// <summary>
    /// �տ�˺�
    /// </summary>
    [JsonPropertyName("fpayeeAcctCode")]
    public string PayeeAccountCode { get; set; }

    /// <summary>
    /// �տ����
    /// </summary>
    [JsonPropertyName("fpayeeName")]
    public string PayeeName { get; set; }

    /// <summary>
    /// �տ������
    /// </summary>
    [JsonPropertyName("fpayeeBank")]
    public string PayeeBank { get; set; }

    /// <summary>
    /// �������ڣ���ʽ��yyyy-MM-dd��
    /// </summary>
    [JsonPropertyName("date")]
    public string OccurDate { get; set; }

    /// <summary>
    /// �տ����룺
    /// SFKFX001: ����
    /// SFKFX002: ���
    /// SFKFX003: ����
    /// </summary>
    [JsonPropertyName("fway")]
    public string DirectionCode { get; set; }

    /// <summary>
    /// �տ�����ƣ�SFKFX001�����SFKFX002����SFKFX003��������
    /// </summary>
    [JsonPropertyName("fwayName")]
    public string DirectionName { get; set; }

    /// <summary>
    /// �������ַ������ͣ�������λС����
    /// </summary>
    [JsonPropertyName("famount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// ��ע
    /// </summary>
    [JsonPropertyName("fsummary")]
    public string Summary { get; set; }

    public BankTransaction ToObject()
    {
        var direction = DirectionCode switch { "SFKFX001" or "SFKFX003" => TransctionDirection.Pay,/* "SFKFX002"*/ _ => TransctionDirection.Receive };

        return new BankTransaction
        {
            AccountBank = direction == TransctionDirection.Pay ? PayerBank : PayeeBank,
            AccountName = direction == TransctionDirection.Pay ? PayerName : PayeeName,
            AccountNo = direction == TransctionDirection.Pay ? PayerAccountCode : PayeeAccountCode,
            CounterBank = direction != TransctionDirection.Pay ? PayerBank : PayeeBank,
            CounterName = direction != TransctionDirection.Pay ? PayerName : PayeeName,
            CounterNo = direction != TransctionDirection.Pay ? PayerAccountCode : PayeeAccountCode,
            Id = $"{PayeeAccountCode.GetHashCode()}|{PayerAccountCode.GetHashCode()}|{OccurDate}{Amount}",
            Serial = $"{PayeeAccountCode.GetHashCode()}|{PayerAccountCode.GetHashCode()}|{OccurDate}{Amount}",
            Amount = Amount,
            Direction = direction,
            Remark = Summary,
            Time = DateTime.ParseExact(OccurDate, "yyyy-MM-dd HH:mm:ss", null)
        };
    }
}

#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��