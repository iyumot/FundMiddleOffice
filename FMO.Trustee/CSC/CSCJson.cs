using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;


#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
public partial class CSC
{
    public class RetJson
    {
        [JsonPropertyName("retCode")]
        public string Code { get; set; }


        [JsonPropertyName("retMsg")]
        public string Msg { get; set; }

    }




    public class RetJson<T>
    {
        [JsonPropertyName("retCode")]
        public string Code { get; set; }


        [JsonPropertyName("retMsg")]
        public string Msg { get; set; }

        [JsonPropertyName("data")]
        public required DataJsonWrap<T> Data { get; set; }
    }

    public class DataJsonWrap<T>
    {

        [JsonPropertyName("result")]
        public required T[] Data { get; set; }

        [JsonPropertyName("rowCount")]
        public int RowCount { get; set; }


        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }
    }


    public interface IJson<T>
    {
        T ToObject();
    }

    internal class TransferRecordJson : IJson<TransferRecord>
    {
        /// <summary>
        /// 交易确认日期 (格式: YYYYMMDD, 8位)
        /// </summary>
        [JsonPropertyName("confDate")]
        public string ConfDate { get; set; }

        /// <summary>
        /// 申请日期 (格式: YYYYMMDD, 8位)
        /// </summary>
        [JsonPropertyName("applyDate")]
        public string ApplyDate { get; set; }

        /// <summary>
        /// 基金代码 (32位)
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// 基金名称 (250字符)
        /// </summary>
        [JsonPropertyName("fundName")]
        public string FundName { get; set; }

        /// <summary>
        /// 销售商代码 (6位)
        /// </summary>
        [JsonPropertyName("agencyNo")]
        public string AgencyNo { get; set; }

        /// <summary>
        /// 销售商名称 (64字符)
        /// </summary>
        [JsonPropertyName("agencyName")]
        public string AgencyName { get; set; }

        /// <summary>
        /// 投资者名称 (64字符)
        /// </summary>
        [JsonPropertyName("custName")]
        public string CustName { get; set; }

        /// <summary>
        /// 基金账号 (20字符)
        /// </summary>
        [JsonPropertyName("fundAcco")]
        public string FundAcco { get; set; }

        /// <summary>
        /// 交易账号 (16字符)
        /// </summary>
        [JsonPropertyName("tradeAcco")]
        public string TradeAcco { get; set; }

        /// <summary>
        /// 银行账号 (32字符)
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public string BankAcco { get; set; }

        /// <summary>
        /// 银行编号 (6字符)
        /// </summary>
        [JsonPropertyName("bankNo")]
        public string BankNo { get; set; }

        /// <summary>
        /// 开户行名称 (250字符)
        /// </summary>
        [JsonPropertyName("openBankName")]
        public string OpenBankName { get; set; }

        /// <summary>
        /// 银行户名 (64字符)
        /// </summary>
        [JsonPropertyName("nameInBank")]
        public string NameInBank { get; set; }

        /// <summary>
        /// 客户类型 (6字符)
        /// </summary>
        [JsonPropertyName("custType")]
        public string CustType { get; set; }

        /// <summary>
        /// 证件类型 (3字符)
        /// </summary>
        [JsonPropertyName("certiType")]
        public string CertiType { get; set; }

        /// <summary>
        /// 证件号 (32字符)
        /// </summary>
        [JsonPropertyName("certiNo")]
        public string CertiNo { get; set; }

        /// <summary>
        /// TA发起标志 (2字符)
        /// </summary>
        [JsonPropertyName("taFlag")]
        public string TaFlag { get; set; }

        /// <summary>
        /// 确认状态 (2字符)
        /// </summary>
        [JsonPropertyName("confStatus")]
        public string ConfStatus { get; set; }

        /// <summary>
        /// 确认结果描述 (250字符)
        /// </summary>
        [JsonPropertyName("describe")]
        public string Describe { get; set; }

        /// <summary>
        /// 分红方式 (2字符)
        /// </summary>
        [JsonPropertyName("bonusType")]
        public string BonusType { get; set; }

        /// <summary>
        /// 申请金额 (20字符)
        /// </summary>
        [JsonPropertyName("balance")]
        public string Balance { get; set; }

        /// <summary>
        /// 申请份额 (20字符)
        /// </summary>
        [JsonPropertyName("shares")]
        public string Shares { get; set; }

        /// <summary>
        /// 单位净值 (10字符)
        /// </summary>
        [JsonPropertyName("netValue")]
        public string NetValue { get; set; }

        /// <summary>
        /// 确认金额 (20字符)
        /// </summary>
        [JsonPropertyName("confBalance")]
        public string ConfBalance { get; set; }

        /// <summary>
        /// 确认份额 (20字符)
        /// </summary>
        [JsonPropertyName("confShares")]
        public string ConfShares { get; set; }

        /// <summary>
        /// 手续费 (20字符)
        /// </summary>
        [JsonPropertyName("charge")]
        public string Charge { get; set; }

        /// <summary>
        /// 归管理人手续费 (20字符)
        /// </summary>
        [JsonPropertyName("managerCharge")]
        public string ManagerCharge { get; set; }

        /// <summary>
        /// 归销售商手续费 (20字符)
        /// </summary>
        [JsonPropertyName("distributorCharge")]
        public string DistributorCharge { get; set; }

        /// <summary>
        /// 归产品手续费 (20字符)
        /// </summary>
        [JsonPropertyName("fundcharge")]
        public string Fundcharge { get; set; }

        /// <summary>
        /// 业绩报酬 (20字符)
        /// </summary>
        [JsonPropertyName("achievementPay")]
        public string AchievementPay { get; set; }

        /// <summary>
        /// TA确认编号 (32字符)
        /// </summary>
        [JsonPropertyName("cserialNo")]
        public string CserialNo { get; set; }

        /// <summary>
        /// 确认业务类型 (6字符)
        /// </summary>
        [JsonPropertyName("busiFlag")]
        public string BusiFlag { get; set; }

        /// <summary>
        /// 原外部系统的申请流水号 (32字符)
        /// </summary>
        [JsonPropertyName("originalNo")]
        public string OriginalNo { get; set; }

        /// <summary>
        /// 操作方式 (1字符)
        /// </summary>
        [JsonPropertyName("operWayNew")]
        public string OperWayNew { get; set; }

        /// <summary>
        /// 120	认购
        //122	申购
        //124	赎回
        //126	转销售机构
        //127	转销售机构入
        //129	设置分红方式
        //131	份额冻结
        //132	份额解冻
        //133	转让
        //134	受让
        //136	份额转换
        //137	份额转换入
        //142	强行赎回
        //143	红利发放
        //144	强行调增
        //145	强行调减
        //149	募集失败
        //150	基金清盘
        /// </summary>
        /// <returns></returns>
        public TransferRecord ToObject()
        {
            var r = new TransferRecord
            {
                FundCode = FundCode,
                ConfirmedDate = DateOnly.ParseExact(ConfDate, "yyyyMMdd"),
                RequestDate = DateOnly.ParseExact(ApplyDate, "yyyyMMdd"),
                FundName = FundName,
                Agency = AgencyName,
                CustomerName = CustName,
                CustomerIdentity = CertiNo,
                RequestAmount = ParseDecimal(Balance),
                RequestShare = ParseDecimal(Shares),
                ConfirmedAmount = ParseDecimal(ConfBalance),
                ConfirmedShare = ParseDecimal(ConfShares),
                CreateDate = DateOnly.FromDateTime(DateTime.Today),
                ExternalId = CserialNo,
                PerformanceFee = ParseDecimal(AchievementPay),
                Fee = ParseDecimal(Charge),
                ExternalRequestId = OriginalNo,
                Type = ParseType(BusiFlag),
                Source = "api",
            };
            // 净额
            r.ConfirmedNetAmount = r.ConfirmedAmount - r.Fee;

            return r;
        }


        private TransferRecordType ParseType(string str)
        {
            switch (str)
            {
                case "120": // 认购 
                    return TransferRecordType.Subscription;
                case "122": // 申购 
                    return TransferRecordType.Purchase;
                case "124": // 赎回 
                    return TransferRecordType.Redemption;
                case "126": // 转销售机构 
                    return TransferRecordType.MoveIn;
                case "127": // 转销售机构入 
                    return TransferRecordType.MoveIn;
                case "129": // 设置分红方式 
                    return TransferRecordType.BonusType;
                case "131": // 份额冻结 
                    return TransferRecordType.Frozen;
                case "132": // 份额解冻 
                    return TransferRecordType.Thawed;
                case "133": // 转让 
                    return TransferRecordType.TransferOut;
                case "134": // 受让 
                    return TransferRecordType.TransferIn;
                case "136": // 份额转换 
                    return TransferRecordType.SwitchOut;
                case "137": // 份额转换入 
                    return TransferRecordType.SwitchIn;

                case "142": // 强行赎回 
                    return TransferRecordType.ForceRedemption;
                case "143": // 红利发放 
                    return TransferRecordType.Distribution;
                case "144": // 强行调增 
                    return TransferRecordType.Increase;
                case "145": // 强行调减 
                    return TransferRecordType.Decrease;
                case "149": // 募集失败 
                    return TransferRecordType.RaisingFailed;
                case "150": // 基金清盘 
                    return TransferRecordType.Clear;

                default:
                    return TransferRecordType.UNK;
            }
        }

    }



    public class FundDailyFeeJson
    {

        /// <summary>
        /// 费用计算日期
        /// </summary>
        [JsonPropertyName("confDate")]
        public string ConfDate { get; set; }

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

        #region 费用计提
        /// <summary>
        /// 管理费
        /// </summary>
        [JsonPropertyName("managementFee")]
        public string ManagementFee { get; set; }

        /// <summary>
        /// 投顾费
        /// </summary>
        [JsonPropertyName("investmentFee")]
        public string InvestmentFee { get; set; }

        /// <summary>
        /// 销售服务费
        /// </summary>
        [JsonPropertyName("salesFee")]
        public string SalesFee { get; set; }

        /// <summary>
        /// 托管费
        /// </summary>
        [JsonPropertyName("custodianFee")]
        public string CustodianFee { get; set; }

        /// <summary>
        /// 外包服务费
        /// </summary>
        [JsonPropertyName("outsourcingFee")]
        public string OutsourcingFee { get; set; }

        /// <summary>
        /// 业绩报酬计提
        /// </summary>
        [JsonPropertyName("reward")]
        public string Reward { get; set; }

        /// <summary>
        /// 增值税计提
        /// </summary>
        [JsonPropertyName("addedTax")]
        public string AddedTax { get; set; }

        /// <summary>
        /// 附加税计提
        /// </summary>
        [JsonPropertyName("surTax")]
        public string SurTax { get; set; }

        /// <summary>
        /// 增值税及附加税计提
        /// </summary>
        [JsonPropertyName("addedSurTax")]
        public string AddedSurTax { get; set; }
        #endregion

        #region 费用支付
        /// <summary>
        /// 管理费支付
        /// </summary>
        [JsonPropertyName("managementFeePay")]
        public string ManagementFeePay { get; set; }

        /// <summary>
        /// 投顾费支付
        /// </summary>
        [JsonPropertyName("investmentFeePay")]
        public string InvestmentFeePay { get; set; }

        /// <summary>
        /// 销售服务费支付
        /// </summary>
        [JsonPropertyName("salesFeePay")]
        public string SalesFeePay { get; set; }

        /// <summary>
        /// 托管费支付
        /// </summary>
        [JsonPropertyName("custodianFeePay")]
        public string CustodianFeePay { get; set; }

        /// <summary>
        /// 外包服务费支付
        /// </summary>
        [JsonPropertyName("outsourcingFeePay")]
        public string OutsourcingFeePay { get; set; }

        /// <summary>
        /// 业绩报酬支付
        /// </summary>
        [JsonPropertyName("rewardPay")]
        public string RewardPay { get; set; }

        /// <summary>
        /// 增值税及附加税支付
        /// </summary>
        [JsonPropertyName("addedSurTaxPay")]
        public string AddedSurTaxPay { get; set; }
        #endregion

        #region 费用余额
        /// <summary>
        /// 管理费余额
        /// </summary>
        [JsonPropertyName("managementFeeBalance")]
        public string ManagementFeeBalance { get; set; }

        /// <summary>
        /// 投顾费余额
        /// </summary>
        [JsonPropertyName("investmentFeeBalance")]
        public string InvestmentFeeBalance { get; set; }

        /// <summary>
        /// 销售服务费余额
        /// </summary>
        [JsonPropertyName("salesFeeBalance")]
        public string SalesFeeBalance { get; set; }

        /// <summary>
        /// 托管费余额
        /// </summary>
        [JsonPropertyName("custodianFeeBalance")]
        public string CustodianFeeBalance { get; set; }

        /// <summary>
        /// 外包服务费余额
        /// </summary>
        [JsonPropertyName("outsourcingFeeBalance")]
        public string OutsourcingFeeBalance { get; set; }

        /// <summary>
        /// 业绩报酬余额
        /// </summary>
        [JsonPropertyName("rewardBalance")]
        public string RewardBalance { get; set; }

        /// <summary>
        /// 增值税及附加税余额
        /// </summary>
        [JsonPropertyName("addedSurTaxBalance")]
        public string AddedSurTaxBalance { get; set; }
        #endregion


        public FundDailyFee ToObject()
        {


            return new FundDailyFee
            {
                FundCode = FundCode,
                Date = DateOnly.ParseExact(ConfDate, "yyyyMMdd"),

                ManagerFeeAccrued = ParseDecimal(ManagementFee),
                ManagerFeePaid = ParseDecimal(ManagementFeePay),
                ManagerFeeBalance = ParseDecimal(ManagementFeeBalance),
                PerformanceFeeAccrued = ParseDecimal(Reward),
                PerformanceFeePaid = ParseDecimal(RewardPay),
                PerformanceFeeBalance = ParseDecimal(RewardBalance),
                CustodianFeeAccrued = ParseDecimal(CustodianFee),
                CustodianFeePaid = ParseDecimal(CustodianFeePay),
                CustodianFeeBalance = ParseDecimal(CustodianFeeBalance),
                ConsultantFeeAccrued = ParseDecimal(InvestmentFee),
                ConsultantFeePaid = ParseDecimal(InvestmentFeePay),
                ConsultantFeeBalance = ParseDecimal(InvestmentFeeBalance),
                SalesFeeAccrued = ParseDecimal(SalesFee),
                SalesFeePaid = ParseDecimal(SalesFeePay),
                SalesFeeBalance = ParseDecimal(SalesFeeBalance),
                OutsourcingFeeAccrued = ParseDecimal(OutsourcingFee),
                OutsourcingFeePaid = ParseDecimal(OutsourcingFeePay),
                OutsourcingFeeBalance = ParseDecimal(OutsourcingFeeBalance),
            };
        }

    }



    public class BankTransactionJson
    {
        /// <summary>
        /// 交易日期，格式：YYYY-MM-DD
        /// </summary>
        [JsonPropertyName("tradeDate")]

        public string TradeDate { get; set; }

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
        /// 交易时间，格式：HHMMSS
        /// </summary>
        [JsonPropertyName("tradeTime")]

        public string TradeTime { get; set; }

        /// <summary>
        /// 交易金额，格式：小数点后两位
        /// </summary>
        [JsonPropertyName("amt")]

        public string Amount { get; set; }

        /// <summary>
        /// 借贷标识：1-借，2-贷
        /// </summary>
        [JsonPropertyName("transferInOut")]
        public string TransferType { get; set; }

        /// <summary>
        /// 账户余额，格式：小数点后两位
        /// </summary>
        [JsonPropertyName("balance")]

        public string Balance { get; set; }

        /// <summary>
        /// 对方账号
        /// </summary>
        [JsonPropertyName("optBankAcco")]

        public string CounterpartyAccount { get; set; }

        /// <summary>
        /// 对方户名
        /// </summary>
        [JsonPropertyName("optAccName")]

        public string CounterpartyName { get; set; }

        /// <summary>
        /// 对方开户行名称
        /// </summary>
        [JsonPropertyName("optOpenBankName")]

        public string CounterpartyBank { get; set; }

        /// <summary>
        /// 本方账号
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public string OurAccount { get; set; }

        /// <summary>
        /// 交易摘要
        /// </summary>
        [JsonPropertyName("digest")]

        public string Summary { get; set; }

        /// <summary>
        /// 表记录ID
        /// </summary>
        [JsonPropertyName("id")]
        public string RecordId { get; set; }

        /// <summary>
        /// 交易流水号
        /// </summary>
        [JsonPropertyName("serialNo")]

        public string TransactionNo { get; set; }

        /// <summary>
        /// 流水详细状态
        /// 0-无关联订单，1-已下单有余款，2-已下单或已退款，3-退款中，5-已下单多余款项退款中，空-其他状态
        /// </summary>
        [JsonPropertyName("detailStatus")]

        public string Status { get; set; }


        public BankTransaction ToObject()
        {
            return new BankTransaction
            {
                Id = $"{OurAccount}|{TransactionNo}",
                Time = DateTime.ParseExact(TradeDate + TradeTime, "yyyyMMddHHmmss", null),
                Amount = ParseDecimal(Amount),
                AccountNo = OurAccount,
                // 此属性缺失
                AccountBank = "unset",
                AccountName = "unset",
                Direction = TransferType == "1" ? TransctionDirection.Pay : TransctionDirection.Receive,
                CounterBank = CounterpartyBank,
                CounterName = CounterpartyName,
                CounterNo = CounterpartyAccount,
                Balance = ParseDecimal(Balance),
                Serial = TransactionNo,
                Remark = Summary,
            };
        }
    }



    public class BankBalanceJson
    {  /// <summary>
       /// 币种
       /// </summary>
        [JsonPropertyName("curType")]
        public string Currency { get; set; }

        /// <summary>
        /// 基金代码（最大长度32）
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// 基金名称（最大长度250）
        /// </summary>
        [JsonPropertyName("fundName")]
        public string FundName { get; set; }

        /// <summary>
        /// 账户类型（最大长度2）
        /// </summary>
        [JsonPropertyName("accType")]
        public string AccountType { get; set; }

        /// <summary>
        /// 银行账号（最大长度32）
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public string BankAccountNo { get; set; }

        /// <summary>
        /// 账户名称（最大长度64）
        /// </summary>
        [JsonPropertyName("accName")]
        public string AccountName { get; set; }

        /// <summary>
        /// 银行编号（最大长度6，通常为人行大额支付码前三位）
        /// </summary>
        [JsonPropertyName("bankNo")]
        public string BankNumber { get; set; }

        /// <summary>
        /// 开户行名称（最大长度250）
        /// </summary>
        [JsonPropertyName("openBankName")]
        public string OpenBankName { get; set; }

        /// <summary>
        /// 银行余额（最大长度20，保留两位小数）
        /// </summary>
        [JsonPropertyName("acctBal")]
        public string AccountBalance { get; set; }

        /// <summary>
        /// 托管户账户状态（仅托管户有效，募集户无意义）
        /// 参见 3.16 托管账户状态
        /// </summary>
        [JsonPropertyName("acctStatus")]
        public string CustodialStatus { get; set; }

        /// <summary>
        /// 募集户账户状态（仅募集户有效，托管户无意义）
        /// 参见 3.23 募集户账户状态
        /// </summary>
        [JsonPropertyName("raiseStatus")]
        public string RaiseStatus { get; set; }

        /// <summary>
        /// 最后更新时间（格式：YYYY-MM-dd HH:mm:ss）
        /// </summary>
        [JsonPropertyName("retTime")]
        public string LastUpdateTime { get; set; }



        public FundBankBalance ToObject()
        {
            return new FundBankBalance
            {
                AccountName = AccountName,
                AccountNo = BankAccountNo,
                Name = OpenBankName,
                FundName = FundName,
                FundCode = FundCode,
                Balance = string.IsNullOrWhiteSpace(AccountBalance) ? 0 : ParseDecimal(AccountBalance),
                Currency = ParseCurrency(Currency),
                Time = string.IsNullOrWhiteSpace(LastUpdateTime) ? default : DateTime.ParseExact(LastUpdateTime, "yyyy-MM-dd HH:mm:ss", null)
            };
        }

    }


    public class SubjectFundMappingJson
    {
        /// <summary>
        /// 产品编码（必填，最大长度32）
        /// </summary>
        [JsonPropertyName("productNo")]
        public string ProductNo { get; set; }

        /// <summary>
        /// 产品名称（必填，最大长度250）
        /// </summary>
        [JsonPropertyName("productName")]
        public string ProductName { get; set; }

        /// <summary>
        /// 公司名称（必填，最大长度128）
        /// </summary>
        [JsonPropertyName("companyName")]
        public string CompanyName { get; set; }

        /// <summary>
        /// 产品状态（必填，字典见附录3.4，最大长度2）
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary>
        /// 成立时间（必填，格式yyyyMMdd）
        /// </summary>
        [JsonPropertyName("publishDate")]
        public string PublishDate { get; set; }

        /// <summary>
        /// 托管机构（非必填，最大长度128）
        /// </summary>
        [JsonPropertyName("custodianOrg")]
        public string CustodianOrg { get; set; }

        /// <summary>
        /// 外包机构（非必填，最大长度128）
        /// </summary>
        [JsonPropertyName("outsourcingOrg")]
        public string OutsourcingOrg { get; set; }

        /// <summary>
        /// 母产品代码（非必填，仅子产品返回，最大长度32）
        /// </summary>
        [JsonPropertyName("parentProductNo")]
        public string ParentProductNo { get; set; }

        /// <summary>
        /// 母产品名称（非必填，仅子产品返回，最大长度250）
        /// </summary>
        [JsonPropertyName("parentProductName")]
        public string ParentProductName { get; set; }


        public SubjectFundMapping ToObject()
        {
            var sc = !string.IsNullOrWhiteSpace(ParentProductName) ? ProductName.Replace(ProductName, "") : "";
            return new SubjectFundMapping
            {
                FundCode = ProductNo,
                FundName = ProductName,
                MasterCode = ParentProductNo,
                MasterName = ParentProductName,
                ShareClass = sc,
                Status = GetStatus(Status),
            };
        }


        public FundStatus GetStatus(string s)
        {
            return s switch
            {
                "0" => FundStatus.Initiate,
                "1" => FundStatus.Normal,
                "2" => FundStatus.StartLiquidation,
                "3" => FundStatus.Liquidation,
                _ => FundStatus.Unk,
            };
        }
    }




    private static string ParseCurrency(string currencyCode)
    {
        switch (currencyCode)
        {
            case "156":
                return "CNY"; // 人民币
            case "250":
                return "CHF"; // 瑞士法郎
            case "280":
                return "DEM"; // 德国马克（已停用）
            case "344":
                return "HKD"; // 港元
            case "392":
                return "JPY"; // 日元
            case "826":
                return "GBP"; // 英镑
            case "840":
                return "USD"; // 美元
            case "954":
                return "EUR"; // 欧元
            default:
                return "";  // 或者抛出异常，根据需要处理无效编码
        }
    }

}

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。