﻿using FMO.Models;
using LiteDB;
using Serilog;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
namespace FMO.Utilities;

internal record PatchRecord(int Id, DateTime Time);

public static class DatabaseAssist
{
    private static Dictionary<int, Action<BaseDatabase>> patchs = new()
    {
        {2, db=>{
            // 6.30 解决中信ta，有unset
            var haser = db.GetCollection<TransferRecord>().Find(x => x.Source != null && x.Source.Contains("citics")).Any(x => x.CustomerName == "unset");
            using (var pdb = DbHelper.Platform()) //删除记录
            {
                pdb.GetCollection("TrusteeMethodShotRange").Delete("trustee_citicsQueryTransferRecords");
                pdb.GetCollection("TrusteeMethodShotRange").Delete("trustee_cmsQueryTransferRecords");
            }
            db.GetCollection<TransferRecord>().DeleteMany(x => x.Source == "" || x.Source == null);
        } },

        {5, db=>{
        using var pdb = DbHelper.Platform();
            if (!pdb.GetCollectionNames().Contains($"TrusteeMethodShotRange{5}"))
                pdb.RenameCollection("TrusteeMethodShotRange", $"TrusteeMethodShotRange{5}");
        } },

        {11, db=>{
            var da = db.GetCollection(nameof(Investor)).FindAll().ToArray();
            foreach (var item in da)
            {
                if (item.ContainsKey("RiskLevel"))
                    item[nameof(RiskEvaluation)] = ((RiskEvaluation)(int)Enum.Parse<RiskLevel>(item[nameof(RiskLevel)].AsString)).ToString();
            }
            db.GetCollection(nameof(Investor)).Update(da);
        } },

        {21, db=>{
            db.DropCollection("customer_accounts");
            var accounts = db.GetCollection<BankTransaction>().FindAll().Select(x=> (x.CounterBank, x.CounterName, x.CounterNo)).DistinctBy(x=>x.CounterNo).ToList();
            var entities = accounts.Select(x=> new BankAccount{ BankOfDeposit = x.CounterBank, Name  = x.CounterName, Number = x.CounterNo }).ToArray();
            var customers = db.GetCollection<Investor>().FindAll().ToArray();

            foreach (var ba in entities)
            {
                // 同名
                var c = customers.Where(x=>x.Name == ba.Name).ToArray();
                if(c.Length == 1)
                {
                    ba.OwnerId = c[0].Id;
                    continue;
                }

                c = customers.Where(x=>Investor.IsNamePair(ba.Name, x.Name)).ToArray();
                if(c.Length == 1)
                {
                    ba.OwnerId = c[0].Id;
                    continue;
                }

                if(ba.Name?.Contains("基金")??false)
                {
                    c = customers.Where(x=>x.Name.Contains("基金") && ba.Name.Contains(x.Name)).ToArray();
                    if(c.Length == 1)
                    {
                        ba.OwnerId = c[0].Id;
                        continue;
                    }
                }
            }

            db.GetCollection<BankAccount>("customer_accounts").Upsert(entities);
        }},

    };


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

    /// <summary>
    /// 当首次初始化时
    /// </summary>
    public static void InitPatch()
    {
        using var db = DbHelper.Base();
        var col = db.GetCollection<PatchRecord>();
        col.Upsert(patchs.Select(x => new PatchRecord(x.Key, DateTime.Now)));
    }

    private static void Patch()
    {
        using var db = DbHelper.Base();
        var col = db.GetCollection<PatchRecord>();
        foreach (var (k, v) in patchs)
        {
            if (col.FindById(k) is null)
            {
                v.Invoke(db);
                col.Upsert(new PatchRecord(k, DateTime.Now));
            }
        }
    }

    //private static void Patch()
    //{
    //    using var db = DbHelper.Base();
    //    var col = db.GetCollection<PatchRecord>();


    //    if (col.FindById(2) is null)
    //    {
    //        // 6.30 解决中信ta，有unset
    //        var haser = db.GetCollection<TransferRecord>().Find(x => x.Source != null && x.Source.Contains("citics")).Any(x => x.CustomerName == "unset");
    //        using (var pdb = DbHelper.Platform()) //删除记录
    //        {
    //            pdb.GetCollection("TrusteeMethodShotRange").Delete("trustee_citicsQueryTransferRecords");
    //            pdb.GetCollection("TrusteeMethodShotRange").Delete("trustee_cmsQueryTransferRecords");
    //        }
    //        db.GetCollection<TransferRecord>().DeleteMany(x => x.Source == "" || x.Source == null);

    //        col.Insert(new PatchRecord(2, DateTime.Now));
    //    }

