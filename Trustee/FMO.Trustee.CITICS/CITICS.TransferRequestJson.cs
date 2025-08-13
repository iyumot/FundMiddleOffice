using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;

#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

internal class TransferRequestJson : JsonBase
{
    /// <summary>
    /// ������
    /// </summary>
    [JsonPropertyName("requestNo")]
    public string RequestNo { get; set; }

    /// <summary>
    /// �������ڣ���ʽ��YYYYMMDD��
    /// </summary>
    [JsonPropertyName("requestDate")]
    public string RequestDate { get; set; }

    /// <summary>
    /// ����ʱ�䣨��ʽ��HHMMSS��
    /// ����Ϊnull
    /// </summary>
    [JsonPropertyName("requestTime")]
    public string RequestTime { get; set; }

    /// <summary>
    /// �������
    /// </summary>
    [JsonPropertyName("fundCode")]
    public string FundCode { get; set; }

    /// <summary>
    /// ҵ������
    /// ���� BusinFlagEnum ö�ٶ���
    /// </summary>
    [JsonPropertyName("businFlag")]
    public string BusinFlag { get; set; }

    /// <summary>
    /// �����˺�
    /// </summary>
    [JsonPropertyName("tradeAcco")]
    public string TradeAccount { get; set; }

    /// <summary>
    /// �����ʺ�
    /// </summary>
    [JsonPropertyName("fundAcco")]
    public string FundAccount { get; set; }

    /// <summary>
    /// �����̴���
    /// </summary>
    [JsonPropertyName("agencyNo")]
    public string AgencyCode { get; set; }

    /// <summary>
    /// ����������
    /// </summary>
    [JsonPropertyName("agencyName")]
    public string AgencyName { get; set; }

    /// <summary>
    /// ������
    /// </summary>
    [JsonPropertyName("balance")]
    public decimal? ApplyAmount { get; set; }

    /// <summary>
    /// ����ݶ�
    /// </summary>
    [JsonPropertyName("shares")]
    public decimal? ApplyShares { get; set; }

    /// <summary>
    /// �����˺�
    /// </summary>
    [JsonPropertyName("bankAcco")]
    public string BankAccountNo { get; set; }

    /// <summary>
    /// ���п�����
    /// </summary>
    [JsonPropertyName("bankName")]
    public string BankName { get; set; }

    /// <summary>
    /// ���л���
    /// </summary>
    [JsonPropertyName("nameInBank")]
    public string BankAccountName { get; set; }

    /// <summary>
    /// �������
    /// </summary>
    [JsonPropertyName("netNo")]
    public string BranchCode { get; set; }

    /// <summary>
    /// ָ������
    /// </summary>
    [JsonPropertyName("definedFee")]
    public decimal? DefinedFee { get; set; }

    /// <summary>
    /// �ۿ�
    /// </summary>
    [JsonPropertyName("agio")]
    public string DiscountRate { get; set; }

    /// <summary>
    /// �޶���ط�ʽ��
    /// 0 - ȡ��
    /// 1 - ˳��
    /// null - Ĭ��˳��
    /// </summary>
    [JsonPropertyName("exceedFlag")]
    public string? ExceedFlag { get; set; }

    /// <summary>
    /// ���֣�����ң�Ĭ��ֵ��
    /// </summary>
    [JsonPropertyName("moneyTypeCn")]
    public string Currency { get; set; } = "�����";

    /// <summary>
    /// Ͷ��������
    /// </summary>
    [JsonPropertyName("custName")]
    public string InvestorName { get; set; }

    /// <summary>
    /// �ͻ����ͣ��ο���¼4��
    /// </summary>
    [JsonPropertyName("custType")]
    public string CustomerType { get; set; }

    /// <summary>
    /// ֤�����ͣ��ο���¼4��
    /// </summary>
    [JsonPropertyName("certiType")]
    public string CertificateType { get; set; }

    /// <summary>
    /// ֤����
    /// </summary>
    [JsonPropertyName("certiNo")]
    public string CertificateNumber { get; set; }


    public TransferRequest ToObject()
    {
        var r = new TransferRequest
        {
            InvestorIdentity = CertificateNumber,
            InvestorName = InvestorName,
            FundCode = FundCode,
            FundName = "unset",
            RequestDate = DateOnly.ParseExact(RequestDate, "yyyyMMdd"),
            RequestType = ParseRequestType(BusinFlag),
            ExternalId = $"{CITICS._Identifier}.{RequestNo}",
            Agency = AgencyName,
            FeeDiscount = ParseDecimal(DiscountRate),
            LargeRedemptionFlag = ExceedFlag switch { "0" or null => LargeRedemptionFlag.CancelRemaining, _ => LargeRedemptionFlag.RollOver },
            Source = "api",
            RequestAmount = ApplyAmount ?? 0,
            RequestShare = ApplyShares ?? 0,
            Fee = DefinedFee ?? 0,
            CreateDate = DateOnly.FromDateTime(DateTime.Now),
        };


        if (r.RequestType == TransferRequestType.UNK)
            JsonBase.ReportSpecialType(new(0, CITICS._Identifier, nameof(TransferRequest), RequestNo, BusinFlag));

        return r;
    }



    /// <summary>
    /// 01-�Ϲ�	50-�������
    ///02-�깺	51-������ֹ
    ///03-���	52-��������
    ///04-ת�й�	54-����ʧ��
    ///05-�й�ת��	70-ǿ�Ƶ���
    ///06-�й�ת��	71-ǿ�Ƶ���
    ///07-�޸ķֺ췽ʽ	74-��������
    ///10-�ݶ��	77-�����깺�޸�
    ///11-�ݶ�ⶳ	78-��������޸�
    ///12-�ǽ��׹���	81-���𿪻�
    ///13-����ת��(��)  82-��������
    ///14-�ǽ��׹�����	83-�˻��޸�
    ///15-�ǽ��׹�����	84-�˻�����
    ///16-����ת����	85-�˻��ⶳ
    ///17-����	88-�˻��Ǽ�
    ///20-�ڲ�ת�й�	89-ȡ���Ǽ�
    ///21-�ڲ�ת�й���	90-�����깺Э��
    ///22-�ڲ�ת�йܳ�	91-�������Э��
    ///31-���������˺�	92-��ŵ�Ż�Э��
    ///32-��������˺�	93-�����깺ȡ��
    ///41-����	94-�������ȡ��
    ///42-����	96-�������˺�
    ///46-��������ѯ	97-�ڲ�ת�й�
    ///47-��������ͨ	98-�ڲ��й�ת��
    ///48-������ȡ��	99-�ڲ��й�ת��
    ///49-����������
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static TransferRequestType ParseRequestType(string str)
    {
        switch (str)
        {
            case "01": return TransferRequestType.Subscription;
            //case "50": return TransferRequestType.Subscription; û�з���
            case "02": return TransferRequestType.Purchase;
            case "03": return TransferRequestType.Redemption;
            //case "04": return TransferRequestType.Subscription;
            case "07": return TransferRequestType.BonusType;
            case "05":
            case "70":
            case "15":
            case "16":
            case "21":
            case "41":
                return TransferRequestType.Increase;
            case "06":
            case "71":
            case "14":
            case "13":
            case "22":
            case "42":
                return TransferRequestType.Decrease;
            default: // �������ͺ���
                return TransferRequestType.UNK;
        }
    }
}


#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��