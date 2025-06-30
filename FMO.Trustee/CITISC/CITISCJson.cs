using FMO.Models;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FMO.Trustee;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
public partial class CITICS
{

    public class RootJson
    {
        [JsonPropertyName("data")]
        public JsonObject? Data { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; } = -1;

        [JsonPropertyName("message")]
        public string? Msg { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

    }



    public class ReturnJsonRoot<T>
    {
        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Msg { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

    }



    public class QueryRoot<T>
    {
        /// <summary>
        /// 当前页
        /// </summary>
        [JsonPropertyName("pageNum")]
        public int PageNum { get; set; }

        /// <summary>
        /// 当前页的数量
        /// </summary>
        [JsonPropertyName("size")]
        public int Size { get; set; }

        /// <summary>
        /// 结果
        /// </summary>
        [JsonPropertyName("list")]
        public List<T>? List { get; set; }

        /// <summary>
        /// 下一页
        /// </summary>
        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage { get; set; }


        [JsonPropertyName("pages")]
        public int Pages { get; set; }

        [JsonPropertyName("navigatePages")]
        public int NavPages { get; set; }


        public int PageCount => Pages == 0 ? NavPages : Pages;

    }


    public class TokenJson
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }


    public class InvestorJson
    {
        [JsonPropertyName("custName")]
        public string CustName { get; set; } // 投资者名称

        [JsonPropertyName("fundAcco")]
        public string FundAcco { get; set; } // 基金账号

        [JsonPropertyName("tradeAcco")]
        public string TradeAcco { get; set; } // 交易账号

        [JsonPropertyName("custType")]
        public string CustType { get; set; } // 客户类型（参见附录4）

        [JsonPropertyName("certiType")]
        public string CertiType { get; set; } // 证件类型（参见附录4）

        [JsonPropertyName("certiNo")]
        public string CertiNo { get; set; } // 证件号

        [JsonPropertyName("bankNo")]
        public string? BankNo { get; set; } // 银行编号

        [JsonPropertyName("bankAccount")]
        public string? BankAccount { get; set; } // 银行账号

        [JsonPropertyName("bankOpenName")]
        public string? BankOpenName { get; set; } // 开户行名称

        [JsonPropertyName("bankAccountName")]
        public string? BankAccountName { get; set; } // 银行户名

        [JsonPropertyName("address")]
        public string? Address { get; set; } // 通讯地址

        [JsonPropertyName("tel")]
        public string? Tel { get; set; } // 联系电话

        [JsonPropertyName("zipCode")]
        public string? ZipCode { get; set; } // 邮编

        [JsonPropertyName("agencyNo")]
        public string? AgencyNo { get; set; } // 销售商代码，ZX6表示直销

        [JsonPropertyName("email")]
        public string? Email { get; set; } // 邮箱

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

    }

    public static IDType ParseIdType(string custType, string certiType)
    {
        if (string.IsNullOrEmpty(custType) || string.IsNullOrEmpty(certiType))
            return IDType.Unknown;

        // 处理客户类型：0=机构，1=个人，2=产品
        switch (custType)
        {
            case "0": // 机构
                return certiType.ToUpper() switch
                {
                    "0" => IDType.OrganizationCode, // 组织机构代码证
                    "1" => IDType.BusinessLicenseNumber,      // 营业执照
                    "2" => IDType.RegistrationNumber,         // 行政机关
                    "3" => IDType.OrganizationCode,           // 社会团体
                    "4" => IDType.Other,                     // 军队
                    "5" => IDType.Other,                     // 武警
                    "6" => IDType.Other,                     // 下属机构
                    "7" => IDType.Other,                     // 基金会
                    "8" => IDType.Other,                     // 其他机构
                    "9" => IDType.ProductFilingCode,         // 登记证书
                    "A" => IDType.ManagerRegistrationCode,   // 批文
                    _ => IDType.Unknown
                };

            case "1": // 个人
                return certiType.ToUpper() switch
                {
                    "0" => IDType.IdentityCard,                    // 身份证
                    "1" => IDType.PassportChina,                   // 中国护照
                    "2" => IDType.OfficerID,                       // 军官证
                    "3" => IDType.SoldierID,                       // 士兵证
                    "4" => IDType.HongKongMacauPass,               // 港澳居民来往内地通行证
                    "5" => IDType.HouseholdRegister,               // 户口本
                    "6" => IDType.PassportForeign,                 // 外籍护照
                    "7" => IDType.Other,                           // 其他
                    "8" => IDType.CivilianID,                      // 文职证
                    "9" => IDType.PoliceID,                        // 警官证
                    "A" => IDType.TaiwanCompatriotsID,             // 台胞证
                    "B" => IDType.ForeignPermanentResidentID,      // 外国人永久居留身份证
                    "C" => IDType.ResidencePermitForHongKongMacaoAndTaiwanResidents, // 港澳台居民居住证
                    _ => IDType.Unknown
                };

            case "2": // 产品
                return certiType.ToUpper() switch
                {
                    "1" => IDType.BusinessLicenseNumber, // 营业执照（直销接口不允许使用）
                    "8" => IDType.Other,                 // 其他
                    "9" => IDType.ProductFilingCode,     // 登记证书（直销接口不允许使用）
                    "A" => IDType.ManagerRegistrationCode, // 批文
                    _ => IDType.Unknown
                };

            default:
                return IDType.Unknown;
        }
    }


    /// <summary>
    /// 金融产品费用数据模型
    /// </summary>
    public class FundDailyFeeJson
    {
        /// <summary>
        /// 产品代码
        /// </summary>
        [JsonPropertyName("fcpdm")]
        public string ProductCode { get; set; }

