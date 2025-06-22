using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;

public partial class CSC
{
    public class RetJson
    {
        [JsonPropertyName("retCode")]
        public required string Code { get; set; }


        [JsonPropertyName("retMsg")]
        public required string Msg { get; set; }

    }




    public class RetJson<T>
    {
        [JsonPropertyName("retCode")]
        public required string Code { get; set; }


        [JsonPropertyName("retMsg")]
        public required string Msg { get; set; }

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
        public required string ConfDate { get; set; }

        /// <summary>
        /// 申请日期 (格式: YYYYMMDD, 8位)
        /// </summary>
        [JsonPropertyName("applyDate")]
        public required string ApplyDate { get; set; }

        /// <summary>
        /// 基金代码 (32位)
        /// </summary>
        [JsonPropertyName("fundCode")]
        public required string FundCode { get; set; }

        /// <summary>
        /// 基金名称 (250字符)
        /// </summary>
        [JsonPropertyName("fundName")]
        public required string FundName { get; set; }

        /// <summary>
        /// 销售商代码 (6位)
        /// </summary>
        [JsonPropertyName("agencyNo")]
        public required string AgencyNo { get; set; }

        /// <summary>
        /// 销售商名称 (64字符)
        /// </summary>
        [JsonPropertyName("agencyName")]
        public required string AgencyName { get; set; }

        /// <summary>
        /// 投资者名称 (64字符)
        /// </summary>
        [JsonPropertyName("custName")]
        public required string CustName { get; set; }

        /// <summary>
        /// 基金账号 (20字符)
        /// </summary>
        [JsonPropertyName("fundAcco")]
        public required string FundAcco { get; set; }

        /// <summary>
        /// 交易账号 (16字符)
        /// </summary>
        [JsonPropertyName("tradeAcco")]
        public required string TradeAcco { get; set; }

        /// <summary>
        /// 银行账号 (32字符)
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public required string BankAcco { get; set; }

        /// <summary>
        /// 银行编号 (6字符)
        /// </summary>
        [JsonPropertyName("bankNo")]
        public required string BankNo { get; set; }

        /// <summary>
        /// 开户行名称 (250字符)
        /// </summary>
        [JsonPropertyName("openBankName")]
        public required string OpenBankName { get; set; }

        /// <summary>
        /// 银行户名 (64字符)
        /// </summary>
        [JsonPropertyName("nameInBank")]
        public required string NameInBank { get; set; }

        /// <summary>
        /// 客户类型 (6字符)
        /// </summary>
        [JsonPropertyName("custType")]
        public required string CustType { get; set; }

        /// <summary>
        /// 证件类型 (3字符)
        /// </summary>
        [JsonPropertyName("certiType")]
        public required string CertiType { get; set; }

        /// <summary>
        /// 证件号 (32字符)
        /// </summary>
        [JsonPropertyName("certiNo")]
        public required string CertiNo { get; set; }

        /// <summary>
        /// TA发起标志 (2字符)
        /// </summary>
        [JsonPropertyName("taFlag")]
        public required string TaFlag { get; set; }

        /// <summary>
        /// 确认状态 (2字符)
        /// </summary>
        [JsonPropertyName("confStatus")]
        public required string ConfStatus { get; set; }

        /// <summary>
        /// 确认结果描述 (250字符)
        /// </summary>
        [JsonPropertyName("describe")]
        public required string Describe { get; set; }

        /// <summary>
        /// 分红方式 (2字符)
        /// </summary>
        [JsonPropertyName("bonusType")]
        public required string BonusType { get; set; }

        /// <summary>
        /// 申请金额 (20字符)
        /// </summary>
        [JsonPropertyName("balance")]
        public required string Balance { get; set; }

        /// <summary>
        /// 申请份额 (20字符)
        /// </summary>
        [JsonPropertyName("shares")]
        public required string Shares { get; set; }

        /// <summary>
        /// 单位净值 (10字符)
        /// </summary>
        [JsonPropertyName("netValue")]
        public required string NetValue { get; set; }

        /// <summary>
        /// 确认金额 (20字符)
        /// </summary>
        [JsonPropertyName("confBalance")]
        public required string ConfBalance { get; set; }

        /// <summary>
        /// 确认份额 (20字符)
        /// </summary>
        [JsonPropertyName("confShares")]
        public required string ConfShares { get; set; }

        /// <summary>
        /// 手续费 (20字符)
        /// </summary>
        [JsonPropertyName("charge")]
        public required string Charge { get; set; }

        /// <summary>
        /// 归管理人手续费 (20字符)
        /// </summary>
        [JsonPropertyName("managerCharge")]
        public required string ManagerCharge { get; set; }

        /// <summary>
        /// 归销售商手续费 (20字符)
        /// </summary>
        [JsonPropertyName("distributorCharge")]
        public required string DistributorCharge { get; set; }

        /// <summary>
        /// 归产品手续费 (20字符)
        /// </summary>
        [JsonPropertyName("fundcharge")]
        public required string Fundcharge { get; set; }

