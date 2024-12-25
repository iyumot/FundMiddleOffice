using FMO.IO.Trustee.CITISC.Json.Customer;
using FMO.IO.Trustee.CITISC.Json.FundRasing;
using FMO.Models;

namespace FMO.IO.Trustee.CITISC;

internal static class DtoHelper
{
    public static BankTransaction ToTransaction(this TransactionRecord item, string Identifier)
    {
        return new BankTransaction
        {
            Id = Identifier + item.BankJour,

            PayName = item.Direction == "1" ? item.RecAccName : item.PayAccName,
            PayBank = item.Direction == "1" ? item.RecBankName : item.PayBankName,
            PayNo = item.Direction == "1" ? item.RecAccNo : item.PayAccNo,

            ReceiveName = item.Direction != "1" ? item.RecAccName : item.PayAccName,
            ReceiveBank = item.Direction != "1" ? item.RecBankName : item.PayBankName,
            ReceiveNo = item.Direction != "1" ? item.RecAccNo : item.PayAccNo,

            Balance = decimal.Parse(item.Balance),
            Origin = Identifier,
            TransactionId = item.BankJour,
            Remark = item.Remark,
            Time = item.TradeTm
        };
    }







    private static InstitutionCustomerType GetInstitutionCustomerType(string str)
    {
        switch (str)
        {
            case "私募基金管理人": return InstitutionCustomerType.PrivateFundManager;
            case "期货公司": return InstitutionCustomerType.FuturesCompany;
            case "基金管理公司子公司": return InstitutionCustomerType.FundManagementSubsidiary;
            case "证券公司子公司": return InstitutionCustomerType.SecuritiesSubsidiary;
            case "期货公司子公司": return InstitutionCustomerType.FuturesSubsidiary;
            case "财务公司": return InstitutionCustomerType.FinanceCompany;
            case "其他境内金融机构": return InstitutionCustomerType.OtherDomesticFinancialInstitution;
            case "机关法人": return InstitutionCustomerType.GovernmentAgency;
            case "事业单位法人": return InstitutionCustomerType.PublicInstitution;
            case "社会团体法人": return InstitutionCustomerType.SocialOrganization;
            case "非金融机构企业法人": return InstitutionCustomerType.NonFinancialCorporateEntity;
            case "非金融类非法人机构": return InstitutionCustomerType.NonFinancialNonCorporateEntity;
            case "境外代理人": return InstitutionCustomerType.OverseasAgent;
            case "境外金融机构": return InstitutionCustomerType.OverseasFinancialInstitution;
            case "外国战略投资者": return InstitutionCustomerType.ForeignStrategicInvestor;
            case "保险公司": return InstitutionCustomerType.InsuranceCompany;
            case "境外非金融机构": return InstitutionCustomerType.OverseasNonFinancialInstitution;
            case "基金管理公司": return InstitutionCustomerType.FundManagementCompany;
            case "银行子公司": return InstitutionCustomerType.BankSubsidiary;
            case "保险子公司": return InstitutionCustomerType.InsuranceSubsidiary;
            case "信托公司": return InstitutionCustomerType.TrustCompany;
            case "证券公司": return InstitutionCustomerType.SecuritiesCompany;
            case "其它": return InstitutionCustomerType.Other;
            case "银行": return InstitutionCustomerType.Bank;

            default:
                throw new Exception($"未知的机构类型：{str}");
        }
    }


