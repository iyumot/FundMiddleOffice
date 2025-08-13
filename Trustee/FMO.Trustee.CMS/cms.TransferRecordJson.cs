using FMO.Models;
using System.Text.Json.Serialization;


namespace FMO.Trustee;

internal class TransferRecordJson : JsonBase
{

    [JsonPropertyName("custName")]
    public required string CustName { get; set; }

    [JsonPropertyName("custType")]
    public required string CustType { get; set; }

    [JsonPropertyName("certificateType")]
    public required string CertificateType { get; set; }

    [JsonPropertyName("certificateNo")]
    public required string CertificateNo { get; set; }

    [JsonPropertyName("taAccountId")]
    public required string TaAccountId { get; set; }

    [JsonPropertyName("transactionAccountId")]
    public required string TransactionAccountId { get; set; }

    [JsonPropertyName("fundName")]
    public required string FundName { get; set; }

    [JsonPropertyName("fundCode")]
    public required string FundCode { get; set; }

    [JsonPropertyName("businessCode")]
    public required string BusinessCode { get; set; }

    [JsonPropertyName("applicationAmount")]
    public required string ApplicationAmount { get; set; } // ������λС��

    [JsonPropertyName("applicationVol")]
    public required string ApplicationVol { get; set; } // ������λС��

    [JsonPropertyName("transactionDate")]
    public required string TransactionDate { get; set; } // ��ʽ��yyyymmdd

    [JsonPropertyName("nav")]
    public required string Nav { get; set; } // �����տɱ���8λС��

    [JsonPropertyName("transactionCfmDate")]
    public required string TransactionCfmDate { get; set; } // ��ʽ��yyyymmdd

    [JsonPropertyName("confirmedVol")]
    public required string ConfirmedVol { get; set; } // ������λС��

    [JsonPropertyName("confirmedAmount")]
    public required string ConfirmedAmount { get; set; } // ������λС��

    [JsonPropertyName("confirmedNavVol")]
    public required string ConfirmedNavVol { get; set; } // ������λС��

    [JsonPropertyName("charge")]
    public required string Charge { get; set; } // ������λС��

    [JsonPropertyName("performance")]
    public required string Performance { get; set; } // ������λС��

    [JsonPropertyName("distributorCode")]
    public required string DistributorCode { get; set; }

    [JsonPropertyName("distributorName")]
    public required string DistributorName { get; set; }

    [JsonPropertyName("remark1")]
    public required string Remark1 { get; set; }

    [JsonPropertyName("remark2")]
    public required string Remark2 { get; set; }

    [JsonPropertyName("note")]
    public required string Note { get; set; }

    [JsonPropertyName("origDefNo")]
    public required string OrigDefNo { get; set; }

    [JsonPropertyName("shareBonusType")]
    public required string ShareBonusType { get; set; }

    [JsonPropertyName("attributionManagerFee")]
    public required string AttributionManagerFee { get; set; } // ������λС��

    [JsonPropertyName("attributionFundAssetFee")]
    public required string AttributionFundAssetFee { get; set; } // ������λС��

    [JsonPropertyName("interest")]
    public required string Interest { get; set; } // ������λС��

    [JsonPropertyName("attributionSellAgencyFee")]
    public required string AttributionSellAgencyFee { get; set; } // ������λС��

    [JsonPropertyName("applyNo")]
    public required string ApplyNo { get; set; }

    public TransferRecord ToObject()
    {
        TransferRecordType transferRecordType = Translate(BusinessCode);
        if (transferRecordType == TransferRecordType.UNK && BusinessCode switch { "120" => false, _ => true })
            ReportJsonUnexpected(CMS._Identifier, nameof(CMS.QueryTransferRecords), $"TA[{ApplyNo}] {TransactionCfmDate} �ݶ{ConfirmedNavVol} ��{ConfirmedAmount} ��ҵ������[{BusinessCode}]�޷�ʶ��");

        var r = new TransferRecord
        {
            InvestorIdentity = CertificateNo,
            InvestorName = CustName,
            Agency = DistributorName,
            RequestDate = DateOnly.ParseExact(TransactionDate, "yyyyMMdd"),
            RequestAmount = ParseDecimal(ApplicationAmount),
            RequestShare = ParseDecimal(ApplicationVol),
            ConfirmedDate = DateOnly.ParseExact(TransactionCfmDate, "yyyyMMdd"),
            ConfirmedAmount = ParseDecimal(ConfirmedAmount),
            ConfirmedShare = ParseDecimal(ConfirmedVol),
            ConfirmedNetAmount = ParseDecimal(ConfirmedNavVol),
            CreateDate = DateOnly.FromDateTime(DateTime.Today),
            ExternalId = $"{CMS._Identifier}.{Remark1}",
            Type = transferRecordType,
            Fee = ParseDecimal(Charge),
            PerformanceFee = ParseDecimal(Performance),
            ExternalRequestId = $"{CMS._Identifier}.{Remark1}",//$"{CMS._Identifier}.{ApplyNo}",
            FundCode = FundCode,
            FundName = FundName,
            Source = "api",
        }; 

        if (r.Type == TransferRecordType.UNK)
            JsonBase.ReportSpecialType(new(0, CMS._Identifier, nameof(TransferRecord), Remark1, BusinessCode));

        return r;
    }



    public static TransferRecordType Translate(string c)
    {
        return c switch
        {
            "120" => TransferRecordType.InitialOffer, //"�Ϲ�ȷ��" ����û�зݶ�����, ���Ϲ����
            "122" => TransferRecordType.Purchase,     //"�깺ȷ��",
            "124" => TransferRecordType.Redemption,// "���ȷ��",
            "126" => TransferRecordType.MoveIn,   //"ת��ȷ��",
            "127" => TransferRecordType.MoveIn,   //"ת������/����ת��",
            "128" => TransferRecordType.MoveOut,  //"ת������/����ת��",
            "129" => TransferRecordType.BonusType,//"�ֺ췽ʽ",
            "130" => TransferRecordType.Subscription, // "�Ϲ����",
            "131" => TransferRecordType.Frozen,       //"�����������",
            "132" => TransferRecordType.Thawed,       //"��������ⶳ",
                                                      //"133" => TARecordType.TransferIn,   //"�ǽ��׹���",
            "134" => TransferRecordType.TransferIn,   //"�ǽ��׹���ת��",
            "135" => TransferRecordType.TransferOut,  //"�ǽ��׹���ת��",
                                                      //"136" => TARecordType.SwitchIn,     //"����ת��",
            "137" => TransferRecordType.SwitchIn,     //"����ת��ת��",
            "138" => TransferRecordType.SwitchOut,    //"����ת��ת��",


            "139" => TransferRecordType.Purchase,     //"��ʱ�����깺",
            "142" => TransferRecordType.ForceRedemption,//"ǿ�����",
            "143" => TransferRecordType.Distribution,     //"�ֺ�ȷ��",
            "144" => TransferRecordType.Increase,     //"ǿ�е���",
            "145" => TransferRecordType.Decrease,     //"ǿ�е���",
            _ => TransferRecordType.UNK,              //"δ֪ҵ������"
        };
    }
}
