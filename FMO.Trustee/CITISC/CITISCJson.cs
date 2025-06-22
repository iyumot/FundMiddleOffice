using FMO.Models;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FMO.Trustee;

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
        public required string CustName { get; set; } // 投资者名称

        [JsonPropertyName("fundAcco")]
        public required string FundAcco { get; set; } // 基金账号

        [JsonPropertyName("tradeAcco")]
        public required string TradeAcco { get; set; } // 交易账号

        [JsonPropertyName("custType")]
        public required string CustType { get; set; } // 客户类型（参见附录4）

        [JsonPropertyName("certiType")]
        public required string CertiType { get; set; } // 证件类型（参见附录4）

        [JsonPropertyName("certiNo")]
        public string? CertiNo { get; set; } // 证件号

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
            throw new NotImplementedException();
            return new Investor
            {
                Name = CustName
            };
        }

    }

    public class TransferRecordJson
    {
        [JsonPropertyName("ackNo")]
        public string? AckNo { get; set; }

        [JsonPropertyName("exRequestNo")]
        public string? ExRequestNo { get; set; }

        [JsonPropertyName("fundAcco")]
        public string? FundAcco { get; set; }

        [JsonPropertyName("tradeAcco")]
        public string? TradeAcco { get; set; }

        /// <summary>
        /// 业务类型：
        ///01-认购	50-基金成立
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
        [JsonPropertyName("apkind")]
        public string? Apkind { get; set; } // 业务类型代码

        [JsonPropertyName("taFlag")]
        public string? TaFlag { get; set; } // TA发起标志：0-否，1-是

        [JsonPropertyName("fundCode")]
        public string? FundCode { get; set; }

        [JsonPropertyName("shareLevel")]
        public string? ShareLevel { get; set; } // 份额类别：A-前收费，B-后收费

        [JsonPropertyName("currency")]
        public string? Currency { get; set; } // 币种：人民币、美元 或 null

        [JsonPropertyName("largeRedemptionFlag")]
        public string? LargeRedemptionFlag { get; set; } // 巨额赎回处理标志：0-取消，1-顺延

        [JsonPropertyName("subAmt")]
        public string? SubAmt { get; set; } // 申请金额

        [JsonPropertyName("subQuty")]
        public string? SubQuty { get; set; } // 申请份额

        [JsonPropertyName("bonusType")]
        public string? BonusType { get; set; } // 分红方式：0-红利再投资，1-现金红利

        [JsonPropertyName("nav")]
        public string? Nav { get; set; } // 单位净值

        [JsonPropertyName("ackAmt")]
        public string? AckAmt { get; set; } // 确认金额

        [JsonPropertyName("ackQuty")]
        public string? AckQuty { get; set; } // 确认份额

        [JsonPropertyName("tradeFee")]
        public string? TradeFee { get; set; } // 交易费

        [JsonPropertyName("taFee")]
        public string? TaFee { get; set; } // 过户费

        [JsonPropertyName("backFee")]
        public string? BackFee { get; set; } // 后收费

        [JsonPropertyName("realBalance")]
        public string? RealBalance { get; set; } // 应返还金额

        [JsonPropertyName("profitBalance")]
        public string? ProfitBalance { get; set; } // 业绩报酬

        [JsonPropertyName("profitBalanceForAgency")]
        public string? ProfitBalanceForAgency { get; set; } // 业绩报酬归销售商

        [JsonPropertyName("totalNav")]
        public string? TotalNav { get; set; } // 累计净值

        [JsonPropertyName("totalFee")]
        public string? TotalFee { get; set; } // 总费用

        [JsonPropertyName("agencyFee")]
        public string? AgencyFee { get; set; } // 归销售机构费用

        [JsonPropertyName("fundFee")]
        public string? FundFee { get; set; } // 归基金资产费用

        [JsonPropertyName("registFee")]
        public string? RegistFee { get; set; } // 归管理人费用

        [JsonPropertyName("interest")]
        public string? Interest { get; set; } // 利息

        [JsonPropertyName("interestTax")]
        public string? InterestTax { get; set; } // 利息税

        [JsonPropertyName("interestShare")]
        public string? InterestShare { get; set; } // 利息转份额

        [JsonPropertyName("frozenBalance")]
        public string? FrozenBalance { get; set; } // 确认冻结份额

        [JsonPropertyName("unfrozenBalance")]
        public string? UnfrozenBalance { get; set; } // 确认解冻份额

        [JsonPropertyName("unShares")]
        public string? UnShares { get; set; } // 巨额赎回顺延份额

        [JsonPropertyName("applyDate")]
        public string? ApplyDate { get; set; } // 申请工作日

        [JsonPropertyName("ackDate")]
        public string? AckDate { get; set; } // 确认日期

        /// <summary>
        /// 确认状态:
        ///0-未处理	9-延缓处理
        ///1-确认成功 a-未处理
        ///2-确认失败 b-未处理
        ///3-交易撤消 c-未处理
        ///4-逐笔确认 d-未处理
        ///5-逐笔否决 e-未处理
        ///6-检查特殊 f-未处理
        ///7-巨额赎回延续 g-未处理
        ///8-临时导入
        /// </summary>
        [JsonPropertyName("ackStatus")]
        public string? AckStatus { get; set; } // 确认状态

        [JsonPropertyName("agencyNo")]
        public string? AgencyNo { get; set; } // 销售商代码

        [JsonPropertyName("agencyName")]
        public string? AgencyName { get; set; } // 销售商名称

        [JsonPropertyName("retCod")]
        public string? RetCod { get; set; } // 返回码

        [JsonPropertyName("retMsg")]
        public string? RetMsg { get; set; } // 失败原因

        [JsonPropertyName("adjustCause")]
        public string? AdjustCause { get; set; } // 份额调整原因

        [JsonPropertyName("navDate")]
        public string? NavDate { get; set; } // 净值日期 (YYYYMMDD)

        [JsonPropertyName("oriCserialNo")]
        public string? OriCserialNo { get; set; } // 原确认单号

        internal TransferRecord ToObject()
        {
            throw new NotImplementedException();
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
        public required string ProductCode { get; set; }

        /// <summary>
        /// 分级产品代码
        /// </summary>
        [JsonPropertyName("ffjdm")]
        public string? ClassificationCode { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        [JsonPropertyName("fcpmc")]
        public required string ProductName { get; set; }

        /// <summary>
        /// 业务日期，格式：YYYY - MM - DD
        /// </summary>
        [JsonPropertyName("cdate")]
        public required string BusinessDate { get; set; }

        #region 管理费相关
        /// <summary>
        /// 计提管理费
        /// </summary>
        [JsonPropertyName("jtglf")]
        public required string AccruedManagementFee { get; set; }

        /// <summary>
        /// 支付管理费
        /// </summary>
        [JsonPropertyName("zfglf")]
        public required string PaidManagementFee { get; set; }

        /// <summary>
        /// 未支付管理费
        /// </summary>
        [JsonPropertyName("wzfglf")]
        public required string UnpaidManagementFee { get; set; }
        #endregion

        #region 业绩报酬相关
        /// <summary>
        /// 计提业绩报酬
        /// </summary>
        [JsonPropertyName("jtyjbc")]
        public required string AccruedPerformanceFee { get; set; }

        /// <summary>
        /// 支付业绩报酬
        /// </summary>
        [JsonPropertyName("zfyjbc")]
        public required string PaidPerformanceFee { get; set; }

        /// <summary>
        /// 未支付业绩报酬
        /// </summary>
        [JsonPropertyName("wzfyjbc")]
        public required string UnpaidPerformanceFee { get; set; }
        #endregion

        #region 托管费相关
        /// <summary>
        /// 计提托管费
        /// </summary>
        [JsonPropertyName("jttgf")]
        public required string AccruedCustodyFee { get; set; }

        /// <summary>
        /// 支付托管费
        /// </summary>
        [JsonPropertyName("zftgf")]
        public required string PaidCustodyFee { get; set; }

        /// <summary>
        /// 未支付托管费
        /// </summary>
        [JsonPropertyName("wzftgf")]
        public required string UnpaidCustodyFee { get; set; }
        #endregion

        #region 行政服务费相关
        /// <summary>
        /// 计提行政服务费
        /// </summary>
        [JsonPropertyName("jtxzfwf")]
        public required string AccruedAdministrativeFee { get; set; }

        /// <summary>
        /// 支付行政服务费
        /// </summary>
        [JsonPropertyName("zfxzfwf")]
        public required string PaidAdministrativeFee { get; set; }

        /// <summary>
        /// 未支付行政服务费
        /// </summary>
        [JsonPropertyName("wzfxzfwf")]
        public required string UnpaidAdministrativeFee { get; set; }
        #endregion

        #region 销售服务费相关
        /// <summary>
        /// 计提销售服务费
        /// </summary>
        [JsonPropertyName("jtxsfwf")]
        public required string AccruedSalesServiceFee { get; set; }

        /// <summary>
        /// 支付销售服务费
        /// </summary>
        [JsonPropertyName("zfxsfwf")]
        public required string PaidSalesServiceFee { get; set; }

        /// <summary>
        /// 未支付销售服务费
        /// </summary>
        [JsonPropertyName("wzfxsfwf")]
        public required string UnpaidSalesServiceFee { get; set; }
        #endregion

        #region 投资顾问费相关
        /// <summary>
        /// 计提投资顾问费
        /// </summary>
        [JsonPropertyName("jttzgwf")]
        public required string AccruedInvestmentAdvisoryFee { get; set; }

        /// <summary>
        /// 支付投资顾问费
        /// </summary>
        [JsonPropertyName("zftzgwf")]
        public required string PaidInvestmentAdvisoryFee { get; set; }

        /// <summary>
        /// 未支付投资顾问费
        /// </summary>
        [JsonPropertyName("wzftzgwf")]
        public required string UnpaidInvestmentAdvisoryFee { get; set; }
        #endregion

        #region 增值税附加税相关
        /// <summary>
        /// 计提增值税附加税
        /// </summary>
        [JsonPropertyName("jtzzsfjs")]
        public required string AccruedVatSurcharge { get; set; }

        /// <summary>
        /// 支付增值税附加税
        /// </summary>
        [JsonPropertyName("zfzzsfjs")]
        public required string PaidVatSurcharge { get; set; }

        /// <summary>
        /// 未支付增值税附加税
        /// </summary>
        [JsonPropertyName("wzfzzsfjs")]
        public required string UnpaidVatSurcharge { get; set; }
        #endregion

        #region 审计费相关
        /// <summary>
        /// 计提审计费
        /// </summary>
        [JsonPropertyName("jtsjf")]
        public required string AccruedAuditFee { get; set; }

        /// <summary>
        /// 支付审计费
        /// </summary>
        [JsonPropertyName("zfsjf")]
        public required string PaidAuditFee { get; set; }

        /// <summary>
        /// 未支付审计费
        /// </summary>
        [JsonPropertyName("wzfsjf")]
        public required string UnpaidAuditFee { get; set; }
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
        public required string OccurTime { get; set; }

        /// <summary>
        /// 基金代码
        /// </summary>
        [JsonPropertyName("fundCode")]
        public required string FundCode { get; set; }

        /// <summary>
        /// 基金名称
        /// </summary>
        [JsonPropertyName("fundName")]
        public required string FundName { get; set; }

        /// <summary>
        /// 账户类型，例如 "02-募集户"
        /// </summary>
        [JsonPropertyName("accoType")]
        public required string AccoType { get; set; }

        /// <summary>
        /// 本方账号
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public required string BankAcco { get; set; }

        /// <summary>
        /// 本方账户名称
        /// </summary>
        [JsonPropertyName("accName")]
        public required string AccName { get; set; }

        /// <summary>
        /// 本方开户行名称
        /// </summary>
        [JsonPropertyName("openBankName")]
        public required string OpenBankName { get; set; }

        /// <summary>
        /// 本方银行编号，以人行大额付款码前三位为标准（见附录2）
        /// </summary>
        [JsonPropertyName("bankNo")]
        public required string BankNo { get; set; }

        /// <summary>
        /// 对方账号
        /// </summary>
        [JsonPropertyName("othBankAcco")]
        public required string OthBankAcco { get; set; }

        /// <summary>
        /// 对方账户名称
        /// </summary>
        [JsonPropertyName("othAccName")]
        public required string OthAccName { get; set; }

        /// <summary>
        /// 对方开户行名称
        /// </summary>
        [JsonPropertyName("othOpenBankName")]
        public required string OthOpenBankName { get; set; }

        /// <summary>
        /// 对方银行编号，以人行大额付款码前三位为标准（见附录2）
        /// </summary>
        [JsonPropertyName("othBankNo")]
        public required string OthBankNo { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        [JsonPropertyName("curType")]
        public required string CurType { get; set; }

        /// <summary>
        /// 收付方向，1表示收款，2表示付款
        /// </summary>
        [JsonPropertyName("directFlag")]
        public required string DirectFlag { get; set; }

        /// <summary>
        /// 发生金额
        /// </summary>
        [JsonPropertyName("occurAmt")]
        public required string OccurAmt { get; set; }

        /// <summary>
        /// 账户余额
        /// </summary>
        [JsonPropertyName("acctBal")]
        public required string AcctBal { get; set; }

        /// <summary>
        /// 银行流水号
        /// </summary>
        [JsonPropertyName("bankJour")]
        public required string BankJour { get; set; }

        /// <summary>
        /// 银行返回代码
        /// </summary>
        [JsonPropertyName("bankRetCode")]
        public required string BankRetCode { get; set; }

        /// <summary>
        /// 银行摘要
        /// </summary>
        [JsonPropertyName("bankNote")]
        public required string BankNote { get; set; }



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
                Amount = decimal.Parse(OccurAmt),
                Balance = decimal.Parse(AcctBal),
                Direction = DirectFlag == "付款" ? TransctionDirection.Pay : TransctionDirection.Receive,
                Remark = BankNote,
                Serial = BankJour,
            };
        }
    }




    public class CustodialAccountJson
    {
        [JsonPropertyName("pdCode")]
        public required string ProductCode { get; set; }

        [JsonPropertyName("pdName")]
        public required string ProductName { get; set; }

        [JsonPropertyName("accName")]
        public required string AccountName { get; set; }

        [JsonPropertyName("account")]
        public required string Account { get; set; }

        [JsonPropertyName("bankName")]
        public required string BankName { get; set; }

        [JsonPropertyName("lrgAmtNo")]
        public required string LargePaymentNumber { get; set; }

        [JsonPropertyName("recentBalance")]
        public required string RecentBalance { get; set; }

        [JsonPropertyName("recentBalanceQueryTime")]
        public required string BalanceQueryTime { get; set; }

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
        public required string BusinessDate { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        [JsonPropertyName("custName")]
        public required string CustomerName { get; set; }

        /// <summary>
        /// 基金账号
        /// </summary>
        [JsonPropertyName("fundAcco")]
        public required string FundAccount { get; set; }

        /// <summary>
        /// 产品代码
        /// </summary>
        [JsonPropertyName("fundCode")]
        public required string FundCode { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        [JsonPropertyName("fundName")]
        public required string FundName { get; set; }

        /// <summary>
        /// 持仓份额
        /// </summary>
        [JsonPropertyName("shares")]
        public decimal Shares { get; set; }

        /// <summary>
        /// 计提方式：1-估值计提；2-TA计提
        /// </summary>
        [JsonPropertyName("flag")]
        public required string AccrualMethod { get; set; }

        /// <summary>
        /// 虚拟业绩报酬金额
        /// </summary>
        [JsonPropertyName("virtualBalance")]
        public decimal VirtualPerformanceFee { get; set; }

        /// <summary>
        /// 虚拟净值
        /// </summary>
        [JsonPropertyName("virtualAssetVal")]
        public required string VirtualNetAssetValue { get; set; }

        /// <summary>
        /// 实际净值
        /// </summary>
        [JsonPropertyName("netAssetVal")]
        public required string ActualNetAssetValue { get; set; }

        /// <summary>
        /// 累计净值
        /// </summary>
        [JsonPropertyName("totalAssetVal")]
        public required string TotalNetAssetValue { get; set; }

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
        public required string NetValueCheckStatus { get; set; }
    }


    public class BankBalanceJson
    {  
        /// <summary>
        /// 账号
        /// </summary>
        [JsonPropertyName("YHZH")]
        public required string AccountNumber { get; set; }

        /// <summary>
        /// 账户余额
        /// </summary>
        [JsonPropertyName("ZHYE")]
        public required string AccountBalance { get; set; }

        /// <summary>
        /// 账户名称
        /// </summary>
        [JsonPropertyName("KHHM")]
        public required string AccountHolderName { get; set; }

        /// <summary>
        /// 货币种类：
        /// HKD-港币；
        /// RMB-人民币；
        /// USD-美元
        /// </summary>
        [JsonPropertyName("JSBZ")]
        public required string CurrencyType { get; set; }

        /// <summary>
        /// 余额查询时间，格式：YYYY-MM-DD HH:MM:SS
        /// </summary>
        [JsonPropertyName("CXSJ")]
        public required string QueryTime { get; set; }

        /// <summary>
        /// 账户可用余额
        /// </summary>
        [JsonPropertyName("ZHKYYE")]
        public required string AvailableBalance { get; set; }

        /// <summary>
        /// 处理结果：
        /// 0-成功；
        /// -2-处理中；
        /// 其他值代表失败
        /// </summary>
        [JsonPropertyName("CLJG")]
        public required string ProcessingResult { get; set; }

        /// <summary>
        /// 处理说明
        /// </summary>
        [JsonPropertyName("CLSM")]
        public required string ProcessingDescription { get; set; }

        /// <summary>
        /// 开户行
        /// </summary>
        [JsonPropertyName("KHYH")]
        public required string OpeningBank { get; set; }

        public BankBalance ToObject()
        {
            return new BankBalance
            {
                AccountName = AccountHolderName,
                AccountNo = AccountNumber,
                Name = OpeningBank,
                Balance = decimal.Parse(AccountBalance),
                Currency = CurrencyType,
                Time = DateTime.ParseExact(QueryTime, "yyyy-MM-dd HH:mm:ss", null)
            };
        }
    }
}
