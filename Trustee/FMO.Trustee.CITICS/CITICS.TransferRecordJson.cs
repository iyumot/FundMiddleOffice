using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;

#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

internal class TransferRecordJson : JsonBase
{
    [JsonPropertyName("ackNo")]
    public string AckNo { get; set; }

    [JsonPropertyName("exRequestNo")]
    public string ExRequestNo { get; set; }

    [JsonPropertyName("fundAcco")]
    public string FundAcco { get; set; }

    [JsonPropertyName("tradeAcco")]
    public string TradeAcco { get; set; }

    /// <summary>
    /// ҵ�����ͣ�
    ///01-�Ϲ�	50-�������
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
    [JsonPropertyName("apkind")]
    public string Apkind { get; set; } // ҵ�����ʹ���

    [JsonPropertyName("taFlag")]
    public string TaFlag { get; set; } // TA�����־��0-��1-��

    [JsonPropertyName("fundCode")]
    public string FundCode { get; set; }

    [JsonPropertyName("shareLevel")]
    public string ShareLevel { get; set; } // �ݶ����A-ǰ�շѣ�B-���շ�

    [JsonPropertyName("currency")]
    public string Currency { get; set; } // ���֣�����ҡ���Ԫ �� null

    [JsonPropertyName("largeRedemptionFlag")]
    public string LargeRedemptionFlag { get; set; } // �޶���ش����־��0-ȡ����1-˳��

    [JsonPropertyName("subAmt")]
    public string SubAmt { get; set; } // ������

    [JsonPropertyName("subQuty")]
    public string SubQuty { get; set; } // ����ݶ�

    [JsonPropertyName("bonusType")]
    public string BonusType { get; set; } // �ֺ췽ʽ��0-������Ͷ�ʣ�1-�ֽ����

    [JsonPropertyName("nav")]
    public string Nav { get; set; } // ��λ��ֵ

    [JsonPropertyName("ackAmt")]
    public string AckAmt { get; set; } // ȷ�Ͻ��

    [JsonPropertyName("ackQuty")]
    public string AckQuty { get; set; } // ȷ�Ϸݶ�

    [JsonPropertyName("tradeFee")]
    public string TradeFee { get; set; } // ���׷�

    [JsonPropertyName("taFee")]
    public string TaFee { get; set; } // ������

    [JsonPropertyName("backFee")]
    public string BackFee { get; set; } // ���շ�

    [JsonPropertyName("realBalance")]
    public string RealBalance { get; set; } // Ӧ�������

    [JsonPropertyName("profitBalance")]
    public string ProfitBalance { get; set; } // ҵ������

    [JsonPropertyName("profitBalanceForAgency")]
    public string ProfitBalanceForAgency { get; set; } // ҵ�������������

    [JsonPropertyName("totalNav")]
    public string TotalNav { get; set; } // �ۼƾ�ֵ

    [JsonPropertyName("totalFee")]
    public string TotalFee { get; set; } // �ܷ���

    [JsonPropertyName("agencyFee")]
    public string AgencyFee { get; set; } // �����ۻ�������

    [JsonPropertyName("fundFee")]
    public string FundFee { get; set; } // ������ʲ�����

    [JsonPropertyName("registFee")]
    public string RegistFee { get; set; } // ������˷���

    [JsonPropertyName("interest")]
    public string Interest { get; set; } // ��Ϣ

    [JsonPropertyName("interestTax")]
    public string InterestTax { get; set; } // ��Ϣ˰

    [JsonPropertyName("interestShare")]
    public string InterestShare { get; set; } // ��Ϣת�ݶ�

    [JsonPropertyName("frozenBalance")]
    public string FrozenBalance { get; set; } // ȷ�϶���ݶ�

    [JsonPropertyName("unfrozenBalance")]
    public string UnfrozenBalance { get; set; } // ȷ�Ͻⶳ�ݶ�

    [JsonPropertyName("unShares")]
    public string UnShares { get; set; } // �޶����˳�ӷݶ�

    [JsonPropertyName("applyDate")]
    public string ApplyDate { get; set; } // ���빤����

