using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee.JsonCITICS;

#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

/// <summary>
/// ���ڲ�Ʒ��������ģ��
/// </summary>
internal class FundDailyFeeJson : JsonBase
{
    /// <summary>
    /// ��Ʒ����
    /// </summary>
    [JsonPropertyName("fcpdm")]
    public string ProductCode { get; set; }

    /// <summary>
    /// �ּ���Ʒ����
    /// </summary>
    [JsonPropertyName("ffjdm")]
    public string? ClassificationCode { get; set; }

    /// <summary>
    /// ��Ʒ����
    /// </summary>
    [JsonPropertyName("fcpmc")]
    public string ProductName { get; set; }

    /// <summary>
    /// ҵ�����ڣ���ʽ��YYYY - MM - DD
    /// </summary>
    [JsonPropertyName("cdate")]
    public string BusinessDate { get; set; }

    #region ��������
    /// <summary>
    /// ��������
    /// </summary>
    [JsonPropertyName("jtglf")]
    public string AccruedManagementFee { get; set; }

    /// <summary>
    /// ֧�������
    /// </summary>
    [JsonPropertyName("zfglf")]
    public string PaidManagementFee { get; set; }

    /// <summary>
    /// δ֧�������
    /// </summary>
    [JsonPropertyName("wzfglf")]
    public string UnpaidManagementFee { get; set; }
    #endregion

    #region ҵ���������
    /// <summary>
    /// ����ҵ������
    /// </summary>
    [JsonPropertyName("jtyjbc")]
    public string AccruedPerformanceFee { get; set; }

    /// <summary>
    /// ֧��ҵ������
    /// </summary>
    [JsonPropertyName("zfyjbc")]
    public string PaidPerformanceFee { get; set; }

    /// <summary>
    /// δ֧��ҵ������
    /// </summary>
    [JsonPropertyName("wzfyjbc")]
    public string UnpaidPerformanceFee { get; set; }
    #endregion

    #region �йܷ����
    /// <summary>
    /// �����йܷ�
    /// </summary>
    [JsonPropertyName("jttgf")]
    public string AccruedCustodyFee { get; set; }

    /// <summary>
    /// ֧���йܷ�
    /// </summary>
    [JsonPropertyName("zftgf")]
    public string PaidCustodyFee { get; set; }

    /// <summary>
    /// δ֧���йܷ�
    /// </summary>
    [JsonPropertyName("wzftgf")]
    public string UnpaidCustodyFee { get; set; }
    #endregion

    #region ������������
    /// <summary>
    /// �������������
    /// </summary>
    [JsonPropertyName("jtxzfwf")]
    public string AccruedAdministrativeFee { get; set; }

    /// <summary>
    /// ֧�����������
    /// </summary>
    [JsonPropertyName("zfxzfwf")]
    public string PaidAdministrativeFee { get; set; }

    /// <summary>
    /// δ֧�����������
    /// </summary>
    [JsonPropertyName("wzfxzfwf")]
    public string UnpaidAdministrativeFee { get; set; }
    #endregion

    #region ���۷�������
    /// <summary>
    /// �������۷����
    /// </summary>
    [JsonPropertyName("jtxsfwf")]
    public string AccruedSalesServiceFee { get; set; }

    /// <summary>
    /// ֧�����۷����
    /// </summary>
    [JsonPropertyName("zfxsfwf")]
    public string PaidSalesServiceFee { get; set; }

    /// <summary>
    /// δ֧�����۷����
    /// </summary>
    [JsonPropertyName("wzfxsfwf")]
    public string UnpaidSalesServiceFee { get; set; }
    #endregion

    #region Ͷ�ʹ��ʷ����
    /// <summary>
    /// ����Ͷ�ʹ��ʷ�
    /// </summary>
    [JsonPropertyName("jttzgwf")]
    public string AccruedInvestmentAdvisoryFee { get; set; }

    /// <summary>
    /// ֧��Ͷ�ʹ��ʷ�
    /// </summary>
    [JsonPropertyName("zftzgwf")]
    public string PaidInvestmentAdvisoryFee { get; set; }

    /// <summary>
    /// δ֧��Ͷ�ʹ��ʷ�
    /// </summary>
    [JsonPropertyName("wzftzgwf")]
    public string UnpaidInvestmentAdvisoryFee { get; set; }
    #endregion

    #region ��ֵ˰����˰���
    /// <summary>
    /// ������ֵ˰����˰
    /// </summary>
    [JsonPropertyName("jtzzsfjs")]
    public string AccruedVatSurcharge { get; set; }

    /// <summary>
    /// ֧����ֵ˰����˰
    /// </summary>
    [JsonPropertyName("zfzzsfjs")]
    public string PaidVatSurcharge { get; set; }

    /// <summary>
    /// δ֧����ֵ˰����˰
    /// </summary>
    [JsonPropertyName("wzfzzsfjs")]
    public string UnpaidVatSurcharge { get; set; }
    #endregion

    #region ��Ʒ����
    /// <summary>
    /// ������Ʒ�
    /// </summary>
    [JsonPropertyName("jtsjf")]
    public string AccruedAuditFee { get; set; }

    /// <summary>
    /// ֧����Ʒ�
    /// </summary>
    [JsonPropertyName("zfsjf")]
    public string PaidAuditFee { get; set; }

    /// <summary>
    /// δ֧����Ʒ�
    /// </summary>
    [JsonPropertyName("wzfsjf")]
    public string UnpaidAuditFee { get; set; }
    #endregion


    public FundDailyFee ToObject()
    {
        var code = ClassificationCode switch { "M" => ProductCode, null => ProductCode, var n => n };



        return new FundDailyFee
        {
            FundCode = code,
            Date = DateOnly.ParseExact(BusinessDate, "yyyy-MM-dd"),
            ManagerFeeAccrued = ParseDecimal(AccruedManagementFee),
            ManagerFeePaid = ParseDecimal(PaidManagementFee),
            ManagerFeeBalance = ParseDecimal(UnpaidManagementFee),
            PerformanceFeeAccrued = ParseDecimal(AccruedPerformanceFee),
            PerformanceFeePaid = ParseDecimal(PaidPerformanceFee),
            PerformanceFeeBalance = ParseDecimal(UnpaidPerformanceFee),
            CustodianFeeAccrued = ParseDecimal(AccruedCustodyFee),
            ConsultantFeePaid = ParseDecimal(PaidCustodyFee),
            ConsultantFeeBalance = ParseDecimal(UnpaidCustodyFee),
            OutsourcingFeeAccrued = ParseDecimal(AccruedAdministrativeFee),
            OutsourcingFeePaid = ParseDecimal(PaidAdministrativeFee),
            OutsourcingFeeBalance = ParseDecimal(UnpaidAdministrativeFee),
            SalesFeeAccrued = ParseDecimal(AccruedSalesServiceFee),
            SalesFeePaid = ParseDecimal(PaidSalesServiceFee),
            SalesFeeBalance = ParseDecimal(UnpaidSalesServiceFee),
            ConsultantFeeAccrued = ParseDecimal(AccruedInvestmentAdvisoryFee),
            CustodianFeePaid = ParseDecimal(PaidInvestmentAdvisoryFee),
            CustodianFeeBalance = ParseDecimal(UnpaidInvestmentAdvisoryFee),
        };
    }

}

#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��