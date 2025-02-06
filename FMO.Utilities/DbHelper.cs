using FMO.Models;
using LiteDB;

namespace FMO.Utilities;

public static class DatabaseAssist
{
    /// <summary>
    /// 自检
    /// </summary>
    public static void SystemValidation()
    {
        using (var db = new BaseDatabase())
        {
            db.GetCollection<ICustomer>().EnsureIndex(x => x.Identity);
            db.GetCollection<FundElements>().EnsureIndex(x => x.FundId);

            //var m = db.GetCollection(nameof(Manager)).FindOne(x => x[nameof(Manager.IsMaster)] == true);
            //var dict = m.ToDictionary();
            //var v = m[nameof(Manager.ExpireDate)];
            //dict[nameof(Manager.ExpireDate)] = new BsonValue(DateOnly.FromDateTime(v.AsDateTime));

            //db.GetCollection(nameof(Manager)).Update(new BsonDocument(dict));
        }
    }
}



public class BaseDatabase : LiteDatabase
{

    private const string connectionString = @"FileName=data\base.db;Password=891uiu89f41uf9dij432u89;Connection=Shared";

    public BaseDatabase() : base(connectionString, null)
    {
    }

    public ILiteCollection<DailyValue> GetDailyCollection(int fid)
    {
        return GetCollection<DailyValue>($"fv_{fid}");
    }
}


public class TrusteeDatabase : LiteDatabase
{

    private const string connectionString = @"FileName=data\trustee.db;Password=f34902ufdisuf8s1;Connection=Shared";

    public TrusteeDatabase() : base(connectionString, null)
    {
    }
}