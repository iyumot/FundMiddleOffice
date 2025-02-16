namespace FMO.Models;

public class Investor
{
    public int Id { get; set; }

    public required string Name { get; set; }

    /// <summary>
    /// 证件号
    /// </summary>
    public Identity Identity { get; set; }

    /// <summary>
    /// 证件有效限
    /// </summary>
    public DateEfficient Efficient { get; set; }

    /// <summary>
    /// 证件文件
    /// </summary>
    public VersionedFileInfo? Certifications { get; set; }

    /// <summary>
    /// 法人类型
    /// </summary>
    public EntityType EntityType { get; set; }

    public InvestorType Type { get; set; }




}


