namespace FMO.Models;


public class FundAnnouncement
{

    public int Id { get; set; }

    public int FundId { get; set; }

    public DateOnly Date { get; set; }

    public string? Title { get; set; }

    public FileStorageInfo? File { get; set; }

    public FileStorageInfo? Sealed { get; set; }
}
