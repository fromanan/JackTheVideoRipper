using JackTheVideoRipper.models.rows;

namespace JackTheVideoRipper.models;

public class ContextMenuManager
{
    private readonly ContextMenuStrip _contextMenuListItems;
    
    public ContextMenuManager(ContextMenuStrip contextMenuListItems)
    {
        _contextMenuListItems = contextMenuListItems;
    }

    public static class ContextPaths
    {
        // File Menu
        public const string OPEN_FOLDER             =   "fileContextMenuItem/openFolderToolStripMenuItem";
        public const string OPEN_IN_MEDIA_PLAYER    =   "fileContextMenuItem/openInMediaPlayerToolStripMenuItem";
        public const string OPEN_IN_BROWSER         =   "fileContextMenuItem/openUrlInBrowserToolStripMenuItem";
        
        // Edit Menu
        public const string COPY_URL                =   "editContextMenuItem/copyUrlToolStripMenuItem";
        
        // Process Menu
        public const string RETRY_PROCESS           =   "processContextMenuItem/retryProcessToolStripMenuItem";
        public const string STOP_PROCESS            =   "processContextMenuItem/stopProcessToolStripMenuItem";
        public const string RESUME_PROCESS          =   "processContextMenuItem/resumeProcessToolStripMenuItem";
        public const string PAUSE_PROCESS           =   "processContextMenuItem/pauseProcessToolStripMenuItem";
        public const string PROCESS_MENU            =   "processContextMenuItem";
        
        // Result Menu
        public const string MOVE                    =   "resultContextMenuItem/moveToolStripMenuItem";
        public const string RENAME                  =   "resultContextMenuItem/renameToolStripMenuItem";
        public const string CONVERT                 =   "resultContextMenuItem/convertToolStripMenuItem";
        public const string REPROCESS               =   "resultContextMenuItem/reprocessMediaToolStripMenuItem";
        public const string DELETE                  =   "resultContextMenuItem/deleteFromDiskToolStripMenuItem";
        public const string RESULT_MENU             =   "resultContextMenuItem";

        // Miscellaneous
        public const string REMOVE_ROW              =   "removeRowToolStripMenuItem";
    }
    
    private ToolStripItemCollection ContextItems => _contextMenuListItems.Items;

    // All of the names listed here will conditionally be shown, depending on the process status
    // All others will be visible by default
    // NOTE: These names must match EXACTLY what is in FrameMain, otherwise it will cause exceptions
    private static readonly Dictionary<string, ProcessStatus> _ContextItemsDict = new()
    {
        // File Menu
        { ContextPaths.OPEN_FOLDER,             ProcessStatus.Succeeded },
        { ContextPaths.OPEN_IN_MEDIA_PLAYER,    ProcessStatus.Succeeded },
        
        // Process Menu
        { ContextPaths.RETRY_PROCESS,           ProcessStatus.Error     },
        { ContextPaths.STOP_PROCESS,            ProcessStatus.Running   },
        { ContextPaths.RESUME_PROCESS,          ProcessStatus.Paused    },
        { ContextPaths.PAUSE_PROCESS,           ProcessStatus.Running   },
        { ContextPaths.PROCESS_MENU,            ProcessStatus.Running   },
        
        // Result Menu
        { ContextPaths.MOVE,                    ProcessStatus.Succeeded },
        { ContextPaths.RENAME,                  ProcessStatus.Succeeded },
        { ContextPaths.CONVERT,                 ProcessStatus.Succeeded },
        { ContextPaths.REPROCESS,               ProcessStatus.Succeeded },
        { ContextPaths.DELETE,                  ProcessStatus.Succeeded },
        { ContextPaths.RESULT_MENU,             ProcessStatus.Succeeded },
    };

    public async Task OpenContextMenu()
    {
        bool isDownload = Ripper.Instance.SelectedIsType<DownloadProcessUpdateRow>();
        await Parallel.ForEachAsync(_ContextItemsDict, SetContextVisibility);
        ShowContextItem(ContextPaths.REMOVE_ROW);
        SetContextVisibility(ContextPaths.OPEN_IN_BROWSER, value:isDownload);
        SetContextVisibility(ContextPaths.COPY_URL, value:isDownload);
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
        ToolStripItem? contextItem;
        
        if (name.Contains('/'))
        {
            string[] menuNames = name.Split('/');
            (string parentMenu, string childMenu) = (menuNames[0], menuNames[1]);

            if (ContextItems[parentMenu] is not ToolStripMenuItem parentToolStripMenuItem)
                throw new DeveloperException($"Parent toolstrip menu '{parentMenu}' does not exist or could not be found! (full name: '{name}')");

            contextItem = parentToolStripMenuItem.DropDownItems[childMenu];
        }
        else
        {
            contextItem = ContextItems[name];
        }

        if (contextItem is null)
            throw new DeveloperException($"Context menu item '{name}' does not exist or could not be found!");
        
        contextItem.Visible = ValueIfStatus(processStatus, value);
    }
    
    private static bool ValueIfStatus(ProcessStatus? processStatus = null, bool value = true)
    {
        if (processStatus is null)
            return value;

        if (Ripper.Instance.GetSelectedStatus() is not { } selectedStatus)
            return false;
        
        return processStatus.Value.HasFlag(selectedStatus) ? value : !value;
    }
}