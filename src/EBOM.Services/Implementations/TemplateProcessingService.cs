using EBOM.Core.Interfaces;
using EBOM.Core.Models;
using EBOM.Core.Entities;
using EBOM.Data;
using EBOM.Common.Models.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Data;

namespace EBOM.Services.Implementations;

public class TemplateProcessingService : ITemplateProcessor
{
    private readonly EbomDbContext _context;
    private readonly IValidationService _validationService;
    private readonly EbomSettings _settings;

    public TemplateProcessingService(
        EbomDbContext context,
        IValidationService validationService,
        IOptions<EbomSettings> settings)
    {
        _context = context;
        _validationService = validationService;
        _settings = settings.Value;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<ProcessingResult> ProcessTemplateAsync(Stream fileStream, string fileName, int userId)
    {
        var result = new ProcessingResult();

        try
        {
            // Validate file
            var validationResult = await _validationService.ValidateTemplateFileAsync(fileStream, fileName);
            if (!validationResult.IsValid)
            {
                result.IsSuccess = false;
                result.Errors.AddRange(validationResult.Errors.Select(e => new ProcessingError 
                { 
                    Code = "VALIDATION_ERROR", 
                    Message = e 
                }));
                return result;
            }

            // Extract entity information
            var (entityType, entityName) = ExtractEntityInfo(fileName);
            
            // Load Excel file
            fileStream.Position = 0;
            using var package = new ExcelPackage(fileStream);
            
            // Process configuration sheet
            var configSheet = package.Workbook.Worksheets["Configuration"];
            var mirrorEntities = ExtractMirrorEntities(configSheet);

            // Process data sheets to extract columns
            var columns = await ExtractColumnsAsync(package, entityType);

            // Create or get entity
            var entity = await GetOrCreateEntityAsync(entityName, entityType, userId);

            // Create template revision
            var revision = await CreateTemplateRevisionAsync(entity.EntityID, columns, userId);

            // Process mirror entities
            if (mirrorEntities.Any())
            {
                await ProcessMirrorEntitiesAsync(entity.EntityID, mirrorEntities, userId);
            }

            result.IsSuccess = true;
            result.Metadata["EntityId"] = entity.EntityID;
            result.Metadata["EntityName"] = entityName;
            result.Metadata["EntityType"] = entityType;
            result.Metadata["TemplateRevision"] = revision.TemplateRevisionNumber;
            result.Metadata["ColumnsProcessed"] = columns.Count;

            return result;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Errors.Add(new ProcessingError
            {
                Code = "PROCESSING_ERROR",
                Message = "An error occurred while processing the template",
                Details = ex.Message
            });
            return result;
        }
    }

    public async Task<List<TemplateInfo>> GetActiveTemplatesAsync()
    {
        return await _context.EntityTemplateRevisions
            .Include(r => r.Entity)
            .Where(r => r.IsActive)
            .Select(r => new TemplateInfo
            {
                EntityId = r.EntityID,
                EntityName = r.Entity!.EntityName,
                EntityType = r.Entity.EntityType,
                ActiveRevision = r.TemplateRevisionNumber,
                LastUpdated = r.CreatedAt,
                ColumnCount = r.DependencyDefinitions!.Count
            })
            .ToListAsync();
    }

    public async Task<TemplateInfo?> GetTemplateByEntityAsync(string entityName)
    {
        return await _context.EntityTemplateRevisions
            .Include(r => r.Entity)
            .Where(r => r.Entity!.EntityName == entityName && r.IsActive)
            .Select(r => new TemplateInfo
            {
                EntityId = r.EntityID,
                EntityName = r.Entity!.EntityName,
                EntityType = r.Entity.EntityType,
                ActiveRevision = r.TemplateRevisionNumber,
                LastUpdated = r.CreatedAt,
                ColumnCount = r.DependencyDefinitions!.Count
            })
            .FirstOrDefaultAsync();
    }

    private (string entityType, string entityName) ExtractEntityInfo(string fileName)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var parts = nameWithoutExtension.Split('_');
        var entityType = parts[0];
        var entityName = string.Join("_", parts.Skip(1));
        return (entityType, entityName);
    }

    private List<string> ExtractMirrorEntities(ExcelWorksheet? configSheet)
    {
        var mirrorEntities = new List<string>();
        
        if (configSheet == null)
            return mirrorEntities;

        // Look for MirrorEntity row in configuration
        for (int row = 1; row <= configSheet.Dimension?.Rows; row++)
        {
            var key = configSheet.Cells[row, 1].Value?.ToString();
            if (key == "MirrorEntity")
            {
                var value = configSheet.Cells[row, 2].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    mirrorEntities = value.Split(',')
                        .Select(e => e.Trim())
                        .Where(e => !string.IsNullOrWhiteSpace(e))
                        .ToList();
                }
                break;
            }
        }

