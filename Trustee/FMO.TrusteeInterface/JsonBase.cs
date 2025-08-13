using FMO.Utilities;
using LiteDB;
using System.Collections.Concurrent;

namespace FMO.Trustee;

public class JsonBase
{
    private static ILiteDatabase _db { get; } = new LiteDatabase(@$"FileName=data\platformlog.db;Connection=Shared");

    private static ConcurrentBag<UnusualType> _unusualTypes = new();

    private static Debouncer debouncer = new(SaveUnusual);
     

    public int Id { get; set; }


    public virtual string? JsonId => null;

    protected static decimal ParseDecimal(string value)
    {
        if (string.IsNullOrEmpty(value))
            return 0;

        if (decimal.TryParse(value, out var result))
            return result;

        throw new FormatException($"�޷��� '{value}' ����Ϊdecimal����");
    }

    public static void ReportJsonUnexpected(string identifier, string method, string info)
    {
        _db.GetCollection<TrusteeJsonUnexpected>().Insert(new TrusteeJsonUnexpected(identifier, method, info));
    }

    public static void ReportSpecialType(UnusualType types)
    {
        using var db = DbHelper.Base();
        db.GetCollection<UnusualType>().Insert(types);
        //_unusualTypes.Add(types);
       // debouncer.Invoke();
    }
    private static void SaveUnusual()
    {
        
        using var db = DbHelper.Base();
        db.GetCollection<UnusualType>().Insert(_unusualTypes);
    }


    protected static string ParseCurrency(string currencyCode)
    {
        switch (currencyCode)
        {
            case "156":
                return "CNY"; // �����
            case "250":
                return "CHF"; // ��ʿ����
            case "280":
                return "DEM"; // �¹���ˣ���ͣ�ã�
            case "344":
                return "HKD"; // ��Ԫ
            case "392":
                return "JPY"; // ��Ԫ
            case "826":
                return "GBP"; // Ӣ��
            case "840":
                return "USD"; // ��Ԫ
            case "954":
                return "EUR"; // ŷԪ
            default:
                return "";  // �����׳��쳣��������Ҫ������Ч����
        }
    }
}

