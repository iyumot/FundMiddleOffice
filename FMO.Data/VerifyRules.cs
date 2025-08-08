using FMO.Models;

namespace FMO.Utilities;

public static partial class VerifyRules
{

    public static FundClearDateMissingRule FundClearDateMissingRule { get; } = new();
     

    public static FundDailyMissingRule FundDailyMissingRule { get; } = new();
     
 
}
