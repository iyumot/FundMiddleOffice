using System.ComponentModel;

namespace FMO.Models;


public enum QualificationType
{
    None,

    [Description("资产证明")] Assets,

    [Description("收入证明")] Income,

    [Description("员工")] Employee,

    [Description("金融机构 ")] FinancialInstitution,

    [Description("备案金融产品 ")] RegisteredProduct,

    [Description("金融产品 ")] UnregisteredProduct,

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
/// 
// （一）经有关金融监管部门批准设立的金融机构，包括证券公司、期货公司、基金管理公司及其子公司、商业银行、保险公司、信托公司、财务公司等；经行业协会备案或者登记的证券公司子公司、期货公司子公司、私募基金管理人。

//（二）上述机构面向投资者发行的理财产品，包括但不限于证券公司资产管理产品、基金管理公司及其子公司产品、期货公司资产管理产品、银行理财产品、保险产品、信托产品、经行业协会备案的私募基金。

//（三）社会保障基金、企业年金等养老基金，慈善基金等社会公益基金，合格境外机构投资者（QFII）、人民币合格境外机构投资者（RQFII）。

//（四）同时符合下列条件的法人或者其他组织：

//1.最近1年末净资产不低于2000万元；

//2.最近1年末金融资产不低于1000万元；

//3.具有2年以上证券、基金、期货、黄金、外汇等投资经历。

//（五）同时符合下列条件的自然人：

//1.金融资产不低于500万元，或者最近3年个人年均收入不低于50万元；

//2.具有2年以上证券、基金、期货、黄金、外汇等投资经历，或者具有2年以上金融产品设计、投资、风险管理及相关工作经历，或者属于本条第（一）项规定的专业投资者的高级管理人员、获得职业资格认证的从事金融相关业务的注册会计师和律师。

//前款所称金融资产，是指银行存款、股票、债券、基金份额、资产管理计划、银行理财产品、信托计划、保险产品、期货及其他衍生产品等。

///////////////////////////////////////////////////
//（一）净资产不低于1000万元的单位；
//（二）金融资产不低于300万元或者最近三年个人年均收入不低于50万元
//的个人
/// </summary>
public class InvestorQualification
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public int InvestorId { get; set; }

    public QualificationType MyProperty { get; set; }





    /// <summary>
    /// 普通 / 专业
    /// </summary>
    public QualifiedInvestorType InvestorType { get; set; }
   
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

    /// <summary>
    /// 代理人证件
    /// </summary>
    public FileStorageInfo? Agent { get; set; }

    /// <summary>
    /// 授权书
    /// </summary>
    public FileStorageInfo? Authorization { get; set; }


}
