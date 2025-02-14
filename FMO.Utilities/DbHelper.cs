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
            //db.GetCollection<IInvestor>().EnsureIndex(x => x.Identity);
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


public class FileIndexDatabase : LiteDatabase
{

    private const string connectionString = @"FileName=data\filestorage.db;Connection=Shared";

    public FileIndexDatabase() : base(connectionString, null)
    {
    }
}

/// <summary>
/// 文件索引
/// </summary>
public static class FileIndexService
{
    public static FileIndexDatabase Database { get; }

    public static ILiteCollection<FileStorageInfo> Collection { get; }

    public static FileStorageInfo? Find(int id) => Collection.FindById(id);


    public static bool Store(FileStorageInfo f) => Collection.Upsert(f);


    static FileIndexService()
    {
        Database = new();
        Collection = Database.GetCollection<FileStorageInfo>();
    }






}