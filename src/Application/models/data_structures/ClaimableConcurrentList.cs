using JackTheVideoRipper.framework;
using JackTheVideoRipper.interfaces;

namespace JackTheVideoRipper.models.data_structures;

public class ClaimableConcurrentList<T> : ConcurrentList<T> where T : IClaimable, new()
{
    #region Constructors

    public ClaimableConcurrentList() { }
    
    public ClaimableConcurrentList(int size) : base(size) { }

    #endregion
    
    public async Task<T> Next(Func<T, bool> predicate, int tickInMilliseconds)
    {
        await Tasks.WaitUntil(Where(predicate).Any, tickInMilliseconds);
        
        T result = First(predicate);

        AccessLock.EnterWriteLock();
        
        try
        {
            result.Claimed = true;
        }
        finally
        {
            AccessLock.ExitWriteLock();
        }
        
        return result;
    }
}