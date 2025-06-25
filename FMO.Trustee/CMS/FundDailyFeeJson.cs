using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;


#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
public partial class CMS
{
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


    
     
}



#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。