    //    // 清空work记录
    //    var id = 5;
    //    if (col.FindById(id) is null)
    //    {
    //        using var pdb = DbHelper.Platform();
    //        if (!pdb.GetCollectionNames().Contains($"TrusteeMethodShotRange{id}"))
    //            pdb.RenameCollection("TrusteeMethodShotRange", $"TrusteeMethodShotRange{id}");
    //        //    pdb.DropCollection("TrusteeMethodShotRange");

    //        col.Upsert(new PatchRecord(id, DateTime.Now));
    //    }

    //    // citics QueryTransferRecords 数据不全，清除记录
    //    id = 6;
    //    if (col.FindById(id) is null)
    //    {
    //        using var pdb = DbHelper.Platform();
    //        pdb.GetCollection("TrusteeMethodShotRange").Delete("trustee_citicsQueryTransferRecords");

    //        col.Upsert(new PatchRecord(id, DateTime.Now));
    //    }

    //    // 修复 citics ta 认购类型错误
    //    id = 7;
    //    if (col.FindById(id) is null)
    //    {
    //        using var pdb = DbHelper.Base();
    //        var old = pdb.GetCollection<TransferRecord>().Find(x => x.Source == "api").ToArray();

    //        foreach (var item in old.Where(x => x.Type == TransferRecordType.Subscription && x.ConfirmedShare == 0))
    //        {
    //            var r = old.FirstOrDefault(x => x.ExternalId!.StartsWith(item.ExternalId!));
    //            if (r is not null)
    //            {
    //                r.Type = TransferRecordType.Subscription;
    //                pdb.GetCollection<TransferRecord>().Update(r);
    //            }
    //        }

    //        col.Upsert(new PatchRecord(id, DateTime.Now));
    //    }

    //    id = 8;
    //    if (col.FindById(id) is null)
    //    {
    //        var old = db.GetCollection<TransferRecord>().DeleteMany(x => x.ConfirmedShare == 0 && (x.Type == TransferRecordType.UNK || x.Type == TransferRecordType.Subscription));

    //        col.Upsert(new PatchRecord(id, DateTime.Now));
    //    }


    //    id = 9;
    //    if (col.FindById(id) is null)
    //    {
    //        db.GetCollection<Investor>().DeleteMany(x => x.Name == "unset" || x.Name.Contains("test"));
    //        // 异常投资人数据
    //        var bad = db.GetCollection<Investor>().FindAll().ToArray();
    //        foreach (var (i, item) in bad.Index())
    //        {
    //            var exi = bad[..i].FirstOrDefault(y => y.Identity?.Id == item.Identity?.Id);
    //            if (exi is null)
    //                continue;


    //            var ta = db.GetCollection<TransferRecord>().Find(x => x.CustomerId == item.Id).ToArray();
    //            foreach (var t in ta)
    //                t.CustomerId = exi.Id;

    //            var tq = db.GetCollection<TransferRequest>().Find(x => x.CustomerId == item.Id).ToArray();
    //            foreach (var t in tq)
    //                t.CustomerId = exi.Id;

    //            if (ta.Length > 0)
    //                db.GetCollection<TransferRecord>().Update(ta);
    //            if (tq.Length > 0)
    //                db.GetCollection<TransferRequest>().Update(tq);

    //            db.GetCollection<Investor>().Delete(item.Id);
    //        }



    //        col.Upsert(new PatchRecord(id, DateTime.Now));
    //    }


    //    id = 10;
    //    if (col.FindById(id) is null)
    //    {
    //        db.GetCollection<InvestorQualification>().DeleteMany(x => x.Date == default);

    //        col.Upsert(new PatchRecord(id, DateTime.Now));
    //    }

    //    id = 11;
    //    if (col.FindById(id) is null)
    //    {
    //        var da = db.GetCollection(nameof(Investor)).FindAll().ToArray();
    //        foreach (var item in da)
    //        {
    //            //        if (item.TryGetValue("RiskLevel", out var riskLevelObj) &&
    //            //Enum.IsDefined(typeof(RiskLevel), riskLevelObj))
    //            //        {
    //            //            // 将 RiskLevel 的值转为 int 再映射成 RiskEvaluation 枚举
    //            //            var riskLevelValue = (int)riskLevelObj;
    //            //            if (Enum.IsDefined(typeof(RiskEvaluation), riskLevelValue))
    //            //            {
    //            //                item[nameof(RiskEvaluation)] = (RiskEvaluation)riskLevelValue;
    //            //            }
    //            //        }

    //            if (item.ContainsKey("RiskLevel"))
    //                item[nameof(RiskEvaluation)] = ((RiskEvaluation)(int)Enum.Parse<RiskLevel>(item[nameof(RiskLevel)].AsString)).ToString();
    //        }
    //        db.GetCollection(nameof(Investor)).Update(da);

    //        col.Upsert(new PatchRecord(id, DateTime.Now));
    //    }
    //}

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
