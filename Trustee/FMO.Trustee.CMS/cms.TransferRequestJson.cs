using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;


#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

internal class TransferRequestJson : JsonBase
{
    /// <summary>
    /// �ͻ����ƣ���󳤶ȣ�200��
    /// </summary>
    [JsonPropertyName("custName")]
    public string CustomerName { get; set; }

    /// <summary>
    /// �ͻ����ͣ���󳤶ȣ�30��
    /// </summary>
    [JsonPropertyName("custType")]
    public string CustomerType { get; set; }

    /// <summary>
    /// ֤�����ͣ���󳤶ȣ�50��
    /// </summary>
    [JsonPropertyName("certificateType")]
    public string CertificateType { get; set; }

    /// <summary>
    /// ֤�����루��󳤶ȣ�30��
    /// </summary>
    [JsonPropertyName("certificateNo")]
    public string CertificateNumber { get; set; }

    /// <summary>
    /// �����˺ţ���󳤶ȣ�20��
    /// </summary>
    [JsonPropertyName("taAccountId")]
    public string TaAccountId { get; set; }

    /// <summary>
    /// �����˺ţ���󳤶ȣ�30��
    /// </summary>
    [JsonPropertyName("transactionAccountId")]
    public string TransactionAccountId { get; set; }

    /// <summary>
    /// ��Ʒ���ƣ���󳤶ȣ�300��
    /// </summary>
    [JsonPropertyName("fundName")]
    public string FundName { get; set; }

    /// <summary>
    /// ��Ʒ���루��󳤶ȣ�6��
    /// </summary>
    [JsonPropertyName("fundCode")]
    public string FundCode { get; set; }

    /// <summary>
    /// ҵ�����ͣ���󳤶ȣ�6��
    /// </summary>
    [JsonPropertyName("businessCode")]
    public string BusinessCode { get; set; }

    /// <summary>
    /// �����������λС��
    /// </summary>
    [JsonPropertyName("applicationAmount")]
    public string ApplicationAmount { get; set; }

    /// <summary>
    /// ����ݶ������λС��
    /// </summary>
    [JsonPropertyName("applicationVol")]
    public string ApplicationVol { get; set; }

    /// <summary>
    /// �������ڣ���ʽ��yyyymmdd
    /// </summary>
    [JsonPropertyName("transactionDate")]
    public string TransactionDate { get; set; }

    /// <summary>
    /// �������ۿ��ʣ�������λС��
    /// </summary>
    [JsonPropertyName("discountRateOfCommission")]
    public string DiscountRateOfCommission { get; set; }

    /// <summary>
    /// �����������루��󳤶ȣ�3��
    /// </summary>
    [JsonPropertyName("distributorCode")]
    public string DistributorCode { get; set; }

    /// <summary>
    /// �����������ƣ���󳤶ȣ�300��
    /// </summary>
    [JsonPropertyName("distributorName")]
    public string DistributorName { get; set; }

    /// <summary>
    /// ������ˮ�ţ���󳤶ȣ�500��
    /// </summary>
    [JsonPropertyName("remark1")]
    public string Remark1 { get; set; }

    /// <summary>
    /// Ԥ���ֶ�2����󳤶ȣ�500��
    /// </summary>
    [JsonPropertyName("remark2")]
    public string Remark2 { get; set; }

    /// <summary>
    /// ԤԼ�깺���ڣ���ʽ��yyyymmdd����Ϊ�գ�
    /// </summary>
    [JsonPropertyName("futureBuyDate")]
    public string FutureBuyDate { get; set; }

    /// <summary>
    /// ԤԼ������ڣ���ʽ��yyyymmdd����Ϊ�գ�
    /// </summary>
    [JsonPropertyName("redemptionDateInAdvance")]
    public string RedemptionDateInAdvance { get; set; }


    public TransferRequest ToObject()
    {
        TransferRequestType transferRequestType = TranslateRequest(BusinessCode);
        if (transferRequestType == TransferRequestType.UNK && BusinessCode switch { "120" => false, _ => true })
            ReportJsonUnexpected(CMS._Identifier, nameof(CMS.QueryTransferRequests), $"TA[{Remark1}] {TransactionDate} �ݶ{ApplicationVol} ��{ApplicationAmount} ��ҵ������[{BusinessCode}]�޷�ʶ��");


        var r = new TransferRequest
        {
            CustomerIdentity = CertificateNumber,
            CustomerName = CustomerName,
            FundName = FundName,
            FundCode = FundCode,
            RequestDate = DateOnly.ParseExact(TransactionDate, "yyyyMMdd"),
            RequestType = transferRequestType,
            RequestAmount = ParseDecimal(ApplicationAmount),
            RequestShare = ParseDecimal(ApplicationVol),
            Agency = DistributorName,
            FeeDiscount = ParseDecimal(DiscountRateOfCommission),
            ExternalId = $"{CMS._Identifier}.{Remark1}",
            Source = "api"
        };


        if (r.RequestType == TransferRequestType.UNK)
            JsonBase.ReportSpecialType(new(0, CMS._Identifier, nameof(TransferRequest), Remark1, BusinessCode));

        return r;
    }

    public static TransferRequestType TranslateRequest(string c)
    {
        return c switch
        {
            "122" => TransferRequestType.Purchase,
            "124" => TransferRequestType.Redemption,
            "129" => TransferRequestType.BonusType,//"�ֺ췽ʽ",
            "130" => TransferRequestType.Subscription,
            "142" => TransferRequestType.ForceRedemption,
            "143" => TransferRequestType.Distribution,     //"�ֺ�ȷ��",
            "144" => TransferRequestType.Increase,     //"ǿ�е���", �ݶ����͵���
            "145" => TransferRequestType.Decrease,     //"ǿ�е���",
            "152" => TransferRequestType.Abort,
            _ => TransferRequestType.UNK
        };
    }
}




#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��