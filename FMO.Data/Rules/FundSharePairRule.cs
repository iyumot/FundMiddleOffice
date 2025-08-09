using FMO.Models;
using System.Collections.Concurrent;


namespace FMO.Utilities;


public record FundSharePairContext(string FundName, IEnumerable<FundShareRecord> ByDaily, IEnumerable<FundShareRecord> ByTransfer);


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
        throw new NotImplementedException();
    }

    protected override void ClearParamsOverride()
    {
        Records.Clear();
    }

    public override void Init()
    {
        using var db = DbHelper.Base();
        var fids = Records.Select(x => x.FundId).Union(Records2.Select(x => x.FundId)).Distinct().ToList();

        var finfo = db.GetCollection<Fund>().Query().Where(x => fids.Contains(x.Id) && x.ClearDate == default).Select(x => new { x.Id, x.Name }).ToList();

        var byta = db.GetCollection<FundShareRecord>().FindAll().OrderBy(x => x.Date).GroupBy(x => x.FundId).ToDictionary(x => x.Key, x => x.AsEnumerable());
        var bydy = db.GetCollection<FundShareRecord>("fsr_daily").FindAll().OrderBy(x => x.Date).GroupBy(x => x.FundId).ToDictionary(x => x.Key, x => x.AsEnumerable());


        foreach (var f in finfo)
        {
            var ta = byta.ContainsKey(f.Id) ? byta[f.Id] : [];
            var dy = bydy.ContainsKey(f.Id) ? bydy[f.Id] : [];

            var same = ta.SequenceEqual(dy);

            if (same) continue;


            DataTip<FundSharePairContext> tip = new() { Tags = ["Fund", $"Fund{f.Id}", nameof(FundSharePairRule)], _Context = new FundSharePairContext(f.Name, dy, ta) };
            Tips.TryAdd(f.Id, tip);
            Send(tip);

        }

    }
}
