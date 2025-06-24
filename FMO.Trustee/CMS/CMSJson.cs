using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;


#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
public partial class CMS
{
    public class JsonRoot
    {
        [JsonPropertyName("resultcode")]
        public string Code { get; set; }


        [JsonPropertyName("msg")]
        public string Msg { get; set; }


        [JsonPropertyName("page")]
        public string? Page { get; set; }

        [JsonPropertyName("data")]
        public string? Data { get; set; }
    }
    public class PaginationInfo
    {
        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("pageCount")]
        public int PageCount { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class JsonRoot<T>
    {
        [JsonPropertyName("resultcode")]
        public string Code { get; set; }


        [JsonPropertyName("msg")]
        public string Msg { get; set; }


        [JsonPropertyName("page")]
        public required PaginationInfo Page { get; set; }

        [JsonPropertyName("data")]
        public required T[] Data { get; set; }
    }




    public class SubjectFundMappingJson
    {
        [JsonPropertyName("gradeNo")]
        public string GradeNo { get; set; }

        [JsonPropertyName("gradeName")]
        public string GradeName { get; set; }

        [JsonPropertyName("productNo")]
        public string ProductNo { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; }

        [JsonPropertyName("isGrade")]
        public bool IsGrade { get; set; }

        [JsonPropertyName("registerNo")]
        public string RegisterNo { get; set; }

        public SubjectFundMapping ToObject()
        {
            var sc = IsGrade ? GradeName.Replace(ProductName, "") : "";
            return new SubjectFundMapping { FundCode = GradeNo, FundName = GradeName, MasterCode = ProductNo, MasterName = ProductName, ShareClass = sc };
        }
    }


    public class TransferRequestJson
    {
        /// <summary>
        /// 客户名称（最大长度：200）
        /// </summary>
        [JsonPropertyName("custName")]
        public string CustomerName { get; set; }

        /// <summary>
        /// 客户类型（最大长度：30）
        /// </summary>
        [JsonPropertyName("custType")]
        public string CustomerType { get; set; }

        /// <summary>
        /// 证件类型（最大长度：50）
        /// </summary>
        [JsonPropertyName("certificateType")]
        public string CertificateType { get; set; }

        /// <summary>
        /// 证件号码（最大长度：30）
        /// </summary>
        [JsonPropertyName("certificateNo")]
        public string CertificateNumber { get; set; }

        /// <summary>
        /// 基金账号（最大长度：20）
        /// </summary>
        [JsonPropertyName("taAccountId")]
        public string TaAccountId { get; set; }

        /// <summary>
        /// 交易账号（最大长度：30）
        /// </summary>
        [JsonPropertyName("transactionAccountId")]
        public string TransactionAccountId { get; set; }

        /// <summary>
        /// 产品名称（最大长度：300）
        /// </summary>
        [JsonPropertyName("fundName")]
        public string FundName { get; set; }

        /// <summary>
        /// 产品代码（最大长度：6）
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// 业务类型（最大长度：6）
        /// </summary>
        [JsonPropertyName("businessCode")]
        public string BusinessCode { get; set; }

        /// <summary>
        /// 申请金额，保留两位小数
        /// </summary>
        [JsonPropertyName("applicationAmount")]
        public string ApplicationAmount { get; set; }

        /// <summary>
        /// 申请份额，保留两位小数
        /// </summary>
        [JsonPropertyName("applicationVol")]
        public string ApplicationVol { get; set; }

        /// <summary>
        /// 申请日期，格式：yyyymmdd
        /// </summary>
        [JsonPropertyName("transactionDate")]
        public string TransactionDate { get; set; }

        /// <summary>
        /// 手续费折扣率，保留两位小数
        /// </summary>
        [JsonPropertyName("discountRateOfCommission")]
        public string DiscountRateOfCommission { get; set; }

        /// <summary>
        /// 销售渠道代码（最大长度：3）
        /// </summary>
        [JsonPropertyName("distributorCode")]
        public string DistributorCode { get; set; }

        /// <summary>
        /// 销售渠道名称（最大长度：300）
        /// </summary>
        [JsonPropertyName("distributorName")]
        public string DistributorName { get; set; }

        /// <summary>
        /// 申请流水号（最大长度：500）
        /// </summary>
        [JsonPropertyName("remark1")]
        public string Remark1 { get; set; }

        /// <summary>
        /// 预留字段2（最大长度：500）
        /// </summary>
        [JsonPropertyName("remark2")]
        public string Remark2 { get; set; }

        /// <summary>
        /// 预约申购日期，格式：yyyymmdd（可为空）
        /// </summary>
        [JsonPropertyName("futureBuyDate")]
        public string FutureBuyDate { get; set; }

        /// <summary>
        /// 预约赎回日期，格式：yyyymmdd（可为空）
        /// </summary>
        [JsonPropertyName("redemptionDateInAdvance")]
        public string RedemptionDateInAdvance { get; set; }


        public TransferRequest ToObject()
        {
            return new TransferRequest
            {
                CustomerIdentity = CertificateNumber,
                CustomerName = CustomerName,
                FundName = FundName,
                FundCode = FundCode,
                RequestDate = DateOnly.ParseExact(TransactionDate, "yyyyMMdd"),
                RequestType = TranslateRequest(BusinessCode),
                RequestAmount = ParseDecimal(ApplicationAmount),
                RequestShare = ParseDecimal(ApplicationVol),
                Agency = DistributorName,
                FeeDiscount = ParseDecimal(DiscountRateOfCommission),
                ExternalId = Remark1,
            };
        }
    }

    /// <summary>
    /// 基金费用信息实体类
    /// </summary>
    public class FundDailyFeeJson
    {
        /// <summary>
        /// 产品名称（必填，最大长度300）
        /// </summary>
        [JsonPropertyName("fundName")]
        public string FundName { get; set; }

        /// <summary>
        /// 产品代码（必填，最大长度6）
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// 费用日期（格式：yyyyMMdd，必填）
        /// </summary>
        [JsonPropertyName("busiDate")]
        public string BusiDate { get; set; }

        // 托管费
        [JsonPropertyName("custodianFeeJt")]
        public decimal CustodianFeeJt { get; set; } // 计提

        [JsonPropertyName("custodianFeeZf")]
        public decimal CustodianFeeZf { get; set; } // 支付

        [JsonPropertyName("custodianFeeYe")]
        public decimal CustodianFeeYe { get; set; } // 余额

        // 运营服务费
        [JsonPropertyName("operationServiceFeeJt")]
        public decimal OperationServiceFeeJt { get; set; }

        [JsonPropertyName("operationServiceFeeZf")]
        public decimal OperationServiceFeeZf { get; set; }

        [JsonPropertyName("operationServiceFeeYe")]
        public decimal OperationServiceFeeYe { get; set; }

        // 管理费
        [JsonPropertyName("managementFeeJt")]
        public decimal ManagementFeeJt { get; set; }

        [JsonPropertyName("managementFeeZf")]
        public decimal ManagementFeeZf { get; set; }

        [JsonPropertyName("managementFeeYe")]
        public decimal ManagementFeeYe { get; set; }

        // 业绩报酬费
        [JsonPropertyName("performanceFeeJt")]
        public decimal PerformanceFeeJt { get; set; }

        [JsonPropertyName("performanceFeeZf")]
        public decimal PerformanceFeeZf { get; set; }

        [JsonPropertyName("performanceFeeYe")]
        public decimal PerformanceFeeYe { get; set; }

        // 销售服务费
        [JsonPropertyName("salesandServiceFeesJt")]
        public decimal SalesAndServiceFeesJt { get; set; }

        [JsonPropertyName("salesandServiceFeesZf")]
        public decimal SalesAndServiceFeesZf { get; set; }

        [JsonPropertyName("salesandServiceFeesYe")]
        public decimal SalesAndServiceFeesYe { get; set; }

        // 投资顾问费
        [JsonPropertyName("investmentConsultantFeeJt")]
        public decimal InvestmentConsultantFeeJt { get; set; }

        [JsonPropertyName("investmentConsultantFeeZf")]
        public decimal InvestmentConsultantFeeZf { get; set; }

        [JsonPropertyName("investmentConsultantFeeYe")]
        public decimal InvestmentConsultantFeeYe { get; set; }

        // 客户服务费
        [JsonPropertyName("customerServiceFeeJt")]
        public decimal CustomerServiceFeeJt { get; set; }

        [JsonPropertyName("customerServiceFeeZf")]
        public decimal CustomerServiceFeeZf { get; set; }

        [JsonPropertyName("customerServiceFeeYe")]
        public decimal CustomerServiceFeeYe { get; set; }

        public FundDailyFee ToObject()
        {
            return new FundDailyFee
            {
                FundCode = FundCode,
                Date = DateOnly.ParseExact(BusiDate, "yyyyMMdd"),
                // 管理费
                ManagerFeeAccrued = ManagementFeeJt,
                ManagerFeePaid = ManagementFeeZf,
                ManagerFeeBalance = ManagementFeeYe,

                // 托管费
                CustodianFeeAccrued = CustodianFeeJt,
                CustodianFeePaid = CustodianFeeZf,
                CustodianFeeBalance = CustodianFeeYe,

                // 外包运营服务费（OperationServiceFee）
                OutsourcingFeeAccrued = OperationServiceFeeJt,
                OutsourcingFeePaid = OperationServiceFeeZf,
                OutsourcingFeeBalance = OperationServiceFeeYe,

                // 业绩报酬费
                PerformanceFeeAccrued = PerformanceFeeJt,
                PerformanceFeePaid = PerformanceFeeZf,
                PerformanceFeeBalance = PerformanceFeeYe,

                // 销售服务费
                SalesFeeAccrued = SalesAndServiceFeesJt,
                SalesFeePaid = SalesAndServiceFeesZf,
                SalesFeeBalance = SalesAndServiceFeesYe,

                // 投资顾问费
                ConsultantFeeAccrued = InvestmentConsultantFeeJt,
                ConsultantFeePaid = InvestmentConsultantFeeZf,
                ConsultantFeeBalance = InvestmentConsultantFeeYe

            };
        }

    }


    public class BankTransactionJson
    {
        /// <summary>
        /// 本方账户号
        /// </summary>
        [JsonPropertyName("bfzhh")]
        public string OurAccountNumber { get; set; }

        /// <summary>
        /// 本方账户名
        /// </summary>
        [JsonPropertyName("bfzhmc")]
        public string OurAccountName { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        [JsonPropertyName("bz")]
        public string Currency { get; set; }

        /// <summary>
        /// 产品代码
        /// </summary>
        [JsonPropertyName("cpdm")]
        public string ProductCode { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        [JsonPropertyName("cpmc")]
        public string ProductName { get; set; }

        /// <summary>
        /// 对手方开户行名称
        /// </summary>
        [JsonPropertyName("dsfkhhmc")]
        public string CounterpartyBankName { get; set; }

        /// <summary>
        /// 对方账户号
        /// </summary>
        [JsonPropertyName("dsfzhh")]
        public string CounterpartyAccountNumber { get; set; }

        /// <summary>
        /// 对方账户名
        /// </summary>
        [JsonPropertyName("dsfzhmc")]
        public string CounterpartyAccountName { get; set; }

        /// <summary>
        /// 交易金额
        /// </summary>
        [JsonPropertyName("jyje")]
        public string TransactionAmount { get; set; }

        /// <summary>
        /// 交易日期
        /// </summary>
        [JsonPropertyName("jyrq")]
        public string TransactionDate { get; set; }

        /// <summary>
        /// 交易时间
        /// </summary>
        [JsonPropertyName("jysj")]
        public string TransactionTime { get; set; }

        /// <summary>
        /// 流水号
        /// </summary>
        [JsonPropertyName("lsh")]
        public string TransactionId { get; set; }

        /// <summary>
        /// 募集户开户行
        /// </summary>
        [JsonPropertyName("mjhyh")]
        public string CollectionBank { get; set; }

        /// <summary>
        /// 收付方向（收、付）
        /// </summary>
        [JsonPropertyName("sffx")]
        public string TransactionType { get; set; }

        /// <summary>
        /// 银行摘要
        /// </summary>
        [JsonPropertyName("yhbz")]
        public string BankMemo { get; set; }

        internal BankTransaction ToObject()
        {
            return new BankTransaction
            {
                Id = TransactionId,
                AccountNo = OurAccountNumber,
                AccountBank = CollectionBank,
                AccountName = OurAccountName,
                Serial = TransactionId,
                CounterBank = CounterpartyBankName,
                CounterName = CounterpartyAccountName,
                CounterNo = CounterpartyAccountNumber,
                Amount = decimal.Parse(TransactionAmount),
                Direction = TransactionType == "付" ? TransctionDirection.Pay : TransctionDirection.Receive,
                Remark = BankMemo,
                Time = DateTime.ParseExact(TransactionDate + TransactionTime, "yyyyMMddHH:mm:ss", null)
            };
        }
    }

    public class BankBalanceJson
    {
        /// <summary>
        /// 币种
        /// </summary>
        [JsonPropertyName("bz")]
        public string Currency { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        [JsonPropertyName("clsm")]
        public string Description { get; set; }

        /// <summary>
        /// 产品代码
        /// </summary>
        [JsonPropertyName("cpdm")]
        public string ProductCode { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        [JsonPropertyName("cpmc")]
        public string ProductName { get; set; }

        /// <summary>
        /// 更新时间（格式建议：yyyyMMddHHmmss）
        /// </summary>
        [JsonPropertyName("gxsj")]
        public string UpdateTime { get; set; }

        /// <summary>
        /// 开户行
        /// </summary>
        [JsonPropertyName("khzh")]
        public string BankName { get; set; }

        /// <summary>
        /// 可用余额
        /// </summary>
        [JsonPropertyName("kyye")]
        public string AvailableBalance { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        [JsonPropertyName("zhh")]
        public string AccountNo { get; set; }

        /// <summary>
        /// 账号名称
        /// </summary>
        [JsonPropertyName("zhmc")]
        public string AccountName { get; set; }

        /// <summary>
        /// 账号余额
        /// </summary>
        [JsonPropertyName("zhye")]
        public string AccountBalance { get; set; }

        public FundBankBalance ToObject()
        {
            return new FundBankBalance
            {
                FundCode = ProductCode,
                FundName = ProductName,
                Currency = Currency,
                AccountName = AccountName,
                AccountNo = AccountNo,
                Name = BankName,
                Balance = decimal.Parse(AccountBalance),
                Time = DateTime.ParseExact(UpdateTime, "yyyy-MM-dd HH:mm:ss", null)
            };
        }
    }



    public class InvestorJson
    {
        /// <summary>
        /// 客户名称，最大长度200
        /// </summary>
        [JsonPropertyName("custName")]
        public string CustName { get; set; }

        /// <summary>
        /// 客户类型，最大长度30
        /// </summary>
        [JsonPropertyName("custType")]
        public string CustType { get; set; }

        /// <summary>
        /// 证件类型，最大长度50
        /// </summary>
        [JsonPropertyName("certificateType")]
        public string CertificateType { get; set; }

        /// <summary>
        /// 证件号码，最大长度30
        /// </summary>
        [JsonPropertyName("certificateNo")]
        public string CertificateNo { get; set; }

        /// <summary>
        /// 基金账号，最大长度20
        /// </summary>
        [JsonPropertyName("taAccountId")]
        public string TaAccountId { get; set; }

        /// <summary>
        /// 交易账号，最大长度30
        /// </summary>
        [JsonPropertyName("transactionAccountId")]
        public string TransactionAccountId { get; set; }

        /// <summary>
        /// 银行账户名称，最大长度300
        /// </summary>
        [JsonPropertyName("acctName")]
        public string AcctName { get; set; }

        /// <summary>
        /// 银行账号，最大长度20
        /// </summary>
        [JsonPropertyName("acctNo")]
        public string AcctNo { get; set; }

        /// <summary>
        /// 开户行名称，最大长度50
        /// </summary>
        [JsonPropertyName("clearingAgency")]
        public string ClearingAgency { get; set; }

        /// <summary>
        /// 开户行省份，最大长度50
        /// </summary>
        [JsonPropertyName("provinces")]
        public string Provinces { get; set; }

        /// <summary>
        /// 开户行城市，最大长度50
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; }

        /// <summary>
        /// 基金账号开户日期，格式：yyyymmdd
        /// </summary>
        [JsonPropertyName("openDate")]
        public string OpenDate { get; set; }

        /// <summary>
        /// 客户所属销售渠道，最大长度50
        /// </summary>
        [JsonPropertyName("distributorName")]
        public string DistributorName { get; set; }

        /// <summary>
        /// 销售渠道代码，最大长度3
        /// </summary>
        [JsonPropertyName("distributorCode")]
        public string DistributorCode { get; set; }

        /// <summary>
        /// 是否专业投资机构，最大长度10
        /// </summary>
        [JsonPropertyName("individualOrInstitution")]
        public string IndividualOrInstitution { get; set; }


        public Investor ToObject()
        {
            return new Investor
            {
                Name = CustName,
                Identity = new Identity { Id = CertificateNo, Type = ParseIdType(CertificateType) },
                EntityType = ParseCustomerType(CustType),

            };
        }

        private EntityType ParseCustomerType(string custType)
        {
            return custType switch
            {
                "个人"=> EntityType.Natural,
                "机构" => EntityType.Institution,
                "产品"=> EntityType.Product,
                _ => EntityType.Unk
            };
        }

        private IDType ParseIdType(string certificateType)
        {
            return certificateType switch
            {
                "0" or "未知证件类型" or "未知" => IDType.Unknown,
                "1" or "身份证" or "居民身份证" => IDType.IdentityCard,
                "2" or "社保卡" => IDType.Unknown, // 可根据实际情况映射
                "3" or "中国护照" => IDType.PassportChina,
                "4" or "军官证" => IDType.OfficerID,
                "5" or "士兵证" => IDType.SoldierID,
                "6" or "港澳居民来往内地通行证" => IDType.HongKongMacauPass,
                "7" or "户口本" => IDType.HouseholdRegister,
                "8" or "外国护照" => IDType.PassportForeign,
                "9" or "其他" or "其他证件" => IDType.Other,
                "10" or "文职证" => IDType.CivilianID,
                "11" or "警官证" => IDType.PoliceID,
                "12" or "台胞证" => IDType.TaiwanCompatriotsID,
                "13" or "外国人永久居留身份证" => IDType.ForeignPermanentResidentID,
                "20" => IDType.Unknown,
                "21" or "备案证明" => IDType.ProductFilingCode, // 可根据实际情况映射
                "22" or "组织机构代码" or "组织机构代码证" => IDType.OrganizationCode,
                "23" or "社会统一信用代码" or "统一社会信用代码" => IDType.UnifiedSocialCreditCode,
                "24" or "工商注册号" or "注册号" => IDType.RegistrationNumber,
                "25" or "营业执照" or "营业执照号" => IDType.BusinessLicenseNumber,
                "26" or "行政机关" => IDType.Other, // 可根据实际情况映射
                "27" or "社会团体" => IDType.Other, // 可根据实际情况映射
                "28" or "军队" => IDType.Other, // 可根据实际情况映射
                "29" or "武警" => IDType.Other, // 可根据实际情况映射
                "30" or "下属机构（具有主管单位批文号）" => IDType.Other, // 可根据实际情况映射
                "31" or "基金会" => IDType.Other, // 可根据实际情况映射
                "32" or "登记证书" => IDType.Other, // 可根据实际情况映射
                "33" or "批文" => IDType.Other, // 可根据实际情况映射
                "34" or "其它" => IDType.Other,
                "40" => IDType.Other, // 可根据实际情况映射
                "41" => IDType.BusinessLicenseNumber,
                "42" => IDType.Other, // 可根据实际情况映射
                "43" => IDType.Other,
                "管理人登记编码" => IDType.ManagerRegistrationCode,
                "产品备案编码" => IDType.ProductFilingCode,
                "证券业务许可证" => IDType.SecuritiesBusinessLicense,
                "产品登记编码" => IDType.ProductRegistrationCode,
                "港澳台居民居住证" => IDType.ResidencePermitForHongKongMacaoAndTaiwanResidents,
                "信托登记系统产品编码" => IDType.TrustRegistrationSystemProductCode,
                _ => IDType.Unknown,
            };
        }
    }


    public static TransferRecordType Translate(string c)
    {
        return c switch
        {
            //"120" => TARecordType.Subscription, //"认购确认" 此项没有份额数据, 用认购结果
            "122" => TransferRecordType.Purchase,     //"申购确认",
            "124" => TransferRecordType.Redemption,// "赎回确认",
            "126" => TransferRecordType.MoveIn,   //"转托确认",
            "127" => TransferRecordType.MoveIn,   //"转销售人/机构转入",
            "128" => TransferRecordType.MoveOut,  //"转销售人/机构转出",
            "129" => TransferRecordType.BonusType,//"分红方式",
            "130" => TransferRecordType.Subscription, // "认购结果",
            "131" => TransferRecordType.Frozen,       //"基金份数冻结",
            "132" => TransferRecordType.Thawed,       //"基金份数解冻",
            //"133" => TARecordType.TransferIn,   //"非交易过户",
            "134" => TransferRecordType.TransferIn,   //"非交易过户转入",
            "135" => TransferRecordType.TransferOut,  //"非交易过户转出",
            //"136" => TARecordType.SwitchIn,     //"基金转换",
            "137" => TransferRecordType.SwitchIn,     //"基金转换转入",
            "138" => TransferRecordType.SwitchOut,    //"基金转换转出",


            "139" => TransferRecordType.Purchase,     //"定时定额申购",
            "142" => TransferRecordType.ForceRedemption,//"强制赎回",
            "143" => TransferRecordType.Distribution,     //"分红确认",
            "144" => TransferRecordType.Increase,     //"强行调增",
            "145" => TransferRecordType.Decrease,     //"强行调减",
            _ => TransferRecordType.UNK,              //"未知业务类型"
        };
    }


    public static TransferRequestType TranslateRequest(string c)
    {
        return c switch
        {
            "122" => TransferRequestType.Purchase,
            "124" => TransferRequestType.Redemption,
            "130" => TransferRequestType.Subscription,
            "142" => TransferRequestType.ForceRedemption,
            "144" => TransferRequestType.Increase,     //"强行调增", 份额类型调整
            "145" => TransferRequestType.Decrease,     //"强行调减",
            _ => TransferRequestType.UNK
        };
    }
}



#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。