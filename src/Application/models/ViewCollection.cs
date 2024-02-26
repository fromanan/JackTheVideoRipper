using JackTheVideoRipper.interfaces;
using JackTheVideoRipper.models.enums;

namespace JackTheVideoRipper.models;

public class ViewCollection
{
    private readonly Dictionary<ViewField, IViewSubItem> _viewFieldTable;

    public ViewCollection(IViewItem viewItem)
    {
        _viewFieldTable = new Dictionary<ViewField, IViewSubItem>
        {
            { ViewField.Title,      viewItem.SubItems[0] },
            { ViewField.Status,     viewItem.SubItems[1] },
            { ViewField.MediaType,  viewItem.SubItems[2] },
            { ViewField.Size,       viewItem.SubItems[3] },
            { ViewField.Progress,   viewItem.SubItems[4] },
            { ViewField.Speed,      viewItem.SubItems[5] },
            { ViewField.Eta,        viewItem.SubItems[6] },
            { ViewField.Url,        viewItem.SubItems[7] },
            { ViewField.Path,       viewItem.SubItems[8] }
        };
    }

    public string this[ViewField key]
    {
        get => _viewFieldTable[key].Text.Trim();
        set => _viewFieldTable[key].Text = value;
    }
}