        /// <summary>
        /// 分级产品代码
        /// </summary>
        [JsonPropertyName("ffjdm")]
        public string? ClassificationCode { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        [JsonPropertyName("fcpmc")]
        public string ProductName { get; set; }

        /// <summary>
        /// 业务日期，格式：YYYY - MM - DD
        /// </summary>
        [JsonPropertyName("cdate")]
        public string BusinessDate { get; set; }

        #region 管理费相关
        /// <summary>
        /// 计提管理费
        /// </summary>
        [JsonPropertyName("jtglf")]
        public string AccruedManagementFee { get; set; }

        /// <summary>
        /// 支付管理费
        /// </summary>
        [JsonPropertyName("zfglf")]
        public string PaidManagementFee { get; set; }

        /// <summary>
        /// 未支付管理费
        /// </summary>
        [JsonPropertyName("wzfglf")]
        public string UnpaidManagementFee { get; set; }
        #endregion

        #region 业绩报酬相关
        /// <summary>
        /// 计提业绩报酬
        /// </summary>
        [JsonPropertyName("jtyjbc")]
        public string AccruedPerformanceFee { get; set; }

        /// <summary>
        /// 支付业绩报酬
        /// </summary>
        [JsonPropertyName("zfyjbc")]
        public string PaidPerformanceFee { get; set; }

        /// <summary>
        /// 未支付业绩报酬
        /// </summary>
        [JsonPropertyName("wzfyjbc")]
        public string UnpaidPerformanceFee { get; set; }
        #endregion

        #region 托管费相关
        /// <summary>
        /// 计提托管费
        /// </summary>
        [JsonPropertyName("jttgf")]
        public string AccruedCustodyFee { get; set; }

        /// <summary>
        /// 支付托管费
        /// </summary>
        [JsonPropertyName("zftgf")]
        public string PaidCustodyFee { get; set; }

        /// <summary>
        /// 未支付托管费
        /// </summary>
        [JsonPropertyName("wzftgf")]
        public string UnpaidCustodyFee { get; set; }
        #endregion

        #region 行政服务费相关
        /// <summary>
        /// 计提行政服务费
        /// </summary>
        [JsonPropertyName("jtxzfwf")]
        public string AccruedAdministrativeFee { get; set; }

        /// <summary>
        /// 支付行政服务费
        /// </summary>
        [JsonPropertyName("zfxzfwf")]
        public string PaidAdministrativeFee { get; set; }

        /// <summary>
        /// 未支付行政服务费
        /// </summary>
        [JsonPropertyName("wzfxzfwf")]
        public string UnpaidAdministrativeFee { get; set; }
        #endregion

        #region 销售服务费相关
        /// <summary>
        /// 计提销售服务费
        /// </summary>
        [JsonPropertyName("jtxsfwf")]
        public string AccruedSalesServiceFee { get; set; }

        /// <summary>
        /// 支付销售服务费
        /// </summary>
        [JsonPropertyName("zfxsfwf")]
        public string PaidSalesServiceFee { get; set; }

        /// <summary>
        /// 未支付销售服务费
        /// </summary>
        [JsonPropertyName("wzfxsfwf")]
        public string UnpaidSalesServiceFee { get; set; }
        #endregion

        #region 投资顾问费相关
        /// <summary>
        /// 计提投资顾问费
        /// </summary>
        [JsonPropertyName("jttzgwf")]
        public string AccruedInvestmentAdvisoryFee { get; set; }

        /// <summary>
        /// 支付投资顾问费
        /// </summary>
        [JsonPropertyName("zftzgwf")]
        public string PaidInvestmentAdvisoryFee { get; set; }

        /// <summary>
        /// 未支付投资顾问费
        /// </summary>
        [JsonPropertyName("wzftzgwf")]
        public string UnpaidInvestmentAdvisoryFee { get; set; }
        #endregion

        #region 增值税附加税相关
        /// <summary>
        /// 计提增值税附加税
        /// </summary>
        [JsonPropertyName("jtzzsfjs")]
        public string AccruedVatSurcharge { get; set; }

        /// <summary>
        /// 支付增值税附加税
        /// </summary>
        [JsonPropertyName("zfzzsfjs")]
        public string PaidVatSurcharge { get; set; }

        /// <summary>
        /// 未支付增值税附加税
        /// </summary>
        [JsonPropertyName("wzfzzsfjs")]
        public string UnpaidVatSurcharge { get; set; }
        #endregion

        #region 审计费相关
        /// <summary>
        /// 计提审计费
        /// </summary>
        [JsonPropertyName("jtsjf")]
        public string AccruedAuditFee { get; set; }

        /// <summary>
        /// 支付审计费
        /// </summary>
        [JsonPropertyName("zfsjf")]
        public string PaidAuditFee { get; set; }

        /// <summary>
        /// 未支付审计费
        /// </summary>
        [JsonPropertyName("wzfsjf")]
        public string UnpaidAuditFee { get; set; }
        #endregion


        public FundDailyFee ToObject()
        {
            var code = ClassificationCode switch { "M" => ProductCode, null => ProductCode, var n => n };



            return new FundDailyFee
            {
                FundCode = code,
                Date = DateOnly.ParseExact(BusinessDate, "yyyy-MM-dd"),
                ManagerFeeAccrued = ParseDecimal(AccruedManagementFee),
                ManagerFeePaid = ParseDecimal(PaidManagementFee),
                ManagerFeeBalance = ParseDecimal(UnpaidManagementFee),
                PerformanceFeeAccrued = ParseDecimal(AccruedPerformanceFee),
                PerformanceFeePaid = ParseDecimal(PaidPerformanceFee),
                PerformanceFeeBalance = ParseDecimal(UnpaidPerformanceFee),
                CustodianFeeAccrued = ParseDecimal(AccruedCustodyFee),
                ConsultantFeePaid = ParseDecimal(PaidCustodyFee),
                ConsultantFeeBalance = ParseDecimal(UnpaidCustodyFee),
                OutsourcingFeeAccrued = ParseDecimal(AccruedAdministrativeFee),
                OutsourcingFeePaid = ParseDecimal(PaidAdministrativeFee),
                OutsourcingFeeBalance = ParseDecimal(UnpaidAdministrativeFee),
                SalesFeeAccrued = ParseDecimal(AccruedSalesServiceFee),
                SalesFeePaid = ParseDecimal(PaidSalesServiceFee),
                SalesFeeBalance = ParseDecimal(UnpaidSalesServiceFee),
                ConsultantFeeAccrued = ParseDecimal(AccruedInvestmentAdvisoryFee),
                CustodianFeePaid = ParseDecimal(PaidInvestmentAdvisoryFee),
                CustodianFeeBalance = ParseDecimal(UnpaidInvestmentAdvisoryFee),
            };
        }

        private decimal ParseDecimal(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            if (decimal.TryParse(value, out var result))
                return result;

            throw new FormatException($"无法将 '{value}' 解析为decimal类型");
        }
    }






