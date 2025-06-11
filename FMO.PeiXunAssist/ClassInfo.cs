namespace FMO.LearnAssist;

public class ClassInfo
{
    public required string Id { get; set; }

    /// <summary>
    /// 课程名
    /// </summary>
    public required string Name { get; set; }

    public required string Type { get; set; }


    public bool IsStar => Type == "职业道德";

    /// <summary>
    /// 学时
    /// </summary>
    public decimal Hour { get; set; }

    public DateTime Time { get; set; }

    public decimal Price { get; set; }

}
