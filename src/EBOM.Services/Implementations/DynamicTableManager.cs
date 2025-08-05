using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EBOM.Core.Interfaces;
using EBOM.Data;
using System.Text;

namespace EBOM.Services.Implementations;

public class DynamicTableManager : IDynamicTableManager
{
    private readonly EbomDbContext _context;
    private readonly ILogger<DynamicTableManager> _logger;

    public DynamicTableManager(EbomDbContext context, ILogger<DynamicTableManager> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> CreateDynamicTableAsync(int templateRevisionId, string entityType, string entityName, int revisionNumber)
    {
        var tableName = $"data_{entityType}_{entityName}_{revisionNumber:0000}";
        
        try
        {
            // Check if table already exists
            if (await TableExistsAsync(tableName))
            {
                _logger.LogWarning($"Table {tableName} already exists");
                return tableName;
            }

            // Get column definitions from EntityDependencyDefinition
            var columns = await _context.EntityDependencyDefinitions
                .Where(edd => edd.TemplateRevisionID == templateRevisionId)
                .OrderBy(edd => edd.EntityOrder)
                .ToListAsync();

            if (!columns.Any())
            {
                throw new InvalidOperationException($"No columns found for template revision {templateRevisionId}");
            }

            // Build CREATE TABLE statement
            var createTableSql = new StringBuilder();
            createTableSql.AppendLine($"CREATE TABLE [{tableName}] (");
            createTableSql.AppendLine("    [ID] INT IDENTITY(1,1) PRIMARY KEY,");
            createTableSql.AppendLine("    [DataRevisionID] INT NOT NULL,");
            createTableSql.AppendLine("    [RowNumber] INT NOT NULL,");
            
            // Add dynamic columns based on definitions
            foreach (var column in columns)
            {
                var sqlDataType = GetSqlDataType("text"); // Default to text for now
                createTableSql.AppendLine($"    [Column_{column.EntityDependencyID}] {sqlDataType},");
            }
            
            // Add audit columns
            createTableSql.AppendLine("    [CreatedBy] INT NOT NULL,");
            createTableSql.AppendLine("    [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),");
            createTableSql.AppendLine("    [UpdatedBy] INT NULL,");
            createTableSql.AppendLine("    [UpdatedDate] DATETIME2 NULL,");
            
            // Add indexes
            createTableSql.AppendLine("    INDEX IX_DataRevisionID (DataRevisionID),");
            createTableSql.AppendLine("    INDEX IX_RowNumber (RowNumber)");
            createTableSql.AppendLine(");");

            // Execute CREATE TABLE
            await _context.Database.ExecuteSqlRawAsync(createTableSql.ToString());

            // Add foreign key constraint
            var addForeignKeySql = $@"
                ALTER TABLE [{tableName}] 
                ADD CONSTRAINT FK_{tableName}_DataRevisionID 
                FOREIGN KEY (DataRevisionID) 
                REFERENCES EntityDataRevision(DataRevisionID);";
            
            await _context.Database.ExecuteSqlRawAsync(addForeignKeySql);

            _logger.LogInformation($"Successfully created dynamic table: {tableName}");
            return tableName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating dynamic table {tableName}");
            throw;
        }
    }

    public async Task<bool> TableExistsAsync(string tableName)
    {
        var sql = @"
            SELECT COUNT(*) 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = @p0 
            AND TABLE_SCHEMA = 'dbo'";

        var result = await _context.Database
            .SqlQueryRaw<int>(sql, tableName)
            .FirstOrDefaultAsync();

        return result > 0;
    }

    public async Task DropTableAsync(string tableName)
    {
        try
        {
            if (!await TableExistsAsync(tableName))
            {
                _logger.LogWarning($"Table {tableName} does not exist");
                return;
            }

            // Drop foreign key constraint first
            var dropForeignKeySql = $@"
                IF EXISTS (
                    SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                    WHERE CONSTRAINT_NAME = 'FK_{tableName}_DataRevisionID'
                )
                ALTER TABLE [{tableName}] DROP CONSTRAINT FK_{tableName}_DataRevisionID;";
            
            await _context.Database.ExecuteSqlRawAsync(dropForeignKeySql);

            // Drop table
            var dropTableSql = $"DROP TABLE [{tableName}];";
            await _context.Database.ExecuteSqlRawAsync(dropTableSql);

            _logger.LogInformation($"Successfully dropped dynamic table: {tableName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error dropping dynamic table {tableName}");
            throw;
        }
    }

    public async Task<List<string>> GetDynamicTablesForEntityAsync(int entityId)
    {
        var sql = @"
            SELECT TABLE_NAME 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_SCHEMA = 'dbo' 
            AND TABLE_NAME LIKE 'data_%'
            AND TABLE_NAME LIKE @p0
            ORDER BY TABLE_NAME";

        // Get entity details to build pattern
        var entity = await _context.Entities.FindAsync(entityId);
        if (entity == null)
        {
            return new List<string>();
        }

        var pattern = $"data_{entity.EntityType}_{entity.EntityName}_%";
        var tables = await _context.Database
            .SqlQueryRaw<string>(sql, pattern)
            .ToListAsync();

        return tables;
    }

    private string GetSqlDataType(string dataType)
    {
        return dataType?.ToLower() switch
        {
            "text" => "NVARCHAR(MAX)",
            "number" => "DECIMAL(18,4)",
            "int" => "INT",
            "decimal" => "DECIMAL(18,4)",
            "date" => "DATE",
            "datetime" => "DATETIME2",
            "boolean" => "BIT",
            "guid" => "UNIQUEIDENTIFIER",
            _ => "NVARCHAR(MAX)"
        };
    }
}