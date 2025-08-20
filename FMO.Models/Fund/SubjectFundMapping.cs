namespace FMO.Models;

/// <summary>
/// �Ӳ�Ʒӳ��
/// </summary>
public class SubjectFundMapping
{
    public string Id => FundCode;

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
    /// Э��
    /// </summary>
    public string? AmacCode { get; set; }

    /// <summary>
    /// �ݶ���
    /// </summary>
    public string? ShareClass { get; set; }

    public FundStatus Status { get; set; }
}