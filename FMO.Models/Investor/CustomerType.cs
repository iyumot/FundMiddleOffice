using System.ComponentModel;

namespace FMO.Models;




[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum AmacInvestorType
{
    [Description("请选择")]None,

    [Description("自然人（非员工跟投）")] NonEmployee,

    [Description("自然人（员工跟投）")] Employee, 

    [Description("本产品管理人跟投")] Manager,

    [Description("境内法人机构（公司等）")] LegalEntity,

    [Description("境内非法人机构（个人独资企业）")] IndividualProprietorship,

    [Description("境内非法人机构（一般合伙企业等）")] NonLegalEntity,

    [Description("境外资金（QFII、RQFII等）")] QFII,

    [Description("境外机构")] Foreign,

    [Description("财政直接出资")]
    DirectFinancialInvestment,

    //[Description("其它机构")] Other,


    [Description("产品")] Product,

    [Description("私募基金产品")]
    PrivateFundProduct,

    [Description("证券公司及其子公司资产管理计划")]
    SecuritiesCompanyAssetManagementPlan,

    [Description("基金公司及其子公司资产管理计划")]
    FundCompanyAssetManagementPlan,

    [Description("期货公司及其子公司资产管理计划")]
    FuturesCompanyAssetManagementPlan,

    [Description("信托计划")]
    TrustPlan,

    [Description("商业银行理财产品")]
    CommercialBankFinancialProduct,

    [Description("保险资产管理计划")]
    InsuranceAssetManagementPlan,

    [Description("慈善基金、捐赠基金等社会公益基金")]
    SocialWelfareFund,

    [Description("养老基金")]
    PensionFund,

    [Description("社会保障基金")]
    SocialSecurityFund,

    [Description("企业年金")]
    EnterpriseAnnuity,

    [Description("政府类引导基金")]
    GovernmentGuidanceFund,

}


[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum QualifiedInvestorType
{
    [Description("普通")] Normal,

    [Description("专业")] Professional,
}