    private static ProductCustomerType GetProductCustomerType(string str)
    {
        switch (str)
        {
            case "封闭式公募基金产品": return ProductCustomerType.ClosedEndMutualFund;
            case "银行理财产品": return ProductCustomerType.BankWealthManagementProducts;
            case "信托计划": return ProductCustomerType.TrustPlans;
            case "基金公司专户": return ProductCustomerType.FundCompanySpecialAccounts;
            case "基金子公司产品": return ProductCustomerType.FundSubsidiaryProducts;
            case "保险产品": return ProductCustomerType.InsuranceProducts;
            case "保险公司及其子公司的资产管理计划": return ProductCustomerType.InsuranceCompanyAssetManagementPlans;
            case "证券公司集合理财产品（含证券公司大集合）": return ProductCustomerType.SecuritiesCompanyCollectiveWealthManagementProducts;
            case "证券公司及其子公司专项资管计划": return ProductCustomerType.SecuritiesCompanySpecialAssetManagementPlans;
            case "证券公司及其子公司单一资管计划": return ProductCustomerType.SecuritiesCompanySingleAssetManagementPlans;
            case "期货公司及其子公司的资产管理计划": return ProductCustomerType.FuturesCompanyAssetManagementPlans;
            case "私募投资基金": return ProductCustomerType.PrivateEquityFunds;
            case "政府引导基金": return ProductCustomerType.GovernmentGuidanceFunds;
            case "全国社保基金": return ProductCustomerType.NationalSocialSecurityFund;
            case "地方社保基金": return ProductCustomerType.LocalSocialSecurityFund;
            case "基本养老保险": return ProductCustomerType.BasicOldAgeInsurance;
            case "养老金产品": return ProductCustomerType.PensionProducts;
            case "境外资金（QFII）": return ProductCustomerType.QFII;
            case "境外资金（RQFII）": return ProductCustomerType.RQFII;
            case "其它境外资金": return ProductCustomerType.OtherOverseasFunds;
            case "社会公益基金（慈善基金、捐赠基金等）": return ProductCustomerType.PublicWelfareFunds;
            case "企业年金及职业年金": return ProductCustomerType.CorporateAnnuityAndOccupationalAnnuity;
            case "其他产品": return ProductCustomerType.OtherProducts;
            case "开放式公募基金产品": return ProductCustomerType.OpenEndMutualFund;


            default:
                throw new Exception($"未知的产品类型：{str}");
        }
    }

    private static Gender? GetGender(string str)
    {
        switch (str)
        {
            case "男": return Gender.Male;
            case "女": return Gender.Femaile;

            default:
                return null;
        }
    }



    public static (ICustomer customer, BankAccount account) ToCustomer(this CustomerInfo info)
    {
        CustomerType customerType = info.CustTypeCn switch { "个人" => CustomerType.Natural, "机构" => CustomerType.Institution, "产品" => CustomerType.Product, _ => throw new Exception($"未知的客户类型 {info.CustomName}：{info.ContTypeCn}") };

        IDType idtype = info.IdentityTypeCn switch
        {
            "身份证" => IDType.IdentityCard,
            "中国护照" => IDType.PassportChina,
            "军官证" => IDType.OfficerID,
            "士兵证" => IDType.SoldierID,
            "港澳居民来往内地通行证" => IDType.HongKongMacauPass,
            "户口本" => IDType.HouseholdRegister,
            "外国护照" => IDType.PassportForeign,
            "文职证" => IDType.CivilianID,
            "警官证" => IDType.PoliceID,
            "台胞证" => IDType.TaiwanCompatriotsID,
            "外国人永久居留身份证" => IDType.ForeignPermanentResidentID,
            "组织机构代码证" => IDType.OrganizationCodeCertificate,
            "营业执照" => IDType.BusinessLicense,
            "行政机关" => IDType.AdministrativeAgency,
            "社会团体" => IDType.SocialGroup,
            "军队" => IDType.Military,
            "武警" => IDType.ArmedPolice,
            "下属机构（具有主管单位批文号）" => IDType.SubordinateOrganization,
            "基金会" => IDType.Foundation,
            "登记证书" => IDType.RegistrationCertificate,
            "批文" => IDType.ApprovalDocument,
            "其他" => IDType.Other,
            "其它" => IDType.Other,
            _ => throw new Exception($"未知的证件类型：{info.IdentityTypeCn}")
        };

        var identity = new Identity { Id = info.IdentityNo, Type = idtype };




        ICustomer customer = customerType switch
        {
            CustomerType.Natural => new NaturalCustomer { Name = info.CustomName, Identity = identity, Gender = GetGender(info.SexCn) },
            CustomerType.Institution => new InstitutionCustomer { Name = info.CustomName, Identity = identity, InstitutionType = GetInstitutionCustomerType(info.OrgTypeCn) },
            CustomerType.Product => new ProductCustomer { Name = info.CustomName, Identity = identity, ProductType = GetProductCustomerType(info.OrgTypeCn) },
            _ => throw new NotImplementedException(),
        };

        BankAccount bankAccount = new BankAccount
        {
            BankOfDeposit = info.BankName?.Contains("银行") ?? false ? info.BankName : info.BankNoCn,
            Number = info.BankAcco,
            Name = info.NameInBank,

        };

        return (customer, bankAccount);
    }



}


