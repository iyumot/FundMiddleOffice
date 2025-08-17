namespace FMO.Models;


/// <summary>
/// 最多16种基金定期报告类型
/// 不然FundPeriodicReport的ID会溢出
/// </summary>
public enum PeriodicReportType
{
    /// <summary>
    /// 每月报告
    /// </summary>
    MonthlyReport,
    /// <summary>
    /// 每季度报告
    /// </summary>
    QuarterlyReport,
    /// <summary>
    /// 半年报告
    /// </summary>
    SemiAnnualReport,
    /// <summary>
    /// 年度报告
    /// </summary>
    AnnualReport,

    /// <summary>
    /// 季度更新
    /// </summary>
    QuarterlyUpdate,
}


/// <summary>
/// 定期报告
/// </summary>
public class FundPeriodicReport
{
    public int Id => (PeriodEnd.DayNumber - 719162) << 16 | FundId << 4 | (int)Type;//1970-01-01是公历的第719162天

    public int FundId { get; set; }

    /// <summary>
    /// 定期报告的最后一天
    /// </summary>
    public DateOnly PeriodEnd { get; set; }

    public PeriodicReportType Type { get; set; }


    public SimpleFile? Word { get; set; }

    public SimpleFile? Excel { get; set; }

    public SimpleFile? Xbrl { get; set; }

    public SimpleFile? Pdf { get; set; }
}