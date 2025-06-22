using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;

public partial class CMS
{
    public class JsonRoot
    {
        [JsonPropertyName("resultcode")]
        public required string Code { get; set; }


        [JsonPropertyName("msg")]
        public required string Msg { get; set; }


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
        public required string Code { get; set; }


        [JsonPropertyName("msg")]
        public required string Msg { get; set; }


        [JsonPropertyName("page")]
        public required PaginationInfo Page { get; set; }

        [JsonPropertyName("data")]
        public required T[] Data { get; set; }
    }




    public class SubjectFundMappingJson
    {
        [JsonPropertyName("gradeNo")]
        public required string GradeNo { get; set; }

        [JsonPropertyName("gradeName")]
        public required string GradeName { get; set; }

        [JsonPropertyName("productNo")]
        public required string ProductNo { get; set; }

        [JsonPropertyName("productName")]
        public required string ProductName { get; set; }

        [JsonPropertyName("isGrade")]
        public bool IsGrade { get; set; }

        [JsonPropertyName("registerNo")]
        public required string RegisterNo { get; set; }

        public SubjectFundMapping ToObject()
        {
            var sc = IsGrade ? GradeName.Replace(ProductName, "") : "";
            return new SubjectFundMapping { FundCode = GradeNo, FundName = GradeName, MasterCode = ProductNo, MasterName = ProductName, ShareClass = sc };
        }
    }


    public class TransferRecordJson
    {

        [JsonPropertyName("custName")]
        public required string CustName { get; set; }

        [JsonPropertyName("custType")]
        public required string CustType { get; set; }

        [JsonPropertyName("certificateType")]
        public required string CertificateType { get; set; }

        [JsonPropertyName("certificateNo")]
        public required string CertificateNo { get; set; }

        [JsonPropertyName("taAccountId")]
        public required string TaAccountId { get; set; }

        [JsonPropertyName("transactionAccountId")]
        public required string TransactionAccountId { get; set; }

        [JsonPropertyName("fundName")]
        public required string FundName { get; set; }

        [JsonPropertyName("fundCode")]
        public required string FundCode { get; set; }

        [JsonPropertyName("businessCode")]
        public required string BusinessCode { get; set; }

        [JsonPropertyName("applicationAmount")]
        public required string ApplicationAmount { get; set; } // 保留两位小数

        [JsonPropertyName("applicationVol")]
        public required string ApplicationVol { get; set; } // 保留两位小数

        [JsonPropertyName("transactionDate")]
        public required string TransactionDate { get; set; } // 格式：yyyymmdd

        [JsonPropertyName("nav")]
        public required string Nav { get; set; } // 清盘日可保留8位小数

        [JsonPropertyName("transactionCfmDate")]
        public required string TransactionCfmDate { get; set; } // 格式：yyyymmdd

        [JsonPropertyName("confirmedVol")]
        public required string ConfirmedVol { get; set; } // 保留两位小数

        [JsonPropertyName("confirmedAmount")]
        public required string ConfirmedAmount { get; set; } // 保留两位小数

        [JsonPropertyName("confirmedNavVol")]
        public required string ConfirmedNavVol { get; set; } // 保留两位小数

        [JsonPropertyName("charge")]
        public required string Charge { get; set; } // 保留两位小数

        [JsonPropertyName("performance")]
        public required string Performance { get; set; } // 保留两位小数

        [JsonPropertyName("distributorCode")]
        public required string DistributorCode { get; set; }

        [JsonPropertyName("distributorName")]
        public required string DistributorName { get; set; }

        [JsonPropertyName("remark1")]
        public required string Remark1 { get; set; }

        [JsonPropertyName("remark2")]
        public required string Remark2 { get; set; }

        [JsonPropertyName("note")]
        public required string Note { get; set; }

        [JsonPropertyName("origDefNo")]
        public required string OrigDefNo { get; set; }

        [JsonPropertyName("shareBonusType")]
        public required string ShareBonusType { get; set; }

        [JsonPropertyName("attributionManagerFee")]
        public required string AttributionManagerFee { get; set; } // 保留两位小数

        [JsonPropertyName("attributionFundAssetFee")]
        public required string AttributionFundAssetFee { get; set; } // 保留两位小数

        [JsonPropertyName("interest")]
        public required string Interest { get; set; } // 保留两位小数

        [JsonPropertyName("attributionSellAgencyFee")]
        public required string AttributionSellAgencyFee { get; set; } // 保留两位小数

        [JsonPropertyName("applyNo")]
        public required string ApplyNo { get; set; }

