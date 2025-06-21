namespace FMO.Models;

/// <summary>
/// �Ӳ�Ʒӳ��
/// </summary>
public class SubjectFundMapping
{
    public int Id { get; set; }

    public required string FundName { get; set; }

    public required string FundCode { get; set; }

    /// <summary>
    /// ����Ʒ
    /// </summary>
    public string? MasterName { get; set; }

    /// <summary>
    /// ����Ʒ
    /// </summary>
    public string? MasterCode { get; set; }

    /// <summary>
    /// �ݶ���
    /// </summary>
    public string? ShareClass { get; set; }
}