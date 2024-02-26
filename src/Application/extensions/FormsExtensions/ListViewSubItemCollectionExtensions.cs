using static System.Windows.Forms.ListViewItem;

namespace JackTheVideoRipper.extensions;

public static class ListViewSubItemCollectionExtensions
{
    public static IEnumerable<ListViewSubItem> ToEnumerable(this ListViewSubItemCollection listViewSubItemCollection)
    {
        return Enumerable.Cast<ListViewSubItem>(listViewSubItemCollection);
    }
}