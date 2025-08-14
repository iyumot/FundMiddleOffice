using FMO.Models;

namespace FMO.Trustee;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
public class SubjectFundMappingJson
{
    /// <summary>
    /// 产品代码
    /// </summary>
    public string FundCode { get; set; }

    /// <summary>
    /// 产品名称
    /// </summary>
    public string FundName { get; set; }

    /// <summary>
    /// 成立日期
    /// </summary>
    public string SetupDate { get; set; }

    /// <summary>
    /// 产品状态
    /// </summary>
    /// <remarks>
    /// 1. 成立前
    /// 2. 运行
    /// 3. 清盘
    /// 4. 二次清盘
    /// </remarks>
    public string PdtStatus { get; set; }

    /// <summary>
    /// 协会备案代码
    /// </summary>
    public string XhBadm { get; set; }

    /// <summary>
    /// 分级类型
    /// </summary>
    /// <remarks>
    /// 1: 母产品
    /// 2: 子产品
    /// </remarks>
    public string InvestFunds { get; set; }

    /// <summary>
    /// 是否分级
    /// </summary>
    /// <remarks>
    /// 1: 是
    /// 0: 否
    /// </remarks>
    public string IfGrading { get; set; }

    /// <summary>
    /// 母基金代码
    /// </summary>
    public string ParentTACode { get; set; }

    /// <summary>
    /// 母基金名称
    /// </summary>
    public string ParentFundName { get; set; }

    public SubjectFundMapping ToObject()
    {
        var sc = IfGrading == "1" ? FundName.Replace(ParentFundName, "") : "";
        return new SubjectFundMapping { FundCode = FundCode, FundName = FundName, MasterCode = ParentTACode, MasterName = ParentFundName, ShareClass = sc };
    }
}


#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。