using System.Text.Json.Serialization;

namespace FMO.Trustee;

#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

internal class VirtualNetValueJson : JsonBase
{
    /// <summary>
    /// ҵ�����ڣ���ʽ��YYYY-MM-DD
    /// </summary>
    [JsonPropertyName("date")]
    public string BusinessDate { get; set; }

    /// <summary>
    /// �ͻ�����
    /// </summary>
    [JsonPropertyName("custName")]
    public string InvestorName { get; set; }

    /// <summary>
    /// �����˺�
    /// </summary>
    [JsonPropertyName("fundAcco")]
    public string FundAccount { get; set; }

    /// <summary>
    /// ��Ʒ����
    /// </summary>
    [JsonPropertyName("fundCode")]
    public string FundCode { get; set; }

    /// <summary>
    /// ��Ʒ����
    /// </summary>
    [JsonPropertyName("fundName")]
    public string FundName { get; set; }

    /// <summary>
    /// �ֲַݶ�
    /// </summary>
    [JsonPropertyName("shares")]
    public decimal Shares { get; set; }

    /// <summary>
    /// ���᷽ʽ��1-��ֵ���᣻2-TA����
    /// </summary>
    [JsonPropertyName("flag")]
    public string AccrualMethod { get; set; }

    /// <summary>
    /// ����ҵ��������
    /// </summary>
    [JsonPropertyName("virtualBalance")]
    public decimal VirtualPerformanceFee { get; set; }

    /// <summary>
    /// ���⾻ֵ
    /// </summary>
    [JsonPropertyName("virtualAssetVal")]
    public string VirtualNetAssetValue { get; set; }

    /// <summary>
    /// ʵ�ʾ�ֵ
    /// </summary>
    [JsonPropertyName("netAssetVal")]
    public string ActualNetAssetValue { get; set; }

    /// <summary>
    /// �ۼƾ�ֵ
    /// </summary>
    [JsonPropertyName("totalAssetVal")]
    public string TotalNetAssetValue { get; set; }

    /// <summary>
    /// ����ۼ��ݶ�
    /// </summary>
    [JsonPropertyName("virtualDeductionShare")]
    public decimal VirtualDeductionShare { get; set; }

    /// <summary>
    /// ��ֵ�˶�״̬��
    /// 0-һ�£��йܸ���һ�£�
    /// 1-��һ�£�δ���й�ȷ�ϣ�
    /// 2-�����У�
    /// 3-���йܷ���Ʒ�����йܷ�����
    /// </summary>
    [JsonPropertyName("checkStatus")]
    public string NetValueCheckStatus { get; set; }
}

#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��