    [JsonPropertyName("ackDate")]
    public string AckDate { get; set; } // ȷ������

    /// <summary>
    /// ȷ��״̬:
    ///0-δ����	9-�ӻ�����
    ///1-ȷ�ϳɹ� a-δ����
    ///2-ȷ��ʧ�� b-δ����
    ///3-���׳��� c-δ����
    ///4-���ȷ�� d-δ����
    ///5-��ʷ�� e-δ����
    ///6-������� f-δ����
    ///7-�޶�������� g-δ����
    ///8-��ʱ����
    /// </summary>
    [JsonPropertyName("ackStatus")]
    public string AckStatus { get; set; } // ȷ��״̬

    [JsonPropertyName("agencyNo")]
    public string AgencyNo { get; set; } // �����̴���

    [JsonPropertyName("agencyName")]
    public string AgencyName { get; set; } // ����������

    [JsonPropertyName("retCod")]
    public string RetCod { get; set; } // ������

    [JsonPropertyName("retMsg")]
    public string RetMsg { get; set; } // ʧ��ԭ��

    [JsonPropertyName("adjustCause")]
    public string AdjustCause { get; set; } // �ݶ����ԭ��

    [JsonPropertyName("navDate")]
    public string NavDate { get; set; } // ��ֵ���� (YYYYMMDD)

    [JsonPropertyName("oriCserialNo")]
    public string OriCserialNo { get; set; } // ԭȷ�ϵ���

    public override string? JsonId => $"{CITICS._Identifier}.{AckNo}";

    /// <summary>
    /// ȱ��Customer ��Ϣ
    /// Fund ��Ϣ
    /// </summary>
    /// <returns></returns>
    internal TransferRecord ToObject()
    {
        var r = new TransferRecord
        {
            ExternalId = $"{CITICS._Identifier}.{AckNo}", 
            ExternalRequestId = $"{CITICS._Identifier}.{ExRequestNo}",
            Type = ParseRecordType(Apkind),
            FundCode = FundCode,
            FundName = "unset",
            Agency = AgencyName,
            RequestDate = DateOnly.ParseExact(ApplyDate, "yyyyMMdd"),
            RequestAmount = ParseDecimal(SubAmt),
            RequestShare = ParseDecimal(SubQuty),
            ConfirmedDate = DateOnly.ParseExact(AckDate, "yyyyMMdd"),
            ConfirmedAmount = ParseDecimal(AckAmt),
            ConfirmedShare = ParseDecimal(AckQuty),
            Fee = ParseDecimal(TotalFee),
            PerformanceFee = ParseDecimal(ProfitBalance),
            InvestorIdentity = "unset",
            InvestorName = "unset",
            CreateDate = DateOnly.FromDateTime(DateTime.Now),
            ConfirmedNetAmount = ParseDecimal(RealBalance),
            Source = AckStatus switch { "2" or "3" or "5" => "failed", _ => "api" },
        };
        r.Fee -= r.PerformanceFee;
        r.ConfirmedNetAmount = r.ConfirmedAmount - r.Fee;

        if (r.Type == TransferRecordType.UNK)
            JsonBase.ReportSpecialType(new(0, CITICS._Identifier, nameof(TransferRecord), AckNo, Apkind));
        return r;
    }



