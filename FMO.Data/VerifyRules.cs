using FMO.Models;

namespace FMO.Utilities;

public static partial class VerifyRules
{

    public static FundClearDateMissingRule FundClearDateMissingRule { get; } = new();
     

    public static FundDailyMissingRule FundDailyMissingRule { get; } = new();
     
 
}

//
public static partial class VerifyRules
{
    public static void InitAll()
    {
        VerifyRules.FundClearDateMissingRule.Init();
        VerifyRules.FundDailyMissingRule.Init();
    }

    public static void OnEntityArrival(IEnumerable<xxx> obj)
    {
        aaa.OnEntityArrival(obj);
    }

    public static void OnEntityArrival(IEnumerable<yyy> obj)
    {
        bbb.OnEntityArrival(obj);
    }
}