    public class BankTransactionJson
    {

        /// <summary>
        /// 交易发生时间，格式为 YYYY-MM-DD HH:MM:SS
        /// </summary>
        [JsonPropertyName("occurTime")]
        public string OccurTime { get; set; }

        /// <summary>
        /// 基金代码
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// 基金名称
        /// </summary>
        [JsonPropertyName("fundName")]
        public string FundName { get; set; }

        /// <summary>
        /// 账户类型，例如 "02-募集户"
        /// </summary>
        [JsonPropertyName("accoType")]
        public string AccoType { get; set; }

        /// <summary>
        /// 本方账号
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public string BankAcco { get; set; }

        /// <summary>
        /// 本方账户名称
        /// </summary>
        [JsonPropertyName("accName")]
        public string AccName { get; set; }

        /// <summary>
        /// 本方开户行名称
        /// </summary>
        [JsonPropertyName("openBankName")]
        public string OpenBankName { get; set; }

        /// <summary>
        /// 本方银行编号，以人行大额付款码前三位为标准（见附录2）
        /// </summary>
        [JsonPropertyName("bankNo")]
        public string BankNo { get; set; }

        /// <summary>
        /// 对方账号
        /// </summary>
        [JsonPropertyName("othBankAcco")]
        public string OthBankAcco { get; set; }

        /// <summary>
        /// 对方账户名称
        /// </summary>
        [JsonPropertyName("othAccName")]
        public string OthAccName { get; set; }

        /// <summary>
        /// 对方开户行名称
        /// </summary>
        [JsonPropertyName("othOpenBankName")]
        public string OthOpenBankName { get; set; }

        /// <summary>
        /// 对方银行编号，以人行大额付款码前三位为标准（见附录2）
        /// </summary>
        [JsonPropertyName("othBankNo")]
        public string OthBankNo { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        [JsonPropertyName("curType")]
        public string CurType { get; set; }

        /// <summary>
        /// 收付方向，1表示收款，2表示付款
        /// </summary>
        [JsonPropertyName("directFlag")]
        public string DirectFlag { get; set; }

        /// <summary>
        /// 发生金额
        /// </summary>
        [JsonPropertyName("occurAmt")]
        public string OccurAmt { get; set; }

        /// <summary>
        /// 账户余额
        /// </summary>
        [JsonPropertyName("acctBal")]
        public string AcctBal { get; set; }

        /// <summary>
        /// 银行流水号
        /// </summary>
        [JsonPropertyName("bankJour")]
        public string BankJour { get; set; }

        /// <summary>
        /// 银行返回代码
        /// </summary>
        [JsonPropertyName("bankRetCode")]
        public string BankRetCode { get; set; }

        /// <summary>
        /// 银行摘要
        /// </summary>
        [JsonPropertyName("bankNote")]
        public string BankNote { get; set; }



        public BankTransaction ToObject()
        {
            return new BankTransaction
            {
                Id = $"{BankAcco}|{BankJour}",
                Time = DateTime.ParseExact(OccurTime, "yyyy-MM-dd HH:mm:ss", null),
                AccountBank = OpenBankName,
                AccountName = AccName,
                AccountNo = BankAcco,
                CounterBank = OthOpenBankName,
                CounterName = OthAccName,
                CounterNo = OthBankAcco,
                Amount = ParseDecimal(OccurAmt),
                Balance = ParseDecimal(AcctBal),
                Direction = DirectFlag == "付款" ? TransctionDirection.Pay : TransctionDirection.Receive,
                Remark = BankNote,
                Serial = BankJour,
            };
        }
    }




    public class CustodialAccountJson
    {
        [JsonPropertyName("pdCode")]
        public string ProductCode { get; set; }

        [JsonPropertyName("pdName")]
        public string ProductName { get; set; }

        [JsonPropertyName("accName")]
        public string AccountName { get; set; }

        [JsonPropertyName("account")]
        public string Account { get; set; }

        [JsonPropertyName("bankName")]
        public string BankName { get; set; }

        [JsonPropertyName("lrgAmtNo")]
        public string LargePaymentNumber { get; set; }

        [JsonPropertyName("recentBalance")]
        public string RecentBalance { get; set; }

        [JsonPropertyName("recentBalanceQueryTime")]
        public string BalanceQueryTime { get; set; }

        /// <summary>
        /// 1：正常 2：销户
        /// </summary>
        [JsonPropertyName("status")]
        public int Status { get; set; }


        public FundBankAccount ToObject()
        {
            return new FundBankAccount
            {
                Name = AccountName,
                BankOfDeposit = BankName,
                FundCode = ProductCode,
                LargePayNo = LargePaymentNumber,
                Number = Account,
                IsCanceled = Status == 2,
            };
        }
    }


