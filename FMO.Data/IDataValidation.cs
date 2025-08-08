using CommunityToolkit.Mvvm.Messaging;
using FMO.Logging;


namespace FMO.Utilities;

public interface IDataValidation
{
    public Type[] Related { get; }

    void Init();
    //void OnEntityArrival(IEnumerable<object> obj);
    void Verify();
}


public abstract class VerifyRule : IDataValidation
{
    protected VerifyRule()
    {
        debouncer = new(Verify, 1000);
    }

    private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

    public abstract Type[] Related { get; }


    protected Debouncer debouncer { get; set; }

    public abstract void Init();


    public void OnEntityArrival(IEnumerable<object> obj)
    {
        if (obj is null || !obj.Any()) return;

        try
        {
            semaphoreSlim.Wait();

            OnEntityOverride(obj);

            debouncer.Invoke();
        }
        catch (Exception e) { LogEx.Error($"{e}"); }
        finally { semaphoreSlim.Release(); }
    }

    protected abstract void OnEntityOverride(IEnumerable<object> obj);


    public void Verify()
    {
        try
        {
            semaphoreSlim.Wait();

            VerifyOverride();

            ClearParamsOverride();
        }
        catch (Exception e) { LogEx.Error($"{e}"); }
        finally { semaphoreSlim.Release(); }
    }

    protected abstract void VerifyOverride();

    protected abstract void ClearParamsOverride();

    protected void Send(IDataTip tip) => WeakReferenceMessenger.Default.Send(tip);
}



public abstract class VerifyRuleBase : IDataValidation
{
    protected VerifyRuleBase()
    {
        debouncer = new(Verify, 1000);
    }


    protected SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

    protected Debouncer debouncer { get; set; }

    public Type[] Related => throw new NotImplementedException();

    public void Verify()
    {
        try
        {
            semaphoreSlim.Wait();

            VerifyOverride();

            ClearParamsOverride();
        }
        catch (Exception e) { LogEx.Error($"{e}"); }
        finally { semaphoreSlim.Release(); }
    }

    protected abstract void VerifyOverride();

    protected abstract void ClearParamsOverride();

    protected void Send(IDataTip tip) => WeakReferenceMessenger.Default.Send(tip);

    public abstract void Init();
}

public abstract class VerifyRule<T> : VerifyRuleBase
{
    public void OnEntityArrival(IEnumerable<T> obj)
    {
        if (obj is null || !obj.Any()) return;

        try
        {
            semaphoreSlim.Wait();

            OnEntityOverride(obj);

            debouncer.Invoke();
        }
        catch (Exception e) { LogEx.Error($"{e}"); }
        finally { semaphoreSlim.Release(); }
    }

    protected abstract void OnEntityOverride(IEnumerable<T> obj);

}

public abstract class VerifyRule<T1, T2> : VerifyRuleBase
{  
    public void OnEntityArrival(IEnumerable<T1> obj)
    {
        if (obj is null || !obj.Any()) return;

        try
        {
            semaphoreSlim.Wait();

            OnEntityOverride(obj);

            debouncer.Invoke();
        }
        catch (Exception e) { LogEx.Error($"{e}"); }
        finally { semaphoreSlim.Release(); }
    }
    public void OnEntityArrival(IEnumerable<T2> obj)
    {
        if (obj is null || !obj.Any()) return;

        try
        {
            semaphoreSlim.Wait();

            OnEntityOverride(obj);

            debouncer.Invoke();
        }
        catch (Exception e) { LogEx.Error($"{e}"); }
        finally { semaphoreSlim.Release(); }
    }
    protected abstract void OnEntityOverride(IEnumerable<T1> obj);
    protected abstract void OnEntityOverride(IEnumerable<T2> obj);
}

//public abstract class FundDataValidation : IDataValidation
//{
//    public FundDataValidation(int fundId)
//    {
//        FundId = fundId;
//    }

//    private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

//    public int FundId { get; }

//    public virtual Type[] Related { get; } = [];

//    public abstract void OnEntityOverride(IEnumerable<object> obj);


//    public void Verify()
//    {

//    }

//    public abstract void VerifyOverride();
//}




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
