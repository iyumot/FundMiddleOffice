using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
public partial class CITICS
{
    public class TransferRecordJson
    {
        [JsonPropertyName("ackNo")]
        public string AckNo { get; set; }

        [JsonPropertyName("exRequestNo")]
        public string ExRequestNo { get; set; }

        [JsonPropertyName("fundAcco")]
        public string FundAcco { get; set; }

        [JsonPropertyName("tradeAcco")]
        public string TradeAcco { get; set; }

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
        public string Apkind { get; set; } // 业务类型代码

        [JsonPropertyName("taFlag")]
        public string TaFlag { get; set; } // TA发起标志：0-否，1-是

        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        [JsonPropertyName("shareLevel")]
        public string ShareLevel { get; set; } // 份额类别：A-前收费，B-后收费

        [JsonPropertyName("currency")]
        public string Currency { get; set; } // 币种：人民币、美元 或 null

        [JsonPropertyName("largeRedemptionFlag")]
        public string LargeRedemptionFlag { get; set; } // 巨额赎回处理标志：0-取消，1-顺延

        [JsonPropertyName("subAmt")]
        public string SubAmt { get; set; } // 申请金额

        [JsonPropertyName("subQuty")]
        public string SubQuty { get; set; } // 申请份额

        [JsonPropertyName("bonusType")]
        public string BonusType { get; set; } // 分红方式：0-红利再投资，1-现金红利

        [JsonPropertyName("nav")]
        public string Nav { get; set; } // 单位净值

        [JsonPropertyName("ackAmt")]
        public string AckAmt { get; set; } // 确认金额

        [JsonPropertyName("ackQuty")]
        public string AckQuty { get; set; } // 确认份额

        [JsonPropertyName("tradeFee")]
        public string TradeFee { get; set; } // 交易费

        [JsonPropertyName("taFee")]
        public string TaFee { get; set; } // 过户费

        [JsonPropertyName("backFee")]
        public string BackFee { get; set; } // 后收费

        [JsonPropertyName("realBalance")]
        public string RealBalance { get; set; } // 应返还金额

        [JsonPropertyName("profitBalance")]
        public string ProfitBalance { get; set; } // 业绩报酬

        [JsonPropertyName("profitBalanceForAgency")]
        public string ProfitBalanceForAgency { get; set; } // 业绩报酬归销售商

        [JsonPropertyName("totalNav")]
        public string TotalNav { get; set; } // 累计净值

        [JsonPropertyName("totalFee")]
        public string TotalFee { get; set; } // 总费用

        [JsonPropertyName("agencyFee")]
        public string AgencyFee { get; set; } // 归销售机构费用

        [JsonPropertyName("fundFee")]
        public string FundFee { get; set; } // 归基金资产费用

        [JsonPropertyName("registFee")]
        public string RegistFee { get; set; } // 归管理人费用

        [JsonPropertyName("interest")]
        public string Interest { get; set; } // 利息

        [JsonPropertyName("interestTax")]
        public string InterestTax { get; set; } // 利息税

        [JsonPropertyName("interestShare")]
        public string InterestShare { get; set; } // 利息转份额

        [JsonPropertyName("frozenBalance")]
        public string FrozenBalance { get; set; } // 确认冻结份额

        [JsonPropertyName("unfrozenBalance")]
        public string UnfrozenBalance { get; set; } // 确认解冻份额

        [JsonPropertyName("unShares")]
        public string UnShares { get; set; } // 巨额赎回顺延份额

        [JsonPropertyName("applyDate")]
        public string ApplyDate { get; set; } // 申请工作日

        [JsonPropertyName("ackDate")]
        public string AckDate { get; set; } // 确认日期

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
        public string AckStatus { get; set; } // 确认状态

        [JsonPropertyName("agencyNo")]
        public string AgencyNo { get; set; } // 销售商代码

        [JsonPropertyName("agencyName")]
        public string AgencyName { get; set; } // 销售商名称

        [JsonPropertyName("retCod")]
        public string RetCod { get; set; } // 返回码

        [JsonPropertyName("retMsg")]
        public string RetMsg { get; set; } // 失败原因

        [JsonPropertyName("adjustCause")]
        public string AdjustCause { get; set; } // 份额调整原因

        [JsonPropertyName("navDate")]
        public string NavDate { get; set; } // 净值日期 (YYYYMMDD)

        [JsonPropertyName("oriCserialNo")]
        public string OriCserialNo { get; set; } // 原确认单号

        /// <summary>
        /// 缺少Customer 信息
        /// Fund 信息
        /// </summary>
        /// <returns></returns>
        internal TransferRecord ToObject()
        {
            var r = new TransferRecord
            {
                ExternalId = AckNo,
                ExternalRequestId = ExRequestNo,
                Type = ParseRecordType(Apkind),
                FundCode = FundCode,
                FundName = "unset",
                Agency = AgencyName,
                RequestDate = DateOnly.ParseExact(ApplyDate, "yyyyMMdd"),
                RequestAmount = ParseDecimal(SubAmt),
                RequestShare = ParseDecimal(SubQuty),
                ConfirmedDate = DateOnly.ParseExact(AckDate, "yyyyMMdd"),
                ConfirmedAmount = ParseDecimal(AckAmt),
                ConfirmedShare = ParseDecimal(AckQuty),
                Fee = ParseDecimal(TotalFee),
                PerformanceFee = ParseDecimal(ProfitBalance),
                CustomerIdentity = "unset",
                CustomerName = "unset",
                CreateDate = DateOnly.FromDateTime(DateTime.Now),
                ConfirmedNetAmount = ParseDecimal(RealBalance),
                Source = AckStatus switch { "2" or "3" or "5" => "failed", _ => "api" },               
            };
            r.Fee -= r.PerformanceFee;
            r.ConfirmedNetAmount = r.ConfirmedAmount - r.Fee;
            return r;
        }
    }
}

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。