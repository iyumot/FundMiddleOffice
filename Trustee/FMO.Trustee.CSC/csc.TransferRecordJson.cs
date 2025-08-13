using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee.JsonCSC;


#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

internal class TransferRecordJson : JsonBase
{
    /// <summary>
    /// ����ȷ������ (��ʽ: YYYYMMDD, 8λ)
    /// </summary>
    [JsonPropertyName("confDate")]
    public string ConfDate { get; set; }

    /// <summary>
    /// �������� (��ʽ: YYYYMMDD, 8λ)
    /// </summary>
    [JsonPropertyName("applyDate")]
    public string ApplyDate { get; set; }

    /// <summary>
    /// ������� (32λ)
    /// </summary>
    [JsonPropertyName("fundCode")]
    public string FundCode { get; set; }

    /// <summary>
    /// �������� (250�ַ�)
    /// </summary>
    [JsonPropertyName("fundName")]
    public string FundName { get; set; }

    /// <summary>
    /// �����̴��� (6λ)
    /// </summary>
    [JsonPropertyName("agencyNo")]
    public string AgencyNo { get; set; }

    /// <summary>
    /// ���������� (64�ַ�)
    /// </summary>
    [JsonPropertyName("agencyName")]
    public string AgencyName { get; set; }

    /// <summary>
    /// Ͷ�������� (64�ַ�)
    /// </summary>
    [JsonPropertyName("custName")]
    public string CustName { get; set; }

    /// <summary>
    /// �����˺� (20�ַ�)
    /// </summary>
    [JsonPropertyName("fundAcco")]
    public string FundAcco { get; set; }

    /// <summary>
    /// �����˺� (16�ַ�)
    /// </summary>
    [JsonPropertyName("tradeAcco")]
    public string TradeAcco { get; set; }

    /// <summary>
    /// �����˺� (32�ַ�)
    /// </summary>
    [JsonPropertyName("bankAcco")]
    public string BankAcco { get; set; }

    /// <summary>
    /// ���б�� (6�ַ�)
    /// </summary>
    [JsonPropertyName("bankNo")]
    public string BankNo { get; set; }

    /// <summary>
    /// ���������� (250�ַ�)
    /// </summary>
    [JsonPropertyName("openBankName")]
    public string OpenBankName { get; set; }

    /// <summary>
    /// ���л��� (64�ַ�)
    /// </summary>
    [JsonPropertyName("nameInBank")]
    public string NameInBank { get; set; }

    /// <summary>
    /// �ͻ����� (6�ַ�)
    /// </summary>
    [JsonPropertyName("custType")]
    public string CustType { get; set; }

    /// <summary>
    /// ֤������ (3�ַ�)
    /// </summary>
    [JsonPropertyName("certiType")]
    public string CertiType { get; set; }

    /// <summary>
    /// ֤���� (32�ַ�)
    /// </summary>
    [JsonPropertyName("certiNo")]
    public string CertiNo { get; set; }

    /// <summary>
    /// TA�����־ (2�ַ�)
    /// </summary>
    [JsonPropertyName("taFlag")]
    public string TaFlag { get; set; }

    /// <summary>
    /// ȷ��״̬ (2�ַ�)
    /// </summary>
    [JsonPropertyName("confStatus")]
    public string ConfStatus { get; set; }

    /// <summary>
    /// ȷ�Ͻ������ (250�ַ�)
    /// </summary>
    [JsonPropertyName("describe")]
    public string Describe { get; set; }

    /// <summary>
    /// �ֺ췽ʽ (2�ַ�)
    /// </summary>
    [JsonPropertyName("bonusType")]
    public string BonusType { get; set; }

    /// <summary>
    /// ������ (20�ַ�)
    /// </summary>
    [JsonPropertyName("balance")]
    public string Balance { get; set; }

    /// <summary>
    /// ����ݶ� (20�ַ�)
    /// </summary>
    [JsonPropertyName("shares")]
    public string Shares { get; set; }

    /// <summary>
    /// ��λ��ֵ (10�ַ�)
    /// </summary>
    [JsonPropertyName("netValue")]
    public string NetValue { get; set; }

    /// <summary>
    /// ȷ�Ͻ�� (20�ַ�)
    /// </summary>
    [JsonPropertyName("confBalance")]
    public string ConfBalance { get; set; }

    /// <summary>
    /// ȷ�Ϸݶ� (20�ַ�)
    /// </summary>
    [JsonPropertyName("confShares")]
    public string ConfShares { get; set; }

    /// <summary>
    /// ������ (20�ַ�)
    /// </summary>
    [JsonPropertyName("charge")]
    public string Charge { get; set; }

    /// <summary>
    /// ������������� (20�ַ�)
    /// </summary>
    [JsonPropertyName("managerCharge")]
    public string ManagerCharge { get; set; }

