using LiteDB;

namespace FMO.Schedule;

public class GzMailAttachInfo
{
    public required string Name { get; set; }

    public int? FundId { get; set; }

    public int? DailyId { get; set; }

    [BsonIgnore]
    public bool IsMapped => FundId is not null && DailyId is not null;
}
