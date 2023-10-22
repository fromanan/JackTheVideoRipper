namespace JackTheVideoRipper.models.rows;

public record RowUpdateArgs(string? Status = null, string? MediaType = null, string? FileSize = null,
    string? Progress = null, string? Speed = null, string? Eta = null, string? Url = null, string? Path = null);