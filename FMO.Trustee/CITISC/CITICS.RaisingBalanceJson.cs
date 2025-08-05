using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee.JsonCITICS;

#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

internal class RaisingBalanceJson : JsonBase
{
    /// <summary>
    /// ����ʱ�䣨��ʽ��YYYY-MM-DD HH:mm:ss��
    /// </summary>
    [JsonPropertyName("occurTime")]
    public string OccurTime { get; set; }

    /// <summary>
    /// �������
    /// </summary>
    [JsonPropertyName("fundCode")]
    public string FundCode { get; set; }

    /// <summary>
    /// ��������
    /// </summary>
    [JsonPropertyName("fundName")]
    public string FundName { get; set; }

    /// <summary>
    /// �˻�����
    /// 02 - ļ����
    /// </summary>
    [JsonPropertyName("accoType")]
    public string AccountType { get; set; }

    /// <summary>
    /// �����˺�
    /// </summary>
    [JsonPropertyName("bankAcco")]
    public string BankAccountNo { get; set; }

    /// <summary>
    /// �����˻�����
    /// </summary>
    [JsonPropertyName("accName")]
    public string BankAccountName { get; set; }

    /// <summary>
    /// ���п���������
    /// </summary>
    [JsonPropertyName("openBankName")]
    public string OpenBankName { get; set; }

    /// <summary>
    /// ���б�ţ������д�����ǰ��λΪ��׼��
    /// </summary>
    [JsonPropertyName("bankNo")]
    public string BankNumber { get; set; }

    /// <summary>
    /// ����
    /// </summary>
    [JsonPropertyName("curType")]
    public string Currency { get; set; }

    /// <summary>
    /// �˻����
    /// </summary>
    [JsonPropertyName("acctBal")]
    public string AccountBalance { get; set; }

    /// <summary>
    /// ���з��ش���
    /// </summary>
    [JsonPropertyName("bankRetCode")]
    public string BankReturnCode { get; set; }

    /// <summary>
    /// ����ժҪ
    /// </summary>
    [JsonPropertyName("bankNote")]
    public string BankSummary { get; set; }

    /// <summary>
    /// CNAPS ���֧����
    /// </summary>
    [JsonPropertyName("cnapsCode")]
    public string CNAPSCode { get; set; }


    public FundBankBalance ToObject()
    {
        return new FundBankBalance
        {
            FundCode = FundCode,
            FundName = FundName,
            AccountName = BankAccountName,
            AccountNo = BankNumber,
            Name = OpenBankName,
            Balance = ParseDecimal(AccountBalance),
            Currency = Currency,
            Time = DateTime.ParseExact(OccurTime, "yyyy-MM-dd HH:mm:ss", null),
        };
    }
}

#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��