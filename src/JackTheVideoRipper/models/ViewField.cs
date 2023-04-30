namespace JackTheVideoRipper.models;

[Flags]
public enum ViewField
{
    None        = 0,
    Status      = 1<<0,
    Size        = 1<<1,
    Progress    = 1<<2,
    Speed       = 1<<3,
    Eta         = 1<<4,
    Title       = 1<<5,
    MediaType   = 1<<6,
    Url         = 1<<7,
    Path        = 1<<8,
    Static      = (1<<5) | (1<<6) | (1<<7) | (1<<8),
    Dynamic     = (1<<0) | (1<<1) | (1<<2) | (1<<3) | (1<<4),
    All         = (1<<0) | (1<<1) | (1<<2) | (1<<3) | (1<<4) | (1<<5) | (1<<6) | (1<<7) | (1<<8),
}