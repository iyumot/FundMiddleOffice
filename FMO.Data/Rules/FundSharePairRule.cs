using FMO.Models;
using System.Collections.Concurrent;


namespace FMO.Utilities;


public record class FundSharePairErrorDate(DateOnly Date, decimal? ShareByDaily, decimal? ShareByTransfer)
{
    public bool IsDiff => ShareByDaily != ShareByTransfer;
}

public record FundSharePairContext(string FundName, int ByDailyCount, int ByTransferCount, IEnumerable<FundSharePairErrorDate> Diff);


public class FundSharePairRule : VerifyRule<FundShareRecord, TransferRecord>
{

    public ConcurrentDictionary<int, IDataTip> Tips { get; } = [];


    private List<FundShareRecord> Records { get; } = new();

    private List<TransferRecord> Records2 { get; } = new();


    protected override void OnEntityOverride(IEnumerable<FundShareRecord> obj)
    {
        foreach (var item in obj)
            Records.Add(item);
    }

    protected override void OnEntityOverride(IEnumerable<TransferRecord> obj)
    {
        foreach (var item in obj)
            Records2.Add(item);
    }




    protected override void VerifyOverride()
    {
        var fids = Records.Select(x => x.FundId).Union(Records2.Select(x => x.FundId)).Distinct().ToList();

        using var db = DbHelper.Base();
        var finfo = db.GetCollection<Fund>().Query().Where(x => fids.Contains(x.Id) && x.SetupDate != default).Select(x => new { x.Id, x.Name }).ToList();
        // 排除清盘后的0
        var byta = db.GetCollection<FundShareRecord>().Find(x => x.Share > 0).OrderBy(x => x.Date).GroupBy(x => x.FundId).ToDictionary(x => x.Key, x => x.AsEnumerable());
        var bydy = db.GetCollection<FundShareRecord>("fsr_daily").Find(x => x.Share > 0).OrderBy(x => x.Date).GroupBy(x => x.FundId).ToDictionary(x => x.Key, x => x.AsEnumerable());


        foreach (var f in finfo)
        {
            var ta = byta.ContainsKey(f.Id) ? byta[f.Id].ToList() : [];
            var dy = bydy.ContainsKey(f.Id) ? bydy[f.Id].ToList() : [];

            var same = ta.SequenceEqual(dy);

            // 完全一样 或 特殊情况，当第一个share一致，但是时间不一致
            if (same || (ta.Count > 0 && dy.Count > 0 && ta[0].Share == dy[0].Share && ta.Skip(1).SequenceEqual(dy.Skip(1))))
            {
                if (Tips.ContainsKey(f.Id))
                {
                    Revoke(Tips[f.Id].Id);
                    Tips.TryRemove(f.Id, out var t);
                }
            }
            else
            {
                var context = (from taItem in ta
                               join dyItem in dy on taItem.Date equals dyItem.Date into joined
                               from dyJoined in joined.DefaultIfEmpty()
                               select new FundSharePairErrorDate(taItem.Date, dyJoined?.Share, taItem.Share)).ToList();

                for (int i = 0; i < context.Count - 1; i++)
                {
                    if (context[i + 1].ShareByDaily == context[i + 1].ShareByTransfer)
                        context.RemoveAt(i);
                }


                DataTip<FundSharePairContext> tip = new() { Tags = ["Fund", $"Fund{f.Id}", nameof(FundSharePairRule)], _Context = new FundSharePairContext(f.Name, dy.Count, ta.Count, context) };
                Tips.TryAdd(f.Id, tip);
                Send(tip);
            }
        }
    }

    protected override void ClearParamsOverride()
    {
        Records.Clear();
    }

    public override void Init()
    {
        using var db = DbHelper.Base();

        var finfo = db.GetCollection<Fund>().Query().Where(x => x.SetupDate != default).Select(x => new { x.Id, x.Name }).ToList();

        var byta = db.GetCollection<FundShareRecord>().Find(x => x.Share > 0).OrderBy(x => x.Date).GroupBy(x => x.FundId).ToDictionary(x => x.Key, x => x.AsEnumerable());
        var bydy = db.GetCollection<FundShareRecord>("fsr_daily").Find(x => x.Share > 0).OrderBy(x => x.Date).GroupBy(x => x.FundId).ToDictionary(x => x.Key, x => x.AsEnumerable());


        foreach (var f in finfo)
        {
            var ta = byta.ContainsKey(f.Id) ? byta[f.Id].ToList() : [];
            var dy = bydy.ContainsKey(f.Id) ? bydy[f.Id].ToList() : [];

            var same = ta.SequenceEqual(dy);

            if (same) continue;

            // 特殊情况，当第一个share一致，但是时间不一致
            if (ta.Count > 0 && dy.Count > 0 && ta[0].Share == dy[0].Share && ta.Skip(1).SequenceEqual(dy.Skip(1)))
                continue;

            var allDates = ta.Select(x => x.Date).Union(dy.Select(x => x.Date)).OrderBy(d => d);
            var context = (from date in allDates
                           join taItem in ta on date equals taItem.Date into taJoined
                           from taItem in taJoined.DefaultIfEmpty()
                           join dyItem in dy on date equals dyItem.Date into dyJoined
                           from dyItem in dyJoined.DefaultIfEmpty()
                               //where taItem?.Share != dyItem?.Share
                           select new FundSharePairErrorDate(date, dyItem?.Share, taItem?.Share)).ToList();

            //for (int i = context.Count - 1; i > 0; i--)
            //{
            //    if (context[i].ShareByDaily == context[i].ShareByTransfer && context[i - 1].ShareByDaily == context[i - 1].ShareByTransfer)
            //        context.RemoveAt(i);
            //}


            //ta.GroupJoin(dy, x => x.Date, x => x.Date, (t, ds) => new { t, ds }).SelectMany(x => x.ds.DefaultIfEmpty(), (t, d) => new FundSharePairErrorDate { Date = .Date, ShareByDaily = t.Share, ShareByTransfer = d.)
            DataTip<FundSharePairContext> tip = new() { Tags = ["Fund", $"Fund{f.Id}", nameof(FundSharePairRule)], _Context = new FundSharePairContext(f.Name, dy.Count, ta.Count, context) };
            Tips.TryAdd(f.Id, tip);
            Send(tip);

        }

    }
}
