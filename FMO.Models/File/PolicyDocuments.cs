namespace FMO.Models;

public class PolicyDocument
{
    public string? Label { get; set; }

    public List<SealedFileMeta> Files { get; set; } = [];
}


