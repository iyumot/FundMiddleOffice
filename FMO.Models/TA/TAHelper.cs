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
    public static bool RequiredOrder(this TransferRequestType type)
    {
        return type switch { TransferRequestType.Purchase or TransferRequestType.Subscription or TransferRequestType.Redemption or TransferRequestType.ForceRedemption => true, _ => false };
    }

    public static bool IsBuy(this TransferRequest r) => r.RequestType.IsBuy();
    
    public static bool IsSell(this TransferRequest r) => r.RequestType.IsSell();

    public static bool RequiredOrder(this TransferRequest r) => r.RequestType.RequiredOrder();


    public static bool IsBuy(this TransferRecordType type)
    {
        return type switch { TransferRecordType.Purchase or TransferRecordType.Subscription => true, _ => false };
    }
    public static bool IsSell(this TransferRecordType type)
    {
        return type switch { TransferRecordType.Redemption or TransferRecordType.ForceRedemption => true, _ => false };
    }

    public static bool RequiredOrder(this TransferRecordType type)
    {
        return type switch { TransferRecordType.Purchase or TransferRecordType.Subscription or TransferRecordType.Redemption or TransferRecordType.ForceRedemption => true, _ => false };
    }


    public static bool IsBuy(this TransferRecord r) => r.Type.IsBuy();

    public static bool IsSell(this TransferRecord r) => r.Type.IsSell();

    public static bool RequiredOrder(this TransferRecord r) => r.Type.RequiredOrder();
}
