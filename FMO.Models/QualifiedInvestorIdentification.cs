using System.ComponentModel;

namespace FMO.Models;


public enum QualificationType
{
    None,

    [Description("资产证明")] Assets,

    [Description("收入证明")] Income,

    [Description("金融机构 ")] FinancialInstitution,

    [Description("金融产品 ")] Product,

    [Description("公益基金")] PublicWelfareFund,

    [Description("QFII")] QFII,

    [Description("RQFII")] RQFII,


}

public class InvestorFileInfo
{
    public int Id { get; set; }

    public string? Path { get; set; }

    public required string Name { get; set; }


    public DateTime Time { get; set; }

    public string? Hash { get; set; }

    /// <summary>
    /// 标注
    /// </summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 合格投资者认定
/// </summary>
public class InvestorQualification
{
    /// <summary>
    /// 普通 / 专业
    /// </summary>
    public InvestorType InvestorType { get; set; }


    public int MyProperty { get; set; }

    /// <summary>
    /// 承诺函
    /// </summary>
    public FileStorageInfo? CommitmentLetter { get; set; }

    /// <summary>
    /// 基本信息表
    /// </summary>
    public FileStorageInfo? InfomationSheet { get; set; }

    /// <summary>
    /// 普通/专业投资者告知书
    /// </summary>
    public FileStorageInfo? Notice { get; set; }

    /// <summary>
    /// 税收声明
    /// </summary>
    public FileStorageInfo? TaxDeclaration { get; set; }

    /// <summary>
    /// 证明材料
    /// </summary>
    public FileStorageInfo? CertificationMaterials { get; set; }

}
