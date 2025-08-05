using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee.JsonCITICS;

#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

internal class BankTransactionJson : JsonBase
{

    /// <summary>
    /// ���׷���ʱ�䣬��ʽΪ YYYY-MM-DD HH:MM:SS
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
    /// �˻����ͣ����� "02-ļ����"
    /// </summary>
    [JsonPropertyName("accoType")]
    public string AccoType { get; set; }

    /// <summary>
    /// �����˺�
    /// </summary>
    [JsonPropertyName("bankAcco")]
    public string BankAcco { get; set; }

    /// <summary>
    /// �����˻�����
    /// </summary>
    [JsonPropertyName("accName")]
    public string AccName { get; set; }

    /// <summary>
    /// ��������������
    /// </summary>
    [JsonPropertyName("openBankName")]
    public string OpenBankName { get; set; }

    /// <summary>
    /// �������б�ţ������д�����ǰ��λΪ��׼������¼2��
    /// </summary>
    [JsonPropertyName("bankNo")]
    public string BankNo { get; set; }

    /// <summary>
    /// �Է��˺�
    /// </summary>
    [JsonPropertyName("othBankAcco")]
    public string OthBankAcco { get; set; }

    /// <summary>
    /// �Է��˻�����
    /// </summary>
    [JsonPropertyName("othAccName")]
    public string OthAccName { get; set; }

    /// <summary>
    /// �Է�����������
    /// </summary>
    [JsonPropertyName("othOpenBankName")]
    public string OthOpenBankName { get; set; }

    /// <summary>
    /// �Է����б�ţ������д�����ǰ��λΪ��׼������¼2��
    /// </summary>
    [JsonPropertyName("othBankNo")]
    public string OthBankNo { get; set; }

    /// <summary>
    /// ����
    /// </summary>
    [JsonPropertyName("curType")]
    public string CurType { get; set; }

    /// <summary>
    /// �ո�����1��ʾ�տ2��ʾ����
    /// </summary>
    [JsonPropertyName("directFlag")]
    public string DirectFlag { get; set; }

    /// <summary>
    /// �������
    /// </summary>
    [JsonPropertyName("occurAmt")]
    public string OccurAmt { get; set; }

    /// <summary>
    /// �˻����
    /// </summary>
    [JsonPropertyName("acctBal")]
    public string AcctBal { get; set; }

    /// <summary>
    /// ������ˮ��
    /// </summary>
    [JsonPropertyName("bankJour")]
    public string BankJour { get; set; }

    /// <summary>
    /// ���з��ش���
    /// </summary>
    [JsonPropertyName("bankRetCode")]
    public string BankRetCode { get; set; }

    /// <summary>
    /// ����ժҪ
    /// </summary>
    [JsonPropertyName("bankNote")]
    public string BankNote { get; set; }



    public RaisingBankTransaction ToObject()
    {
        return new RaisingBankTransaction
        {
            Id = $"{BankAcco}|{BankJour}",
            Time = DateTime.ParseExact(OccurTime, "yyyy-MM-dd HH:mm:ss", null),
            FundCode = FundCode,
            AccountBank = OpenBankName,
            AccountName = AccName,
            AccountNo = BankAcco,
            CounterBank = OthOpenBankName,
            CounterName = OthAccName,
            CounterNo = OthBankAcco,
            Amount = ParseDecimal(OccurAmt),
            Balance = ParseDecimal(AcctBal),
            Direction = DirectFlag == "����" ? TransctionDirection.Pay : TransctionDirection.Receive,
            Remark = BankNote,
            Serial = BankJour,
        };
    }
}

#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��