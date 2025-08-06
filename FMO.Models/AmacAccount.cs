namespace FMO.Models;

public record class AmacAccount(string Id, string Name, string Password, bool IsValid);

public record class PfidDirectAccount(string Id, string Name, string Password, string Key, bool IsValid) : AmacAccount(Id, Name, Password, IsValid);
