using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EBOM.Data;
using EBOM.Core.Entities;

namespace EBOM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EntitiesController : ControllerBase
{
    private readonly EbomDbContext _context;
    private readonly ILogger<EntitiesController> _logger;

    public EntitiesController(EbomDbContext context, ILogger<EntitiesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EntityDto>>> GetEntities()
    {
        var entities = await _context.Entities
            .Include(e => e.DataType)
            .Where(e => e.IsActive)
            .Select(e => new EntityDto
            {
                EntityID = e.EntityID,
                EntityName = e.EntityName,
                EntityDisplayName = e.EntityDisplayName,
                EntityType = e.EntityType,
                DataType = e.DataType != null ? e.DataType.DataTypeName : "Unknown",
                IsActive = e.IsActive,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        return Ok(entities);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EntityDto>> GetEntity(int id)
    {
        var entity = await _context.Entities
            .Include(e => e.DataType)
            .Where(e => e.EntityID == id)
            .Select(e => new EntityDto
            {
                EntityID = e.EntityID,
                EntityName = e.EntityName,
                EntityDisplayName = e.EntityDisplayName,
                EntityDescription = e.EntityDescription,
                EntityType = e.EntityType,
                DataType = e.DataType != null ? e.DataType.DataTypeName : "Unknown",
                IsActive = e.IsActive,
                CreatedAt = e.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (entity == null)
        {
            return NotFound();
        }

        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<EntityDto>> CreateEntity(CreateEntityRequest request)
    {
        var entity = new Entity
        {
            EntityName = request.EntityName,
            EntityDisplayName = request.EntityDisplayName,
            EntityDescription = request.EntityDescription,
            EntityType = request.EntityType,
            DataTypeID = request.DataTypeID,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1 // TODO: Get from authenticated user
        };

        _context.Entities.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEntity), new { id = entity.EntityID }, new EntityDto
        {
            EntityID = entity.EntityID,
            EntityName = entity.EntityName,
            EntityDisplayName = entity.EntityDisplayName,
            EntityType = entity.EntityType,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEntity(int id, UpdateEntityRequest request)
    {
        var entity = await _context.Entities.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        entity.EntityDisplayName = request.EntityDisplayName;
        entity.EntityDescription = request.EntityDescription;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = 1; // TODO: Get from authenticated user

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEntity(int id)
    {
        var entity = await _context.Entities.FindAsync(id);
        if (entity == null)
        {
            return NotFound();
        }

        // Soft delete
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = 1; // TODO: Get from authenticated user

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class EntityDto
{
    public int EntityID { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityDisplayName { get; set; } = string.Empty;
    public string? EntityDescription { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateEntityRequest
{
    public string EntityName { get; set; } = string.Empty;
    public string EntityDisplayName { get; set; } = string.Empty;
    public string? EntityDescription { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int DataTypeID { get; set; }
}

public class UpdateEntityRequest
{
    public string EntityDisplayName { get; set; } = string.Empty;
    public string? EntityDescription { get; set; }
    public bool IsActive { get; set; }
}