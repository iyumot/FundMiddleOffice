using FMO.Models;
using System.Collections.Concurrent;

namespace FMO.Utilities;

public interface IDataValidation
{
    public Type[] Related { get; }

    public void OnEntityIn(object[] obj);
}


public abstract class FundDataValidation : IDataValidation
{
    public FundDataValidation(int fundId)
    {
        FundId = fundId;
    }

    public int FundId { get; }

    public virtual Type[] Related { get; } = [];

    public abstract void OnEntityIn(object[] obj);
}



public class FundSharePairValidation : FundDataValidation
{
    private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

    public override Type[] Related { get; } = [typeof(DailyValue), typeof(TransferRecord)];


    private ConcurrentBag<DailyValue> Dailies { get; } = new();

    private ConcurrentBag<TransferRecord> Records { get; } = new();


    private Debouncer debouncer;

    public FundSharePairValidation() : base(0)
    {
        debouncer = new(Check, 1000);
    }

    public override void OnEntityIn(object[] obj)
    {
        if (obj is null || obj.Length == 0) return;

        try
        {
            semaphoreSlim.Wait();

            foreach (var item in obj)
            {
                if (item is DailyValue d)
                    Dailies.Add(d);
                else if (item is TransferRecord t)
                    Records.Add(t);
            }


        }
        catch { }
        finally { semaphoreSlim.Release(); }
    }



    public void Check()
    {
        try
        {
            semaphoreSlim.Wait();

            var gd = Dailies.GroupBy(x => x.FundId);
            var gr = Records.GroupBy(x => x.FundId);

            // 需要检验的
            var fundids = gd.Select(x => x.Key).Union(gr.Select(x => x.Key)).Distinct().ToList();


        }
        catch { }
        finally { semaphoreSlim.Release(); }
    }
}


