using CommunityToolkit.Mvvm.Messaging;
using FMO.Logging;
using FMO.Models;
using LiteDB;
using Serilog;
using System.Text.RegularExpressions;
namespace FMO.Utilities;

internal record PatchRecord(int Id, DateTime Time);

public static class DatabaseAssist
{
    private static Dictionary<int, Action<BaseDatabase>> patchs = new()
    {
        /*[2] = db =>
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

        },*/

        [42] = UpdateTASchcame,

        //[44] = MapTA

        [49] = ChangeAPIConfig,
        [51] = ChangeAmacAccount,
        [54] = ChangeAmacAccount2,

        [55] = FixLogInfo,
        [62] = RebuildFundShareRecord,
        [64] = ChangeOrderFilePathToRelative,
        [65] = UpdateLiqudatingTA
    };

    /// <summary>
    ///  检查基金是否份额是否为0，如果是这样，最后的ta设为清盘
    /// </summary>
    /// <param name="db"></param>
    private static void UpdateLiqudatingTA(BaseDatabase db)
    {
        // 检查基金是否份额是否为0，如果是这样，最后的ta设为清盘
        var fsr = db.GetCollection<FundShareRecord>().FindAll().ToList().OrderByDescending(x => x.Date).GroupBy(x => x.FundId).Select(x => new { FundId = x.Key, Item = x.First() }).Where(x => x.Item.Share == 0);
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
        void UpdateFundShareRecordByTA(ILiteCollection<TransferRecord> table, ILiteCollection<FundShareRecord> tableSR, int fundId, DateOnly from = default)
        {
            var old = tableSR.Query().OrderByDescending(x => x.Date).Where(x => x.FundId == fundId && x.Date < from).FirstOrDefault();

            var data = table.Find(x => x.FundId == fundId).OrderBy(x => x.ConfirmedDate).GroupBy(x => x.ConfirmedDate);
            var list = new List<FundShareRecord>();
            if (old is not null) list.Add(old);
            foreach (var item in data)
            {
                if (item.Sum(x => x.ShareChange()) is decimal change && change != 0)
                    list.Add(new FundShareRecord(fundId, item.Key, change + (list.Count > 0 ? list[^1].Share : 0)));
            }
            tableSR.DeleteMany(x => x.FundId == fundId && x.Date >= from);
            tableSR.Upsert(list);
        }
        void UpdateInvestorBalance(ILiteCollection<TransferRecord> table, ILiteCollection<InvestorBalance> tableIB, int investorId, int fundId, DateOnly from = default)
        {
            var old = tableIB.Query().OrderByDescending(x => x.Date).Where(x => x.Date < from).FirstOrDefault();
            var data = table.Find(x => x.FundId == fundId && x.CustomerId == investorId && x.ConfirmedDate >= from).GroupBy(x => x.ConfirmedDate).OrderBy(x => x.Key);
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
        var t2 = db.GetCollection<FundShareRecord>();
        var t3 = db.GetCollection<FundShareRecord>("fsr_daily");
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
                if (dailyValues.Count > 0) t3.Insert(new FundShareRecord(fid, dailyValues[0].Date, dailyValues[0].Share));
                for (var i = 1; i < dailyValues.Count; i++)
                {
                    if (dailyValues[i].Share != dailyValues[i - 1].Share)
                        t3.Insert(new FundShareRecord(fid, dailyValues[i].Date, dailyValues[i].Share));
                }

                foreach (var cid in t1.Query().Select(x => x.CustomerId).ToList().Distinct())
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


    /// <summary>
    /// 自检
    /// </summary>
    public static void SystemValidation()
    {
        using (var db = DbHelper.Base())
        {
            VerifyAndFixInvestorMission(db);

            ///////////
            // call 默认是null，和false不一致，所以为每个fund，设置一个默认值
            VerifyAndFixElements(db);
            ////////////////////////////

            VerifyAndFixDaily(db);
            ////////////////////////

            Patch();
        }


        using (var db = DbHelper.Platform())
        {
            LiteDB.ILiteCollection<PlatformSynchronizeTime> coll = db.GetCollection<PlatformSynchronizeTime>();
            coll.EnsureIndex(x => new { x.Identifier, x.Method }, true);
        }
    }

    private static void VerifyAndFixDaily(BaseDatabase db)
    {
        // 获取所有以 fv_ 开头的表
        var collections = db.GetCollectionNames()
            .Where(name => name.StartsWith("fv_"));

        foreach (var collectionName in collections)
        {
            try
            {
                var collection = db.GetCollection<DailyValue>(collectionName);
                var deletedCount = collection.DeleteMany(x => x.NetValue == 0);
            }
            catch (Exception ex) { LogEx.Error($"{ex.Message}"); }
        }
    }

    /// <summary>
    /// call 默认是null，和false不一致，所以为每个fund，设置一个默认值
    /// </summary>
    /// <param name="db"></param>
    private static void VerifyAndFixElements(BaseDatabase db)
    {
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
    }

    /// <summary>
    /// 校验ta，为TransferRecord的CustomerId为0的设置Id
    /// </summary>
    /// <param name="db"></param>
    private static void VerifyAndFixInvestorMission(BaseDatabase db)
    {
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
        var runp = records.ExceptBy(requests.Select(x => x.ExternalId), x => x.ExternalId).ExceptBy(requests.Select(x => x.ExternalId), x => x.ExternalRequestId).ToList();

        // // 构建时间线
        // var timeline = btr.Select(x => x.Time).Union(orders.Select(x => new DateTime(x.Date, TimeOnly.MinValue))).Union(requests.Select(x => new DateTime(x.RequestDate, TimeOnly.MinValue))).
        //      Union(records.Select(x => new DateTime(x.ConfirmedDate, TimeOnly.MinValue))).Distinct().ToList();

        ////////////////////////////////////// record
        /// 上对应request 下对应 transaction
        // 找已知的map 
        var rids = records.Select(x => x.Id).ToList();
        var mapTable = db.GetCollection<TransferMapping>();
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
                else if (requests.FirstOrDefault(x => x.FundId == rec.FundId && (x.ExternalId == rec.ExternalRequestId || x.ExternalId == rec.ExternalId)) is TransferRequest req2)
                    old.RequestId = req2.Id;
                else if (requests.FirstOrDefault(x => (x.ExternalId == rec.ExternalRequestId || x.ExternalId == rec.ExternalId)) is TransferRequest req4)
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
