using System.Collections;
using JackTheVideoRipper.interfaces;

namespace JackTheVideoRipper.models.DataStructures;

public class ConcurrentList<T> : IReadOnlyList<T> where T : IClaimable, new()
{
    #region Data Members

    protected readonly ReaderWriterLockSlim AccessLock = new();
    
    protected readonly List<T> Data;

    #endregion

    #region Constructors

    public ConcurrentList()
    {
        Data = new List<T>();
    }
    
    public ConcurrentList(int size)
    {
        Data = Enumerable.Range(0, size).Select(_ => new T()).ToList();
    }

    #endregion

    #region Public Methods

    public IEnumerable<T> Where(Func<T, bool> predicate)
    {
        IEnumerable<T> result;

        AccessLock.EnterReadLock();
            
        try
        {
            result = Data.Where(predicate);
        }
        finally
        {
            AccessLock.ExitReadLock();
        }
            
        return result;
    }

    public T First(Func<T, bool> predicate)
    {
        T result;

        AccessLock.EnterReadLock();
            
        try
        {
            result = Data.First(predicate);
        }
        finally
        {
            AccessLock.ExitReadLock();
        }
            
        return result;
    }

    #endregion

    #region IEnumerable Implementation

    public IEnumerator<T> GetEnumerator()
    {
        List<T>.Enumerator result;

        AccessLock.EnterReadLock();
            
        try
        {
            result = Data.GetEnumerator();
        }
        finally
        {
            AccessLock.ExitReadLock();
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
            
            AccessLock.EnterReadLock();
            try
            {
                length = Count;
            }
            finally
            {
                AccessLock.ExitReadLock();
            }
            
            return length;
        }
    }

    public T this[int index]
    {
        get
        {
            T result;

            AccessLock.EnterReadLock();
            
            try
            {
                result = Data[index];
            }
            finally
            {
                AccessLock.ExitReadLock();
            }
            
            return result;
        }
    }
}