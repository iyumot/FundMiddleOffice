using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee.JsonCITICS;

#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

public class InvestorJson : JsonBase
{
    [JsonPropertyName("custName")]
    public string CustName { get; set; } // Ͷ��������

    [JsonPropertyName("fundAcco")]
    public string FundAcco { get; set; } // �����˺�

    [JsonPropertyName("tradeAcco")]
    public string TradeAcco { get; set; } // �����˺�

    [JsonPropertyName("custType")]
    public string CustType { get; set; } // �ͻ����ͣ��μ���¼4��

    [JsonPropertyName("certiType")]
    public string CertiType { get; set; } // ֤�����ͣ��μ���¼4��

    [JsonPropertyName("certiNo")]
    public string CertiNo { get; set; } // ֤����

    [JsonPropertyName("bankNo")]
    public string? BankNo { get; set; } // ���б��

    [JsonPropertyName("bankAccount")]
    public string? BankAccount { get; set; } // �����˺�

    [JsonPropertyName("bankOpenName")]
    public string? BankOpenName { get; set; } // ����������

    [JsonPropertyName("bankAccountName")]
    public string? BankAccountName { get; set; } // ���л���

    [JsonPropertyName("address")]
    public string? Address { get; set; } // ͨѶ��ַ

    [JsonPropertyName("tel")]
    public string? Tel { get; set; } // ��ϵ�绰

    [JsonPropertyName("zipCode")]
    public string? ZipCode { get; set; } // �ʱ�

    [JsonPropertyName("agencyNo")]
    public string? AgencyNo { get; set; } // �����̴��룬ZX6��ʾֱ��

    [JsonPropertyName("email")]
    public string? Email { get; set; } // ����

    public Investor ToObject()
    {
        return new Investor
        {
            Name = CustName,
            Identity = new Identity { Id = CertiNo, Type = ParseIdType(CustType, CertiType) },
            Email = Email,
            Phone = Tel,
        };
    }



    public static IDType ParseIdType(string custType, string certiType)
    {
        if (string.IsNullOrEmpty(custType) || string.IsNullOrEmpty(certiType))
            return IDType.Unknown;

        // ����ͻ����ͣ�0=������1=���ˣ�2=��Ʒ
        switch (custType)
        {
            case "0": // ����
                return certiType.ToUpper() switch
                {
                    "0" => IDType.OrganizationCode, // ��֯��������֤
                    "1" => IDType.BusinessLicenseNumber,      // Ӫҵִ��
                    "2" => IDType.RegistrationNumber,         // ��������
                    "3" => IDType.OrganizationCode,           // �������
                    "4" => IDType.Other,                     // ����
                    "5" => IDType.Other,                     // �侯
                    "6" => IDType.Other,                     // ��������
                    "7" => IDType.Other,                     // �����
                    "8" => IDType.Other,                     // ��������
                    "9" => IDType.ProductFilingCode,         // �Ǽ�֤��
                    "A" => IDType.ManagerRegistrationCode,   // ����
                    _ => IDType.Unknown
                };

            case "1": // ����
                return certiType.ToUpper() switch
                {
                    "0" => IDType.IdentityCard,                    // ���֤
                    "1" => IDType.PassportChina,                   // �й�����
                    "2" => IDType.OfficerID,                       // ����֤
                    "3" => IDType.SoldierID,                       // ʿ��֤
                    "4" => IDType.HongKongMacauPass,               // �۰ľ��������ڵ�ͨ��֤
                    "5" => IDType.HouseholdRegister,               // ���ڱ�
                    "6" => IDType.PassportForeign,                 // �⼮����
                    "7" => IDType.Other,                           // ����
                    "8" => IDType.CivilianID,                      // ��ְ֤
                    "9" => IDType.PoliceID,                        // ����֤
                    "A" => IDType.TaiwanCompatriotsID,             // ̨��֤
                    "B" => IDType.ForeignPermanentResidentID,      // ��������þ������֤
                    "C" => IDType.ResidencePermitForHongKongMacaoAndTaiwanResidents, // �۰�̨�����ס֤
                    _ => IDType.Unknown
                };

            case "2": // ��Ʒ
                return certiType.ToUpper() switch
                {
                    "1" => IDType.BusinessLicenseNumber, // Ӫҵִ�գ�ֱ���ӿڲ�����ʹ�ã�
                    "8" => IDType.Other,                 // ����
                    "9" => IDType.ProductFilingCode,     // �Ǽ�֤�飨ֱ���ӿڲ�����ʹ�ã�
                    "A" => IDType.ManagerRegistrationCode, // ����
                    _ => IDType.Unknown
                };

            default:
                return IDType.Unknown;
        }
    }


}

#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��