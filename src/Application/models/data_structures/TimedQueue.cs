using JackTheVideoRipper.framework;
using Timer = System.Threading.Timer;

namespace JackTheVideoRipper.models.data_structures;

public class TimedQueue<T>
{
    private readonly Timer _updateTimer;
    
    private readonly Action<T> _updateAction;
    
    private const int _DEFAULT_UPDATE_PERIOD = 500;
    private const int _DEFAULT_QUEUE_TICK = 200;

    public int UpdatePeriod
    {
        set => _updateTimer.Change(0, value);
    }
    
    private bool _started;
    private bool _updating;
    private bool _paused;
    private bool Active => _started && !_paused;

    private readonly Queue<T> _notificationQueue = new();

    public TimedQueue(Action<T> updateAction, int updatePeriod = _DEFAULT_UPDATE_PERIOD)
    {
        _updateAction = updateAction;
        _updateTimer = new Timer(Update, null, 0, updatePeriod);
    }
    
    public void Start()
    {
        if (_started)
            return;
        _started = true;
    }

    public void Pause()
    {
        if (_paused)
            return;
        _paused = true;
    }

    public void Resume()
    {
        if (!_paused)
            return;
        _paused = false;
    }

    public void Enqueue(T item)
    {
        _notificationQueue.Enqueue(item);
    }

    private async void Update(object? sender = null)
    {
        if (_updating)
            return;
        
        _updating = true;
        
        if (!Active)
            await Tasks.WaitUntil(() => Active);
        
        while (_notificationQueue.Count > 0)
        {
            _updateAction(_notificationQueue.Dequeue());
            await Task.Delay(_DEFAULT_QUEUE_TICK);
            if (_paused)
                break;
        }

        _updating = false;
    }
}