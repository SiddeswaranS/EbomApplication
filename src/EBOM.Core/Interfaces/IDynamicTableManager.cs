namespace EBOM.Core.Interfaces;

public interface IDynamicTableManager
{
    Task<string> CreateDynamicTableAsync(int templateRevisionId, string entityType, string entityName, int revisionNumber);
    Task<bool> TableExistsAsync(string tableName);
    Task DropTableAsync(string tableName);
    Task<List<string>> GetDynamicTablesForEntityAsync(int entityId);
}