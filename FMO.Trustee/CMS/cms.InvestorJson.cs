using FMO.Models;
using System.Text.Json.Serialization;


namespace FMO.Trustee.JsonCMS;



#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

public class InvestorJson : JsonBase
{

    /// <summary>
    /// �ͻ����ƣ���󳤶�200
    /// </summary>
    [JsonPropertyName("custName")]
    public string CustName { get; set; }

    /// <summary>
    /// �ͻ����ͣ���󳤶�30
    /// </summary>
    [JsonPropertyName("custType")]
    public string CustType { get; set; }

    /// <summary>
    /// ֤�����ͣ���󳤶�50
    /// </summary>
    [JsonPropertyName("certificateType")]
    public string CertificateType { get; set; }

    /// <summary>
    /// ֤�����룬��󳤶�30
    /// </summary>
    [JsonPropertyName("certificateNo")]
    public string CertificateNo { get; set; }

    /// <summary>
    /// �����˺ţ���󳤶�20
    /// </summary>
    [JsonPropertyName("taAccountId")]
    public string TaAccountId { get; set; }

    /// <summary>
    /// �����˺ţ���󳤶�30
    /// </summary>
    [JsonPropertyName("transactionAccountId")]
    public string TransactionAccountId { get; set; }

    /// <summary>
    /// �����˻����ƣ���󳤶�300
    /// </summary>
    [JsonPropertyName("acctName")]
    public string AcctName { get; set; }

    /// <summary>
    /// �����˺ţ���󳤶�20
    /// </summary>
    [JsonPropertyName("acctNo")]
    public string AcctNo { get; set; }

    /// <summary>
    /// ���������ƣ���󳤶�50
    /// </summary>
    [JsonPropertyName("clearingAgency")]
    public string ClearingAgency { get; set; }

    /// <summary>
    /// ������ʡ�ݣ���󳤶�50
    /// </summary>
    [JsonPropertyName("provinces")]
    public string Provinces { get; set; }

    /// <summary>
    /// �����г��У���󳤶�50
    /// </summary>
    [JsonPropertyName("city")]
    public string City { get; set; }

    /// <summary>
    /// �����˺ſ������ڣ���ʽ��yyyymmdd
    /// </summary>
    [JsonPropertyName("openDate")]
    public string OpenDate { get; set; }

    /// <summary>
    /// �ͻ�����������������󳤶�50
    /// </summary>
    [JsonPropertyName("distributorName")]
    public string DistributorName { get; set; }

    /// <summary>
    /// �����������룬��󳤶�3
    /// </summary>
    [JsonPropertyName("distributorCode")]
    public string DistributorCode { get; set; }

    /// <summary>
    /// �Ƿ�רҵͶ�ʻ�������󳤶�10
    /// </summary>
    [JsonPropertyName("individualOrInstitution")]
    public string IndividualOrInstitution { get; set; }


    public Investor ToObject()
    {
        IDType iDType = ParseIdType(CertificateType);
        EntityType entityType = ParseCustomerType(CustType);

        if (iDType == IDType.Unknown)
            ReportJsonUnexpected(CMS._Identifier, nameof(CMS.QueryInvestors), $"{CustName}��֤������[{CertificateType}]�޷�ʶ��");
        if (entityType == EntityType.Unk)
            ReportJsonUnexpected(CMS._Identifier, nameof(CMS.QueryInvestors), $"{CustName}��ʵ������[{entityType}]�޷�ʶ��");

        return new Investor
        {
            Name = CustName,
            Identity = new Identity { Id = CertificateNo, Type = iDType },
            EntityType = entityType,

        };
    }

    private EntityType ParseCustomerType(string custType)
    {
        return custType switch
        {
            "����" => EntityType.Natural,
            "����" => EntityType.Institution,
            "��Ʒ" => EntityType.Product,
            _ => EntityType.Unk
        };
    }

    private IDType ParseIdType(string certificateType)
    {
        return certificateType switch
        {
            "0" or "δ֪֤������" or "δ֪" => IDType.Unknown,
            "1" or "���֤" or "�������֤" => IDType.IdentityCard,
            "2" or "�籣��" => IDType.Unknown, // �ɸ���ʵ�����ӳ��
            "3" or "�й�����" => IDType.PassportChina,
            "4" or "����֤" => IDType.OfficerID,
            "5" or "ʿ��֤" => IDType.SoldierID,
            "6" or "�۰ľ��������ڵ�ͨ��֤" => IDType.HongKongMacauPass,
            "7" or "���ڱ�" => IDType.HouseholdRegister,
            "8" or "�������" => IDType.PassportForeign,
            "9" or "����" or "����֤��" => IDType.Other,
            "10" or "��ְ֤" => IDType.CivilianID,
            "11" or "����֤" => IDType.PoliceID,
            "12" or "̨��֤" => IDType.TaiwanCompatriotsID,
            "13" or "��������þ������֤" => IDType.ForeignPermanentResidentID,
            "20" => IDType.Unknown,
            "21" or "����֤��" => IDType.ProductFilingCode, // �ɸ���ʵ�����ӳ��
            "22" or "��֯��������" or "��֯��������֤" => IDType.OrganizationCode,
            "23" or "���ͳһ���ô���" or "ͳһ������ô���" => IDType.UnifiedSocialCreditCode,
            "24" or "����ע���" or "ע���" => IDType.RegistrationNumber,
            "25" or "Ӫҵִ��" or "Ӫҵִ�պ�" => IDType.BusinessLicenseNumber,
            "26" or "��������" => IDType.Other, // �ɸ���ʵ�����ӳ��
            "27" or "�������" => IDType.Other, // �ɸ���ʵ�����ӳ��
            "28" or "����" => IDType.Other, // �ɸ���ʵ�����ӳ��
            "29" or "�侯" => IDType.Other, // �ɸ���ʵ�����ӳ��
            "30" or "�����������������ܵ�λ���ĺţ�" => IDType.Other, // �ɸ���ʵ�����ӳ��
            "31" or "�����" => IDType.Other, // �ɸ���ʵ�����ӳ��
            "32" or "�Ǽ�֤��" => IDType.Other, // �ɸ���ʵ�����ӳ��
            "33" or "����" => IDType.Other, // �ɸ���ʵ�����ӳ��
            "34" or "����" => IDType.Other,
            "40" => IDType.Other, // �ɸ���ʵ�����ӳ��
            "41" => IDType.BusinessLicenseNumber,
            "42" => IDType.Other, // �ɸ���ʵ�����ӳ��
            "43" => IDType.Other,
            "�����˵ǼǱ���" => IDType.ManagerRegistrationCode,
            "��Ʒ��������" => IDType.ProductFilingCode,
            "֤ȯҵ�����֤" => IDType.SecuritiesBusinessLicense,
            "��Ʒ�ǼǱ���" => IDType.ProductRegistrationCode,
            "�۰�̨�����ס֤" => IDType.ResidencePermitForHongKongMacaoAndTaiwanResidents,
            "���еǼ�ϵͳ��Ʒ����" => IDType.TrustRegistrationSystemProductCode,
            _ => IDType.Unknown,
        };
    }
}




#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��