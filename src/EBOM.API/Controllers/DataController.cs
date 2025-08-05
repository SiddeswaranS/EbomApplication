using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EBOM.Core.Interfaces;

namespace EBOM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly IDataProcessor _dataProcessor;
    private readonly ILogger<DataController> _logger;

    public DataController(
        IDataProcessor dataProcessor,
        ILogger<DataController> logger)
    {
        _dataProcessor = dataProcessor;
        _logger = logger;
    }

    [HttpPost("upload/{entityId}")]
    [RequestSizeLimit(52428800)] // 50MB
    public async Task<ActionResult<DataUploadResponse>> UploadData(int entityId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new DataUploadResponse
            {
                IsSuccess = false,
                FileName = "",
                EntityId = entityId,
                Errors = new[] { new { Message = "No file uploaded" } }
            });
        }

        try
        {
            // Get user ID from claims (for now, using 1)
            var userId = 1; // TODO: Get from authenticated user

            using var stream = file.OpenReadStream();
            var result = await _dataProcessor.ProcessDataFileAsync(stream, file.FileName, entityId, userId);

            if (result.IsSuccess)
            {
                return Ok(new DataUploadResponse
                {
                    IsSuccess = true,
                    FileName = file.FileName,
                    EntityId = entityId,
                    DataRevisionId = result.DataRevisionId,
                    RowsProcessed = result.RowsProcessed,
                    RowsSkipped = result.RowsSkipped,
                    TableName = result.Metadata.ContainsKey("TableName") ? result.Metadata["TableName"].ToString() : null,
                    SheetsProcessed = result.Metadata.ContainsKey("SheetsProcessed") ? 
                        ((List<string>)result.Metadata["SheetsProcessed"]).ToArray() : null
                });
            }

            return BadRequest(new DataUploadResponse
            {
                IsSuccess = false,
                FileName = file.FileName,
                EntityId = entityId,
                Errors = result.Errors.Select(e => new { e.Message, e.Details }).ToArray(),
                Warnings = result.Warnings.Select(w => new { w.Message, w.Suggestion }).ToArray()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading data for entity {EntityId}", entityId);
            
            return StatusCode(500, new DataUploadResponse
            {
                IsSuccess = false,
                FileName = file.FileName,
                EntityId = entityId,
                Errors = new[] { new { Message = "An error occurred while processing the data", Details = ex.Message } }
            });
        }
    }

    [HttpGet("summary/{entityName}")]
    public async Task<ActionResult> GetDataSummary(string entityName)
    {
        try
        {
            var summary = await _dataProcessor.GetDataSummaryAsync(entityName);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data summary for entity {EntityName}", entityName);
            return StatusCode(500, new { message = "Error retrieving data summary", details = ex.Message });
        }
    }

    [HttpGet("revisions/{entityName}")]
    public async Task<ActionResult> GetDataRevisions(string entityName)
    {
        try
        {
            var revisions = await _dataProcessor.GetDataRevisionsAsync(entityName);
            return Ok(revisions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data revisions for entity {EntityName}", entityName);
            return StatusCode(500, new { message = "Error retrieving data revisions", details = ex.Message });
        }
    }

    [HttpPost("validate/{entityId}")]
    public async Task<ActionResult> ValidateDataRow(int entityId, [FromBody] Dictionary<string, object> rowData)
    {
        try
        {
            var result = await _dataProcessor.ValidateDataRowAsync(entityId, rowData, 1);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating data row for entity {EntityId}", entityId);
            return StatusCode(500, new { message = "Error validating data row", details = ex.Message });
        }
    }
}

public class DataUploadResponse
{
    public bool IsSuccess { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int DataRevisionId { get; set; }
    public int RowsProcessed { get; set; }
    public int RowsSkipped { get; set; }
    public string? TableName { get; set; }
    public string[]? SheetsProcessed { get; set; }
    public object[]? Errors { get; set; }
    public object[]? Warnings { get; set; }
}