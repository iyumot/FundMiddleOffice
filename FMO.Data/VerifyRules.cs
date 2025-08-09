using FMO.Models;

namespace FMO.Utilities;

public static partial class VerifyRules
{

    public static FundClearDateMissingRule FundClearDateMissingRule { get; } = new();
     

    public static FundDailyMissingRule FundDailyMissingRule { get; } = new();

    public static FundSharePairRule FundSharePairRule { get; } = new();
 
}

////生成结果如下 
//public static partial class VerifyRules
//{
//    public static void InitAll()
//    {
//        VerifyRules.FundClearDateMissingRule.Init();
//        VerifyRules.FundDailyMissingRule.Init();
//    }

//    public static void OnEntityArrival(IEnumerable<DailyValue> obj)
//    {
//        FundDailyMissingRule.OnEntityArrival(obj);
//    }

//    public static void OnEntityArrival(IEnumerable<EntityChanged<Fund,DateOnly>> obj)
//    {
//        FundDailyMissingRule.OnEntityArrival(obj);
//        FundClearDateMissingRule.OnEntityArrival(obj);
//    }
//}
