using System.ComponentModel;

namespace FMO.Models;



[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum NaturalType
{

    [Description("非员工")] NonEmployee,


    [Description("员工")] Employee,
}



[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum InvestorType
{
    [Description("自然人")] Natural,

    [Description("机构")] Institution,

    [Description("产品")] Product
}


[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum QualifiedInvestorType
{
    [Description("普通")] Normal,

    [Description("专业")] Professional,
}



[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum DetailCustomerType
{
    None,


    [Description("非员工")] NonEmployee,


    [Description("员工")] Employee,


    /// <summary>
    /// 证券公司
    /// </summary>
    [Description("证券公司")]
    SecuritiesCompany = 101,

    /// <summary>
    /// 证券公司子公司
    /// </summary>
    [Description("证券公司子公司")]
    SecuritiesSubsidiary,

    /// <summary>
    /// 期货公司
    /// </summary>
    [Description("期货公司")]
    FuturesCompany,

    /// <summary>
    /// 期货公司子公司
    /// </summary>
    [Description("期货公司子公司")]
    FuturesSubsidiary,

    /// <summary>
    /// 基金管理公司
    /// </summary>
    [Description("基金管理公司")]
    FundManagementCompany,


    /// <summary>
    /// 基金管理公司子公司
    /// </summary>
    [Description("基金管理公司子公司")]
    FundManagementSubsidiary,

    /// <summary>
    /// 银行
    /// </summary>
    [Description("银行")]
    Bank,

    /// <summary>
    /// 保险公司
    /// </summary>
    [Description("保险公司")]
    InsuranceCompany,

    /// <summary>
    /// 信托公司
    /// </summary>
    [Description("信托公司")]
    TrustCompany,

    /// <summary>
    /// 财务公司
    /// </summary>
    [Description("财务公司")]
    FinanceCompany,

    /// <summary>
    /// 私募基金管理人
    /// </summary>
    [Description("私募基金管理人")]
    PrivateFundManager,

    /// <summary>
    /// 其他境内金融机构
    /// </summary>
    [Description("其他境内金融机构")]
    OtherDomesticFinancialInstitution,


    /// <summary>
    /// 机关法人
    /// </summary>
    [Description("机关法人")]
    GovernmentAgency,

    /// <summary>
    /// 事业单位法人
    /// </summary>
    [Description("事业单位法人")]
    PublicInstitution,

    /// <summary>
    /// 社会团体法人
    /// </summary>
    [Description("社会团体法人")]
    SocialOrganization,

    /// <summary>
    /// 非金融机构企业法人
    /// </summary>
    [Description("非金融机构企业法人")]
    NonFinancialCorporateEntity,

    /// <summary>
    /// 非金融类非法人机构
    /// </summary>
    [Description("非金融类非法人机构")]
    NonFinancialNonCorporateEntity,


    /// <summary>
    /// 境外金融机构
    /// </summary>
    [Description("境外金融机构")]
    OverseasFinancialInstitution,


    /// <summary>
    /// 境外非金融机构
    /// </summary>
    [Description("境外非金融机构")]
    OverseasNonFinancialInstitution,



    /// <summary>
    /// 其它
    /// </summary>
    [Description("其它")]
    Other,


    /// <summary>
    /// 封闭式公募基金产品
    /// </summary>
    [Description("封闭式公募基金产品")]
    ClosedEndMutualFund = 200,

    /// <summary>
    /// 银行理财产品
    /// </summary>
    [Description("银行理财产品")]
    BankWealthManagementProducts,

    /// <summary>
    /// 信托计划
    /// </summary>
    [Description("信托计划")]
    TrustPlans,

    /// <summary>
    /// 基金公司专户
    /// </summary>
    [Description("基金公司专户")]
    FundCompanySpecialAccounts,

    /// <summary>
    /// 基金子公司产品
    /// </summary>
    [Description("基金子公司产品")]
    FundSubsidiaryProducts,

    /// <summary>
    /// 保险产品
    /// </summary>
    [Description("保险产品")]
    InsuranceProducts,

    /// <summary>
    /// 保险公司及其子公司的资产管理计划
    /// </summary>
    [Description("保险公司及其子公司的资产管理计划")]
    InsuranceCompanyAssetManagementPlans,

    /// <summary>
    /// 证券公司集合理财产品（含证券公司大集合）
    /// </summary>
    [Description("证券公司集合理财产品（含证券公司大集合）")]
    SecuritiesCompanyCollectiveWealthManagementProducts,

    /// <summary>
    /// 证券公司及其子公司专项资管计划
    /// </summary>
    [Description("证券公司及其子公司专项资管计划")]
    SecuritiesCompanySpecialAssetManagementPlans,

    /// <summary>
    /// 证券公司及其子公司单一资管计划
    /// </summary>
    [Description("证券公司及其子公司单一资管计划")]
    SecuritiesCompanySingleAssetManagementPlans,

    /// <summary>
    /// 期货公司及其子公司的资产管理计划
    /// </summary>
    [Description("期货公司及其子公司的资产管理计划")]
    FuturesCompanyAssetManagementPlans,

    /// <summary>
    /// 私募投资基金
    /// </summary>
    [Description("私募投资基金")]
    PrivateEquityFunds,

    /// <summary>
    /// 政府引导基金
    /// </summary>
    [Description("政府引导基金")]
    GovernmentGuidanceFunds,

    /// <summary>
    /// 全国社保基金
    /// </summary>
    [Description("全国社保基金")]
    NationalSocialSecurityFund,

    /// <summary>
    /// 地方社保基金
    /// </summary>
    [Description("地方社保基金")]
    LocalSocialSecurityFund,

    /// <summary>
    /// 基本养老保险
    /// </summary>
    [Description("基本养老保险")]
    BasicOldAgeInsurance,

    /// <summary>
    /// 养老金产品
    /// </summary>
    [Description("养老金产品")]
    PensionProducts,

    /// <summary>
    /// 境外资金（QFII）
    /// </summary>
    [Description("境外资金（QFII）")]
    QFII,

    /// <summary>
    /// 境外资金（RQFII）
    /// </summary>
    [Description("境外资金（RQFII）")]
    RQFII,

    /// <summary>
    /// 其它境外资金
    /// </summary>
    [Description("其它境外资金")]
    OtherOverseasFunds,

    /// <summary>
    /// 社会公益基金（慈善基金、捐赠基金等）
    /// </summary>
    [Description("社会公益基金（慈善基金、捐赠基金等）")]
    PublicWelfareFunds,

    /// <summary>
    /// 企业年金及职业年金
    /// </summary>
    [Description("企业年金及职业年金")]
    CorporateAnnuityAndOccupationalAnnuity,

    /// <summary>
    /// 其他产品
    /// </summary>
    [Description("其他产品")]
    OtherProducts,

    /// <summary>
    /// 开放式公募基金产品
    /// </summary>
    [Description("开放式公募基金产品")]
    OpenEndMutualFund
}
