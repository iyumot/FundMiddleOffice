
namespace FMO.Trustee;

public interface ITrustee
{
    string? Domain { get; set; }

    bool IsValid { get; }

    bool Prepare();



}
