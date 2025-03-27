public class Debouncer
{
    private readonly Action _action;
    private readonly int _dueTime;
    private Timer? _timer;
    private Lock _lock = new();

    public Debouncer(Action action, int milseconds = 200)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        _dueTime = milseconds > 0 ? milseconds : throw new ArgumentOutOfRangeException(nameof(milseconds));
        //_timer = new Timer(_ => Work(), null, Timeout.Infinite, _dueTime);
    }

    public void Invoke()
    {
        lock (_lock)
        {
            if (_timer is null)
                _timer = new Timer(_ => Work(), null, _dueTime, Timeout.Infinite);
            else
                _timer.Change(_dueTime, Timeout.Infinite);
        }
    }

    private void Work()
    {
        lock(_lock)
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _action();
            _timer?.Dispose();
            _timer = null;
        }
    }

}