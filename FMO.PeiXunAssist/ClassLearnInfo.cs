namespace FMO.LearnAssist;

public class ClassLearnInfo
{
    /// <summary>
    /// 课程id
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// 课程名
    /// </summary>
    public required string Name { get; set; }

    public required string Type { get; set; }

    /// <summary>
    /// 学时
    /// </summary>
    public decimal Hour { get; set; }

    public DateTime PayTime { get; set; }

    public DateTime LearnTime { get; set; }

    public bool IsLearned { get; set; }

}