        /// <summary>
        /// 业绩报酬 (20字符)
        /// </summary>
        [JsonPropertyName("achievementPay")]
        public required string AchievementPay { get; set; }

        /// <summary>
        /// TA确认编号 (32字符)
        /// </summary>
        [JsonPropertyName("cserialNo")]
        public required string CserialNo { get; set; }

        /// <summary>
        /// 确认业务类型 (6字符)
        /// </summary>
        [JsonPropertyName("busiFlag")]
        public required string BusiFlag { get; set; }

        /// <summary>
        /// 原外部系统的申请流水号 (32字符)
        /// </summary>
        [JsonPropertyName("originalNo")]
        public required string OriginalNo { get; set; }

        /// <summary>
        /// 操作方式 (1字符)
        /// </summary>
        [JsonPropertyName("operWayNew")]
        public required string OperWayNew { get; set; }

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
                RequestAmount = decimal.Parse(Balance),
                RequestShare = decimal.Parse(Shares),
                ConfirmedAmount = decimal.Parse(ConfBalance),
                ConfirmedShare = decimal.Parse(ConfShares),
                CreateDate = DateOnly.FromDateTime(DateTime.Today),
                ExternalId = CserialNo,
                PerformanceFee = decimal.Parse(AchievementPay),
                Fee = decimal.Parse(Charge),
                ExternalRequestId = OriginalNo,
                Type = ParseType(BusiFlag),
                Source = "api",
            };
            // 净额
            r.ConfirmedNetAmount = r.ConfirmedAmount - r.Fee;

