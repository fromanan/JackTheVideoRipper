namespace JackTheVideoRipper.models.enums;

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
    Static      = Title | MediaType | Url | Path,
    Dynamic     = Status | Size | Progress | Speed | Eta,
    All         = Status | Size | Progress | Speed | Eta | Title | MediaType | Url | Path,
}