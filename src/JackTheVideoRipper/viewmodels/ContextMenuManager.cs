using JackTheVideoRipper.models.rows;

namespace JackTheVideoRipper.models;

public class ContextMenuManager
{
    private readonly ContextMenuStrip _contextMenuListItems;
    
    public ContextMenuManager(ContextMenuStrip contextMenuListItems)
    {
        _contextMenuListItems = contextMenuListItems;
    }
    
    private ToolStripItemCollection ContextItems => _contextMenuListItems.Items;

    // All of the names listed here will conditionally be shown, depending on the process status
    // All others will be visible by default
    // NOTE: These names must match EXACTLY what is in FrameMain, otherwise it will cause exceptions
    private static readonly Dictionary<string, ProcessStatus> _ContextItemsDict = new()
    {
        { "retryProcessToolStripMenuItem",      ProcessStatus.Error },
        { "stopProcessToolStripMenuItem",       ProcessStatus.Running },
        { "openFolderToolStripMenuItem",        ProcessStatus.Succeeded},
        { "deleteFromDiskToolStripMenuItem",    ProcessStatus.Succeeded },
        { "openInMediaPlayerToolStripMenuItem", ProcessStatus.Succeeded },
        { "resumeProcessToolStripMenuItem",     ProcessStatus.Paused },
        { "pauseProcessToolStripMenuItem",      ProcessStatus.Running },
        { "reprocessMediaToolStripMenuItem",    ProcessStatus.Completed }
    };

    public async Task OpenContextMenu()
    {
        bool isDownload = Ripper.Instance.SelectedIsType<DownloadProcessUpdateRow>();
        await Parallel.ForEachAsync(_ContextItemsDict, SetContextVisibility);
        ShowContextItem("removeRowToolStripMenuItem");
        SetContextVisibility("openUrlInBrowserToolStripMenuItem", value:isDownload);
        SetContextVisibility("copyUrlToolStripMenuItem", value:isDownload);
        ShowContextMenu();
    }

    private void ShowContextMenu()
    {
        _contextMenuListItems.Show(Cursor.Position);
    }
    
    private void ShowContextItem(string name)
    {
        SetContextVisibility(name);
    }

    private void HideContextItem(string name)
    {
        SetContextVisibility(name, value:false);
    }

    private async ValueTask SetContextVisibility(KeyValuePair<string, ProcessStatus> keyValuePair,
        CancellationToken token)
    {
        await Threading.RunInMainContext(() => SetContextVisibility(keyValuePair.Key, keyValuePair.Value));
    }

    private void SetContextVisibility(string name, ProcessStatus? processStatus = null, bool value = true)
    {
        ContextItems[name].Visible = ValueIfStatus(processStatus, value);
    }
    
    private static bool ValueIfStatus(ProcessStatus? processStatus = null, bool value = true)
    {
        if (processStatus is null)
            return value;
        
        return Ripper.Instance.GetSelectedStatus() == processStatus ? value : !value;
    }
}