using FMO.Models;
using FMO.Trustee;
using LiteDB;
using System.Text.RegularExpressions;

namespace FMO.Utilities;


public static partial class DatabaseAssist
{
    private static Dictionary<int, Action<BaseDatabase>> patchs = new()
    {
        [42] = UpdateTASchcame,

        //[44] = MapTA

        [49] = ChangeAPIConfig,
        [51] = ChangeAmacAccount,
        [54] = ChangeAmacAccount2,

        [55] = FixLogInfo,
        [62] = RebuildFundShareRecord,
        [64] = ChangeOrderFilePathToRelative,
        [65] = UpdateLiqudatingTA,
        [66] = ChangeAPIAndClearData,

        [67] = Customer2Investor,
        [71] = PlatformTable,
        //[68] = AddManualLink
        [72] = MiggrateInstitutionCertifications
    };

    /// <summary>
    /// 迁移InstitutionCertifications
    /// </summary>
    /// <param name="database"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void MiggrateInstitutionCertifications(BaseDatabase db)
    {
        var list = db.GetCollection(nameof(InstitutionCertifications)).FindAll().ToList();
        if (!db.CollectionExists(nameof(InstitutionCertifications) + "_bak"))
            db.GetCollection(nameof(InstitutionCertifications) + "_bak").InsertBulk(list);

        foreach (var item in list)
        {
            foreach (var (k, v) in item)
            {
                if (v.Type == BsonType.Array)
                {
                    if (v.AsArray.FirstOrDefault() is BsonValue bv && bv.Type == BsonType.Document && bv.AsDocument.Keys.Intersect(["Path", "Time", "Hash"]).Count() == 3)
                    {
                        var label = v.AsArray.Select(x => x.AsDocument.TryGetValue("Title", out var t) ? t : null).FirstOrDefault()?.AsString;
                        var meta = v.AsArray.Select(x => FromStorate(x.AsDocument)).Where(x => x is not null);
                        item[k] = BsonMapper.Global.ToDocument(new MultiDualFile { Label = label, Files = [.. meta.Select(x => new DualFileMeta { Normal = x })] });
                    }
                }
                else if (v.Type == BsonType.Document && v.AsDocument.Keys.Intersect(["Path", "Time", "Hash"]).Count() == 3)
                    item[k] = BsonMapper.Global.ToDocument(FromStorate(v.AsDocument));
            }
        }

        db.GetCollection(nameof(InstitutionCertifications)).DeleteAll();
        db.GetCollection(nameof(InstitutionCertifications)).Insert(list);
    }

    private static FileMeta? FromStorate(BsonDocument? fileStorageInfo)
    {
        if (fileStorageInfo is null) return null;

        if (fileStorageInfo.Keys.Intersect(["Path", "Time", "Hash"]).Count() != 3)
            return null;

        return FileMeta.Create(fileStorageInfo["Path"].AsString, fileStorageInfo["Name"].AsString)
            with
        { Time = fileStorageInfo["Time"].AsDateTime, Hash = fileStorageInfo["Hash"].AsString };
    }



    private static void PlatformTable(BaseDatabase database)
    {
        using var db = DbHelper.Platform();
        foreach (var item in db.GetCollectionNames().Where(x => x.StartsWith("trustee_")))
        {
            db.DropCollection(item);
        }

        var data = db.GetCollection<TrusteeMethodShotRange>().FindAll().ToList();

        var good = data.Where(x => x.Id.Contains('.')).ToList();
        var bad = data.Where(x => !x.Id.Contains(".")).Select(x => new { Id = Regex.Replace(x.Id, "trustee_([a-z]+)Q", "trustee_$1.Q"), Item = x }).ToList();

        var mod = bad.Select(x => new TrusteeMethodShotRange(x.Id, x.Item.Begin, x.Item.End)).ExceptBy(good.Select(x => x.Id), x => x.Id).ToList();

        var bi = bad.Select(x => x.Item.Id).ToList();
        db.GetCollection<TrusteeMethodShotRange>().DeleteMany(x => bi.Contains(x.Id));
        db.GetCollection<TrusteeMethodShotRange>().Upsert(mod);
    }

    //private static void AddManualLink(BaseDatabase db)
    //{ 
    //    var recs = db.GetCollection("ta_record_bak").FindAll().ToList();

    //    db.GetCollection<ManualLinkOrder>().InsertBulk(recs.Select(x => new ManualLinkOrder(x["_id"], x["OrderId"], x["ExternalId"])));
    //}

