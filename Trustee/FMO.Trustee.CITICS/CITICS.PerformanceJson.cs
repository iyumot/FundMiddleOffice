using System.Text.Json.Serialization;

namespace FMO.Trustee;

#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

internal class PerformanceJson : JsonBase
{
    [JsonPropertyName("fundAcco")]
    public string FundAccount { get; set; } // �����˺�

    [JsonPropertyName("custName")]
    public string InvestorName { get; set; } // �ͻ�����

    [JsonPropertyName("custType")]
    public string CustomerType { get; set; } // �ͻ�����

    [JsonPropertyName("agencyNo")]
    public string AgencyCode { get; set; } // �����̴���

    [JsonPropertyName("agencyName")]
    public string AgencyName { get; set; } // ����������

    [JsonPropertyName("tradeAcco")]
    public string TradeAccount { get; set; } // �����˺�

    [JsonPropertyName("businFlag")]
    public string BusinessType { get; set; } // ҵ�����ͣ��ο�ӳ���

    [JsonPropertyName("sortFlag")]
    public string SortFlag { get; set; } // ��������: 0-���״��� 1-ҵ�����

    [JsonPropertyName("requestDate")]
    public string RequestDate { get; set; } // ��������

    [JsonPropertyName("confirmDate")]
    public string ConfirmDate { get; set; } // ȷ������

    [JsonPropertyName("fundCode")]
    public string FundCode { get; set; } // �������

    [JsonPropertyName("shareTypeCn")]
    public string ShareTypeName { get; set; } // �ݶ����

    [JsonPropertyName("cserialNo")]
    public string TaConfirmNo { get; set; } // TAȷ�Ϻ�

    [JsonPropertyName("registDate")]
    public string RegisterDate { get; set; } // �ݶ�ע������

    [JsonPropertyName("shares")]
    public string Shares { get; set; } // �����ݶ�

    [JsonPropertyName("beginDate")]
    public string BeginDate { get; set; } // �ڳ�����

    [JsonPropertyName("oriNav")]
    public string OriNav { get; set; } // �ڳ���λ��ֵ

    [JsonPropertyName("oriTotalNav")]
    public string OriTotalNav { get; set; } // �ڳ��ۼƾ�ֵ

    [JsonPropertyName("nav")]
    public string Nav { get; set; } // ��ĩ��λ��ֵ

    [JsonPropertyName("totalNav")]
    public string TotalNav { get; set; } // ��ĩ�ۼƾ�ֵ

    [JsonPropertyName("currRatio")]
    public string CurrentRatio { get; set; } // ��ǰ������

    [JsonPropertyName("yearRatio")]
    public string YearRatio { get; set; } // �껯������

    [JsonPropertyName("oriBalance")]
    public string OriBalance { get; set; } // Ӧ���/���׽��

    [JsonPropertyName("factBalance")]
    public string FactBalance { get; set; } // ʵ�����/���׽��

    [JsonPropertyName("factShares")]
    public string FactShares { get; set; } // ʵ�����/���׷ݶ�

    [JsonPropertyName("bonusBalance")]
    public string BonusBalance { get; set; } // �ֺ��ܽ��

    [JsonPropertyName("oriCserialNo")]
    public string OriginalTaConfirmNo { get; set; } // ԭȷ�ϵ���

    [JsonPropertyName("hold")]
    public string HoldDays { get; set; } // ��������

    [JsonPropertyName("indexYearRatio")]
    public string IndexYearRatio { get; set; } // ֤ȯָ���껯������

    [JsonPropertyName("beginIndexPrice")]
    public string BeginIndexPrice { get; set; } // �ڳ�ָ���۸�

    [JsonPropertyName("endIndexPrice")]
    public string EndIndexPrice { get; set; } // ��ĩָ���۸�

    [JsonPropertyName("calcFlag")]
    public string CalcFlag { get; set; } // �����ʶ��0-���ᣬ1-����
}

#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��