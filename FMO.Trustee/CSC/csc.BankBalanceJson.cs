using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee.JsonCSC;


#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

public class BankBalanceJson : JsonBase
{  /// <summary>
   /// ����
   /// </summary>
    [JsonPropertyName("curType")]
    public string Currency { get; set; }

    /// <summary>
    /// ������루��󳤶�32��
    /// </summary>
    [JsonPropertyName("fundCode")]
    public string FundCode { get; set; }

    /// <summary>
    /// �������ƣ���󳤶�250��
    /// </summary>
    [JsonPropertyName("fundName")]
    public string FundName { get; set; }

    /// <summary>
    /// �˻����ͣ���󳤶�2��
    /// </summary>
    [JsonPropertyName("accType")]
    public string AccountType { get; set; }

    /// <summary>
    /// �����˺ţ���󳤶�32��
    /// </summary>
    [JsonPropertyName("bankAcco")]
    public string BankAccountNo { get; set; }

    /// <summary>
    /// �˻����ƣ���󳤶�64��
    /// </summary>
    [JsonPropertyName("accName")]
    public string AccountName { get; set; }

    /// <summary>
    /// ���б�ţ���󳤶�6��ͨ��Ϊ���д��֧����ǰ��λ��
    /// </summary>
    [JsonPropertyName("bankNo")]
    public string BankNumber { get; set; }

    /// <summary>
    /// ���������ƣ���󳤶�250��
    /// </summary>
    [JsonPropertyName("openBankName")]
    public string OpenBankName { get; set; }

    /// <summary>
    /// ��������󳤶�20��������λС����
    /// </summary>
    [JsonPropertyName("acctBal")]
    public string AccountBalance { get; set; }

    /// <summary>
    /// �йܻ��˻�״̬�����йܻ���Ч��ļ���������壩
    /// �μ� 3.16 �й��˻�״̬
    /// </summary>
    [JsonPropertyName("acctStatus")]
    public string CustodialStatus { get; set; }

    /// <summary>
    /// ļ�����˻�״̬����ļ������Ч���йܻ������壩
    /// �μ� 3.23 ļ�����˻�״̬
    /// </summary>
    [JsonPropertyName("raiseStatus")]
    public string RaiseStatus { get; set; }

    /// <summary>
    /// ������ʱ�䣨��ʽ��YYYY-MM-dd HH:mm:ss��
    /// </summary>
    [JsonPropertyName("retTime")]
    public string LastUpdateTime { get; set; }



    public FundBankBalance ToObject()
    {
        return new FundBankBalance
        {
            AccountName = AccountName,
            AccountNo = BankAccountNo,
            Name = OpenBankName,
            FundName = FundName,
            FundCode = FundCode,
            Balance = string.IsNullOrWhiteSpace(AccountBalance) ? 0 : ParseDecimal(AccountBalance),
            Currency = ParseCurrency(Currency),
            Time = string.IsNullOrWhiteSpace(LastUpdateTime) ? default : DateTime.ParseExact(LastUpdateTime, "yyyy-MM-dd HH:mm:ss", null)
        };
    }

}

#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��