        public TransferRecord ToObject()
        {
            return new TransferRecord
            {
                CustomerIdentity = CertificateNo,
                CustomerName = CustName,
                Agency = DistributorName,
                RequestDate = DateOnly.ParseExact(TransactionDate, "yyyyMMdd"),
                RequestAmount = decimal.Parse(ApplicationAmount),
                RequestShare = decimal.Parse(ApplicationVol),
                ConfirmedDate = DateOnly.ParseExact(TransactionCfmDate, "yyyyMMdd"),
                ConfirmedAmount = decimal.Parse(ConfirmedAmount),
                ConfirmedShare = decimal.Parse(ConfirmedVol),
                ConfirmedNetAmount = decimal.Parse(ConfirmedNavVol),
                CreateDate = DateOnly.FromDateTime(DateTime.Today),
                ExternalId = Remark1,
                Type = Translate(BusinessCode),
                Fee = decimal.Parse(Charge),
                PerformanceFee = decimal.Parse(Performance),
                ExternalRequestId = ApplyNo,
                FundCode = FundCode,
                FundName = FundName,
                Source = "api",
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
        public required string FundName { get; set; }

        /// <summary>
        /// 产品代码（必填，最大长度6）
        /// </summary>
        [JsonPropertyName("fundCode")]
        public required string FundCode { get; set; }

        /// <summary>
        /// 费用日期（格式：yyyyMMdd，必填）
        /// </summary>
        [JsonPropertyName("busiDate")]
        public required string BusiDate { get; set; }

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
        public required string OurAccountNumber { get; set; }

        /// <summary>
        /// 本方账户名
        /// </summary>
        [JsonPropertyName("bfzhmc")]
        public required string OurAccountName { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        [JsonPropertyName("bz")]
        public required string Currency { get; set; }

        /// <summary>
        /// 产品代码
        /// </summary>
        [JsonPropertyName("cpdm")]
        public required string ProductCode { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        [JsonPropertyName("cpmc")]
        public required string ProductName { get; set; }

        /// <summary>
        /// 对手方开户行名称
        /// </summary>
        [JsonPropertyName("dsfkhhmc")]
        public required string CounterpartyBankName { get; set; }

        /// <summary>
        /// 对方账户号
        /// </summary>
        [JsonPropertyName("dsfzhh")]
        public required string CounterpartyAccountNumber { get; set; }

        /// <summary>
        /// 对方账户名
        /// </summary>
        [JsonPropertyName("dsfzhmc")]
        public required string CounterpartyAccountName { get; set; }

        /// <summary>
        /// 交易金额
        /// </summary>
        [JsonPropertyName("jyje")]
        public required string TransactionAmount { get; set; }

        /// <summary>
        /// 交易日期
        /// </summary>
        [JsonPropertyName("jyrq")]
        public required string TransactionDate { get; set; }

        /// <summary>
        /// 交易时间
        /// </summary>
        [JsonPropertyName("jysj")]
        public required string TransactionTime { get; set; }

        /// <summary>
        /// 流水号
        /// </summary>
        [JsonPropertyName("lsh")]
        public required string TransactionId { get; set; }

        /// <summary>
        /// 募集户开户行
        /// </summary>
        [JsonPropertyName("mjhyh")]
        public required string CollectionBank { get; set; }

        /// <summary>
        /// 收付方向（收、付）
        /// </summary>
        [JsonPropertyName("sffx")]
        public required string TransactionType { get; set; }

        /// <summary>
        /// 银行摘要
        /// </summary>
        [JsonPropertyName("yhbz")]
        public required string BankMemo { get; set; }

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



    public static TARecordType Translate(string c)
    {
        return c switch
        {
            //"120" => TARecordType.Subscription, //"认购确认" 此项没有份额数据, 用认购结果
            "122" => TARecordType.Purchase,     //"申购确认",
            "124" => TARecordType.Redemption,// "赎回确认",
            "126" => TARecordType.MoveIn,   //"转托确认",
            "127" => TARecordType.MoveIn,   //"转销售人/机构转入",
            "128" => TARecordType.MoveOut,  //"转销售人/机构转出",
            "129" => TARecordType.BonusType,//"分红方式",
            "130" => TARecordType.Subscription, // "认购结果",
            "131" => TARecordType.Frozen,       //"基金份数冻结",
            "132" => TARecordType.Thawed,       //"基金份数解冻",
            //"133" => TARecordType.TransferIn,   //"非交易过户",
            "134" => TARecordType.TransferIn,   //"非交易过户转入",
            "135" => TARecordType.TransferOut,  //"非交易过户转出",
            //"136" => TARecordType.SwitchIn,     //"基金转换",
            "137" => TARecordType.SwitchIn,     //"基金转换转入",
            "138" => TARecordType.SwitchOut,    //"基金转换转出",


            "139" => TARecordType.Purchase,     //"定时定额申购",
            "142" => TARecordType.ForceRedemption,//"强制赎回",
            "143" => TARecordType.Distribution,     //"分红确认",
            "144" => TARecordType.Increase,     //"强行调增",
            "145" => TARecordType.Decrease,     //"强行调减",
            _ => TARecordType.UNK,              //"未知业务类型"
        };
    }
}