    private static void Customer2Investor(BaseDatabase db)
    {
        var req = db.GetCollection(nameof(TransferRequest)).FindAll().ToArray();
        foreach (var item in req)
        {
            var modi = item.Keys.Where(x => x.StartsWith("Customer")).ToList();
            foreach (var m in modi)
            {
                item[m.Replace("Customer", "Investor")] = item[m];
            }
        }
        db.GetCollection(nameof(TransferRequest)).Update(req);


        var rec = db.GetCollection(nameof(TransferRecord)).FindAll().ToArray();
        foreach (var item in rec)
        {
            var modi = item.Keys.Where(x => x.StartsWith("Customer")).ToList();
            foreach (var m in modi)
            {
                item[m.Replace("Customer", "Investor")] = item[m];
            }
        }
        db.GetCollection(nameof(TransferRecord)).Update(rec);
    }


    /// <summary>
    /// 修改API的结构，需要重新获取数据
    /// </summary>
    /// <param name="database"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void ChangeAPIAndClearData(BaseDatabase database)
    {
        // 备份
        ILiteCollection<BsonDocument> creq = database.GetCollection("ta_request_bak");
        creq.DeleteAll();
        creq.InsertBulk(database.GetCollection(nameof(TransferRequest)).FindAll().ToList());

        ILiteCollection<BsonDocument> crec = database.GetCollection("ta_record_bak");
        crec.DeleteAll();
        crec.InsertBulk(database.GetCollection(nameof(TransferRecord)).FindAll().ToList());

        database.DropCollection(nameof(TransferRecord));
        database.DropCollection(nameof(TransferRequest));

        using var db = DbHelper.Platform();
        db.GetCollection<TrusteeMethodShotRange>().DeleteMany(x => x.Id.EndsWith(nameof(ITrustee.QueryTransferRecords)));
        db.GetCollection<TrusteeMethodShotRange>().DeleteMany(x => x.Id.EndsWith(nameof(ITrustee.QueryTransferRequests)));

    }

    /// <summary>
    ///  检查基金是否份额是否为0，如果是这样，最后的ta设为清盘
    /// </summary>
    /// <param name="db"></param>
    private static void UpdateLiqudatingTA(BaseDatabase db)
    {
        // 检查基金是否份额是否为0，如果是这样，最后的ta设为清盘
        var fsr = db.GetCollection<FundShareRecordByTransfer>().FindAll().ToList().OrderByDescending(x => x.Date).GroupBy(x => x.FundId).Select(x => new { FundId = x.Key, Item = x.First() }).Where(x => x.Item.Share == 0);
        foreach (var f in fsr)
        {
            var last = f.Item.Date;
            db.BeginTrans();
            foreach (var item in db.GetCollection<TransferRecord>().Find(x => x.FundId == f.FundId && x.ConfirmedDate == last).ToList())
            {
                if (item.Type != TransferRecordType.Redemption && item.Type != TransferRecordType.ForceRedemption)
                {
                    db.Rollback(); // 异常
                    break;
                }
                item.IsLiquidating = true;
                db.GetCollection<TransferRecord>().Update(item);
            }
            db.Commit();
        }
    }



    private static void ChangeOrderFilePathToRelative(BaseDatabase db)
    {
        void Modify(FileStorageInfo? fileStorageInfo)
        {
            if (fileStorageInfo?.Path is not null && !fileStorageInfo.Exists && Regex.Match(fileStorageInfo.Path, @"\w:(?:\\\w+\\)+files") is Match m && m.Success && fileStorageInfo.Path.Replace(m.Value, "files") is string newp && File.Exists(newp))
                fileStorageInfo.Path = newp;
        }

        var orders = db.GetCollection<TransferOrder>().FindAll().ToList();
        orders.ForEach(o =>
        {
            Modify(o.OrderSheet);
            Modify(o.Contract);
            Modify(o.RiskDiscloure);
            Modify(o.RiskPair);
            Modify(o.Videotape);
            Modify(o.Review);
        });
        db.GetCollection<TransferOrder>().Update(orders);
    }

