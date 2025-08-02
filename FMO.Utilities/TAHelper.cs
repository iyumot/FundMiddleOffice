using FMO.Models;

namespace FMO.Utilities;

public static class TAHelper
{

    public static bool IsBuy(this TransferRequestType type)
    {
        return type switch { TransferRequestType.Purchase or TransferRequestType.Subscription => true, _ => false };
    }
    public static bool IsSell(this TransferRequestType type)
    {
        return type switch { TransferRequestType.Redemption or TransferRequestType.ForceRedemption => true, _ => false };
    }

    public static bool IsBuy(this TransferRequest r) => r.RequestType.IsBuy();
    
    public static bool IsSell(this TransferRequest r) => r.RequestType.IsSell();
}
