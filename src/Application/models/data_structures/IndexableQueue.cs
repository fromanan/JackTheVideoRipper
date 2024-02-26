namespace JackTheVideoRipper.models.data_structures;

public class IndexableQueue<T> : List<T>
{
    private readonly ReaderWriterLockSlim _accessLock = new();

    public int Length
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
    
    public T Dequeue()
    {
        T nextProcess = Peek();
        Pop();
        return nextProcess;
    }

    public T Peek()
    {
        T nextProcess;
        
        _accessLock.EnterReadLock();
        try
        {
            nextProcess = this.First();
        }
        finally
        {
            _accessLock.ExitReadLock();
        }

        return nextProcess;
    }

    public void Pop()
    {
        _accessLock.EnterWriteLock();
        try
        {
            RemoveAt(0);
        }
        finally
        {
            _accessLock.ExitWriteLock();
        }
    }

    public bool Empty()
    {
        return Length is 0;
    }
}