    public class VirtualNetValueJson
    {
        /// <summary>
        /// 业务日期，格式：YYYY-MM-DD
        /// </summary>
        [JsonPropertyName("date")]
        public string BusinessDate { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        [JsonPropertyName("custName")]
        public string CustomerName { get; set; }

        /// <summary>
        /// 基金账号
        /// </summary>
        [JsonPropertyName("fundAcco")]
        public string FundAccount { get; set; }

        /// <summary>
        /// 产品代码
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        [JsonPropertyName("fundName")]
        public string FundName { get; set; }

        /// <summary>
        /// 持仓份额
        /// </summary>
        [JsonPropertyName("shares")]
        public decimal Shares { get; set; }

        /// <summary>
        /// 计提方式：1-估值计提；2-TA计提
        /// </summary>
        [JsonPropertyName("flag")]
        public string AccrualMethod { get; set; }

        /// <summary>
        /// 虚拟业绩报酬金额
        /// </summary>
        [JsonPropertyName("virtualBalance")]
        public decimal VirtualPerformanceFee { get; set; }

        /// <summary>
        /// 虚拟净值
        /// </summary>
        [JsonPropertyName("virtualAssetVal")]
        public string VirtualNetAssetValue { get; set; }

        /// <summary>
        /// 实际净值
        /// </summary>
        [JsonPropertyName("netAssetVal")]
        public string ActualNetAssetValue { get; set; }

        /// <summary>
        /// 累计净值
        /// </summary>
        [JsonPropertyName("totalAssetVal")]
        public string TotalNetAssetValue { get; set; }

        /// <summary>
        /// 虚拟扣减份额
        /// </summary>
        [JsonPropertyName("virtualDeductionShare")]
        public decimal VirtualDeductionShare { get; set; }

        /// <summary>
        /// 净值核对状态：
        /// 0-一致，托管复核一致；
        /// 1-不一致，未经托管确认；
        /// 2-处理中；
        /// 3-无托管方产品，无托管方复核
        /// </summary>
        [JsonPropertyName("checkStatus")]
        public string NetValueCheckStatus { get; set; }
    }


    public class BankBalanceJson
    {
        /// <summary>
        /// 账号
        /// </summary>
        [JsonPropertyName("YHZH")]
        public string AccountNumber { get; set; }

        /// <summary>
        /// 账户余额
        /// </summary>
        [JsonPropertyName("ZHYE")]
        public string AccountBalance { get; set; }

        /// <summary>
        /// 账户名称
        /// </summary>
        [JsonPropertyName("KHHM")]
        public string AccountHolderName { get; set; }

        /// <summary>
        /// 货币种类：
        /// HKD-港币；
        /// RMB-人民币；
        /// USD-美元
        /// </summary>
        [JsonPropertyName("JSBZ")]
        public string CurrencyType { get; set; }

        /// <summary>
        /// 余额查询时间，格式：YYYY-MM-DD HH:MM:SS
        /// </summary>
        [JsonPropertyName("CXSJ")]
        public string QueryTime { get; set; }

        /// <summary>
        /// 账户可用余额
        /// </summary>
        [JsonPropertyName("ZHKYYE")]
        public string AvailableBalance { get; set; }

        /// <summary>
        /// 处理结果：
        /// 0-成功；
        /// -2-处理中；
        /// 其他值代表失败
        /// </summary>
        [JsonPropertyName("CLJG")]
        public string ProcessingResult { get; set; }

        /// <summary>
        /// 处理说明
        /// </summary>
        [JsonPropertyName("CLSM")]
        public string ProcessingDescription { get; set; }

        /// <summary>
        /// 开户行
        /// </summary>
        [JsonPropertyName("KHYH")]
        public string OpeningBank { get; set; }

        public BankBalance ToObject()
        {
            return new BankBalance
            {
                AccountName = AccountHolderName,
                AccountNo = AccountNumber,
                Name = OpeningBank,
                Balance = ParseDecimal(AccountBalance),
                Currency = CurrencyType,
                Time = DateTime.ParseExact(QueryTime, "yyyy-MM-dd HH:mm:ss", null)
            };
        }
    }


    public class CustodialTransactionJson
    {
        /// <summary>
        /// 银行代码
        /// </summary>
        [JsonPropertyName("YHDM")]
        public string BankCode { get; set; }

        /// <summary>
        /// 银行名称
        /// </summary>
        [JsonPropertyName("YHMC")]
        public string BankName { get; set; }

        /// <summary>
        /// 付方账号（CLJG为-1时填查询帐号）
        /// </summary>
        [JsonPropertyName("FKFZH")]
        public string PayerAccountNo { get; set; }

        /// <summary>
        /// 付方户名
        /// </summary>
        [JsonPropertyName("FKFHM")]
        public string PayerAccountName { get; set; }

        /// <summary>
        /// 收方账号（CLJG为-1时填查询帐号）
        /// </summary>
        [JsonPropertyName("SKFZH")]
        public string PayeeAccountNo { get; set; }

        /// <summary>
        /// 收方户名
        /// </summary>
        [JsonPropertyName("SKFHM")]
        public string PayeeAccountName { get; set; }

