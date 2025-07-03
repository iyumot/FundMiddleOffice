using FMO.Models;
using LiteDB;
using Serilog;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
namespace FMO.Utilities;

internal record PatchRecord(int Id, DateTime Time);

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
            //db.GetCollection<FundElements>().EnsureIndex(x => x.FundId);

            //var m = db.GetCollection(nameof(Manager)).FindOne(x => x[nameof(Manager.IsMaster)] == true);
            //var dict = m.ToDictionary();
            //var v = m[nameof(Manager.ExpireDate)];
            //dict[nameof(Manager.ExpireDate)] = new BsonValue(DateOnly.FromDateTime(v.AsDateTime));

            //db.GetCollection(nameof(Manager)).Update(new BsonDocument(dict));

            // 校验ta，为TransferRecord的CustomerId为0的设置Id
            var d = db.GetCollection<TransferRecord>().Find(x => x.CustomerId == 0).ToArray();
            if (d.Length > 0)
            {
                var cc = db.GetCollection<Investor>().FindAll().ToArray();
                foreach (var item in d)
                {
                    var tmp = cc.Where(x => x.Identity.Id == item.CustomerIdentity).ToArray();

                    // 没有找到investor
                    if (tmp.Length == 0)
                    {
                        var c = new Investor { Name = item.CustomerName, Identity = new Identity { Id = item.CustomerIdentity } };
                        db.GetCollection<Investor>().Insert(c);
                        item.CustomerId = c.Id;
                    }
                    else if (tmp.Count() == 1)
                        item.CustomerId = tmp.First().Id;
                    else Log.Error($"TransferRecord {item.Id} {item.FundName} {item.CustomerName} 与多个Inverstor对应");
                }
                db.GetCollection<TransferRecord>().Update(d);
            }


            Patch();      
        }


        using (var db = DbHelper.Platform())
        {
            LiteDB.ILiteCollection<PlatformSynchronizeTime> coll = db.GetCollection<PlatformSynchronizeTime>();
            coll.EnsureIndex(x => new { x.Identifier, x.Method }, true);
        }
    }

    public static void Miggrate()
    {
        using var db = DbHelper.Base();
        var objs = db.GetCollection(nameof(Manager)).FindAll().ToArray();
        var col = db.GetCollectionNames();
        foreach (var item in objs)
        {
            if (item["_id"].Type == BsonType.Int32) return;

            var id = item["_id"].AsString;
            item.Remove("_id");
            item.Add("_id", 1);
            item.Add(nameof(Identity), BsonMapper.Global.ToDocument(new Identity { Type = IDType.OrganizationCode, Id = id }));
        }
        db.DropCollection(nameof(Manager));
        db.GetCollection(nameof(Manager)).Insert(objs);

    }


    private static void Patch()
    {
        using var db = DbHelper.Base();
        var col = db.GetCollection<PatchRecord>();


        if(col.FindById(2) is null)
        { 
            // 6.30 解决中信ta，有unset
            var haser = db.GetCollection<TransferRecord>().Find(x => x.Source != null && x.Source.Contains("citics")).Any(x => x.CustomerName == "unset");
            using (var pdb = DbHelper.Platform()) //删除记录
            {
                pdb.GetCollection("TrusteeMethodShotRange").Delete("trustee_citicsQueryTransferRecords");
                pdb.GetCollection("TrusteeMethodShotRange").Delete("trustee_cmsQueryTransferRecords");
            }
            db.GetCollection<TransferRecord>().DeleteMany(x => x.Source == "" || x.Source == null);

            col.Insert(new PatchRecord(2, DateTime.Now));
        }

        // 清空work记录
        var id = 4;
        if (col.FindById(id) is null)
        {
            using var pdb = DbHelper.Platform();
            pdb.RenameCollection("TrusteeMethodShotRange", $"TrusteeMethodShotRange{id}");
            //    pdb.DropCollection("TrusteeMethodShotRange");
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

    public Fund? FindFund(string? fundCode)
    {
        var c = GetCollection<Fund>();

        if (fundCode?.Length > 0)
        {
            // code匹配
            var f = c.FindOne(x => x.Code != null && fundCode.Contains(x.Code!));
            if (f is not null) return f;

            // SNN111 NN111A/B SNN111A/B 这类
            f = c.FindAll().Where(x => x.Code is not null && fundCode.Contains(x.Code![1..])).FirstOrDefault();
            if (f is not null) return f;
        }
        return null;
    }

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

    public static BaseDatabase ShareClass()
    {
        return new BaseDatabase(@$"FileName=data\sc.db;Password={_password};Connection=Shared");
    }


    public static LiteDatabase Platform() => new LiteDatabase(@$"FileName=data\platform.db;Password={_password};Connection=Shared");




    public static bool BuildFundShareRecord(this ILiteDatabase db, int fundid)
    {
        try
        {
            if (fundid == 0) return false;


            var data = db.GetCollection<TransferRecord>().Find(x => x.FundId == fundid).GroupBy(x => x.ConfirmedDate).OrderBy(x => x.Key);
            var list = new List<FundShareRecord>();
            foreach (var item in data)
                list.Add(new FundShareRecord(0, fundid, item.Key, item.Sum(x => x.ShareChange()) + (list.Count > 0 ? list[^1].Share : 0)));

            db.GetCollection<FundShareRecord>().DeleteMany(x => x.FundId == fundid);
            db.GetCollection<FundShareRecord>().Insert(list);

            return true;
        }
        catch (Exception e)
        {
            Log.Error($"BuildFundShareRecord {e.Message}");
            return false;
        }
    }

    public static void BuildFundShareRecord(this ILiteDatabase db, params int[] fundids)
    {
        foreach (var fundid in fundids)
            BuildFundShareRecord(db, fundid);
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
