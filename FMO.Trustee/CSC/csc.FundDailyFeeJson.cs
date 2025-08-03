using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;


#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

public class FundDailyFeeJson : JsonBase
{

    /// <summary>
    /// ���ü�������
    /// </summary>
    [JsonPropertyName("confDate")]
    public string ConfDate { get; set; }

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

    #region ���ü���
    /// <summary>
    /// �����
    /// </summary>
    [JsonPropertyName("managementFee")]
    public string ManagementFee { get; set; }

    /// <summary>
    /// Ͷ�˷�
    /// </summary>
    [JsonPropertyName("investmentFee")]
    public string InvestmentFee { get; set; }

    /// <summary>
    /// ���۷����
    /// </summary>
    [JsonPropertyName("salesFee")]
    public string SalesFee { get; set; }

    /// <summary>
    /// �йܷ�
    /// </summary>
    [JsonPropertyName("custodianFee")]
    public string CustodianFee { get; set; }

    /// <summary>
    /// ��������
    /// </summary>
    [JsonPropertyName("outsourcingFee")]
    public string OutsourcingFee { get; set; }

    /// <summary>
    /// ҵ���������
    /// </summary>
    [JsonPropertyName("reward")]
    public string Reward { get; set; }

    /// <summary>
    /// ��ֵ˰����
    /// </summary>
    [JsonPropertyName("addedTax")]
    public string AddedTax { get; set; }

    /// <summary>
    /// ����˰����
    /// </summary>
    [JsonPropertyName("surTax")]
    public string SurTax { get; set; }

    /// <summary>
    /// ��ֵ˰������˰����
    /// </summary>
    [JsonPropertyName("addedSurTax")]
    public string AddedSurTax { get; set; }
    #endregion

    #region ����֧��
    /// <summary>
    /// �����֧��
    /// </summary>
    [JsonPropertyName("managementFeePay")]
    public string ManagementFeePay { get; set; }

    /// <summary>
    /// Ͷ�˷�֧��
    /// </summary>
    [JsonPropertyName("investmentFeePay")]
    public string InvestmentFeePay { get; set; }

    /// <summary>
    /// ���۷����֧��
    /// </summary>
    [JsonPropertyName("salesFeePay")]
    public string SalesFeePay { get; set; }

    /// <summary>
    /// �йܷ�֧��
    /// </summary>
    [JsonPropertyName("custodianFeePay")]
    public string CustodianFeePay { get; set; }

    /// <summary>
    /// ��������֧��
    /// </summary>
    [JsonPropertyName("outsourcingFeePay")]
    public string OutsourcingFeePay { get; set; }

    /// <summary>
    /// ҵ������֧��
    /// </summary>
    [JsonPropertyName("rewardPay")]
    public string RewardPay { get; set; }

    /// <summary>
    /// ��ֵ˰������˰֧��
    /// </summary>
    [JsonPropertyName("addedSurTaxPay")]
    public string AddedSurTaxPay { get; set; }
    #endregion

    #region �������
    /// <summary>
    /// ��������
    /// </summary>
    [JsonPropertyName("managementFeeBalance")]
    public string ManagementFeeBalance { get; set; }

    /// <summary>
    /// Ͷ�˷����
    /// </summary>
    [JsonPropertyName("investmentFeeBalance")]
    public string InvestmentFeeBalance { get; set; }

    /// <summary>
    /// ���۷�������
    /// </summary>
    [JsonPropertyName("salesFeeBalance")]
    public string SalesFeeBalance { get; set; }

    /// <summary>
    /// �йܷ����
    /// </summary>
    [JsonPropertyName("custodianFeeBalance")]
    public string CustodianFeeBalance { get; set; }

    /// <summary>
    /// �����������
    /// </summary>
    [JsonPropertyName("outsourcingFeeBalance")]
    public string OutsourcingFeeBalance { get; set; }

    /// <summary>
    /// ҵ���������
    /// </summary>
    [JsonPropertyName("rewardBalance")]
    public string RewardBalance { get; set; }

    /// <summary>
    /// ��ֵ˰������˰���
    /// </summary>
    [JsonPropertyName("addedSurTaxBalance")]
    public string AddedSurTaxBalance { get; set; }
    #endregion


    public FundDailyFee ToObject()
    {


        return new FundDailyFee
        {
            FundCode = FundCode,
            Date = DateOnly.ParseExact(ConfDate, "yyyyMMdd"),

            ManagerFeeAccrued = ParseDecimal(ManagementFee),
            ManagerFeePaid = ParseDecimal(ManagementFeePay),
            ManagerFeeBalance = ParseDecimal(ManagementFeeBalance),
            PerformanceFeeAccrued = ParseDecimal(Reward),
            PerformanceFeePaid = ParseDecimal(RewardPay),
            PerformanceFeeBalance = ParseDecimal(RewardBalance),
            CustodianFeeAccrued = ParseDecimal(CustodianFee),
            CustodianFeePaid = ParseDecimal(CustodianFeePay),
            CustodianFeeBalance = ParseDecimal(CustodianFeeBalance),
            ConsultantFeeAccrued = ParseDecimal(InvestmentFee),
            ConsultantFeePaid = ParseDecimal(InvestmentFeePay),
            ConsultantFeeBalance = ParseDecimal(InvestmentFeeBalance),
            SalesFeeAccrued = ParseDecimal(SalesFee),
            SalesFeePaid = ParseDecimal(SalesFeePay),
            SalesFeeBalance = ParseDecimal(SalesFeeBalance),
            OutsourcingFeeAccrued = ParseDecimal(OutsourcingFee),
            OutsourcingFeePaid = ParseDecimal(OutsourcingFeePay),
            OutsourcingFeeBalance = ParseDecimal(OutsourcingFeeBalance),
        };
    }

}


#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��