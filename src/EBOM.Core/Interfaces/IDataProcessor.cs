using EBOM.Core.Models;

namespace EBOM.Core.Interfaces;

public interface IDataProcessor
{
    Task<DataProcessingResult> ProcessDataFileAsync(Stream fileStream, string fileName, int entityId, int userId);
    Task<DataProcessingResult> ProcessDataFromExcelAsync(object workbook, string fileName, int entityId, int userId);
    Task<bool> ValidateDataAgainstTemplateAsync(int entityId, List<Dictionary<string, object>> data);
    Task<int> CreateDataRevisionAsync(int entityId, int userId, string fileName);
    Task BulkInsertDataAsync(string tableName, int dataRevisionId, List<Dictionary<string, object>> data, int userId);
    Task<DataValidationResult> ValidateDataRowAsync(int entityId, Dictionary<string, object> row, int rowNumber);
    Task<DataSummary> GetDataSummaryAsync(string entityName);
    Task<List<DataRevisionInfo>> GetDataRevisionsAsync(string entityName);
}

public class DataProcessingResult
{
    public bool IsSuccess { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int DataRevisionId { get; set; }
    public int RowsProcessed { get; set; }
    public int RowsSkipped { get; set; }
    public List<ProcessingError> Errors { get; set; } = new();
    public List<ProcessingWarning> Warnings { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class DataValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> NormalizedData { get; set; } = new();
}