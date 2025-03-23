using FMO.Models;
using LiteDB;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
namespace FMO.Utilities;

public static class DatabaseAssist
{
    /// <summary>
    /// 自检
    /// </summary>
    public static void SystemValidation()
    {
        using (var db = DbHelper.Base())
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

    public BaseDatabase(string con) : base(con, null) { }

    public ILiteCollection<DailyValue> GetDailyCollection(int fid)
    {
        return GetCollection<DailyValue>($"fv_{fid}");
    }
}

public static class DbHelper
{
    private static string _password;

    static DbHelper()
    {
        _password = ConfigurationManager.AppSettings["dbpw"] ?? "fjd32890f5djflds";
        _password += "jgkfld9024039284jrwe";

        using (MD5 sha256 = MD5.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(_password);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            _password = Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }


    public static BaseDatabase Base()
    {
        return new BaseDatabase(@$"FileName=data\base.db;Password={_password};Connection=Shared"); 
    }


    public static LiteDatabase Trustee() => new LiteDatabase(@$"FileName=data\trustee.db;Password={_password};Connection=Shared");


    public static LiteDatabase Digital() => new LiteDatabase(@$"FileName=data\digital.db;Password={_password};Connection=Shared");


    public static LiteDatabase Platform() => new LiteDatabase(@$"FileName=data\platform.db;Password={_password};Connection=Shared");


    public static void initpassword()
    {
        var di = new DirectoryInfo("data");
        foreach (var f in di.GetFiles("*.db"))
        {
            if (f.Name.Contains("backup")) continue;

            string p1 = "891uiu89f41uf9dij432u89";
            string p2 = "f34902ufdisuf8s1";

            var db = new LiteDatabase(@$"FileName=data\base.db;Password={p1};Connection=Shared");

            try
            {
                var n = db.Collation;

                db.Rebuild(new LiteDB.Engine.RebuildOptions { Password = _password , IncludeErrorReport = true});
            }
            catch
            {
                db = new LiteDatabase(@$"FileName=data\base.db;Password={p2};Connection=Shared");

                try
                {
                    var n = db.Collation;

                    db.Rebuild(new LiteDB.Engine.RebuildOptions { Password = _password });
                }
                catch
                {
                    
                }
            }
        }
    }

}


//public class TrusteeDatabase : LiteDatabase
//{

//    private const string connectionString = @"FileName=data\trustee.db;Password=f34902ufdisuf8s1;Connection=Shared";

//    public TrusteeDatabase() : base(connectionString, null)
//    {
//    }
//}

//public class DSDatabase : LiteDatabase
//{

//    private const string connectionString = @"FileName=data\digital.db;Password=f34902ufdisuf8s1;Connection=Shared";

//    public DSDatabase() : base(connectionString, null)
//    {
//    }
//}

public class FileIndexDatabase : LiteDatabase
{

    private const string connectionString = @"FileName=data\filestorage.db;Connection=Shared";

    public FileIndexDatabase() : base(connectionString, null)
    {
    }
}
