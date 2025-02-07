namespace FMO.Schedule;

public class MissionMessage
{
    public required string Id { get; set; }

    public bool IsWorking { get; set; }
}
public class MissionProgressMessage
{
    public required string Id { get; set; }

    public double Progress { get; set; }
}
