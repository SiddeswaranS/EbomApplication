using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using EBOM.Core.Entities;
using EBOM.Core.Interfaces;
using EBOM.Core.Models;
using EBOM.Data;
using System.Data;
using System.Text;

namespace EBOM.Services.Implementations;

public class DataProcessor : IDataProcessor
{
    private readonly EbomDbContext _context;
    private readonly IDynamicTableManager _dynamicTableManager;
    private readonly IValidationService _validationService;
    private readonly ILogger<DataProcessor> _logger;

    public DataProcessor(
        EbomDbContext context,
        IDynamicTableManager dynamicTableManager,
        IValidationService validationService,
        ILogger<DataProcessor> logger)
    {
        _context = context;
        _dynamicTableManager = dynamicTableManager;
        _validationService = validationService;
        _logger = logger;
    }

    public async Task<DataProcessingResult> ProcessDataFileAsync(Stream fileStream, string fileName, int entityId, int userId)
    {
        var result = new DataProcessingResult
        {
            FileName = fileName,
            EntityId = entityId
        };

        try
        {
            // Validate file extension
            if (!IsValidDataFile(fileName))
            {
                result.Errors.Add(new ProcessingError
                {
                    Message = "Invalid file format. Only Excel files (.xlsx, .xls) are supported.",
                    Details = $"File: {fileName}"
                });
                return result;
            }

            // Load Excel workbook
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(fileStream);
            
            return await ProcessDataFromExcelAsync(package, fileName, entityId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing data file {fileName} for entity {entityId}");
            result.Errors.Add(new ProcessingError
            {
                Message = "Error processing data file",
                Details = ex.Message
            });
            return result;
        }
    }

    public async Task<DataProcessingResult> ProcessDataFromExcelAsync(object workbook, string fileName, int entityId, int userId)
    {
        var result = new DataProcessingResult
        {
            FileName = fileName,
            EntityId = entityId
        };

        try
        {
            var package = workbook as ExcelPackage;
            if (package == null)
            {
                result.Errors.Add(new ProcessingError
                {
                    Message = "Invalid workbook object",
                    Details = "Expected ExcelPackage object"
                });
                return result;
            }

            // Get entity and active template
            var entity = await _context.Entities
                .FirstOrDefaultAsync(e => e.EntityID == entityId);

            if (entity == null)
            {
                result.Errors.Add(new ProcessingError
                {
                    Message = "Entity not found",
                    Details = $"EntityId: {entityId}"
                });
                return result;
            }

            // Get active template revision for this entity
            var activeTemplate = await _context.EntityTemplateRevisions
                .FirstOrDefaultAsync(etr => etr.EntityID == entityId && etr.IsActive);
            
            if (activeTemplate == null)
            {
                result.Errors.Add(new ProcessingError
                {
                    Message = "No active template found for entity",
                    Details = $"Entity: {entity.EntityName}"
                });
                return result;
            }

            // Process data sheets
            var allData = new List<Dictionary<string, object>>();
            var sheetNames = new List<string>();

            foreach (var worksheet in package.Workbook.Worksheets)
            {
                if (worksheet.Name.StartsWith("Data", StringComparison.OrdinalIgnoreCase))
                {
                    sheetNames.Add(worksheet.Name);
                    var sheetData = ExtractDataFromWorksheet(worksheet, new List<EntityDependencyDefinition>());
                    allData.AddRange(sheetData);
                }
            }

            if (!allData.Any())
            {
                result.Errors.Add(new ProcessingError
                {
                    Message = "No data sheets found or no data extracted",
                    Details = "Expected sheets starting with 'Data'"
                });
                return result;
            }

            // Validate data against template
            if (!await ValidateDataAgainstTemplateAsync(entityId, allData))
            {
                result.Errors.Add(new ProcessingError
                {
                    Message = "Data validation failed against template",
                    Details = "Data structure does not match template requirements"
                });
                return result;
            }

            // Create data revision
            var dataRevisionId = await CreateDataRevisionAsync(entityId, userId, fileName);
            result.DataRevisionId = dataRevisionId;

            // Create or get dynamic table
            var tableName = $"data_{entity.EntityType}_{entity.EntityName}_{activeTemplate.TemplateRevisionNumber:0000}";
            
            if (!await _dynamicTableManager.TableExistsAsync(tableName))
            {
                await _dynamicTableManager.CreateDynamicTableAsync(
                    activeTemplate.TemplateRevisionID, 
                    entity.EntityType, 
                    entity.EntityName, 
                    activeTemplate.TemplateRevisionNumber);
            }

            // Bulk insert data
            await BulkInsertDataAsync(tableName, dataRevisionId, allData, userId);

            result.IsSuccess = true;
            result.RowsProcessed = allData.Count;
            result.Metadata["SheetsProcessed"] = sheetNames;
            result.Metadata["TableName"] = tableName;

            _logger.LogInformation($"Successfully processed {allData.Count} rows from {fileName} for entity {entityId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing Excel data for entity {entityId}");
            result.Errors.Add(new ProcessingError
            {
                Message = "Error processing Excel data",
                Details = ex.Message
            });
        }

        return result;
    }

    public async Task<bool> ValidateDataAgainstTemplateAsync(int entityId, List<Dictionary<string, object>> data)
    {
        try
        {
            var entity = await _context.Entities.FirstOrDefaultAsync(e => e.EntityID == entityId);
            if (entity == null) return false;

            // For now, just return true as basic validation
            // This can be enhanced with actual template validation logic
            return data.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error validating data against template for entity {entityId}");
            return false;
        }
    }

    public async Task<int> CreateDataRevisionAsync(int entityId, int userId, string fileName)
    {
        var dataRevision = new EntityDataRevision
        {
            EntityId = entityId,
            DataRevisionNumber = await GetNextDataRevisionNumberAsync(entityId),
            DataRevisionDescription = fileName,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.EntityDataRevisions.Add(dataRevision);
        await _context.SaveChangesAsync();

        return dataRevision.DataRevisionId;
    }

    public async Task BulkInsertDataAsync(string tableName, int dataRevisionId, List<Dictionary<string, object>> data, int userId)
    {
        if (!data.Any()) return;

        try
        {
            var columns = data.First().Keys.ToList();
            var insertSql = new StringBuilder();
            
            insertSql.AppendLine($"INSERT INTO [{tableName}] (");
            insertSql.AppendLine("    [DataRevisionID], [RowNumber], [CreatedBy], [CreatedDate],");
            insertSql.AppendLine($"    [{string.Join("], [", columns)}]");
            insertSql.AppendLine(") VALUES");

            var valuesClauses = new List<string>();
            for (int i = 0; i < data.Count; i++)
            {
                var row = data[i];
                var values = new List<string>
                {
                    dataRevisionId.ToString(),
                    (i + 1).ToString(),
                    userId.ToString(),
                    "GETUTCDATE()"
                };

                foreach (var column in columns)
                {
                    var value = row.ContainsKey(column) ? row[column] : null;
                    values.Add(value == null ? "NULL" : $"'{value.ToString().Replace("'", "''")}'");
                }

                valuesClauses.Add($"({string.Join(", ", values)})");
            }

            insertSql.AppendLine(string.Join(",\n", valuesClauses));

            await _context.Database.ExecuteSqlRawAsync(insertSql.ToString());
            
            _logger.LogInformation($"Bulk inserted {data.Count} rows into {tableName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error bulk inserting data into {tableName}");
            throw;
        }
    }

    public async Task<DataValidationResult> ValidateDataRowAsync(int entityId, Dictionary<string, object> row, int rowNumber)
    {
        var result = new DataValidationResult { IsValid = true };
        
        try
        {
            var entity = await _context.Entities.FirstOrDefaultAsync(e => e.EntityID == entityId);
            if (entity == null)
            {
                result.IsValid = false;
                result.Errors.Add("Entity not found for validation");
                return result;
            }

            // Basic validation - just check if row has data
            if (!row.Any())
            {
                result.IsValid = false;
                result.Errors.Add($"Row {rowNumber}: No data provided");
            }

            // Copy normalized data
            foreach (var kvp in row)
            {
                result.NormalizedData[kvp.Key] = kvp.Value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error validating data row {rowNumber} for entity {entityId}");
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
        }

        return result;
    }

    public async Task<DataSummary> GetDataSummaryAsync(string entityName)
    {
        var entity = await _context.Entities
            .FirstOrDefaultAsync(e => e.EntityName == entityName);

        if (entity == null)
        {
            return new DataSummary { EntityName = entityName, TotalRows = 0 };
        }

        // Get dynamic table names for this entity
        var tableNames = await _dynamicTableManager.GetDynamicTablesForEntityAsync(entity.EntityID);
        var totalRows = 0;

        foreach (var tableName in tableNames)
        {
            var countSql = $"SELECT COUNT(*) FROM [{tableName}]";
            var count = await _context.Database.SqlQueryRaw<int>(countSql).FirstOrDefaultAsync();
            totalRows += count;
        }

        return new DataSummary
        {
            EntityName = entityName,
            TotalRows = totalRows,
            ActiveRevision = 1,
            LastUpload = DateTime.UtcNow
        };
    }

    public async Task<List<DataRevisionInfo>> GetDataRevisionsAsync(string entityName)
    {
        var revisions = await _context.EntityDataRevisions
            .Include(edr => edr.Entity)
            .Where(edr => edr.Entity.EntityName == entityName)
            .OrderByDescending(edr => edr.DataRevisionNumber)
            .Select(edr => new DataRevisionInfo
            {
                DataRevisionId = edr.DataRevisionId,
                DataRevisionNumber = edr.DataRevisionNumber,
                Description = edr.DataRevisionDescription,
                CreatedAt = edr.CreatedAt,
                CreatedBy = "Admin", // TODO: Get actual user name
                RowCount = 0 // TODO: Get actual row count from dynamic table
            })
            .ToListAsync();

        return revisions;
    }

    private List<Dictionary<string, object>> ExtractDataFromWorksheet(ExcelWorksheet worksheet, List<EntityDependencyDefinition> columnDefinitions)
    {
        var data = new List<Dictionary<string, object>>();
        
        if (worksheet.Dimension == null) return data;

        // Get header row (assume row 1)
        var headers = new Dictionary<int, string>();
        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
        {
            var headerValue = worksheet.Cells[1, col].Value?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(headerValue))
            {
                headers[col] = headerValue;
            }
        }

        // Extract data rows
        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
        {
            var rowData = new Dictionary<string, object>();
            bool hasData = false;

            foreach (var header in headers)
            {
                var cellValue = worksheet.Cells[row, header.Key].Value;
                if (cellValue != null)
                {
                    rowData[header.Value] = cellValue;
                    hasData = true;
                }
            }

            if (hasData)
            {
                data.Add(rowData);
            }
        }

        return data;
    }

    private async Task<int> GetNextDataRevisionNumberAsync(int entityId)
    {
        var maxRevision = await _context.EntityDataRevisions
            .Where(edr => edr.EntityId == entityId)
            .MaxAsync(edr => (int?)edr.DataRevisionNumber) ?? 0;

        return maxRevision + 1;
    }

    private bool IsValidDataFile(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension == ".xlsx" || extension == ".xls";
    }

    private bool ValidateDataType(object value, string expectedType)
    {
        if (value == null) return true;

        return expectedType?.ToLower() switch
        {
            "text" => true, // Any value can be converted to text
            "number" or "decimal" => double.TryParse(value.ToString(), out _),
            "int" => int.TryParse(value.ToString(), out _),
            "date" => DateTime.TryParse(value.ToString(), out _),
            "datetime" => DateTime.TryParse(value.ToString(), out _),
            "boolean" => bool.TryParse(value.ToString(), out _) || 
                        value.ToString() == "1" || value.ToString() == "0",
            _ => true
        };
    }

    private object NormalizeValue(object value, string dataType)
    {
        if (value == null) return null;

        return dataType?.ToLower() switch
        {
            "number" or "decimal" => double.TryParse(value.ToString(), out var d) ? d : value,
            "int" => int.TryParse(value.ToString(), out var i) ? i : value,
            "date" or "datetime" => DateTime.TryParse(value.ToString(), out var dt) ? dt : value,
            "boolean" => bool.TryParse(value.ToString(), out var b) ? b : 
                        (value.ToString() == "1" ? true : value.ToString() == "0" ? false : value),
            _ => value.ToString()
        };
    }
}