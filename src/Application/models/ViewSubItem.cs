using JackTheVideoRipper.interfaces;

namespace JackTheVideoRipper.models;

public readonly struct ViewSubItem : IViewSubItem
{
    public ListViewItem.ListViewSubItem ListViewSubItem { get; }

    public ViewSubItem(ListViewItem.ListViewSubItem listViewSubItem)
    {
        ListViewSubItem = listViewSubItem;
    }

    public string Text
    {
        get => ListViewSubItem.Text;
        set => ListViewSubItem.Text = value;
    }
}