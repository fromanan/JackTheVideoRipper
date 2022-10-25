﻿namespace JackTheVideoRipper;

public readonly struct ProcessError
{
    public readonly string ProcessTag;
    public readonly Exception Exception;
    public readonly DateTime Timestamp;
    public readonly string AdditionalDetails;

    public ProcessError(ProcessUpdateRow processUpdateRow, Exception exception, string additionalDetails = "")
    {
        ProcessTag = processUpdateRow.Tag;
        Exception = exception;
        Timestamp = DateTime.Now;
        AdditionalDetails = additionalDetails;
    }
    
    public ProcessError(ListViewItem listViewItem, Exception exception, string additionalDetails = "")
    {
        ProcessTag = listViewItem.Tag.ToString() ?? "";
        Exception = exception;
        Timestamp = DateTime.Now;
        AdditionalDetails = additionalDetails;
    }
    
    public ProcessError(string processTag, Exception exception, string additionalDetails = "")
    {
        ProcessTag = processTag;
        Exception = exception;
        Timestamp = DateTime.Now;
        AdditionalDetails = additionalDetails;
    }
}