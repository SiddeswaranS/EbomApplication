namespace EBOM.Common.Models.Configuration;

public class EbomSettings
{
    public string SeparatorColumn { get; set; } = string.Empty;
    public List<string> ExcludedColumns { get; set; } = new();
    public List<string> SupportedFileExtensions { get; set; } = new();
    public long MaxFileSize { get; set; }
    public int BatchSize { get; set; }
    public DataValidationSettings DataValidation { get; set; } = new();
}

public class DataValidationSettings
{
    public int MaxSampleRows { get; set; }
    public RequiredSheetsSettings RequiredSheets { get; set; } = new();
}

public class RequiredSheetsSettings
{
    public string DataSheetPattern { get; set; } = string.Empty;
    public string ConfigurationSheet { get; set; } = string.Empty;
}