    private static TransferRecordType ParseRecordType(string businessTypeCode)
    {
        return businessTypeCode switch
        {
            "01" => TransferRecordType.InitialOffer,         // �Ϲ� ���� ȷ�Ϸݶ���0
            "02" => TransferRecordType.Purchase,             // �깺
            "03" => TransferRecordType.Redemption,           // ���
            "04" => TransferRecordType.MoveOut,              // ת�йܣ�ת����
            "05" => TransferRecordType.MoveIn,               // �й�ת�루ת�룩
            "06" => TransferRecordType.MoveOut,              // �й�ת����ת����
            "07" => TransferRecordType.BonusType,            // �޸ķֺ췽ʽ
            "10" => TransferRecordType.Frozen,               // �ݶ��
            "11" => TransferRecordType.Thawed,               // �ݶ�ⶳ
            "12" => TransferRecordType.TransferIn,           // �ǽ��׹�����ת�룩
            "13" => TransferRecordType.SwitchOut,            // ����ת��(��)
            "14" => TransferRecordType.TransferOut,          // �ǽ��׹�������ת����
            "15" => TransferRecordType.TransferIn,           // �ǽ��׹����루ת�룩
            "16" => TransferRecordType.SwitchIn,             // ����ת����
            "17" => TransferRecordType.UNK,                  // �������޶�ӦTA��¼���ͣ�
            "20" => TransferRecordType.TransferOut,          // �ڲ�ת�йܣ�ת����
            "21" => TransferRecordType.TransferIn,           // �ڲ�ת�й��루ת�룩
            "22" => TransferRecordType.TransferOut,          // �ڲ�ת�йܳ���ת����
            "31" => TransferRecordType.UNK,                  // ���������˺ţ�����ȷ��Ӧ��
            "32" => TransferRecordType.UNK,                  // ��������˺ţ�����ȷ��Ӧ��
            "41" => TransferRecordType.Purchase,             // ���루��ͬ���깺��
            "42" => TransferRecordType.Redemption,           // ��������ͬ����أ�
            "46" => TransferRecordType.UNK,                  // ��������ѯ���޶�Ӧ��
            "47" => TransferRecordType.UNK,                  // ��������ͨ���޶�Ӧ��
            "48" => TransferRecordType.UNK,                  // ������ȡ�����޶�Ӧ��
            "49" => TransferRecordType.UNK,                  // ����������޶�Ӧ��
            "50" => TransferRecordType.Subscription,                  // ���������ͨ�����ǵ��ʼ�¼�� �������Ϲ�
            "51" => TransferRecordType.UNK,                  // ������ֹ��ͨ�����ǵ��ʼ�¼��
            "52" => TransferRecordType.Clear,                // ��������
            "54" => TransferRecordType.RaisingFailed,        // ����ʧ��
            "70" => TransferRecordType.Increase,             // ǿ�Ƶ���
            "71" => TransferRecordType.Decrease,             // ǿ�Ƶ���
            "74" => TransferRecordType.Distribution,         // ��������
            "77" => TransferRecordType.UNK,                  // �����깺�޸ģ�����ȷ��Ӧ��
            "78" => TransferRecordType.UNK,                  // ��������޸ģ�����ȷ��Ӧ��
            "81" => TransferRecordType.UNK,                  // ���𿪻����ǽ��׼�¼��
            "82" => TransferRecordType.UNK,                  // �����������ǽ��׼�¼��
            "83" => TransferRecordType.UNK,                  // �˻��޸ģ��ǽ��׼�¼��
            "84" => TransferRecordType.Frozen,               // �˻����ᣨ��ݶ�����ƣ�
            "85" => TransferRecordType.Thawed,               // �˻��ⶳ����ݶ�ⶳ���ƣ�
            "88" => TransferRecordType.UNK,                  // �˻��Ǽǣ��ǽ��׼�¼��
            "89" => TransferRecordType.UNK,                  // ȡ���Ǽǣ��ǽ��׼�¼��
            "90" => TransferRecordType.UNK,                  // �����깺Э�飨�ǽ��׼�¼��
            "91" => TransferRecordType.UNK,                  // �������Э�飨�ǽ��׼�¼��
            "92" => TransferRecordType.UNK,                  // ��ŵ�Ż�Э�飨�ǽ��׼�¼��
            "93" => TransferRecordType.UNK,                  // �����깺ȡ�����ǽ��׼�¼��
            "94" => TransferRecordType.UNK,                  // �������ȡ�����ǽ��׼�¼��
            "96" => TransferRecordType.UNK,                  // �������˺ţ��ǽ��׼�¼��
            "97" => TransferRecordType.TransferOut,          // �ڲ�ת�йܣ�ת����
            "98" => TransferRecordType.TransferIn,           // �ڲ��й�ת�루ת�룩
            "99" => TransferRecordType.TransferOut,          // �ڲ��й�ת����ת����

            _ => TransferRecordType.UNK
        };

    }


}

#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��