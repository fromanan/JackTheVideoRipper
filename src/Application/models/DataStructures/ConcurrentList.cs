using System.Collections;

namespace JackTheVideoRipper.models.DataStructures;

public class ConcurrentList<T> : IReadOnlyList<T> where T : IClaimable
{
    #region Data Members

    private readonly ReaderWriterLockSlim _accessLock = new();
    
    private readonly List<T> _data;

    #endregion

    #region Constructors

    public ConcurrentList()
    {
        _data = new List<T>();
    }
    
    public ConcurrentList(int size)
    {
        _data = new List<T>(new T[size]);
    }

    #endregion

    #region Public Methods

    public async Task<T> Next(Func<T, bool> predicate, int tickInMilliseconds)
    {
        await Tasks.WaitUntil(Where(predicate).Any, tickInMilliseconds);
        
        T result = First(predicate);

        _accessLock.EnterWriteLock();
        
        try
        {
            result.Claimed = true;
        }
        finally
        {
            _accessLock.ExitWriteLock();
        }
        
        return result;
    }

    public IEnumerable<T> Where(Func<T, bool> predicate)
    {
        IEnumerable<T> result;

        _accessLock.EnterReadLock();
            
        try
        {
            result = _data.Where(predicate);
        }
        finally
        {
            _accessLock.ExitReadLock();
        }
            
        return result;
    }

    public T First(Func<T, bool> predicate)
    {
        T result;

        _accessLock.EnterReadLock();
            
        try
        {
            result = _data.First(predicate);
        }
        finally
        {
            _accessLock.ExitReadLock();
        }
            
        return result;
    }

    #endregion

    #region IEnumerable Implementation

    public IEnumerator<T> GetEnumerator()
    {
        List<T>.Enumerator result;

        _accessLock.EnterReadLock();
            
        try
        {
            result = _data.GetEnumerator();
        }
        finally
        {
            _accessLock.ExitReadLock();
        }
            
        return result;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    public int Count
    {
        get
        {
            int length;
            
            _accessLock.EnterReadLock();
            try
            {
                length = Count;
            }
            finally
            {
                _accessLock.ExitReadLock();
            }
            
            return length;
        }
    }

    public T this[int index]
    {
        get
        {
            T result;

            _accessLock.EnterReadLock();
            
            try
            {
                result = _data[index];
            }
            finally
            {
                _accessLock.ExitReadLock();
            }
            
            return result;
        }
    }
}