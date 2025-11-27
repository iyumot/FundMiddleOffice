

using System;
using System.Collections.Generic;

namespace FMO.DS.MeiShi.Json.Order;

/// <summary>
/// 根级JSON响应实体类
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// 响应状态码（1008表示请求成功）
    /// </summary>
    public int code { get; set; }

    /// <summary>
    /// 响应数据主体
    /// </summary>
    public ResponseData data { get; set; }

    /// <summary>
    /// 响应消息（如"请求成功"）
    /// </summary>
    public string message { get; set; }

    /// <summary>
    /// 总记录数（此处为0，可能与data中的total字段区分使用）
    /// </summary>
    public int total { get; set; }
}

/// <summary>
/// 响应数据主体类
/// </summary>
public class ResponseData
{
    /// <summary>
    /// 删除数量
    /// </summary>
    public int deleteNum { get; set; }
     

    /// <summary>
    /// 每页显示条数
    /// </summary>
    public int pageSize { get; set; }

    /// <summary>
    /// 签约记录列表
    /// </summary>
    public List<SignRecord> list { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int pageNum { get; set; }

    /// <summary>
    /// 客户总数
    /// </summary>
    public int customersNumber { get; set; }

   

    /// <summary>
    /// 总记录数
    /// </summary>
    public int total { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int pages { get; set; }

    /// <summary>
    /// 签约总金额
    /// </summary>
    public decimal contractedNumAmount { get; set; }

    /// <summary>
    /// 签约总笔数
    /// </summary>
    public int contractedNum { get; set; }

 
}

/// <summary>
/// 签约记录详情类（对应list中的单条数据）
/// </summary>
public class SignRecord
{
    /// <summary>
    /// 订单类型（1表示特定类型订单）
    /// </summary>
    public int orderType { get; set; }

    /// <summary>
    /// 产品风险类型（4表示对应风险等级）
    /// </summary>
    public int productRiskType { get; set; }

    /// <summary>
    /// 赎回金额（单位：元）
    /// </summary>
    public decimal? redemptionMoney { get; set; }

    /// <summary>
    /// 身份识别状态（1表示已识别）
    /// </summary>
    public int identifyStatus { get; set; }

    /// <summary>
    /// 交易金额字符串（null表示无对应数据）
    /// </summary>
    public string? tradeMoneyStr { get; set; }

    /// <summary>
    /// 产品名称
    /// </summary>
    public string? productName { get; set; }

    /// <summary>
    /// 客户类型（2表示特定客户类型，如企业客户）
    /// </summary>
    public int customerType { get; set; }

    /// <summary>
    /// 签约方式（4表示对应签约渠道，如线上签约）
    /// </summary>
    public int contractWay { get; set; }

    /// <summary>
    /// 签约类型（3表示特定签约类别）
    /// </summary>
    public int? signType { get; set; }

    /// <summary>
    /// 投资人名称（null表示未填写或无需填写）
    /// </summary>
    public string? capitalistName { get; set; }

    /// <summary>
    /// 分支机构（null表示无对应信息）
    /// </summary>
    public string? subbranch { get; set; }

    /// <summary>
    /// 无效来源（null表示有效或无无效记录）
    /// </summary>
    public string? invalidSource { get; set; }
 

    /// <summary>
    /// 审核类型（1表示对应审核类别）
    /// </summary>
    public int checkType { get; set; }

    /// <summary>
    /// 父产品ID（关联主产品）
    /// </summary>
    public long? parentProductId { get; set; }

    /// <summary>
    /// 产品ID（当前产品唯一标识）
    /// </summary>
    public long productId { get; set; }

    /// <summary>
    /// 适配类型字符串（null表示无对应描述）
    /// </summary>
    public string aptnessTypeStr { get; set; }

    /// <summary>
    /// 签约流程ID（签约流程唯一标识）
    /// </summary>
    public long signFlowId { get; set; }

    /// <summary>
    /// 是否可下载签约确认书（true表示可下载）
    /// </summary>
    public bool downloadSignConfirm { get; set; }

    /// <summary>
    /// 管理人名称（null表示未填写）
    /// </summary>
    public string managerNames { get; set; }

    /// <summary>
    /// 客户已签约风险类型（4表示对应风险等级）
    /// </summary>
    public int? customerSignedRiskType { get; set; }

    /// <summary>
    /// 是否显示记录状态（false表示不显示）
    /// </summary>
    public bool displayRecordStatus { get; set; }

    /// <summary>
    /// 流程更新时间（时间戳，单位：毫秒）
    /// </summary>
    public long flowUpdateTime { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    public string InvestorName { get; set; }

    /// <summary>
    /// 签约流程开始时间
    /// </summary>
    public string signFlowStartDate { get; set; }

    /// <summary>
    /// 赎回金额区间（null表示无对应信息）
    /// </summary>
    public string redemptionMoneySpan { get; set; }

    /// <summary>
    /// 是否修改开放日（false表示未修改）
    /// </summary>
    public bool modifyOpenDay { get; set; }

    /// <summary>
    /// 卡号（脱敏或标识卡号）
    /// </summary>
    public string cardNumber { get; set; }

    /// <summary>
    /// 投资人类型（2表示对应投资人类别）
    /// </summary>
    public int? investorType { get; set; }

    /// <summary>
    /// 管理人用户名（null表示未填写）
    /// </summary>
    public string managerUserNames { get; set; }

    /// <summary>
    /// 客户风险类型（4表示对应风险等级）
    /// </summary>
    public int? customerRiskType { get; set; }

    /// <summary>
    /// 开放日
    /// </summary>
    public string openDay { get; set; }

    /// <summary>
    /// 交易手续费（null表示无手续费或未计算）
    /// </summary>
    public object tradeFee { get; set; }

    /// <summary>
    /// 签约状态（2表示对应状态）
    /// </summary>
    public int signStatus { get; set; }

    /// <summary>
    /// 签约流程结束时间
    /// </summary>
    public string signFlowEndDate { get; set; }

    /// <summary>
    /// 适配类型（4表示对应适配等级）
    /// </summary>
    //public int aptnessType { get; set; }

    /// <summary>
    /// 银行名称（null表示未关联银行）
    /// </summary>
    public string bankName { get; set; }
 

    /// <summary>
    /// 签约确认（null表示未确认或无需确认）
    /// </summary>
    public object signConfirm { get; set; }
     

    /// <summary>
    /// 开放类型（1表示对应开放类别）
    /// </summary>
    public int? openType { get; set; }

    /// <summary>
    /// 机构订单状态（0表示初始状态）
    /// </summary>
    public int orgOrderStatus { get; set; }

    /// <summary>
    /// 客户ID（客户唯一标识）
    /// </summary>
    public long InvestorId { get; set; }

    /// <summary>
    /// 产品全称
    /// </summary>
    public string productFullName { get; set; }

    /// <summary>
    /// 产品日期ID（null表示无对应信息）
    /// </summary>
    //public object productDayId { get; set; }

    /// <summary>
    /// 交易类型（3表示赎回交易）
    /// </summary>
    public int? tradeType { get; set; }

    /// <summary>
    /// 投资人（null表示未填写）
    /// </summary>
    //public object capitalist { get; set; }

    /// <summary>
    /// 签约流程来源类型（4表示对应来源渠道）
    /// </summary>
    public int signFlowSourceType { get; set; }

    /// <summary>
    /// 签约金额（单位：元）
    /// </summary>
    public decimal? signMoney { get; set; }

    /// <summary>
    /// 赎回类型（1表示对应赎回类别）
    /// </summary>
    public int? redemptionType { get; set; }

    /// <summary>
    /// 是否删除（0表示未删除）
    /// </summary>
    public int isDelete { get; set; }

    /// <summary>
    /// 签约开放日（null表示无对应信息）
    /// </summary>
    public object signOpenDay { get; set; }

    /// <summary>
    /// 账号（空字符串表示未填写）
    /// </summary>
    public string accountNumber { get; set; }

    /// <summary>
    /// 失效日期（null表示有效）
    /// </summary>
    public object invalidDate { get; set; }

    /// <summary>
    /// 交易金额（null表示无对应数据）
    /// </summary>
    public decimal? tradeMoney { get; set; }

    /// <summary>
    /// 记录状态（null表示默认状态）
    /// </summary>
    public object recordStatus { get; set; }

    /// <summary>
    /// 状态描述（如"签约完成"）
    /// </summary>
    public string codeText { get; set; }

    /// <summary>
    /// 完成日期（null表示未完成）
    /// </summary>
    public object completionDate { get; set; }

    /// <summary>
    /// 是否回滚（0表示未回滚）
    /// </summary>
    public int isRollBacked { get; set; }

    /// <summary>
    /// 流程类型（3表示对应流程类别）
    /// </summary>
    public int flowType { get; set; }
}