        /// <summary>
        /// 发生金额
        /// </summary>
        [JsonPropertyName("FSJE")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 结算币种
        /// HKD: 港币, RMB: 人民币, USD: 美元
        /// </summary>
        [JsonPropertyName("JSBZ")]
        public string Currency { get; set; }

        /// <summary>
        /// 借贷标志：
        /// 借、贷、付款=借、扣款=借、收款=贷、扣款撤销、收款撤销
        /// </summary>
        [JsonPropertyName("JDBZ")]
        public string DebitCreditFlag { get; set; }

        /// <summary>
        /// 摘要名称
        /// </summary>
        [JsonPropertyName("ZYMC")]
        public string Summary { get; set; }

        /// <summary>
        /// 发生时间，格式 yyyy-MM-dd HH:mm:ss
        /// </summary>
        [JsonPropertyName("FSSJ")]
        public string OccurTime { get; set; }

        /// <summary>
        /// 返回信息
        /// </summary>
        [JsonPropertyName("FHXX")]
        public string ReturnInfo { get; set; }

        /// <summary>
        /// 备用字段
        /// </summary>
        [JsonPropertyName("BYZD")]
        public string ReservedField { get; set; }

        /// <summary>
        /// 账户余额
        /// </summary>
        [JsonPropertyName("ZHYE")]
        public decimal AccountBalance { get; set; }

        /// <summary>
        /// 可用余额
        /// </summary>
        [JsonPropertyName("KYYE")]
        public decimal AvailableBalance { get; set; }

        /// <summary>
        /// 流水号
        /// </summary>
        [JsonPropertyName("LSH")]
        public string SerialNumber { get; set; }

        /// <summary>
        /// 处理结果
        /// 0 - 成功
        /// -2 - 处理中
        /// 其他值代表失败
        /// </summary>
        [JsonPropertyName("CLJG")]
        public string ProcessResult { get; set; }

        /// <summary>
        /// 处理说明
        /// </summary>
        [JsonPropertyName("CLSM")]
        public string ProcessDescription { get; set; }


        public BankTransaction ToObject()
        {
            return new BankTransaction
            {
                AccountBank = BankName,
                AccountName = PayerAccountName,
                AccountNo = PayerAccountNo,
                CounterBank = "",
                CounterName = PayeeAccountName,
                CounterNo = PayeeAccountNo,
                Id = SerialNumber,
                Serial = SerialNumber,
                Amount = Amount,
                Balance = AccountBalance,
                Currency = Currency,
                Direction = DebitCreditFlag switch { string s when s.Contains("撤销") => TransctionDirection.Cancel, "借" or "借款" or "扣款" => TransctionDirection.Pay, _ => TransctionDirection.Receive },
                Remark = Summary,
                Time = DateTime.ParseExact(OccurTime, "yyyy-MM-dd HH:mm:ss", null)
            };
        }

    }






    public class CustodialTransactionJson2
    {
        /// <summary>
        /// 付方账号
        /// </summary>
        [JsonPropertyName("fpayerAcctCode")]
        public string PayerAccountCode { get; set; }

        /// <summary>
        /// 付方户名
        /// </summary>
        [JsonPropertyName("fpayerName")]
        public string PayerName { get; set; }

        /// <summary>
        /// 付方开户行
        /// </summary>
        [JsonPropertyName("fpayerBank")]
        public string PayerBank { get; set; }

        /// <summary>
        /// 收款方账号
        /// </summary>
        [JsonPropertyName("fpayeeAcctCode")]
        public string PayeeAccountCode { get; set; }

        /// <summary>
        /// 收款方户名
        /// </summary>
        [JsonPropertyName("fpayeeName")]
        public string PayeeName { get; set; }

        /// <summary>
        /// 收款方开户行
        /// </summary>
        [JsonPropertyName("fpayeeBank")]
        public string PayeeBank { get; set; }

        /// <summary>
        /// 发生日期（格式：yyyy-MM-dd）
        /// </summary>
        [JsonPropertyName("date")]
        public string OccurDate { get; set; }

        /// <summary>
        /// 收款方向代码：
        /// SFKFX001: 出款
        /// SFKFX002: 入款
        /// SFKFX003: 调拨
        /// </summary>
        [JsonPropertyName("fway")]
        public string DirectionCode { get; set; }

        /// <summary>
        /// 收款方向名称（SFKFX001：出款；SFKFX002：入款；SFKFX003：调拨）
        /// </summary>
        [JsonPropertyName("fwayName")]
        public string DirectionName { get; set; }

