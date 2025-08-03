using CommunityToolkit.Mvvm.Messaging;
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
    private static Dictionary<int, Action<BaseDatabase>> patchs = new()
    {
        [2] = db =>
        {
            // 6.30 解决中信ta，有unset
            var haser = db.GetCollection<TransferRecord>().Find(x => x.Source != null && x.Source.Contains("citics")).Any(x => x.CustomerName == "unset");
            using (var pdb = DbHelper.Platform()) //删除记录
            {
                pdb.GetCollection("TrusteeMethodShotRange").Delete("trustee_citicsQueryTransferRecords");
                pdb.GetCollection("TrusteeMethodShotRange").Delete("trustee_cmsQueryTransferRecords");
            }
            db.GetCollection<TransferRecord>().DeleteMany(x => x.Source == "" || x.Source == null);
        },

        [5] = db =>
            {
                using var pdb = DbHelper.Platform();
                if (!pdb.GetCollectionNames().Contains($"TrusteeMethodShotRange{5}"))
                    pdb.RenameCollection("TrusteeMethodShotRange", $"TrusteeMethodShotRange{5}");
            },

        [11] = db =>
             {
                 var da = db.GetCollection(nameof(Investor)).FindAll().ToArray();
                 foreach (var item in da)
                 {
                     if (item.ContainsKey("RiskLevel"))
                         item[nameof(RiskEvaluation)] = ((RiskEvaluation)(int)Enum.Parse<RiskLevel>(item[nameof(RiskLevel)].AsString)).ToString();
                 }
                 db.GetCollection(nameof(Investor)).Update(da);
             },

        [22] = db =>
            {
                db.GetCollection(nameof(InvestorBankAccount)).Insert(db.GetCollection("customer_accounts").FindAll().ToArray());
                //db.RenameCollection("customer_accounts", nameof(InvestorBankAccount));

            },

        [23] = db =>
            {
                var o = db.GetCollection<TransferOrder>().FindAll().ToArray();
                var rr = db.GetCollection<TransferRecord>().FindAll().ToArray();
                foreach (var item in o)
                {
                    if (rr.FirstOrDefault(x => x.OrderId == item.Id) is TransferRecord r)
                    {
                        item.FundName = r.FundName;
                        item.ShareClass = r.ShareClass;
                    }
                }
                db.GetCollection<TransferOrder>().Update(o);

            },
        [24] = db =>
            {
                var rr = db.GetCollection<TransferRecord>().Find(x => x.Type == TransferRecordType.Redemption || x.Type == TransferRecordType.ForceRedemption).OrderBy(x => x.ConfirmedDate).ToArray();
                foreach (var item in rr.Where(x => x.ConfirmedShare == 0))
                {
                    // citics 多保存的错误赎回
                    if (rr.Any(x => x.ConfirmedDate == item.ConfirmedDate && x.CustomerId == item.CustomerId && x.FundId == item.FundId && x.RequestAmount == item.RequestAmount && x.RequestShare == item.RequestShare && x.ConfirmedShare > 0))
                        db.GetCollection<TransferRecord>().Delete(item.Id);
                }

            },

        [25] = db =>
            {
                var d = db.GetCollection<InvestorQualification>().Find(x => x.InvestorId == 0).ToArray();
                var cc = db.GetCollection<Investor>().FindAll().ToArray();

                foreach (var item in d)
                {
                    if (cc.FirstOrDefault(x => x.Name == item.InvestorName && x.Identity?.Id == item.IdentityCode) is Investor investor)
                        item.InvestorId = investor.Id;
                }
                db.GetCollection<InvestorQualification>().Update(d);

            },
        [27] = db =>
        {
            var doc = db.GetCollection(nameof(FundBankAccount)).FindAll();
            foreach (var item in doc)
            {
                item["Id"] = item["Number"];
            }
            db.GetCollection(nameof(FundBankAccount)).Update(doc);
        },

        [29] = db =>
        {
            var t = db.GetCollection<RaisingBankTransaction>(nameof(BankTransaction)).FindAll().ToArray();

            Dictionary<string, int> map = new();

            foreach (var item in t)
            {
                var n = item.AccountName;
                if (!string.IsNullOrWhiteSpace(n) && map.TryGetValue(n, out int id))
                {
                    item.FundId = id;
                    continue;
                }

                var (f, c) = db.FindByName(n);
                if (f is not null)
                {
                    item.FundId = f.Id;
                    map[n] = f.Id;
                    continue;
                }

                // 尝试通过账号查找
                if (db.GetCollection<FundBankAccount>().FindOne(x => x.Id == item.AccountNo) is FundBankAccount fundAccount)
                {
                    item.FundId = fundAccount.FundId;
                    item.FundCode = fundAccount.FundCode;
                    map[n] = fundAccount.FundId;
                }

                else Log.Error($"RaisingBankTransaction 未找到对应的基金 {item.PrintProperties()} ");
            }

            db.GetCollection<RaisingBankTransaction>().Upsert(t);
        },

        [37] = db =>
        {
            var fundclearinfo = db.GetCollection<Fund>().Query().Select(x => new { x.Id, x.ShortName, x.Status, x.ClearDate }).ToList().Where(x => x.Status >= FundStatus.StartLiquidation).ToList();
            var fundids = fundclearinfo.Select(x => x.Id).ToList();
            var groupedRecords = db.GetCollection<TransferRecord>().Query().Where(x => fundids.Contains(x.FundId)).OrderBy(x => x.ConfirmedDate).ToList().GroupBy(x => x.FundId);

            //Parallel.ForEach(groupedRecords, ft =>
            foreach (var ft in groupedRecords)
            {
                var fid = ft.Key;
                // 检查 
                if (ft.Sum(x => x.ShareChange()) != 0)
                {
                    Log.Error($"Fund {fid} 在清算后，还有份额，请检查数据");
                    continue;
                }

                DateOnly last = default;
                foreach (var item in ft.Reverse())
                {
                    if (item.Type != TransferRecordType.Redemption && item.Type != TransferRecordType.ForceRedemption)
                        continue;

                    if (last == default)
                        last = item.ConfirmedDate;
                    else if (item.ConfirmedDate != last)
                        break;

                    item.IsLiquidating = true;
                    db.GetCollection<TransferRecord>().Update(item);
                }
            }//);

        },

        [42] = UpdateTASchcame,

        //[44] = MapTA
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
            var d = db.GetCollection<TransferRecord>().FindAll().ToArray();
            var cc = db.GetCollection<Investor>().FindAll().ToArray();
            var cids = cc.Select(x => x.Id).ToArray();

            foreach (var item in d.ExceptBy([0, .. cids], x => x.CustomerId))
            {
                var tmp = cc.Where(x => x.Identity?.Id == item.CustomerIdentity).ToArray();

                // 没有找到investor
                if (tmp.Length == 0)
                {
                    var c = new Investor { Name = item.CustomerName, Identity = new Identity { Id = item.CustomerIdentity } };
                    db.GetCollection<Investor>().Insert(c);
                    item.CustomerId = c.Id;

                    WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Info, $"新增投资人 {item.CustomerName}，请完善材料"));
                }
                else if (tmp.Length == 1)
                    item.CustomerId = tmp.First().Id;
                else
                {
                    Log.Error($"TransferRecord {item.Id} {item.FundName} {item.CustomerName} 与多个Inverstor对应");
                    WeakReferenceMessenger.Default.Send(new ToastMessage(LogLevel.Warning, $"{item.FundName} {item.CustomerName} 交易无法对应投资人，因为证件号重复"));
                }
            }
            db.GetCollection<TransferRecord>().Update(d);

            ///////////
            // call 默认是null，和false不一致，所以为每个fund，设置一个默认值
            var pair = db.GetCollection<FundFlow>().Query().Select(x => new { x.FundId, x.Id }).ToArray().OrderBy(x => x.Id).GroupBy(x => x.FundId).Select(x => new { FundId = x.Key, Id = x.First().Id });
            foreach (var item in pair)
            {
                var ele = db.GetCollection<FundElements>().FindById(item.FundId);
                if (!ele.Callback.Changes.ContainsKey(item.Id))
                {
                    ele.Callback.SetValue(new(), item.Id);
                    db.GetCollection<FundElements>().Update(ele);
                }
            }
            ////////////////////////////


            ////////////////////////

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


    /// <summary>
    /// 更新结构，增加了OrderRequired
    /// </summary>
    /// <param name="db"></param>
    private static void UpdateTASchcame(BaseDatabase db)
    {
        db.GetCollection<TransferRequest>().Update(db.GetCollection<TransferRequest>().FindAll().ToList());
        db.GetCollection<TransferRecord>().Update(db.GetCollection<TransferRecord>().FindAll().ToList());
    }


    private static void MapTA(BaseDatabase db)
    {
        var btr = db.GetCollection<RaisingBankTransaction>().FindAll().OrderBy(x => x.Time).ToList();
        var orders = db.GetCollection<TransferOrder>().FindAll().OrderBy(x => x.Date).ToList();
        var requests = db.GetCollection<TransferRequest>().Find(x => x.OrderRequired).OrderBy(x => x.RequestDate).ToList();
        var records = db.GetCollection<TransferRecord>().Find(x => x.OrderRequired).OrderBy(x => x.ConfirmedDate).ToList();

        // var jo = requests.Join(records, x => x.ExternalId, x => x.ExternalId, (x, y) => new { x, y });

        // var unp = requests.Select(x=>x.ExternalId).Except(records.Select(x => x.ExternalRequestId)).Except(records.Select(x=>x.ExternalId)).ToList();
        var  runp = records.ExceptBy(requests.Select(x => x.ExternalId), x=>x.ExternalId).ExceptBy(requests.Select(x => x.ExternalId), x => x.ExternalRequestId).ToList();

        // // 构建时间线
        // var timeline = btr.Select(x => x.Time).Union(orders.Select(x => new DateTime(x.Date, TimeOnly.MinValue))).Union(requests.Select(x => new DateTime(x.RequestDate, TimeOnly.MinValue))).
        //      Union(records.Select(x => new DateTime(x.ConfirmedDate, TimeOnly.MinValue))).Distinct().ToList();

        ////////////////////////////////////// record
        /// 上对应request 下对应 transaction
        // 找已知的map 
        var rids = records.Select(x => x.Id).ToList();
        var mapTable = db.GetCollection<TransferMapping>(); mapTable.DeleteMany(x => true);
        var exists = mapTable.Find(x => rids.Contains(x.RecordId)).ToList();
        foreach (var rec in records)
        {
            var old = exists.FirstOrDefault(x => x.RecordId == rec.Id) ?? new() { RecordId = rec.Id, OrderId = rec.OrderId };

            // 未匹配request
            if (old.RequestId == 0)
            {
                // cms的ExternalRequestId不匹配
                if (db.GetCollection<TransferRequest>().FindOne(x => x.FundId == rec.FundId && (x.ExternalId == rec.ExternalRequestId || x.ExternalId == rec.ExternalId)) is TransferRequest req)
                    old.RequestId = req.Id;
                else if(requests.FirstOrDefault(x => x.FundId == rec.FundId && (x.ExternalId == rec.ExternalRequestId || x.ExternalId == rec.ExternalId)) is TransferRequest req2)
                    old.RequestId = req2.Id;
                else if (requests.FirstOrDefault(x =>  (x.ExternalId == rec.ExternalRequestId || x.ExternalId == rec.ExternalId)) is TransferRequest req4)
                    old.RequestId = req4.Id;
            }

            // 未匹配流水
            if (string.IsNullOrWhiteSpace(old.TransactionId))
            {
                var banks = db.GetCollection<InvestorBankAccount>().Find(x => x.OwnerId == rec.CustomerId).Select(x => x.Number);
                if (rec.IsSell()) // 只有赎回需要匹配
                {
                    var limit = new DateTime(rec.ConfirmedDate, TimeOnly.MinValue);
                    foreach (var t in db.GetCollection<RaisingBankTransaction>().Find(x => x.FundId == rec.FundId && x.Direction == TransctionDirection.Pay
                            && x.Time >= limit && (banks.Contains(x.CounterNo) || x.CounterName == rec.CustomerName)).OrderByDescending(x => x.Time))
                    {
                        if (t.Amount == rec.RequestAmount)
                            old.TransactionId = t.Id;

                    }
                }
            }
            mapTable.Upsert(old);
        }

        var list = mapTable.FindAll().ToList();

        ////////////////////////////////////// request
        /// 上对应order 下对应 record
        rids = requests.Select(x => x.Id).ToList();
        exists = mapTable.Find(x => rids.Contains(x.RequestId)).ToList();
        foreach (var req in requests)
        {

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


    public (Fund? Fund, string? Class) FindByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return default;

        var fund = GetCollection<Fund>().FindOne(x => x.Name == name);
        if (fund is not null) return (fund, null);

        // 尝试通过名称包含来查找 xxA xxB等子份额
        var poss = GetCollection<Fund>().Find(x => name.StartsWith(x.Name)).ToArray();
        if (poss.Length == 1)
            return (poss[0], name[poss[0].Name.Length..]);

        // 曾用名
        var ava = GetCollection<FundElements>().FindAll().Select(x => x.FullName.Changes.Select(y => new { id = x.Id, fn = y.Value })).ToList().SelectMany(x => x);

        var old = ava.FirstOrDefault(x => name.StartsWith(x.fn));
        if (old is not null)
            return (GetCollection<Fund>().FindById(old.id), name == old.fn ? null : name[old.fn.Length..]);
        return default;
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




    public static bool RebuildFundShareRecord(this ILiteDatabase db, int fundid)
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

    public static void RebuildFundShareRecord(this ILiteDatabase db, params int[] fundids)
    {
        foreach (var fundid in fundids)
            RebuildFundShareRecord(db, fundid);
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
