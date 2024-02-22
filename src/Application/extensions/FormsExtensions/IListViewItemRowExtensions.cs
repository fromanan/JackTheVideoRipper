﻿using JackTheVideoRipper.interfaces;

namespace JackTheVideoRipper.extensions;

public static class IListViewItemRowExtensions
{
    public static IViewItemEnumerable SelectViewItems(this IEnumerable<IListViewItemRow> listViewItemRows)
    {
        return listViewItemRows.Select(v => v.ViewItem);
    }
}