    private static void RebuildFundShareRecord(BaseDatabase db)
    {
        void UpdateFundShareRecordByTA(ILiteCollection<TransferRecord> table, ILiteCollection<FundShareRecordByTransfer> tableSR, int fundId, DateOnly from = default)
        {
            var old = tableSR.Query().OrderByDescending(x => x.Date).Where(x => x.FundId == fundId && x.Date < from).FirstOrDefault();

            var data = table.Find(x => x.FundId == fundId).OrderBy(x => x.ConfirmedDate).GroupBy(x => x.ConfirmedDate);
            var list = new List<FundShareRecordByTransfer>();
            if (old is not null) list.Add(old);
            foreach (var item in data)
            {
                if (item.Sum(x => x.ShareChange()) is decimal change && change != 0)
                    list.Add(new FundShareRecordByTransfer(fundId, item.First().RequestDate, item.Key, change + (list.Count > 0 ? list[^1].Share : 0)));
            }
            tableSR.DeleteMany(x => x.FundId == fundId && x.Date >= from);
            tableSR.Upsert(list);
        }
        void UpdateInvestorBalance(ILiteCollection<TransferRecord> table, ILiteCollection<InvestorBalance> tableIB, int investorId, int fundId, DateOnly from = default)
        {
            var old = tableIB.Query().OrderByDescending(x => x.Date).Where(x => x.Date < from).FirstOrDefault();
            var data = table.Find(x => x.FundId == fundId && x.InvestorId == investorId && x.ConfirmedDate >= from).GroupBy(x => x.ConfirmedDate).OrderBy(x => x.Key);
            var list = new List<InvestorBalance>();
            if (old is not null) list.Add(old);
            foreach (var tf in data)
            {
                var share = tf.Sum(x => x.ShareChange());
                var deposit = tf.Where(x => x.Type switch { TransferRecordType.Subscription or TransferRecordType.Purchase or TransferRecordType.MoveIn or TransferRecordType.SwitchIn or TransferRecordType.TransferIn => true, _ => false }).Sum(x => x.ConfirmedNetAmount);
                var withdraw = tf.Where(x => x.Type switch { TransferRecordType.Redemption or TransferRecordType.ForceRedemption or TransferRecordType.MoveOut or TransferRecordType.SwitchOut or TransferRecordType.TransferOut or TransferRecordType.Distribution => true, _ => false }).Sum(x => x.ConfirmedNetAmount);

                var last = list.LastOrDefault() ?? new();
                var cur = new InvestorBalance { FundId = fundId, InvestorId = investorId, Share = share + last.Share, Deposit = deposit + last.Deposit, Withdraw = withdraw + last.Withdraw, Date = tf.Key };
                list.Add(cur);
            }

            tableIB.DeleteMany(x => x.FundId == fundId && x.InvestorId == investorId && x.Date >= from);
            tableIB.Upsert(list);
        }

        var funds = db.GetCollection<Fund>().Query().Select(f => f.Id).ToList();

        var t1 = db.GetCollection<TransferRecord>();
        var t2 = db.GetCollection<FundShareRecordByTransfer>();
        var t3 = db.GetCollection<FundShareRecordByDaily>();
        var t4 = db.GetCollection<InvestorBalance>();

        db.BeginTrans();
        try
        {
            t2.DeleteAll();
            t3.DeleteAll();
            t4.DeleteAll();

            foreach (var fid in funds)
            {
                UpdateFundShareRecordByTA(t1, t2, fid);

                var dailyValues = db.GetDailyCollection(fid).Query().Where(x => x.Share > 0).OrderBy(x => x.Date).ToList();
                if (dailyValues.Count > 0) t3.Insert(new FundShareRecordByDaily(fid, dailyValues[0].Date, dailyValues[0].Share));
                for (var i = 1; i < dailyValues.Count; i++)
                {
                    if (dailyValues[i].Share != dailyValues[i - 1].Share)
                        t3.Insert(new FundShareRecordByDaily(fid, dailyValues[i].Date, dailyValues[i].Share));
                }

                foreach (var cid in t1.Query().Select(x => x.InvestorId).ToList().Distinct())
                    UpdateInvestorBalance(t1, t4, cid, fid);

            }
            db.Commit();
        }
        catch { db.Rollback(); }
    }

    private static void FixLogInfo(BaseDatabase database)
    {
        using var db = new LiteDatabase(@$"FileName=data\platformlog.db;Connection=Shared");
        db.GetCollection("LogInfo").DeleteMany(x => !x["_id"].IsInt32);
    }

    private static void ChangeAmacAccount2(BaseDatabase database)
    {
        database.GetCollection<AmacReportAccount>().Delete("pmg");

        if (database.GetCollection<AmacReportAccount>().FindById("pfiddirect") is AmacReportAccount a)
        {
            database.GetCollection<AmacReportAccount>().Insert(a with { Id = "pmg" });

            database.GetCollection<AmacReportAccount>().Delete("pfiddirect");
        }
    }

    private static void ChangeAmacAccount(BaseDatabase database)
    {
        var config = database.GetCollection("AmacAccount").FindAll().ToArray();
        foreach (var item in config)
        {
            if (!item.ContainsKey("_type")) continue;
            var str = item["_type"].AsString.Split(',');
            item["_type"] = $"{str[0].Replace("IO.AMAC", "Models")},FMO.Models";
        }


        database.GetCollection("AmacAccount").Update(config);
    }

    private static void ChangeAPIConfig(BaseDatabase database)
    {
        using var db = DbHelper.Platform();
        var config = db.GetCollection("IAPIConfig").FindAll().ToArray();
        foreach (var item in config)
        {
            var idf = item["_id"].AsString.Split('_')[1];
            item["_type"] = $"FMO.Trustee.APIConfig,FMO.Trustee.{idf.ToUpper()}";
        }


        db.GetCollection("IAPIConfig").Update(config);
    }
}
