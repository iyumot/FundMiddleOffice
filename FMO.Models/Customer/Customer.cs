namespace FMO.Models;





public enum CustomerType
{
    Natural,

    Institution,

    Product
}



/// <summary>
/// 存储在base
/// </summary>
public interface ICustomer
{
    int _id { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// 证件
    /// </summary>
    Identity Identity { get; set; }

    CustomerType CustomerType => this switch { NaturalCustomer => CustomerType.Natural, InstitutionCustomer => CustomerType.Institution, ProductCustomer => CustomerType.Product, _ => throw new NotImplementedException() };
}



/// <summary>
/// 自然人
/// </summary>
public class NaturalCustomer : ICustomer
{
    public int _id { get; set; }

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
}

/// <summary>
/// 机构
/// </summary>
public class InstitutionCustomer : ICustomer
{
    public int _id { get; set; }

    public required string Name { get; set; }

    public required Identity Identity { get; set; }

    public required InstitutionCustomerType InstitutionType { get; set; }



}


/// <summary>
/// 产品
/// </summary>
public class ProductCustomer : ICustomer
{
    public int _id { get; set; }

    public required string Name { get; set; }

    public required Identity Identity { get; set; }


    public required ProductCustomerType ProductType { get; set; }
}
