using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee.JsonCMS;


#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

/// <summary>
/// ���������Ϣʵ����
/// </summary>
public class FundDailyFeeJson : JsonBase
{
    /// <summary>
    /// ��Ʒ���ƣ������󳤶�300��
    /// </summary>
    [JsonPropertyName("fundName")]
    public string FundName { get; set; }

    /// <summary>
    /// ��Ʒ���루�����󳤶�6��
    /// </summary>
    [JsonPropertyName("fundCode")]
    public string FundCode { get; set; }

    /// <summary>
    /// �������ڣ���ʽ��yyyyMMdd�����
    /// </summary>
    [JsonPropertyName("busiDate")]
    public string BusiDate { get; set; }

    // �йܷ�
    [JsonPropertyName("custodianFeeJt")]
    public string CustodianFeeJt { get; set; } // ����

    [JsonPropertyName("custodianFeeZf")]
    public string CustodianFeeZf { get; set; } // ֧��

    [JsonPropertyName("custodianFeeYe")]
    public string CustodianFeeYe { get; set; } // ���

    // ��Ӫ�����
    [JsonPropertyName("operationServiceFeeJt")]
    public string OperationServiceFeeJt { get; set; }

    [JsonPropertyName("operationServiceFeeZf")]
    public string OperationServiceFeeZf { get; set; }

    [JsonPropertyName("operationServiceFeeYe")]
    public string OperationServiceFeeYe { get; set; }

    // �����
    [JsonPropertyName("managementFeeJt")]
    public string ManagementFeeJt { get; set; }

    [JsonPropertyName("managementFeeZf")]
    public string ManagementFeeZf { get; set; }

    [JsonPropertyName("managementFeeYe")]
    public string ManagementFeeYe { get; set; }

    // ҵ�������
    [JsonPropertyName("performanceFeeJt")]
    public string PerformanceFeeJt { get; set; }

    [JsonPropertyName("performanceFeeZf")]
    public string PerformanceFeeZf { get; set; }

    [JsonPropertyName("performanceFeeYe")]
    public string PerformanceFeeYe { get; set; }

    // ���۷����
    [JsonPropertyName("salesandServiceFeesJt")]
    public string SalesAndServiceFeesJt { get; set; }

    [JsonPropertyName("salesandServiceFeesZf")]
    public string SalesAndServiceFeesZf { get; set; }

    [JsonPropertyName("salesandServiceFeesYe")]
    public string SalesAndServiceFeesYe { get; set; }

    // Ͷ�ʹ��ʷ�
    [JsonPropertyName("investmentConsultantFeeJt")]
    public string InvestmentConsultantFeeJt { get; set; }

    [JsonPropertyName("investmentConsultantFeeZf")]
    public string InvestmentConsultantFeeZf { get; set; }

    [JsonPropertyName("investmentConsultantFeeYe")]
    public string InvestmentConsultantFeeYe { get; set; }

    // �ͻ������
    [JsonPropertyName("customerServiceFeeJt")]
    public string CustomerServiceFeeJt { get; set; }

    [JsonPropertyName("customerServiceFeeZf")]
    public string CustomerServiceFeeZf { get; set; }

    [JsonPropertyName("customerServiceFeeYe")]
    public string CustomerServiceFeeYe { get; set; }

    public FundDailyFee ToObject()
    {
        return new FundDailyFee
        {
            FundCode = FundCode,
            Date = DateOnly.ParseExact(BusiDate, "yyyyMMdd"),
            // �����
            ManagerFeeAccrued = ParseDecimal(ManagementFeeJt),
            ManagerFeePaid = ParseDecimal(ManagementFeeZf),
            ManagerFeeBalance = ParseDecimal(ManagementFeeYe),

            // �йܷ�
            CustodianFeeAccrued = ParseDecimal(CustodianFeeJt),
            CustodianFeePaid = ParseDecimal(CustodianFeeZf),
            CustodianFeeBalance = ParseDecimal(CustodianFeeYe),

            // �����Ӫ����ѣ�OperationServiceFee��
            OutsourcingFeeAccrued = ParseDecimal(OperationServiceFeeJt),
            OutsourcingFeePaid = ParseDecimal(OperationServiceFeeZf),
            OutsourcingFeeBalance = ParseDecimal(OperationServiceFeeYe),

            // ҵ�������
            PerformanceFeeAccrued = ParseDecimal(PerformanceFeeJt),
            PerformanceFeePaid = ParseDecimal(PerformanceFeeZf),
            PerformanceFeeBalance = ParseDecimal(PerformanceFeeYe),

            // ���۷����
            SalesFeeAccrued = ParseDecimal(SalesAndServiceFeesJt),
            SalesFeePaid = ParseDecimal(SalesAndServiceFeesZf),
            SalesFeeBalance = ParseDecimal(SalesAndServiceFeesYe),

            // Ͷ�ʹ��ʷ�
            ConsultantFeeAccrued = ParseDecimal(InvestmentConsultantFeeJt),
            ConsultantFeePaid = ParseDecimal(InvestmentConsultantFeeZf),
            ConsultantFeeBalance = ParseDecimal(InvestmentConsultantFeeYe)

        };
    }

}




#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��