using JackTheVideoRipper.interfaces;
using static System.Windows.Forms.ListView;

namespace JackTheVideoRipper.extensions;

public static class ListViewItemCollectionExtensions
{
    public static void AddRange(this ListViewItemCollection itemCollection, IEnumerable<ListViewItem> items)
    {
        foreach (ListViewItem item in items)
        {
            itemCollection.Add(item);
        }
    }
    
    public static void RemoveRange(this ListViewItemCollection itemCollection, IEnumerable<ListViewItem> items)
    {
        foreach (ListViewItem item in items)
        {
            itemCollection.Remove(item);
        }
    }
    
    public static void AddRange(this ListViewItemCollection itemCollection, IEnumerable<IViewItem> items)
    {
        foreach (IViewItem item in items)
        {
            itemCollection.Add(item.As<ListViewItem>());
        }
    }
    
    public static void RemoveRange(this ListViewItemCollection itemCollection, IEnumerable<IViewItem> items)
    {
        foreach (IViewItem item in items)
        {
            itemCollection.Remove(item.As<ListViewItem>());
        }
    }

    public static IViewItem[] ToArray(this ListViewItemCollection itemCollection)
    {
        IViewItem[] array = new IViewItem[itemCollection.Count];
        itemCollection.CopyTo(array, 0);
        return array;
    }
}