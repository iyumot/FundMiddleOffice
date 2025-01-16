namespace FMO.Models;

public class FundFileInfo
{
    public int Id { get; set; }

    public string? Path { get; set; }

    public required string Name { get; set; }


    public DateTime Time { get; set; }

    public string? Hash { get; set; }
}



public class FileVersion
{
    public DateTime Time { get; set; }

    public required string Hash { get; set; }

    public required string Path { get; set; }
}

public class VersionedFileInfo
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public List<FileVersion> Files { get; set; } = new();

}