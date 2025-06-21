using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FMO.IO.Trustee.CITISC.Json.Customer;


#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
internal class JsonRootDto
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("pages")]
    public int Pages { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("data")]
    public List<CustomerInfo> Data { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("fs_code")]
    public string FsCode { get; set; }
}


internal class CustomerInfo
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("explain")]
    public string Explain { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("instrepr_phone")]
    public string InstreprPhone { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("holding_name")]
    public string HoldingName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("education")]
    public string Education { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("industry_detail_cn")]
    public string IndustryDetailCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("id_vali_date")]
    public string IdValiDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("qualification_type")]
    public string QualificationType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("zip_code")]
    public string ZipCode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("trade_acco_state")]
    public string TradeAccoState { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("has_agent")]
    public string HasAgent { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("product_expiring_date")]
    public string ProductExpiringDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("valid_bala")]
    public string ValidBala { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("cust_no")]
    public string CustNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("manager_code")]
    public string ManagerCode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("product_id")]
    public string ProductId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("hg_flag")]
    public string HgFlag { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("bad_integrity_cn")]
    public string BadIntegrityCn { get; set; }

    /// <summary>
    /// 身份证
    /// </summary>
    [JsonPropertyName("identity_type_in_bank_cn")]
    public string IdentityTypeInBankCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("is_allow_edit")]
    public string IsAllowEdit { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("bank_city_no")]
    public string BankCityNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("myself_is_bfcy")]
    public string MyselfIsBfcy { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("cust_risk_type")]
    public string CustRiskType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("holding_info_list")]
    public string HoldingInfoList { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("work_region_code")]
    public string WorkRegionCode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("acco_tgr_id")]
    public string AccoTgrId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("registered_currency_cn")]
    public string RegisteredCurrencyCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("org_type_cn")]
    public string OrgTypeCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("tuo_shou_flag")]
    public string TuoShouFlag { get; set; }

    /// <summary>
    /// 未知
    /// </summary>
    [JsonPropertyName("manager_flag_cn")]
    public string ManagerFlagCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("cust_kind")]
    public string CustKind { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("bank_province_code")]
    public string BankProvinceCode { get; set; }

    /// <summary>
    /// 个人|身份证
    /// </summary>
    [JsonPropertyName("cust_info")]
    public string CustInfo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("principal_no")]
    public string PrincipalNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("nationality")]
    public string Nationality { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("asset_manager_name")]
    public string AssetManagerName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("trustee_name")]
    public string TrusteeName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("detail")]
    public string Detail { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("frequently_address")]
    public string FrequentlyAddress { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("registered_currency")]
    public string RegisteredCurrency { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("modify_flag")]
    public string ModifyFlag { get; set; }

    /// <summary>
    /// 汤雪明
    /// </summary>
    [JsonPropertyName("name_in_bank")]
    public string NameInBank { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("min_risk_flag")]
    public string MinRiskFlag { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("bank_acco")]
    public string BankAcco { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("mobile_no")]
    public string MobileNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("asset_manager_id_valid_date")]
    public string AssetManagerIdValidDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("cont_email")]
    public string ContEmail { get; set; }

    /// <summary>
    /// 个人
    /// </summary>
    [JsonPropertyName("cust_type_cn")]
    public string CustTypeCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("bank_no")]
    public string BankNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("product_code")]
    public string ProductCode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("cont_type")]
    public string ContType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("exempt_beneficiary")]
    public string ExemptBeneficiary { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("send_state")]
    public string SendState { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("profession_flag")]
    public string ProfessionFlag { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("tax_no")]
    public string TaxNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("old_trade_acco")]
    public string OldTradeAcco { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("check_flag")]
    public string CheckFlag { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("instrepr_email")]
    public string InstreprEmail { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("cont_no")]
    public string ContNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("natural_type")]
    public string NaturalType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("natural_type_cn")]
    public string NaturalTypeCn { get; set; }

    /// <summary>
    /// 是
    /// </summary>
    [JsonPropertyName("first_open_flag_cn")]
    public string FirstOpenFlagCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("cancel_flag")]
    public string CancelFlag { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("organization_no")]
    public string OrganizationNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("fund_acco")]
    public string FundAcco { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("branch_bank")]
    public string BranchBank { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("effect_date")]
    public string EffectDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("contact_name")]
    public string ContactName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("instrepr_id_no")]
    public string InstreprIdNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("address")]
    public string Address { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("vocation_cn")]
    public string VocationCn { get; set; }

    /// <summary>
    /// 普通合格投资人
    /// </summary>
    [JsonPropertyName("cust_kind_cn")]
    public string CustKindCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("sex")]
    public string Sex { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("reg_capital")]
    public string RegCapital { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("principal_valid_date")]
    public string PrincipalValidDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("tax_bodies")]
    public string TaxBodies { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("is_allow_destory_acco")]
    public string IsAllowDestoryAcco { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("vocation")]
    public string Vocation { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("asset_manager_id_type_cn")]
    public string AssetManagerIdTypeCn { get; set; }

    /// <summary>
    /// 未知
    /// </summary>
    [JsonPropertyName("investor_kind_type_cn")]
    public string InvestorKindTypeCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("nationality_cn")]
    public string NationalityCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("confirm_flag")]
    public string ConfirmFlag { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("certificate_no")]
    public string CertificateNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("instrepr_id_type")]
    public string InstreprIdType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("reg_region_code_cn")]
    public string RegRegionCodeCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("phone_no")]
    public string PhoneNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("pub_profession_sub_type_cn")]
    public string PubProfessionSubTypeCn { get; set; }

    /// <summary>
    /// 中国工商银行
    /// </summary>
    [JsonPropertyName("bank_no_cn")]
    public string BankNoCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("principal_name")]
    public string PrincipalName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("asset_manager_id_type")]
    public string AssetManagerIdType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("product_acco_record_no")]
    public string ProductAccoRecordNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("office")]
    public string Office { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("work_region_code_cn")]
    public string WorkRegionCodeCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("bad_integrity_desc")]
    public string BadIntegrityDesc { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("acco_manager_id")]
    public string AccoManagerId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("product_abbreviation")]
    public string ProductAbbreviation { get; set; }

    /// <summary>
    /// 汤雪明
    /// </summary>
    [JsonPropertyName("custom_name")]
    public string CustomName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("asset_manager_id_no")]
    public string AssetManagerIdNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("cust_type")]
    public string CustType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("manager_flag")]
    public string ManagerFlag { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("instrepr_name")]
    public string InstreprName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("assist_id_type")]
    public string AssistIdType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("identity_type")]
    public string IdentityType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("bank_city_no_cn")]
    public string BankCityNoCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("open_date")]
    public string OpenDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("cont_type_cn")]
    public string ContTypeCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("reg_capital_cn")]
    public string RegCapitalCn { get; set; }

    /// <summary>
    /// 是
    /// </summary>
    [JsonPropertyName("hg_flag_cn")]
    public string HgFlagCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("product_record_no")]
    public string ProductRecordNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("frequently_region_code")]
    public string FrequentlyRegionCode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("aml_risk_type")]
    public string AmlRiskType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("link_man")]
    public string LinkMan { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("assist_id_no")]
    public string AssistIdNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("investor_kind_type")]
    public string InvestorKindType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("instrepr_valid_date")]
    public string InstreprValidDate { get; set; }

    /// <summary>
    /// 是
    /// </summary>
    [JsonPropertyName("profession_flag_cn")]
    public string ProfessionFlagCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("reg_region_code")]
    public string RegRegionCode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("cor_property")]
    public string CorProperty { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("identity_type_in_bank")]
    public string IdentityTypeInBank { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("trustee_no")]
    public string TrusteeNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("pub_profession_sub_type")]
    public string PubProfessionSubType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("beneficiary")]
    public string Beneficiary { get; set; }

    /// <summary>
    /// 正常
    /// </summary>
    [JsonPropertyName("trade_acco_state_cn")]
    public string TradeAccoStateCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("tuo_shou_msg")]
    public string TuoShouMsg { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("identity_no")]
    public string IdentityNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("industry_detail")]
    public string IndustryDetail { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("fax_no")]
    public string FaxNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("pub_profession_valid_date")]
    public string PubProfessionValidDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("cincome")]
    public string Cincome { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("cont_phone")]
    public string ContPhone { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("qualification_no")]
    public string QualificationNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("income")]
    public string Income { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("marriage_cn")]
    public string MarriageCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("frequently_region_code_cn")]
    public string FrequentlyRegionCodeCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("identity_no_in_bank")]
    public string IdentityNoInBank { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("bfcy_info_list")]
    public string BfcyInfoList { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("reg_address")]
    public string RegAddress { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("trade_acco")]
    public string TradeAcco { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("assist_id_valid_date")]
    public string AssistIdValidDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("cont_valid_date")]
    public string ContValidDate { get; set; }

    /// <summary>
    /// 未知
    /// </summary>
    [JsonPropertyName("cust_risk_type_cn")]
    public string CustRiskTypeCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("instrepr_type_cn")]
    public string InstreprTypeCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("bank_mobile_no")]
    public string BankMobileNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("tuo_shou_code")]
    public string TuoShouCode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("marriage")]
    public string Marriage { get; set; }

    /// <summary>
    /// 中国工商银行股份有限公司海盐支行
    /// </summary>
    [JsonPropertyName("bank_name")]
    public string BankName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("principal_type")]
    public string PrincipalType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("min_risk_flag_cn")]
    public string MinRiskFlagCn { get; set; }

    /// <summary>
    /// 男
    /// </summary>
    [JsonPropertyName("sex_cn")]
    public string SexCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("first_open_flag")]
    public string FirstOpenFlag { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("trade_cn")]
    public string TradeCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("org_type")]
    public string OrgType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("business_range")]
    public string BusinessRange { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("edit_regist_capital")]
    public string EditRegistCapital { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("fund_acco_state")]
    public string FundAccoState { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("cor_property_cn")]
    public string CorPropertyCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("principal_tel_no")]
    public string PrincipalTelNo { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("principal_email")]
    public string PrincipalEmail { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("my_self_is_contorler")]
    public string MySelfIsContorler { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("principal_type_cn")]
    public string PrincipalTypeCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("birth_day")]
    public string BirthDay { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("manager_name")]
    public string ManagerName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("actual_controller_list")]
    public string ActualControllerList { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("trade")]
    public string Trade { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("work_address")]
    public string WorkAddress { get; set; }

    /// <summary>
    /// 身份证
    /// </summary>
    [JsonPropertyName("identity_type_cn")]
    public string IdentityTypeCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("check_id_vali_date")]
    public string CheckIdValiDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("office_tel")]
    public string OfficeTel { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("education_cn")]
    public string EducationCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("bank_province_code_cn")]
    public string BankProvinceCodeCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("tuo_shou_code_cn")]
    public string TuoShouCodeCn { get; set; }

    /// <summary>
    /// 正常
    /// </summary>
    [JsonPropertyName("fund_acco_state_cn")]
    public string FundAccoStateCn { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("bad_integrity")]
    public string BadIntegrity { get; set; }

    /// <summary>
    ///  
    /// </summary>
    [JsonPropertyName("acco_name")]
    public string AccoName { get; set; }
}



#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。