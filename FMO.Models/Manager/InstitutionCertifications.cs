namespace FMO.Models;

/// <summary>
/// 机构证件
/// </summary>
public class InstitutionCertifications
{
    /// <summary>
    /// 证件号
    /// </summary>
    public required string Id { get; set; }


    public List<FileStorageInfo>? BusinessLicense { get; set; }

    public List<FileStorageInfo>? BusinessLicense2 { get; set; }


    /// <summary>
    /// 开户许可证
    /// </summary>
    public List<FileStorageInfo>? AccountOpeningLicense { get; set; }


    /// <summary>
    /// 章程
    /// </summary>
    public List<FileStorageInfo>? CharterDocument { get; set; }

    /// <summary>
    /// 法人身份证
    /// </summary>
    public List<FileStorageInfo>? LegalPersonIdCard { get; set; }

}