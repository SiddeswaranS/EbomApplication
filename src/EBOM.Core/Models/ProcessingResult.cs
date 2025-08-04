namespace EBOM.Core.Models;

public class ProcessingResult
{
    public bool IsSuccess { get; set; }
    public List<ProcessingError> Errors { get; set; } = new();
    public List<ProcessingWarning> Warnings { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ProcessingError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? Source { get; set; }
    public int? LineNumber { get; set; }
    public string? ColumnName { get; set; }
}

public class ProcessingWarning
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Suggestion { get; set; }
}