using System.Collections.Concurrent;
using Timer = System.Threading.Timer;

namespace JackTheVideoRipper.models.data_structures;

public class UpdateBuffer<T>
{
    #region Data Members

    private readonly ConcurrentDictionary<string, T> _table = new();
    
    private readonly Timer _updateTimer;

    private const int _DEFAULT_UPDATE_PERIOD = 500;

    #endregion

    #region Properties
    
    public int UpdatePeriod
    {
        set => _updateTimer.Change(0, value);
    }
    
    public bool Empty => _table.IsEmpty;

    public IEnumerable<T> Data => _table.Values;

    #endregion

    #region Events

    public event Action<IEnumerable<T>> OnBufferDispatched = delegate { };

    #endregion

    #region Constructor

    public UpdateBuffer(int updatePeriod = _DEFAULT_UPDATE_PERIOD)
    {
        _updateTimer = new Timer(Dispatch, null, 0, updatePeriod);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Public facing method to queue a new task into the buffer
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add(string key, T value)
    {
        _table[key] = value;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Timed event to release all pending tasks
    /// </summary>
    /// <param name="state"></param>
    private void Dispatch(object? state)
    {
        if (Empty)
            return;
        OnBufferDispatched.Invoke(Data.ToArray());
        _table.Clear();
    }

    #endregion
}