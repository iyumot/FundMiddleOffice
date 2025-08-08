using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using System.Collections.Concurrent;


namespace FMO.Utilities;

public record FundDailyMissingContext(string FundName, string MissingInfo);

public class FundDailyMissingRule : VerifyRule
{
    public List<int> BadFundIds { get; } = [];

    public ConcurrentBag<DailyValue> Dailies { get; } = [];


    public ConcurrentDictionary<int, List<DateOnly>> MissDays { get; } = [];

    /// <summary>
    /// fundid tip
    /// </summary>
    public ConcurrentDictionary<int, IDataTip> Tips { get; } = [];


    public override Type[] Related { get; } = [typeof(DailyValue)];

    public override void Init()
    {

        using var db = DbHelper.Base();
        var finfo = db.GetCollection<Fund>().Query().Where(x =>/*(int)x.Status >= (int)FundStatus.Normal &&*/ x.SetupDate.Year > 1970).Select(x => new { x.Id, x.Name, x.SetupDate, x.ClearDate }).ToList();

        foreach (var f in finfo)
        {
            var has = db.GetDailyCollection(f.Id).Query().OrderBy(x => x.Date).Select(x => x.Date).ToList();
            if (has.Count == 0)
                has.Add(f.SetupDate);

            DateOnly begin = f.SetupDate, end = f.ClearDate == default ? DateOnly.FromDateTime(DateTime.Now) : f.ClearDate;

            List<DateOnly> missing = Days.TradingDaysBetween(begin, end).Except(has).ToList();
            if (missing.Count == 0) continue;


            var tip = new DataTip<FundDailyMissingContext>() { Tags = ["Fund", $"Fund{f.Id}", nameof(FundDailyMissingRule)], _Context = new FundDailyMissingContext(f.Name, $"{missing.Count}个：{string.Join(',', missing.Take(5))}{missing.Count switch { > 5 => "...", _ => "" }}") };
            Tips.TryAdd(f.Id, tip);
            Send(tip);
        }

    }

    protected override void ClearParamsOverride() => BadFundIds.Clear();

    protected override void OnEntityOverride(IEnumerable<object> obj)
    {
        foreach (var item in obj)
            if (item is DailyValue v)
                Dailies.Add(v);
    }

    protected override void VerifyOverride()
    {
        using var db = DbHelper.Base();
        foreach (var g in Dailies.GroupBy(x => new { x.FundId, x.Class }))
        {
            int fundId = g.Key.FundId;
            if (Tips.ContainsKey(fundId) && Tips[fundId] is DataTip<FundDailyMissingContext> tip)
            {
                var f = db.GetCollection<Fund>().FindById(fundId);
                DateOnly begin = f.SetupDate, end = f.ClearDate == default ? DateOnly.FromDateTime(DateTime.Now) : f.ClearDate;

                var has = db.GetDailyCollection(f.Id).Query().OrderBy(x => x.Date).Select(x => x.Date).ToList();
                if (has.Count == 0)
                    has.Add(f.SetupDate);
                List<DateOnly> missing = Days.TradingDaysBetween(begin, end).Except(has).ToList();


                // 删除旧的
                WeakReferenceMessenger.Default.Send(new DataTipRemove(tip.Id));

                if (missing.Count == 0)
                {
                    Tips.TryRemove(fundId, out var _);
                    continue;
                }
                tip._Context = tip._Context! with { MissingInfo = $"{missing.Count}个：{string.Join(',', missing.Take(5))}..." };
                Send(tip);
            }
        }
    }
}
