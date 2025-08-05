using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee.JsonCITICS;

#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

internal class DistrubutionJson : JsonBase
{
    /// <summary>
    /// �����˺�
    /// </summary>
    [JsonPropertyName("fundAcco")]
    public string FundAccount { get; set; }

    /// <summary>
    /// �ͻ�����
    /// </summary>
    [JsonPropertyName("custName")]
    public string CustomerName { get; set; }

    /// <summary>
    /// �����̴��루ZX6 ��ʾֱ����
    /// </summary>
    [JsonPropertyName("agencyNo")]
    public string AgencyCode { get; set; }

    /// <summary>
    /// ����������
    /// </summary>
    [JsonPropertyName("agencyName")]
    public string AgencyName { get; set; }

    /// <summary>
    /// �����˺�
    /// </summary>
    [JsonPropertyName("tradeAcco")]
    public string TradeAccount { get; set; }

    /// <summary>
    /// ȷ�����ڣ���ʽ��YYYYMMDD��
    /// </summary>
    [JsonPropertyName("confirmDate")]
    public string ConfirmDate { get; set; }

    /// <summary>
    /// TAȷ�Ϻ�
    /// </summary>
    [JsonPropertyName("cserialNo")]
    public string TaConfirmNo { get; set; }

    /// <summary>
    /// �ֺ�Ǽ�����
    /// </summary>
    [JsonPropertyName("regDate")]
    public string RegisterDate { get; set; }

    /// <summary>
    /// ������������
    /// </summary>
    [JsonPropertyName("date")]
    public string DividendDate { get; set; }

    /// <summary>
    /// ��������
    /// </summary>
    [JsonPropertyName("fundName")]
    public string FundName { get; set; }

    /// <summary>
    /// �������
    /// </summary>
    [JsonPropertyName("fundCode")]
    public string FundCode { get; set; }

    /// <summary>
    /// �ֺ�����ݶ�
    /// </summary>
    [JsonPropertyName("totalShare")]
    public string TotalShares { get; set; }

    /// <summary>
    /// ÿ��λ�ֺ���
    /// </summary>
    [JsonPropertyName("unitProfit")]
    public string UnitProfit { get; set; }

    /// <summary>
    /// �����ܶ�
    /// </summary>
    [JsonPropertyName("totalProfit")]
    public string TotalProfit { get; set; }

    /// <summary>
    /// �ֺ췽ʽ��
    /// 0 - ������Ͷ��
    /// 1 - �ֽ����
    /// </summary>
    [JsonPropertyName("flag")]
    public string DividendType { get; set; }

    /// <summary>
    /// ʵ���ֽ����
    /// </summary>
    [JsonPropertyName("realBalance")]
    public string RealCashDividend { get; set; }

    /// <summary>
    /// ��Ͷ�ʺ������
    /// </summary>
    [JsonPropertyName("reinvestBalance")]
    public string ReinvestAmount { get; set; }

    /// <summary>
    /// ��Ͷ�ʷݶ�
    /// </summary>
    [JsonPropertyName("realShares")]
    public string ReinvestShares { get; set; }

    /// <summary>
    /// ��Ͷ������
    /// </summary>
    [JsonPropertyName("lastDate")]
    public string ReinvestDate { get; set; }

    /// <summary>
    /// ��Ͷ�ʵ�λ��ֵ
    /// </summary>
    [JsonPropertyName("netValue")]
    public string NetValue { get; set; }

    /// <summary>
    /// ʵ��ҵ����ɽ��
    /// </summary>
    [JsonPropertyName("deductBalance")]
    public string PerformanceFee { get; set; }

    /// <summary>
    /// �ͻ����ͣ��μ���¼4��
    /// </summary>
    [JsonPropertyName("custType")]
    public string CustomerType { get; set; }

    /// <summary>
    /// ֤�����ͣ��μ���¼4��
    /// </summary>
    [JsonPropertyName("certiType")]
    public string CertificateType { get; set; }

    /// <summary>
    /// ֤������
    /// </summary>
    [JsonPropertyName("certiNo")]
    public string CertificateNumber { get; set; }

    /// <summary>
    /// ��Ȩ��Ϣ��
    /// </summary>
    [JsonPropertyName("exDividendDate")]
    public string ExDividendDate { get; set; }


    public TransferRecord ToObject()
    {
        return new TransferRecord
        {
            CustomerIdentity = CertificateNumber,
            CustomerName = CustomerName,
            ExternalId = TaConfirmNo,
            ConfirmedDate = DateOnly.ParseExact(ConfirmDate, "yyyyMMdd"),
            ConfirmedShare = ParseDecimal(ReinvestShares),
            ConfirmedAmount = ParseDecimal(RealCashDividend),
            ConfirmedNetAmount = ParseDecimal(RealCashDividend),
            RequestDate = DateOnly.ParseExact(ExDividendDate, "yyyyMMdd"),
            RequestAmount = 0,
            RequestShare = ParseDecimal(TotalShares),
            Type = TransferRecordType.Distribution,
            Agency = AgencyName,
            CreateDate = DateOnly.FromDateTime(DateTime.Now),
            PerformanceFee = ParseDecimal(PerformanceFee),
            FundCode = FundCode,
            FundName = FundName,
            Source = "api"
        };
    }
}


#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��