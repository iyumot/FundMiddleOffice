
namespace FMO.Trustee;

public class CSTISC:ITrustee
{
    public string? Domain { get; set; }

    public string? Auth { get; set; }


    public bool IsValid { get; }


    public bool Prepare()
    {




        return false;
    }


}