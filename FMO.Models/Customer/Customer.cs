using System.ComponentModel;

namespace FMO.Models;




[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum CustomerType
{
    [Description("自然人")] Natural,

    [Description("机构")] Institution,

    [Description("产品")] Product
}


[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum InvestorType
{
    [Description("普通")] Normal,

    [Description("专业")] Professional,
}


/// <summary>
/// 存储在base
/// </summary>
public interface ICustomer
{
    int Id { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// 证件
    /// </summary>
    Identity Identity { get; set; }

    /// <summary>
    /// 证件有效期
    /// </summary>
    DateEfficient Efficient { get; set; }


    //List<InvestorFileInfo> Certifications { get; set; }
     

    CustomerType CustomerType => this switch { NaturalCustomer => CustomerType.Natural, InstitutionCustomer => CustomerType.Institution, ProductCustomer => CustomerType.Product, _ => throw new NotImplementedException() };
}



/// <summary>
/// 自然人
/// </summary>
public class NaturalCustomer : ICustomer
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required Identity Identity { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public Gender? Gender { get; set; }


    /// <summary>
    /// 民族
    /// </summary>
    public string? Nation { get; set; }

    /// <summary>
    /// 生日
    /// </summary>
    public DateOnly Birthday { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    public DateEfficient Efficient { get; set; }
}

/// <summary>
/// 机构
/// </summary>
public class InstitutionCustomer : ICustomer
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required Identity Identity { get; set; }

    public required InstitutionCustomerType InstitutionType { get; set; }

    public DateEfficient Efficient { get; set; }


}


/// <summary>
/// 产品
/// </summary>
public class ProductCustomer : ICustomer
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required Identity Identity { get; set; }


    public required ProductCustomerType ProductType { get; set; }

    public DateEfficient Efficient { get; set; }
}