        return mirrorEntities;
    }

    private async Task<List<ColumnDefinition>> ExtractColumnsAsync(ExcelPackage package, string entityType)
    {
        var columns = new List<ColumnDefinition>();
        var processedColumns = new HashSet<string>();

        // Get first data sheet
        var dataSheet = package.Workbook.Worksheets
            .FirstOrDefault(ws => System.Text.RegularExpressions.Regex.IsMatch(ws.Name, @"^Data\d{2}of\d{2}$"));

        if (dataSheet == null || dataSheet.Dimension == null)
            return columns;

        // Find separator column index
        int separatorIndex = -1;
        var headerRow = 1;
        
        for (int col = 1; col <= dataSheet.Dimension.Columns; col++)
        {
            var columnName = dataSheet.Cells[headerRow, col].Value?.ToString();
            
            if (string.IsNullOrWhiteSpace(columnName))
                continue;

            if (_settings.ExcludedColumns.Contains(columnName, StringComparer.OrdinalIgnoreCase))
                continue;

            if (columnName == _settings.SeparatorColumn)
            {
                separatorIndex = col;
                continue;
            }

            if (!processedColumns.Contains(columnName))
            {
                processedColumns.Add(columnName);
                
                var isValueType = separatorIndex == -1 || col < separatorIndex;
                
                columns.Add(new ColumnDefinition
                {
                    EntityName = columnName,
                    EntityType = entityType,
                    IsValueType = isValueType,
                    ColumnOrder = columns.Count + 1
                });
            }
        }

        return columns;
    }

    private async Task<Entity> GetOrCreateEntityAsync(string entityName, string entityType, int userId)
    {
        var entity = await _context.Entities
            .FirstOrDefaultAsync(e => e.EntityName == entityName);

        if (entity == null)
        {
            entity = new Entity
            {
                EntityName = entityName,
                EntityDisplayName = entityName,
                EntityType = entityType,
                DataTypeID = 1, // Default to String
                IsActive = true,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Entities.Add(entity);
            await _context.SaveChangesAsync();
        }

        return entity;
    }

    private async Task<EntityTemplateRevision> CreateTemplateRevisionAsync(
        int entityId, 
        List<ColumnDefinition> columns, 
        int userId)
    {
        // Check for existing active revision
        var currentRevision = await _context.EntityTemplateRevisions
            .Include(r => r.DependencyDefinitions)
            .Where(r => r.EntityID == entityId && r.IsActive)
            .FirstOrDefaultAsync();

        // Compare columns
        if (currentRevision != null)
        {
            var existingColumns = currentRevision.DependencyDefinitions!
                .OrderBy(d => d.EntityOrder)
                .ToList();

            bool columnsMatch = existingColumns.Count == columns.Count &&
                existingColumns.All(ec => columns.Any(nc => 
                    nc.EntityName == ec.DependentEntity!.EntityName &&
                    nc.IsValueType == ec.IsValueType));

            if (columnsMatch)
            {
                return currentRevision;
            }

            // Deactivate current revision
            currentRevision.IsActive = false;
            currentRevision.UpdatedAt = DateTime.UtcNow;
            currentRevision.UpdatedBy = userId;
        }

        // Create new revision
        var newRevisionNumber = currentRevision?.TemplateRevisionNumber + 1 ?? 1;
        
        var revision = new EntityTemplateRevision
        {
            EntityID = entityId,
            TemplateRevisionNumber = newRevisionNumber,
            IsActive = true,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.EntityTemplateRevisions.Add(revision);
        await _context.SaveChangesAsync();

        // Create dependency definitions
        foreach (var column in columns)
        {
            var dependentEntity = await GetOrCreateEntityAsync(column.EntityName, column.EntityType, userId);
            
            var dependency = new EntityDependencyDefinition
            {
                TemplateRevisionID = revision.TemplateRevisionID,
                EntityID = entityId,
                DependentEntityID = dependentEntity.EntityID,
                EntityOrder = column.ColumnOrder,
                IsValueType = column.IsValueType,
                IsActive = true,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.EntityDependencyDefinitions.Add(dependency);
        }

        await _context.SaveChangesAsync();
        
        return revision;
    }

    private async Task ProcessMirrorEntitiesAsync(int entityId, List<string> mirrorEntityNames, int userId)
    {
        // Remove existing mirror relationships
        var existingMirrors = await _context.MirrorEntities
            .Where(m => m.EntityId == entityId)
            .ToListAsync();
        
        _context.MirrorEntities.RemoveRange(existingMirrors);

        // Add new mirror relationships
        foreach (var mirrorName in mirrorEntityNames)
        {
            var mirrorEntity = await _context.Entities
                .FirstOrDefaultAsync(e => e.EntityName == mirrorName);

            if (mirrorEntity != null)
            {
                var mirror = new MirrorEntity
                {
                    EntityId = entityId,
                    MirrorEntityId = mirrorEntity.EntityID,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MirrorEntities.Add(mirror);
            }
        }

        await _context.SaveChangesAsync();
    }
}

public class ColumnDefinition
{
    public string EntityName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public bool IsValueType { get; set; }
    public int ColumnOrder { get; set; }
}