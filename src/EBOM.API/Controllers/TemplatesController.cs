using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EBOM.Core.Interfaces;
using EBOM.Core.Models;

namespace EBOM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateProcessor _templateProcessor;
    private readonly ILogger<TemplatesController> _logger;

    public TemplatesController(
        ITemplateProcessor templateProcessor,
        ILogger<TemplatesController> logger)
    {
        _templateProcessor = templateProcessor;
        _logger = logger;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(52428800)] // 50MB
    public async Task<ActionResult<TemplateUploadResponse>> UploadTemplate(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new TemplateUploadResponse
            {
                IsSuccess = false,
                FileName = "",
                EntityType = "",
                EntityName = "",
                Errors = new[] { new { Message = "No file uploaded" } }
            });
        }

        try
        {
            // Get user ID from claims (for now, using 1)
            var userId = 1; // TODO: Get from authenticated user

            using var stream = file.OpenReadStream();
            var result = await _templateProcessor.ProcessTemplateAsync(stream, file.FileName, userId);

            if (result.IsSuccess)
            {
                var (entityType, entityName) = ExtractEntityInfo(file.FileName);
                
                return Ok(new TemplateUploadResponse
                {
                    IsSuccess = true,
                    FileName = file.FileName,
                    EntityType = entityType,
                    EntityName = entityName,
                    EntityId = result.Metadata.ContainsKey("EntityId") ? (int)result.Metadata["EntityId"] : null,
                    TemplateRevision = result.Metadata.ContainsKey("TemplateRevision") ? (int)result.Metadata["TemplateRevision"] : null,
                    ColumnsProcessed = result.Metadata.ContainsKey("ColumnsProcessed") ? (int)result.Metadata["ColumnsProcessed"] : null
                });
            }

            return BadRequest(new TemplateUploadResponse
            {
                IsSuccess = false,
                FileName = file.FileName,
                EntityType = "",
                EntityName = "",
                Errors = result.Errors.Select(e => new { e.Message, e.Details }).ToArray()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading template");
            
            return StatusCode(500, new TemplateUploadResponse
            {
                IsSuccess = false,
                FileName = file.FileName,
                EntityType = "",
                EntityName = "",
                Errors = new[] { new { Message = "An error occurred while processing the template", Details = ex.Message } }
            });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<TemplateInfo>>> GetActiveTemplates()
    {
        var templates = await _templateProcessor.GetActiveTemplatesAsync();
        return Ok(templates);
    }

    [HttpGet("entity/{entityName}")]
    public async Task<ActionResult<TemplateInfo>> GetTemplateByEntity(string entityName)
    {
        var template = await _templateProcessor.GetTemplateByEntityAsync(entityName);
        
        if (template == null)
        {
            return NotFound(new { message = $"No active template found for entity '{entityName}'" });
        }

        return Ok(template);
    }

    private (string entityType, string entityName) ExtractEntityInfo(string fileName)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var parts = nameWithoutExtension.Split('_');
        
        if (parts.Length >= 2)
        {
            return (parts[0], string.Join("_", parts.Skip(1)));
        }
        
        return ("", nameWithoutExtension);
    }
}

public class TemplateUploadResponse
{
    public bool IsSuccess { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public int? TemplateRevision { get; set; }
    public int? ColumnsProcessed { get; set; }
    public object[]? ColumnDetails { get; set; }
    public object[]? Errors { get; set; }
    public object[]? Warnings { get; set; }
}