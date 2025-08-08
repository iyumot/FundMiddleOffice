using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using System.Collections.Concurrent;


namespace FMO.Utilities;

public record FundClearDateMissingContext(string Name, string? Code);

public class FundClearDateMissingRule : VerifyRule<Fund>
{


    private ConcurrentDictionary<int, DataTip> Tips { get; } = [];

    public ConcurrentBag<int> Params { get; } = [];

    //public override Type[] Related { get; } = [typeof(Fund)];

    public override void Init()
    {

        using var db = DbHelper.Base();
        var finfo = db.GetCollection<Fund>().Query().Where(x => x.ClearDate == default).Select(x => new { x.Id, x.Name, x.Code, x.Status, x.ClearDate }).ToList();

        foreach (var f in finfo.Where(x => x.Status >= FundStatus.StartLiquidation))
        {
            DataTip tip = new() { Tags = ["Fund", $"Fund{f.Id}", nameof(FundClearDateMissingRule)], Context = new FundClearDateMissingContext(f.Name, f.Code) };
            Tips.TryAdd(f.Id, tip);
            Send(tip);
        }
    }

    protected override void OnEntityOverride(IEnumerable<Fund> obj)
    {
        foreach (var item in obj)
            if (item is Fund f)
                Params.Add(f.Id);
    }


    protected override void VerifyOverride()
    {
        var arr = Params.ToList();

        using var db = DbHelper.Base();
        var error = db.GetCollection<Fund>().Query().Where(x => arr.Contains(x.Id)).Select(x => new { x.Id, x.Name, x.Code, x.Status, x.ClearDate }).ToList().Where(x => x.Status >= FundStatus.StartLiquidation && x.ClearDate == default);

        var filterd = Tips.Where(x => arr.Contains(x.Key));

        //removed 
        var removed = error.Any() ? filterd.ExceptBy(error.Select(x => x.Id), x => x.Key) : filterd;
        foreach (var item in removed)
        {
            Tips.Remove(item.Key, out var _);
            WeakReferenceMessenger.Default.Send(new DataTipRemove(item.Value.Id));
        }

        // 不关心重复的

        // add
        var add = Tips.Count > 0 ? error.ExceptBy(filterd.Select(x => x.Key), x => x.Id) : error;
        foreach (var f in add)
        {
            DataTip tip = new() { Tags = ["Fund", $"Fund{f.Id}"], Context = new FundClearDateMissingContext(f.Name, f.Code) };
            Tips.TryAdd(f.Id, tip);
            Send(tip);
        }
    }


    protected override void ClearParamsOverride() => Params.Clear();

}

//public class FundSharePairValidation : FundDataValidation
//{
//    private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

//    public override Type[] Related { get; } = [typeof(DailyValue), typeof(TransferRecord)];


//    private ConcurrentBag<DailyValue> Dailies { get; } = new();

//    private ConcurrentBag<TransferRecord> Records { get; } = new();


//    private Debouncer debouncer;

//    public FundSharePairValidation() : base(0)
//    {
//        debouncer = new(Check, 1000);
//    }

//    public override void OnEntityOverride(IEnumerable<object> obj)
//    {
//        if (obj is null || obj.Length == 0) return;

//        try
//        {
//            semaphoreSlim.Wait();

//            foreach (var item in obj)
//            {
//                if (item is DailyValue d)
//                    Dailies.Add(d);
//                else if (item is TransferRecord t)
//                    Records.Add(t);
//            }


//        }
//        catch { }
//        finally { semaphoreSlim.Release(); }
//    }



//    public void Check()
//    {
//        try
//        {
//            semaphoreSlim.Wait();

//            var gd = Dailies.GroupBy(x => x.FundId);
//            var gr = Records.GroupBy(x => x.FundId);

//            // 需要检验的
//            var fundids = gd.Select(x => x.Key).Union(gr.Select(x => x.Key)).Distinct().ToList();


//        }
//        catch { }
//        finally { semaphoreSlim.Release(); }
//    }
//}
