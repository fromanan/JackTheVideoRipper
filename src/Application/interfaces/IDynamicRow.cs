using JackTheVideoRipper.models.processes;

namespace JackTheVideoRipper.interfaces;

/// <summary>
/// Defines a row that is updated by the view
/// </summary>
public interface IDynamicRow : IListViewItemRow
{
    Task<ProcessUpdateArgs> Update();
}