        /// <summary>
        /// 发生金额（字符串类型，保留两位小数）
        /// </summary>
        [JsonPropertyName("famount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [JsonPropertyName("fsummary")]
        public string Summary { get; set; }

        public BankTransaction ToObject()
        {
            var direction = DirectionCode switch { "SFKFX001" or "SFKFX003" => TransctionDirection.Pay,/* "SFKFX002"*/ _ => TransctionDirection.Receive };

            return new BankTransaction
            {
                AccountBank = direction == TransctionDirection.Pay ? PayerBank : PayeeBank,
                AccountName = direction == TransctionDirection.Pay ? PayerName : PayeeName,
                AccountNo = direction == TransctionDirection.Pay ? PayerAccountCode : PayeeAccountCode,
                CounterBank = direction != TransctionDirection.Pay ? PayerBank : PayeeBank,
                CounterName = direction != TransctionDirection.Pay ? PayerName : PayeeName,
                CounterNo = direction != TransctionDirection.Pay ? PayerAccountCode : PayeeAccountCode,
                Id = $"{PayeeAccountCode.GetHashCode()}|{PayerAccountCode.GetHashCode()}|{OccurDate}{Amount}",
                Serial = $"{PayeeAccountCode.GetHashCode()}|{PayerAccountCode.GetHashCode()}|{OccurDate}{Amount}",
                Amount = Amount,
                Direction = direction,
                Remark = Summary,
                Time = DateTime.ParseExact(OccurDate, "yyyy-MM-dd HH:mm:ss", null)
            };
        }
    }



    public class RaisingBalanceJson
    {
        /// <summary>
        /// 发生时间（格式：YYYY-MM-DD HH:mm:ss）
        /// </summary>
        [JsonPropertyName("occurTime")]
        public string OccurTime { get; set; }

        /// <summary>
        /// 基金代码
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// 基金名称
        /// </summary>
        [JsonPropertyName("fundName")]
        public string FundName { get; set; }

        /// <summary>
        /// 账户类型
        /// 02 - 募集户
        /// </summary>
        [JsonPropertyName("accoType")]
        public string AccountType { get; set; }

        /// <summary>
        /// 银行账号
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public string BankAccountNo { get; set; }

        /// <summary>
        /// 银行账户名称
        /// </summary>
        [JsonPropertyName("accName")]
        public string BankAccountName { get; set; }

        /// <summary>
        /// 银行开户行名称
        /// </summary>
        [JsonPropertyName("openBankName")]
        public string OpenBankName { get; set; }

        /// <summary>
        /// 银行编号（以人行大额付款码前三位为标准）
        /// </summary>
        [JsonPropertyName("bankNo")]
        public string BankNumber { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        [JsonPropertyName("curType")]
        public string Currency { get; set; }

        /// <summary>
        /// 账户余额
        /// </summary>
        [JsonPropertyName("acctBal")]
        public string AccountBalance { get; set; }

        /// <summary>
        /// 银行返回代码
        /// </summary>
        [JsonPropertyName("bankRetCode")]
        public string BankReturnCode { get; set; }

        /// <summary>
        /// 银行摘要
        /// </summary>
        [JsonPropertyName("bankNote")]
        public string BankSummary { get; set; }

        /// <summary>
        /// CNAPS 大额支付号
        /// </summary>
        [JsonPropertyName("cnapsCode")]
        public string CNAPSCode { get; set; }


        public FundBankBalance ToObject()
        {
            return new FundBankBalance
            {
                FundCode = FundCode,
                FundName = FundName,
                AccountName = BankAccountName,
                AccountNo = BankNumber,
                Name = OpenBankName,
                Balance = ParseDecimal(AccountBalance),
                Currency = Currency,
                Time = DateTime.ParseExact(OccurTime, "yyyy-MM-dd HH:mm:ss", null),
            };
        }
    }




    public class TransferRequestJson
    {
        /// <summary>
        /// 申请编号
        /// </summary>
        [JsonPropertyName("requestNo")]
        public string RequestNo { get; set; }

        /// <summary>
        /// 申请日期（格式：YYYYMMDD）
        /// </summary>
        [JsonPropertyName("requestDate")]
        public string RequestDate { get; set; }

        /// <summary>
        /// 申请时间（格式：HHMMSS）
        /// </summary>
        [JsonPropertyName("requestTime")]
        public string RequestTime { get; set; }

        /// <summary>
        /// 基金代码
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// 业务类型
        /// 参照 BusinFlagEnum 枚举定义
        /// </summary>
        [JsonPropertyName("businFlag")]
        public string BusinFlag { get; set; }

        /// <summary>
        /// 交易账号
        /// </summary>
        [JsonPropertyName("tradeAcco")]
        public string TradeAccount { get; set; }

        /// <summary>
        /// 基金帐号
        /// </summary>
        [JsonPropertyName("fundAcco")]
        public string FundAccount { get; set; }

        /// <summary>
        /// 销售商代码
        /// </summary>
        [JsonPropertyName("agencyNo")]
        public string AgencyCode { get; set; }

        /// <summary>
        /// 销售商名称
        /// </summary>
        [JsonPropertyName("agencyName")]
        public string AgencyName { get; set; }

        /// <summary>
        /// 申请金额
        /// </summary>
        [JsonPropertyName("balance")]
        public decimal ApplyAmount { get; set; }

        /// <summary>
        /// 申请份额
        /// </summary>
        [JsonPropertyName("shares")]
        public decimal ApplyShares { get; set; }

        /// <summary>
        /// 银行账号
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public string BankAccountNo { get; set; }

        /// <summary>
        /// 银行开户行
        /// </summary>
        [JsonPropertyName("bankName")]
        public string BankName { get; set; }

        /// <summary>
        /// 银行户名
        /// </summary>
        [JsonPropertyName("nameInBank")]
        public string BankAccountName { get; set; }

        /// <summary>
        /// 网点代码
        /// </summary>
        [JsonPropertyName("netNo")]
        public string BranchCode { get; set; }

        /// <summary>
        /// 指定费用
        /// </summary>
        [JsonPropertyName("definedFee")]
        public decimal DefinedFee { get; set; }

        /// <summary>
        /// 折扣
        /// </summary>
        [JsonPropertyName("agio")]
        public string DiscountRate { get; set; }

        /// <summary>
        /// 巨额赎回方式：
        /// 0 - 取消
        /// 1 - 顺延
        /// null - 默认顺延
        /// </summary>
        [JsonPropertyName("exceedFlag")]
        public string? ExceedFlag { get; set; }

        /// <summary>
        /// 币种：人民币（默认值）
        /// </summary>
        [JsonPropertyName("moneyTypeCn")]
        public string Currency { get; set; } = "人民币";

        /// <summary>
        /// 投资者名称
        /// </summary>
        [JsonPropertyName("custName")]
        public string CustomerName { get; set; }

        /// <summary>
        /// 客户类型（参考附录4）
        /// </summary>
        [JsonPropertyName("custType")]
        public string CustomerType { get; set; }

        /// <summary>
        /// 证件类型（参考附录4）
        /// </summary>
        [JsonPropertyName("certiType")]
        public string CertificateType { get; set; }

        /// <summary>
        /// 证件号
        /// </summary>
        [JsonPropertyName("certiNo")]
        public string CertificateNumber { get; set; }


        public TransferRequest ToObject()
        {
            return new TransferRequest
            {
                CustomerIdentity = CertificateNumber,
                CustomerName = CustomerName,
                FundCode = FundCode,
                FundName = "unset",
                RequestDate = DateOnly.ParseExact(RequestDate, "yyyyMMdd"),
                RequestType = ParseRequestType(BusinFlag),
                ExternalId = RequestNo,
                Agency = AgencyName,
                FeeDiscount = ParseDecimal(DiscountRate),
                LargeRedemptionFlag = ExceedFlag switch { "0" => LargeRedemptionFlag.CancelRemaining, _ => LargeRedemptionFlag.RollOver },
                Source = "api",
                RequestAmount = ApplyAmount,
                RequestShare = ApplyShares,
                Fee = DefinedFee,
                CreateDate = DateOnly.FromDateTime(DateTime.Now),
            };
        }

    }

    /// <summary>
    /// 01-认购	50-基金成立
    ///02-申购	51-基金终止
    ///03-赎回	52-基金清盘
    ///04-转托管	54-发行失败
    ///05-托管转入	70-强制调增
    ///06-托管转出	71-强制调减
    ///07-修改分红方式	74-红利发放
    ///10-份额冻结	77-定期申购修改
    ///11-份额解冻	78-定期赎回修改
    ///12-非交易过户	81-基金开户
    ///13-基金转换(出)  82-基金销户
    ///14-非交易过户出	83-账户修改
    ///15-非交易过户入	84-账户冻结
    ///16-基金转换入	85-账户解冻
    ///17-撤单	88-账户登记
    ///20-内部转托管	89-取消登记
    ///21-内部转托管入	90-定期申购协议
    ///22-内部转托管出	91-定期赎回协议
    ///31-增开交易账号	92-承诺优惠协议
    ///32-变更交易账号	93-定期申购取消
    ///41-买入	94-定期赎回取消
    ///42-卖出	96-销交易账号
    ///46-联名卡查询	97-内部转托管
    ///47-联名卡开通	98-内部托管转入
    ///48-联名卡取消	99-内部托管转出
    ///49-联名卡还款
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static TransferRequestType ParseRequestType(string str)
    {
        switch (str)
        {
            case "01": return TransferRequestType.Subscription;
            case "02": return TransferRequestType.Purchase;
            case "03": return TransferRequestType.Redemption;
            case "04": return TransferRequestType.Subscription;
            case "05":
            case "70":
            case "15":
            case "16":
            case "21":
            case "41":
                return TransferRequestType.Increase;
            case "06":
            case "71":
            case "14":
            case "13":
            case "22":
            case "42":
                return TransferRequestType.Decrease;
            default: // 其它类型忽略
                return TransferRequestType.UNK;
        }
    }


    private static TransferRecordType ParseRecordType(string businessTypeCode)
    {
        return businessTypeCode switch
        {
            "01" => TransferRecordType.Subscription,         // 认购
            "02" => TransferRecordType.Purchase,             // 申购
            "03" => TransferRecordType.Redemption,           // 赎回
            "04" => TransferRecordType.MoveOut,              // 转托管（转出）
            "05" => TransferRecordType.MoveIn,               // 托管转入（转入）
            "06" => TransferRecordType.MoveOut,              // 托管转出（转出）
            "07" => TransferRecordType.BonusType,            // 修改分红方式
            "10" => TransferRecordType.Frozen,               // 份额冻结
            "11" => TransferRecordType.Thawed,               // 份额解冻
            "12" => TransferRecordType.TransferIn,           // 非交易过户（转入）
            "13" => TransferRecordType.SwitchOut,            // 基金转换(出)
            "14" => TransferRecordType.TransferOut,          // 非交易过户出（转出）
            "15" => TransferRecordType.TransferIn,           // 非交易过户入（转入）
            "16" => TransferRecordType.SwitchIn,             // 基金转换入
            "17" => TransferRecordType.UNK,                  // 撤单（无对应TA记录类型）
            "20" => TransferRecordType.TransferOut,          // 内部转托管（转出）
            "21" => TransferRecordType.TransferIn,           // 内部转托管入（转入）
            "22" => TransferRecordType.TransferOut,          // 内部转托管出（转出）
            "31" => TransferRecordType.UNK,                  // 增开交易账号（无明确对应）
            "32" => TransferRecordType.UNK,                  // 变更交易账号（无明确对应）
            "41" => TransferRecordType.Purchase,             // 买入（等同于申购）
            "42" => TransferRecordType.Redemption,           // 卖出（等同于赎回）
            "46" => TransferRecordType.UNK,                  // 联名卡查询（无对应）
            "47" => TransferRecordType.UNK,                  // 联名卡开通（无对应）
            "48" => TransferRecordType.UNK,                  // 联名卡取消（无对应）
            "49" => TransferRecordType.UNK,                  // 联名卡还款（无对应）
            "50" => TransferRecordType.UNK,                  // 基金成立（通常不是单笔记录）
            "51" => TransferRecordType.UNK,                  // 基金终止（通常不是单笔记录）
            "52" => TransferRecordType.Clear,                // 基金清盘
            "54" => TransferRecordType.RaisingFailed,        // 发行失败
            "70" => TransferRecordType.Increase,             // 强制调增
            "71" => TransferRecordType.Decrease,             // 强制调减
            "74" => TransferRecordType.Distribution,         // 红利发放
            "77" => TransferRecordType.UNK,                  // 定期申购修改（无明确对应）
            "78" => TransferRecordType.UNK,                  // 定期赎回修改（无明确对应）
            "81" => TransferRecordType.UNK,                  // 基金开户（非交易记录）
            "82" => TransferRecordType.UNK,                  // 基金销户（非交易记录）
            "83" => TransferRecordType.UNK,                  // 账户修改（非交易记录）
            "84" => TransferRecordType.Frozen,               // 账户冻结（与份额冻结类似）
            "85" => TransferRecordType.Thawed,               // 账户解冻（与份额解冻类似）
            "88" => TransferRecordType.UNK,                  // 账户登记（非交易记录）
            "89" => TransferRecordType.UNK,                  // 取消登记（非交易记录）
            "90" => TransferRecordType.UNK,                  // 定期申购协议（非交易记录）
            "91" => TransferRecordType.UNK,                  // 定期赎回协议（非交易记录）
            "92" => TransferRecordType.UNK,                  // 承诺优惠协议（非交易记录）
            "93" => TransferRecordType.UNK,                  // 定期申购取消（非交易记录）
            "94" => TransferRecordType.UNK,                  // 定期赎回取消（非交易记录）
            "96" => TransferRecordType.UNK,                  // 销交易账号（非交易记录）
            "97" => TransferRecordType.TransferOut,          // 内部转托管（转出）
            "98" => TransferRecordType.TransferIn,           // 内部托管转入（转入）
            "99" => TransferRecordType.TransferOut,          // 内部托管转出（转出）

            _ => TransferRecordType.UNK
        };
    }



    public class InvestorAccountMapping
    {
        //FundAccount
        public string Id { get; set; }

        public string Name { get; set; }

        public string Indentity { get; set; }
    }


    public class DistrubutionJson
    {
        /// <summary>
        /// 基金账号
        /// </summary>
        [JsonPropertyName("fundAcco")]
        public string FundAccount { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        [JsonPropertyName("custName")]
        public string CustomerName { get; set; }

        /// <summary>
        /// 销售商代码（ZX6 表示直销）
        /// </summary>
        [JsonPropertyName("agencyNo")]
        public string AgencyCode { get; set; }

        /// <summary>
        /// 销售商名称
        /// </summary>
        [JsonPropertyName("agencyName")]
        public string AgencyName { get; set; }

        /// <summary>
        /// 交易账号
        /// </summary>
        [JsonPropertyName("tradeAcco")]
        public string TradeAccount { get; set; }

        /// <summary>
        /// 确认日期（格式：YYYYMMDD）
        /// </summary>
        [JsonPropertyName("confirmDate")]
        public string ConfirmDate { get; set; }

        /// <summary>
        /// TA确认号
        /// </summary>
        [JsonPropertyName("cserialNo")]
        public string TaConfirmNo { get; set; }

        /// <summary>
        /// 分红登记日期
        /// </summary>
        [JsonPropertyName("regDate")]
        public string RegisterDate { get; set; }

        /// <summary>
        /// 红利发放日期
        /// </summary>
        [JsonPropertyName("date")]
        public string DividendDate { get; set; }

        /// <summary>
        /// 基金名称
        /// </summary>
        [JsonPropertyName("fundName")]
        public string FundName { get; set; }

        /// <summary>
        /// 基金代码
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// 分红基数份额
        /// </summary>
        [JsonPropertyName("totalShare")]
        public string TotalShares { get; set; }

        /// <summary>
        /// 每单位分红金额
        /// </summary>
        [JsonPropertyName("unitProfit")]
        public string UnitProfit { get; set; }

        /// <summary>
        /// 红利总额
        /// </summary>
        [JsonPropertyName("totalProfit")]
        public string TotalProfit { get; set; }

        /// <summary>
        /// 分红方式：
        /// 0 - 红利再投资
        /// 1 - 现金红利
        /// </summary>
        [JsonPropertyName("flag")]
        public string DividendType { get; set; }

        /// <summary>
        /// 实发现金红利
        /// </summary>
        [JsonPropertyName("realBalance")]
        public string RealCashDividend { get; set; }

        /// <summary>
        /// 再投资红利金额
        /// </summary>
        [JsonPropertyName("reinvestBalance")]
        public string ReinvestAmount { get; set; }

        /// <summary>
        /// 再投资份额
        /// </summary>
        [JsonPropertyName("realShares")]
        public string ReinvestShares { get; set; }

        /// <summary>
        /// 再投资日期
        /// </summary>
        [JsonPropertyName("lastDate")]
        public string ReinvestDate { get; set; }

        /// <summary>
        /// 再投资单位净值
        /// </summary>
        [JsonPropertyName("netValue")]
        public string NetValue { get; set; }

        /// <summary>
        /// 实际业绩提成金额
        /// </summary>
        [JsonPropertyName("deductBalance")]
        public string PerformanceFee { get; set; }

        /// <summary>
        /// 客户类型（参见附录4）
        /// </summary>
        [JsonPropertyName("custType")]
        public string CustomerType { get; set; }

        /// <summary>
        /// 证件类型（参见附录4）
        /// </summary>
        [JsonPropertyName("certiType")]
        public string CertificateType { get; set; }

        /// <summary>
        /// 证件号码
        /// </summary>
        [JsonPropertyName("certiNo")]
        public string CertificateNumber { get; set; }

        /// <summary>
        /// 除权除息日
        /// </summary>
        [JsonPropertyName("exDividendDate")]
        public string ExDividendDate { get; set; }


        public TransferRecord ToObject()
        {
            return new TransferRecord
            {
                CustomerIdentity = CertificateNumber,
                CustomerName = CustomerName,
                ExternalId = TaConfirmNo,
                ConfirmedDate = DateOnly.ParseExact(ConfirmDate, "yyyyMMdd"),
                ConfirmedShare = ParseDecimal(ReinvestShares),
                ConfirmedAmount = ParseDecimal(RealCashDividend),
                ConfirmedNetAmount = ParseDecimal(RealCashDividend),
                RequestDate = DateOnly.ParseExact(ExDividendDate, "yyyyMMdd"),
                RequestAmount = 0,
                RequestShare = ParseDecimal(TotalShares),
                Type = TransferRecordType.Distribution,
                Agency = AgencyName,
                CreateDate = DateOnly.FromDateTime(DateTime.Now),
                PerformanceFee = ParseDecimal(PerformanceFee),
                FundCode = FundCode,
                FundName = FundName,
                Source = "api"
            };
        }
    }
}

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。