            return r;
        }


        private TARecordType ParseType(string str)
        {
            switch (str)
            {
                case "120": // 认购 
                    return TARecordType.Subscription;
                case "122": // 申购 
                    return TARecordType.Purchase;
                case "124": // 赎回 
                    return TARecordType.Redemption;
                case "126": // 转销售机构 
                    return TARecordType.MoveIn;
                case "127": // 转销售机构入 
                    return TARecordType.MoveIn;
                case "129": // 设置分红方式 
                    return TARecordType.BonusType;
                case "131": // 份额冻结 
                    return TARecordType.Frozen;
                case "132": // 份额解冻 
                    return TARecordType.Thawed;
                case "133": // 转让 
                    return TARecordType.TransferOut;
                case "134": // 受让 
                    return TARecordType.TransferIn;
                case "136": // 份额转换 
                    return TARecordType.SwitchOut;
                case "137": // 份额转换入 
                    return TARecordType.SwitchIn;

                case "142": // 强行赎回 
                    return TARecordType.ForceRedemption;
                case "143": // 红利发放 
                    return TARecordType.Distribution;
                case "144": // 强行调增 
                    return TARecordType.Increase;
                case "145": // 强行调减 
                    return TARecordType.Decrease;
                case "149": // 募集失败 
                    return TARecordType.RaisingFailed;
                case "150": // 基金清盘 
                    return TARecordType.Clear;

                default:
                    return TARecordType.UNK;
            }
        }

    }



    public class FundDailyFeeJson
    {

        /// <summary>
        /// 费用计算日期
        /// </summary>
        [JsonPropertyName("confDate")]
        public required string ConfDate { get; set; }

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

        #region 费用计提
        /// <summary>
        /// 管理费
        /// </summary>
        [JsonPropertyName("managementFee")]
        public required string ManagementFee { get; set; }

        /// <summary>
        /// 投顾费
        /// </summary>
        [JsonPropertyName("investmentFee")]
        public required string InvestmentFee { get; set; }

        /// <summary>
        /// 销售服务费
        /// </summary>
        [JsonPropertyName("salesFee")]
        public required string SalesFee { get; set; }

        /// <summary>
        /// 托管费
        /// </summary>
        [JsonPropertyName("custodianFee")]
        public required string CustodianFee { get; set; }

        /// <summary>
        /// 外包服务费
        /// </summary>
        [JsonPropertyName("outsourcingFee")]
        public required string OutsourcingFee { get; set; }

        /// <summary>
        /// 业绩报酬计提
        /// </summary>
        [JsonPropertyName("reward")]
        public required string Reward { get; set; }

        /// <summary>
        /// 增值税计提
        /// </summary>
        [JsonPropertyName("addedTax")]
        public required string AddedTax { get; set; }

        /// <summary>
        /// 附加税计提
        /// </summary>
        [JsonPropertyName("surTax")]
        public required string SurTax { get; set; }

        /// <summary>
        /// 增值税及附加税计提
        /// </summary>
        [JsonPropertyName("addedSurTax")]
        public required string AddedSurTax { get; set; }
        #endregion

        #region 费用支付
        /// <summary>
        /// 管理费支付
        /// </summary>
        [JsonPropertyName("managementFeePay")]
        public required string ManagementFeePay { get; set; }

        /// <summary>
        /// 投顾费支付
        /// </summary>
        [JsonPropertyName("investmentFeePay")]
        public required string InvestmentFeePay { get; set; }

        /// <summary>
        /// 销售服务费支付
        /// </summary>
        [JsonPropertyName("salesFeePay")]
        public required string SalesFeePay { get; set; }

        /// <summary>
        /// 托管费支付
        /// </summary>
        [JsonPropertyName("custodianFeePay")]
        public required string CustodianFeePay { get; set; }

        /// <summary>
        /// 外包服务费支付
        /// </summary>
        [JsonPropertyName("outsourcingFeePay")]
        public required string OutsourcingFeePay { get; set; }

        /// <summary>
        /// 业绩报酬支付
        /// </summary>
        [JsonPropertyName("rewardPay")]
        public required string RewardPay { get; set; }

        /// <summary>
        /// 增值税及附加税支付
        /// </summary>
        [JsonPropertyName("addedSurTaxPay")]
        public required string AddedSurTaxPay { get; set; }
        #endregion

        #region 费用余额
        /// <summary>
        /// 管理费余额
        /// </summary>
        [JsonPropertyName("managementFeeBalance")]
        public required string ManagementFeeBalance { get; set; }

        /// <summary>
        /// 投顾费余额
        /// </summary>
        [JsonPropertyName("investmentFeeBalance")]
        public required string InvestmentFeeBalance { get; set; }

        /// <summary>
        /// 销售服务费余额
        /// </summary>
        [JsonPropertyName("salesFeeBalance")]
        public required string SalesFeeBalance { get; set; }

        /// <summary>
        /// 托管费余额
        /// </summary>
        [JsonPropertyName("custodianFeeBalance")]
        public required string CustodianFeeBalance { get; set; }

        /// <summary>
        /// 外包服务费余额
        /// </summary>
        [JsonPropertyName("outsourcingFeeBalance")]
        public required string OutsourcingFeeBalance { get; set; }

        /// <summary>
        /// 业绩报酬余额
        /// </summary>
        [JsonPropertyName("rewardBalance")]
        public required string RewardBalance { get; set; }

        /// <summary>
        /// 增值税及附加税余额
        /// </summary>
        [JsonPropertyName("addedSurTaxBalance")]
        public required string AddedSurTaxBalance { get; set; }
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
        /// 交易日期，格式：YYYY-MM-DD
        /// </summary>
        [JsonPropertyName("tradeDate")]

        public required string TradeDate { get; set; }

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
        /// 交易时间，格式：HHMMSS
        /// </summary>
        [JsonPropertyName("tradeTime")]

        public required string TradeTime { get; set; }

        /// <summary>
        /// 交易金额，格式：小数点后两位
        /// </summary>
        [JsonPropertyName("amt")]

        public required string Amount { get; set; }

        /// <summary>
        /// 借贷标识：1-借，2-贷
        /// </summary>
        [JsonPropertyName("transferInOut")]
        public required string TransferType { get; set; }

        /// <summary>
        /// 账户余额，格式：小数点后两位
        /// </summary>
        [JsonPropertyName("balance")]

        public required string Balance { get; set; }

        /// <summary>
        /// 对方账号
        /// </summary>
        [JsonPropertyName("optBankAcco")]

        public required string CounterpartyAccount { get; set; }

        /// <summary>
        /// 对方户名
        /// </summary>
        [JsonPropertyName("optAccName")]

        public required string CounterpartyName { get; set; }

        /// <summary>
        /// 对方开户行名称
        /// </summary>
        [JsonPropertyName("optOpenBankName")]

        public required string CounterpartyBank { get; set; }

        /// <summary>
        /// 本方账号
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public required string OurAccount { get; set; }

        /// <summary>
        /// 交易摘要
        /// </summary>
        [JsonPropertyName("digest")]

        public required string Summary { get; set; }

        /// <summary>
        /// 表记录ID
        /// </summary>
        [JsonPropertyName("id")]
        public required string RecordId { get; set; }

        /// <summary>
        /// 交易流水号
        /// </summary>
        [JsonPropertyName("serialNo")]

        public required string TransactionNo { get; set; }

        /// <summary>
        /// 流水详细状态
        /// 0-无关联订单，1-已下单有余款，2-已下单或已退款，3-退款中，5-已下单多余款项退款中，空-其他状态
        /// </summary>
        [JsonPropertyName("detailStatus")]

        public required string Status { get; set; }


        public BankTransaction ToObject()
        {
            return new BankTransaction
            {
                Id = $"{OurAccount}|{TransactionNo}",
                Time = DateTime.ParseExact(TradeDate + TradeTime, "yyyyMMddHHmmss", null),
                Amount = decimal.Parse(Amount),
                AccountNo = OurAccount,
                // 此属性缺失
                AccountBank = "unset",
                AccountName = "unset",
                Direction = TransferType == "1" ? TransctionDirection.Pay : TransctionDirection.Receive,
                CounterBank = CounterpartyBank,
                CounterName = CounterpartyName,
                CounterNo = CounterpartyAccount,
                Balance = decimal.Parse(Balance),
                Serial = TransactionNo,
                Remark = Summary,
            };
        }
    }












}
