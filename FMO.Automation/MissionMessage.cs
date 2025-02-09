﻿namespace FMO.Schedule;

public class MissionMessage
{
    public required int Id { get; set; }

    public bool IsWorking { get; set; }

    public DateTime? LastRun { get; set; }

    public DateTime? NextRun { get; set; }
}



public class MissionProgressMessage
{
    public required int Id { get; set; }

    public double Progress { get; set; }
}
