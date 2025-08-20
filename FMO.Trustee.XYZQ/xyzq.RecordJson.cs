using FMO.Models;

namespace FMO.Trustee;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。

internal class RecordJson:JsonBase
{

    /// <summary>
    /// 交易确认单号
    /// </summary>
    public string cserialno { get; set; }

    /// <summary>
    /// 基金账号
    /// </summary>
    public string fundacco { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    public string custname { get; set; }

    /// <summary>
    /// 销售商名称
    /// </summary>
    public string agencyname { get; set; }

    /// <summary>
    /// 销售商代码
    /// </summary>
    public string agencyno { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    public string businname { get; set; }

    /// <summary>
    /// 申请日期
    /// </summary>
    public string date { get; set; }

    /// <summary>
    /// 确认日期
    /// </summary>
    public string cdate { get; set; }

    /// <summary>
    /// 产品编码
    /// </summary>
    public string fundcode { get; set; }

    /// <summary>
    /// 产品名称
    /// </summary>
    public string fundname { get; set; }

    /// <summary>
    /// 申请金额
    /// </summary>
    public string balance { get; set; }

    /// <summary>
    /// 申请份额
    /// </summary>
    public string shares { get; set; }

    /// <summary>
    /// 确认金额
    /// </summary>
    public string confirmbalance { get; set; }

    /// <summary>
    /// 确认份额
    /// </summary>
    public string confirmshares { get; set; }

    /// <summary>
    /// 交易费用
    /// </summary>
    public string tradefare { get; set; }

    /// <summary>
    /// 归管理人费
    /// </summary>
    public string registfare { get; set; }

    /// <summary>
    /// 归基金资产
    /// </summary>
    public string fundfare { get; set; }

    /// <summary>
    /// 业绩报酬
    /// </summary>
    public string factdeductbalance { get; set; }

    /// <summary>
    /// 单位净值
    /// </summary>
    public string netvalue { get; set; }

    /// <summary>
    /// 确认状态
    /// </summary>
    public string statusname { get; set; }

    /// <summary>
    /// 证件类型
    /// </summary>
    public string identitytype { get; set; }

    /// <summary>
    /// 证件号码
    /// </summary>
    public string identityno { get; set; }

    /// <summary>
    /// 交易账号
    /// </summary>
    public string tradeacco { get; set; }

    /// <summary>
    /// 客户类型
    /// </summary>
    public string custtypename { get; set; }

    /// <summary>
    /// 总费用
    /// </summary>
    public string totalfare { get; set; }

    /// <summary>
    /// 归销售机构费用
    /// </summary>
    public string agencyfare { get; set; }

    /// <summary>
    /// 分红方式
    /// </summary>
    public string bonustype { get; set; }

    /// <summary>
    /// 银行名称
    /// </summary>
    public string bankno { get; set; }

    /// <summary>
    /// 银行账号
    /// </summary>
    public string bankacco { get; set; }

    /// <summary>
    /// 银行户名
    /// </summary>
    public string nameinbank { get; set; }

    public TransferRecord ToObject()
    {
        var r = new TransferRecord
        {
            FundCode = fundcode,
            ConfirmedDate = DateOnly.ParseExact(cdate, "yyyyMMdd"),
            RequestDate = DateOnly.ParseExact(date, "yyyyMMdd"),
            FundName = fundname,
            Agency = agencyname,
            InvestorName = custname,
            InvestorIdentity = identityno,
            RequestAmount = ParseDecimal(balance),
            RequestShare = ParseDecimal(shares),
            ConfirmedAmount = ParseDecimal(confirmbalance),
            ConfirmedShare = ParseDecimal(confirmshares),
            CreateDate = DateOnly.FromDateTime(DateTime.Today),
            ExternalId = $"{XYZQ._Identifier}.{cserialno}",
            PerformanceFee = ParseDecimal(factdeductbalance),
            Fee = ParseDecimal(totalfare),
            //ExternalRequestId = $"{CSC._Identifier}.{OriginalNo}",
            Type = Parse(businname),
            Source = "api",
        };
        // 净额
        r.ConfirmedNetAmount = r.ConfirmedAmount - r.Fee;

        if (r.Type == TransferRecordType.UNK)
            JsonBase.ReportSpecialType(new(0, XYZQ._Identifier, nameof(TransferRecord), r.ExternalId, businname));
        return r;
    }
    public static TransferRecordType Parse(string type)
    {
        return type switch
        {
            // -----------------------------
            // 常见交易类
            // -----------------------------
            "01" => TransferRecordType.Subscription,         // 认购
            "02" => TransferRecordType.Purchase,             // 申购
            "03" => TransferRecordType.Redemption,           // 赎回
            "05" => TransferRecordType.TransferIn,           // 托管转入
            "06" => TransferRecordType.TransferOut,          // 托管转出
            "07" => TransferRecordType.BonusType,            // 修改分红方式
            "13" => TransferRecordType.SwitchOut,            // 基金转换(出)
            "15" => TransferRecordType.TransferOut,          // 非交易过户出
            "16" => TransferRecordType.SwitchIn,             // 基金转换入
            "14" => TransferRecordType.TransferIn,           // 非交易过户入
           // "17" => TransferRecordType.Abort,                // 撤单
            "20" => TransferRecordType.TransferOut,          // 内部转托管
            "21" => TransferRecordType.TransferIn,           // 内部转托管入
            "22" => TransferRecordType.TransferOut,          // 内部转托管出
            "25" => TransferRecordType.Redemption,           // 预约赎回
            "39" => TransferRecordType.Purchase,             // 定期定额申购
            "50" => TransferRecordType.InitialOffer,         // 基金成立 → 视为认购
            "53" => TransferRecordType.ForceRedemption,      // 强制赎回
            "70" => TransferRecordType.Increase,             // 强制调增
            "71" => TransferRecordType.Decrease,             // 强制调减
            "74" => TransferRecordType.Distribution,         // 红利发放（分红）
            "95" => TransferRecordType.Redemption,           // 定期定额赎回

            // -----------------------------
            // 特殊操作 & 扩展
            // -----------------------------
            "A1" => TransferRecordType.Increase,             // 盘后合并入
            "A2" => TransferRecordType.Increase,             // 盘后分拆入
            "A3" => TransferRecordType.Decrease,             // 盘后分拆出
            "A4" => TransferRecordType.Increase,             // 盘后合并出 → 实际是调减？但逻辑上合并出是减少，这里可能需业务确认
            "A5" => TransferRecordType.Increase,             // 赎回保底强增
            "A6" => TransferRecordType.Decrease,             // 赎回保底强减
            "A7" => TransferRecordType.Increase,             // 统一保底保本强增
            "A8" => TransferRecordType.Decrease,             // 统一保底保本强减
            "A9" => TransferRecordType.Increase,             // 保本转期换算强增
            "AA" => TransferRecordType.Decrease,             // 保本转期换算强减
            "B0" => TransferRecordType.SwitchIn,             // 跨TA基金转换（整体）
            "B1" => TransferRecordType.SwitchOut,            // 跨TA基金转换出
            "B2" => TransferRecordType.SwitchIn,             // 跨TA基金转换入
            "C1" => TransferRecordType.TransferIn,           // 网点变更入
            "C2" => TransferRecordType.TransferOut,          // 网点变更出
            "C3" => TransferRecordType.Decrease,             // 基金拆分强减
            "F1" => TransferRecordType.Increase,             // 基金拆分强增
            "F2" => TransferRecordType.Distribution,         // 正收益支付 → 分红
            "F3" => TransferRecordType.SwitchIn,             // 基金分级入
            "F4" => TransferRecordType.Distribution,         // 负收益支付 → 分红
            "F5" => TransferRecordType.SwitchOut,            // 基金分级出
            "F6" => TransferRecordType.Increase,             // 统一业绩提成强增
            "F7" => TransferRecordType.Decrease,             // 统一业绩提成强减
            "F8" => TransferRecordType.Increase,             // 分红业绩提成强增
            "F9" => TransferRecordType.Decrease,             // 分红业绩提成强减
            "G1" => TransferRecordType.Redemption,           // 定期兑付赎回
            "JS" => TransferRecordType.Purchase,             // 买入待交收恢复交收 → 买入
            "JY" => TransferRecordType.TransferIn,           // 解押 → 视为转入
            "KC" => TransferRecordType.TransferOut,          // 快速过户出
            "KH" => TransferRecordType.TransferOut,          // 快速过户（默认视为出）
            "KR" => TransferRecordType.TransferIn,           // 快速过户入
            "M2" => TransferRecordType.Increase,             // 管理人强增
            "M3" => TransferRecordType.Decrease,             // 管理人强减
            "M4" => TransferRecordType.Increase,             // 恢复交收强增
            "M5" => TransferRecordType.Increase,             // 二级账户收益强增
            "M6" => TransferRecordType.Decrease,             // 一级账户收益强减
            "mc" => TransferRecordType.Redemption,           // 卖出
            "mr" => TransferRecordType.Purchase,             // 买入
            "Z2" => TransferRecordType.Subscription,         // 合同生效 → 认购
            "Z3" => TransferRecordType.Purchase,             // 开放日参与 → 申购
            "Z4" => TransferRecordType.Redemption,           // 开放日退出 → 赎回
            "Z5" => TransferRecordType.Redemption,           // 违约退出 → 赎回
           // "Z6" => TransferRecordType.Abort,                // 合同终止 → 撤单/取消
           // "Z7" => TransferRecordType.Abort,                // 募集失败 → 取消
            "ZC" => TransferRecordType.TransferOut,          // 转指定出
            "ZR" => TransferRecordType.TransferIn,           // 转指定入
            "ZT" => TransferRecordType.SwitchIn,             // 定期转换协议 → 转入
            "ZY" => TransferRecordType.TransferIn,           // 质押 → 视为转入（或可单独处理）

            // -----------------------------
            // 其他未明确映射（可选：返回 UNK 或按需扩展）
            // -----------------------------
            "04" => TransferRecordType.TransferOut,          // 转托管 → 视为转出
            "10" => TransferRecordType.UNK,                  // 份额冻结（无对应）
            "11" => TransferRecordType.UNK,                  // 份额解冻（无对应）
            "12" => TransferRecordType.TransferOut,          // 非交易过户 → 视为转出
            "51" => TransferRecordType.UNK,                  // 基金终止（无直接对应）
            "52" => TransferRecordType.UNK,                  // 基金清盘
           // "54" => TransferRecordType.Abort,                // 发行失败 → 取消
            "58" => TransferRecordType.UNK,                  // 统一（含义模糊）
            "67" => TransferRecordType.UNK,                  // 联名卡开通（非交易类）
            "68" => TransferRecordType.UNK,                  // 联名卡撤销
            _ => TransferRecordType.UNK                      // 默认未知
        };
    }
}

  
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。