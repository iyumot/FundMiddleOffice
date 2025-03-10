using LiteDB;

namespace FMO.IO.DS;

public class SynchronizeTime
{

    [BsonId]
    public required string Identifier { get; set; }


    /// <summary>
    /// 客户同步时间
    /// </summary>
    public DateTime Customer { get; set; }

    /// <summary>
    /// TA同步时间
    /// </summary>
    public DateTime TA { get; set; }

}