    /// <summary>
    /// �������������� (20�ַ�)
    /// </summary>
    [JsonPropertyName("distributorCharge")]
    public string DistributorCharge { get; set; }

    /// <summary>
    /// ���Ʒ������ (20�ַ�)
    /// </summary>
    [JsonPropertyName("fundcharge")]
    public string Fundcharge { get; set; }

    /// <summary>
    /// ҵ������ (20�ַ�)
    /// </summary>
    [JsonPropertyName("achievementPay")]
    public string AchievementPay { get; set; }

    /// <summary>
    /// TAȷ�ϱ�� (32�ַ�)
    /// </summary>
    [JsonPropertyName("cserialNo")]
    public string CserialNo { get; set; }

    /// <summary>
    /// ȷ��ҵ������ (6�ַ�)
    /// </summary>
    [JsonPropertyName("busiFlag")]
    public string BusiFlag { get; set; }

    /// <summary>
    /// ԭ�ⲿϵͳ��������ˮ�� (32�ַ�)
    /// </summary>
    [JsonPropertyName("originalNo")]
    public string OriginalNo { get; set; }

    /// <summary>
    /// ������ʽ (1�ַ�)
    /// </summary>
    [JsonPropertyName("operWayNew")]
    public string OperWayNew { get; set; }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public TransferRecord ToObject()
    {
        var r = new TransferRecord
        {
            FundCode = FundCode,
            ConfirmedDate = DateOnly.ParseExact(ConfDate, "yyyyMMdd"),
            RequestDate = DateOnly.ParseExact(ApplyDate, "yyyyMMdd"),
            FundName = FundName,
            Agency = AgencyName,
            InvestorName = CustName,
            InvestorIdentity = CertiNo,
            RequestAmount = ParseDecimal(Balance),
            RequestShare = ParseDecimal(Shares),
            ConfirmedAmount = ParseDecimal(ConfBalance),
            ConfirmedShare = ParseDecimal(ConfShares),
            CreateDate = DateOnly.FromDateTime(DateTime.Today),
            ExternalId = $"{CSC._Identifier}.{CserialNo}",
            PerformanceFee = ParseDecimal(AchievementPay),
            Fee = ParseDecimal(Charge),
            ExternalRequestId = $"{CSC._Identifier}.{OriginalNo}",
            Type = ParseType(BusiFlag),
            Source = "api",
        };
        // ����
        r.ConfirmedNetAmount = r.ConfirmedAmount - r.Fee;

        if (r.Type == TransferRecordType.UNK)
            JsonBase.ReportSpecialType(new(0, CSC._Identifier, nameof(TransferRecord), CserialNo, BusiFlag));
        return r;
    }


    /// 120	�Ϲ�
    //122	�깺
    //124	���
    //126	ת���ۻ���
    //127	ת���ۻ�����
    //129	���÷ֺ췽ʽ
    //131	�ݶ��
    //132	�ݶ�ⶳ
    //133	ת��
    //134	����
    //136	�ݶ�ת��
    //137	�ݶ�ת����
    //142	ǿ�����
    //143	��������
    //144	ǿ�е���
    //145	ǿ�е���
    //149	ļ��ʧ��
    //150	��������
    private TransferRecordType ParseType(string str)
    {
        switch (str)
        {
            case "120": // �Ϲ� 
                return TransferRecordType.Subscription;
            case "122": // �깺 
                return TransferRecordType.Purchase;
            case "124": // ��� 
                return TransferRecordType.Redemption;
            case "126": // ת���ۻ��� 
                return TransferRecordType.MoveIn;
            case "127": // ת���ۻ����� 
                return TransferRecordType.MoveIn;
            case "129": // ���÷ֺ췽ʽ 
                return TransferRecordType.BonusType; 
            case "131": // �ݶ�� 
                return TransferRecordType.Frozen;
            case "132": // �ݶ�ⶳ 
                return TransferRecordType.Thawed;
            case "133": // ת�� 
                return TransferRecordType.TransferOut;
            case "134": // ���� 
                return TransferRecordType.TransferIn;
            case "136": // �ݶ�ת�� 
                return TransferRecordType.SwitchOut;
            case "137": // �ݶ�ת���� 
                return TransferRecordType.SwitchIn;

            case "142": // ǿ����� 
                return TransferRecordType.ForceRedemption;
            case "143": // �������� 
                return TransferRecordType.Distribution;
            case "144": // ǿ�е��� 
                return TransferRecordType.Increase;
            case "145": // ǿ�е��� 
                return TransferRecordType.Decrease;
            case "149": // ļ��ʧ�� 
                return TransferRecordType.RaisingFailed;
            case "150": // �������� 
                return TransferRecordType.Clear;

            default: 
                return TransferRecordType.UNK;
        }
    }

}

#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��