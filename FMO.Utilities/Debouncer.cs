public class Debouncer
{
    private readonly Action _action;
    private readonly int _dueTime;
    private Timer _timer;
    private Lock _lock = new();

    public Debouncer(Action action, int milseconds)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        _dueTime = milseconds > 0 ? milseconds : throw new ArgumentOutOfRangeException(nameof(milseconds));
        _timer = new Timer(_ => Work(), null, _dueTime, Timeout.Infinite);
    }

    public void Invoke()
    {
        if (_timer is null)
            _timer = new Timer(_ => Work(), null, _dueTime, Timeout.Infinite);
        else
            _timer.Change(_dueTime, Timeout.Infinite);
    }

    private void Work()
    {
        _action();
        _timer.Change(Timeout.Infinite, 0);
    }


    public void Dispose()
    {
        _timer?.Dispose();
    }
}