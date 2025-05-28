using System.Text.Json.Serialization;

namespace FMO.Models;


/// <summary>
/// 机构：公司/企业
/// </summary>
public class Institution : LegalEntity
{

    public string? EnglishName { get; set; }

    /// <summary>
    /// 实控人
    /// </summary>
    public string? ArtificialPerson { get; set; }

    /// <summary>
    /// 法人代表、委派代表
    /// </summary>
    public Person? LegalAgent { get; set; }

    /// <summary>
    /// 实控人
    /// </summary>
    public Person? ActualController { get; set; }



    /// <summary>
    /// 成立日期
    /// </summary>
    public DateOnly SetupDate { get; set; }

    /// <summary>
    /// 到期日
    /// </summary>
    public DateOnly ExpireDate { get; set; }

    /// <summary>
    /// 统一信用代码
    /// </summary>
    //public string? InstitutionCode { get; set; }

    /// <summary>
    /// 注册地址
    /// </summary>
    public string? RegisterAddress { get; set; }

    /// <summary>
    /// 办公地址
    /// </summary>
    public string? OfficeAddress { get; set; }

    /// <summary>
    /// 注册资本
    /// </summary>
    public decimal RegisterCapital { get; set; }

    /// <summary>
    /// 实收资本
    /// </summary>
    public decimal? RealCapital { get; set; }

    /// <summary>
    /// 经营范围
    /// </summary>
    public string? BusinessScope { get; set; }

    /// <summary>
    /// 电话
    /// </summary>
    public string? Telephone { get; set; }

    /// <summary>
    /// 传真
    /// </summary>
    public string? Fax { get; set; }



    /// <summary>
    /// 官网
    /// </summary>
    public string? WebSite { get; set; }


    public override EntityType Type => EntityType.Institution;
 
}



public class ShareRelation
{
    public int Id { get; set; }

    public int MyProperty { get; set; }

    public required string InstitutionId { get; set; }


}

public class ShareHolder
{
    /// <summary>
    /// 实体ID，身份证等
    /// </summary>
    public int Id { get; set; }

    public required string Name { get; set; }

    /// <summary>
    /// 实体类型
    /// </summary>
    public EntityType Type { get; set; }
    

    // 持股的机构、公司
    public required string InstitutionId { get; set; }
     
     
    public decimal Share { get; set; }

    public decimal Ratio { get; set; }

}