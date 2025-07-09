
using FMO.Models;
using System.Text.RegularExpressions;

namespace FMO.IO.DS.MeiShi.Json.Customer;


#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。

public class TotalInvestmentAmountDTO
{
    /// <summary>
    /// 
    /// </summary>
    public string cny { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string cnyStr { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string usd { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string usdStr { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string hkd { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string hkdStr { get; set; }
}

public class HoldTotalAmountDTO
{
    /// <summary>
    /// 
    /// </summary>
    public string cny { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string cnyStr { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string usd { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string usdStr { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string hkd { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string hkdStr { get; set; }
}

public class TotalValueDTO
{
    /// <summary>
    /// 
    /// </summary>
    public string cny { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string cnyStr { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string usd { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string usdStr { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string hkd { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string hkdStr { get; set; }
}

public class HoldingEarningsDTO
{
    /// <summary>
    /// 
    /// </summary>
    public string cny { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string cnyStr { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string usd { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string usdStr { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string hkd { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string hkdStr { get; set; }
}

public class NetPurchaseAmountDTO
{
    /// <summary>
    /// 
    /// </summary>
    public string cny { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string cnyStr { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string usd { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string usdStr { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string hkd { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string hkdStr { get; set; }
}

public class ListItem
{
    /// <summary>
    /// 
    /// </summary>
    public int? customerId { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? customerType { get; set; }
    /// <summary>
    /// 光大期货光耀专享12号F0F单一资产管理计划
    /// </summary>
    public string? customerName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? customerLevel { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? investorType { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? riskType { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? riskState { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? realNameState { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? isDelete { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? investedFundsNum { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? totalInvestmentAmount { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public TotalInvestmentAmountDTO totalInvestmentAmountDTO { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? fundsAlreadyOwnedNum { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? alreadyOwnedProductNames { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? alreadyOwnedProductIds { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? holdTotalAmount { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public HoldTotalAmountDTO holdTotalAmountDTO { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public TotalValueDTO totalValueDTO { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? holdingEarnings { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public HoldingEarningsDTO holdingEarningsDTO { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? mobile { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? email { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? cardType { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? cardNumber { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? forbidden { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? saleUserName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? remark { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? dealer { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? currentAutomatic { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? historyAutomatic { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? currentManual { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? historyManual { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? channelNames { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? origin { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? otherOrigin { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? isVIP { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? nickName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? openId { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? registerTime { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? sourceType { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? createTime { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? riskLimitDate { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? isRiskLongTime { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? isExpire { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? legalPersonCardNumber { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? authorizedRepresentativeCardNumber { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? customerGroupNames { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? companyCode { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? identifyStatus { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? identifyLimitDate { get; set; }
    /// <summary>
    /// 2028年03月06日
    /// </summary>
    public string? identifyLimitDateString { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? riskLimitDateString { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? channelType { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? channelTypeString { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? lastLoginTime { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? isSubscribed { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? isCanSubscribed { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? inviterName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? oneLevelChannelName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? twoLevelChannelName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? customizeElementMap { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? personAddress { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? companyCardValidStartTime { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? companyCardValidEndTime { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? companyIsCardLongTime { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? businessScope { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? feeNetValue { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? archivalOrganization { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? productScale { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? customizeElementInfoMap { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? customerSource { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? customerSourceTime { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? maintenanceType { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? timeMaintenanceType { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? managerTakeOverTime { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? aptnessType { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? netPurchaseAmount { get; set; }
    /// <summary>
    /// 
    /// </summary>
    //public NetPurchaseAmountDTO netPurchaseAmountDTO { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? cumulativeIncome { get; set; }
    /// <summary>
    /// 
    /// </summary>
    //public string? cumulativeIncomeDTO { get; set; }
    /// <summary>
    /// 
    /// </summary>
   // public string? trusteeOriginalData { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? trusteeOriginalDataString { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? saleUserIds { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int? cnfundAssociationInvestorType { get; set; }

    internal Investor ToCustomer()
    {
        var v = new Investor
        {
            Name = customerName!,
            EntityType = customerType switch { 1 => EntityType.Natural, 2 => EntityType.Institution, 3 => EntityType.Product, _ => EntityType.Unk },
            Identity = GetIdentity(),
            Phone = mobile,
            Email = email,
            RiskEvaluation = riskType switch { >= 5 => RiskEvaluation.C5, > 0 => (RiskEvaluation)riskType, _ => RiskEvaluation.Unk }
        };


        if (customerName!.Contains("私募"))
            v.Type = AmacInvestorType.PrivateFundProduct;
        else if (Regex.IsMatch(customerName, ".{2,}期货.*资产管理计划"))
            v.Type = AmacInvestorType.FuturesCompanyAssetManagementPlan;
        else if (Regex.IsMatch(customerName, ".{2,}证券.*资产管理计划"))
            v.Type = AmacInvestorType.SecuritiesCompanyAssetManagementPlan;
        else if (Regex.IsMatch(customerName, ".{2,}基金.*资产管理计划"))
            v.Type = AmacInvestorType.FundCompanyAssetManagementPlan;
        else if (Regex.IsMatch(customerName, "有限公司"))
            v.Type = AmacInvestorType.LegalEntity;
        else if (Regex.IsMatch(customerName, "有限合伙"))
            v.Type = AmacInvestorType.NonLegalEntity;

        if (v.EntityType == EntityType.Natural)
            v.Type = AmacInvestorType.NonEmployee;

        return v;
    }


    internal Identity GetIdentity()
    {
        return new Identity
        {
            Id = cardNumber!,
            Type = cardType switch
            {
                1 => IDType.IdentityCard,
                2 => IDType.HouseholdRegister,
                3 => IDType.ForeignPermanentResidentID,
                4 => IDType.PassportChina,
                5 => IDType.SoldierID,
                6 => IDType.HongKongMacauPass,
                7 => IDType.TaiwanCompatriotsID,
                8 => IDType.BusinessLicenseNumber,
                9 => IDType.OrganizationCode,
                12 => IDType.Other,
                13 => IDType.ProductFilingCode,
                14 => IDType.OfficerID,
                15 => IDType.PassportForeign,
                16 => IDType.CivilianID,
                17 => IDType.PoliceID,
                _ => IDType.Unknown
            }
        };

    }


}

public class Data
{
    /// <summary>
    /// 
    /// </summary>
    public List<ListItem> list { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int total { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int pageNum { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int pageSize { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int pages { get; set; }
}

public class Root
{
    /// <summary>
    /// 
    /// </summary>
    public int code { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public Data data { get; set; }
    /// <summary>
    /// 请求成功
    /// </summary>
    public string message { get; set; }
}

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。