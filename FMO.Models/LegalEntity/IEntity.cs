namespace FMO.Models;

public interface IEntity
{
    public int Id { get; set; }

    public string Name { get; set; }

    public Identity ? Identity { get; set; }

    public DateEfficient Efficient { get; set; }

    public string? Phone { get; set; }
}



/// <summary>
/// 人
/// </summary>
public class Individual : IEntity
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public Identity? Identity { get; set; }

    public DateEfficient Efficient { get; set; }

    public string? Phone { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 简介
    /// </summary>
    public string? Profile { get; set; }
}


public class Organization : IEntity
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public Identity? Identity { get; set; }

    public DateEfficient Efficient { get; set; }

    public string? Phone { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? Email { get; set; }


    /// <summary>
    /// 成立日期
    /// </summary>
    public DateOnly SetupDate { get; set; }

    /// <summary>
    /// 到期日
    /// </summary>
    public DateOnly ExpireDate { get; set; }

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
    public decimal? RegisterCapital { get; set; }

    /// <summary>
    /// 实收资本
    /// </summary>
    public decimal? RealCapital { get; set; }

    /// <summary>
    /// 经营范围
    /// </summary>
    public string? BusinessScope { get; set; }


    /// <summary>
    /// 传真
    /// </summary>
    public string? Fax { get; set; }



    /// <summary>
    /// 官网
    /// </summary>
    public string? WebSite { get; set; }

}



public class ShareHolder
{
    public int Id { get; set; }

    /// <summary>
    /// 持有人
    /// </summary>
    public int HolderId { get; set; }


    /// <summary>
    /// 持股的机构、公司
    /// </summary>
    public int OrganizationId { get; set; }


    public decimal Share { get; set; }

    public decimal Ratio { get; set; }

}