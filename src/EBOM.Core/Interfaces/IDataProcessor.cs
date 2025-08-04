using EBOM.Core.Models;

namespace EBOM.Core.Interfaces;

public interface IDataProcessor
{
    Task<ProcessingResult> ProcessDataAsync(Stream fileStream, string fileName, int userId);
    Task<DataSummary> GetDataSummaryAsync(string entityName);
    Task<List<DataRevisionInfo>> GetDataRevisionsAsync(string entityName);
}