using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee.JsonCSC;


#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

public class TransferRequestJson : JsonBase
{
    /// <summary>
    /// �������ڣ���ʽ��yyyyMMdd��
    /// </summary>
    [JsonPropertyName("applyDate")]
    public string ApplyDate { get; set; }

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

    /// <summary>
    /// �����̴���
    /// </summary>
    [JsonPropertyName("agencyNo")]
    public string AgencyNo { get; set; }

    /// <summary>
    /// ����������
    /// </summary>
    [JsonPropertyName("agencyName")]
    public string AgencyName { get; set; }

    /// <summary>
    /// Ͷ��������
    /// </summary>
    [JsonPropertyName("custName")]
    public string CustName { get; set; }

    /// <summary>
    /// �����˺�
    /// </summary>
    [JsonPropertyName("fundAcco")]
    public string FundAcco { get; set; }

    /// <summary>
    /// �����˺�
    /// </summary>
    [JsonPropertyName("tradeAcco")]
    public string TradeAcco { get; set; }

    /// <summary>
    /// �����˺�
    /// </summary>
    [JsonPropertyName("bankAcco")]
    public string BankAcco { get; set; }

    /// <summary>
    /// ���б��
    /// </summary>
    [JsonPropertyName("bankNo")]
    public string BankNo { get; set; }

    /// <summary>
    /// ����������
    /// </summary>
    [JsonPropertyName("openBankName")]
    public string OpenBankName { get; set; }

    /// <summary>
    /// ���л���
    /// </summary>
    [JsonPropertyName("nameInBank")]
    public string NameInBank { get; set; }

    /// <summary>
    /// �ͻ�����
    /// </summary>
    [JsonPropertyName("custType")]
    public string CustType { get; set; }

    /// <summary>
    /// ֤������
    /// </summary>
    [JsonPropertyName("certiType")]
    public string CertiType { get; set; }

    /// <summary>
    /// ֤����
    /// </summary>
    [JsonPropertyName("certiNo")]
    public string CertiNo { get; set; }

    /// <summary>
    /// TA�����־
    /// </summary>
    [JsonPropertyName("taFlag")]
    public string TaFlag { get; set; }

    /// <summary>
    /// �ֺ췽ʽ
    /// </summary>
    [JsonPropertyName("bonusType")]
    public string BonusType { get; set; }

    /// <summary>
    /// ������
    /// </summary>
    [JsonPropertyName("balance")]
    public string Balance { get; set; }

    /// <summary>
    /// ����ݶ�
    /// </summary>
    [JsonPropertyName("shares")]
    public string Shares { get; set; }

    /// <summary>
    /// ȷ�Ͻ��
    /// </summary>
    [JsonPropertyName("confBalance")]
    public string ConfBalance { get; set; }

    /// <summary>
    /// �������ۿ���
    /// </summary>
    [JsonPropertyName("discountRate")]
    public string DiscountRate { get; set; }

    /// <summary>
    /// ����ҵ������
    /// </summary>
    [JsonPropertyName("appBusiFlag")]
    public string AppBusiFlag { get; set; }

    /// <summary>
    /// ԭ�ⲿϵͳ��������ˮ��
    /// </summary>
    [JsonPropertyName("originalNo")]
    public string OriginalNo { get; set; }

    /// <summary>
    /// ȷ��״̬
    /// </summary>
    [JsonPropertyName("confStatus")]
    public string ConfStatus { get; set; }

    /// <summary>
    /// ȷ�Ͻ������
    /// </summary>
    [JsonPropertyName("describe")]
    public string Describe { get; set; }

    public override string? JsonId => $"{CSC._Identifier}.{OriginalNo}";

    public TransferRequest ToObject()
    {
        return new TransferRequest
        {
            InvestorIdentity = CertiNo,
            InvestorName = CustName,
            FundName = FundName,
            FundCode = FundCode,
            Agency = AgencyName,
            CreateDate = DateOnly.FromDateTime(DateTime.Now),
            ExternalId = $"{CSC._Identifier}.{OriginalNo}", 
            RequestDate = DateOnly.ParseExact(ApplyDate, "yyyyMMdd"),
            RequestAmount = ParseDecimal(Balance),
            RequestShare = ParseDecimal(Shares),
            FeeDiscount = ParseDecimal(DiscountRate),
            Source = "api",
            RequestType = ParseType(AppBusiFlag),
            // LargeRedemptionFlag = TaFlag
        };
    }

    private TransferRequestType ParseType(string requestTypeStr)
    {
        // ע�Ͳ����ĵ���
        var t = requestTypeStr switch
        {
            "20" => TransferRequestType.Subscription,
            "22" => TransferRequestType.Purchase,
            "24" => TransferRequestType.Redemption,
            "25" => TransferRequestType.Redemption,
            //"26" => TransferRequestType.Transfer,
            "27" => TransferRequestType.TransferIn,
            "28" => TransferRequestType.TransferOut,
            "29" => TransferRequestType.BonusType,
            //"30" => TransferRequestType.Subscription, �Ϲ������ʵ��û�з��أ�����20�Ƕ�Ӧrecord
            //"31" => TransferRequestType.Freeze,
            //"32" => TransferRequestType.Unfreeze,
            "33" => TransferRequestType.TransferOut,
            "34" => TransferRequestType.TransferIn,
            //"35" => TransferRequestType.NonTradeTransferOut,
            "36" => TransferRequestType.SwitchOut,
            "37" => TransferRequestType.SwitchIn,
            //"39" => TransferRequestType.SIP,
            "42" => TransferRequestType.ForceRedemption,
            //"43" => TransferRequestType.DividendPayout,
            //"44" => TransferRequestType.ForceIncrease,
            //"45" => TransferRequestType.ForceDecrease,
            "49" => TransferRequestType.Clear,
            "50" => TransferRequestType.Clear,
            //"59" => TransferRequestType.SIPStart,
            //"60" => TransferRequestType.SIPCancel,
            //"61" => TransferRequestType.SIPUpdate,
            //"63" => TransferRequestType.PeriodicRedemption,
            //"97" => TransferRequestType.QuickTransferIn,
            //"98" => TransferRequestType.QuickTransfer,
            _ => TransferRequestType.UNK
        };

        if (t == TransferRequestType.UNK)
            JsonBase.ReportSpecialType(new(0, CSC._Identifier, nameof(TransferRequest), OriginalNo, requestTypeStr));

        return t;
    }
}


#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��