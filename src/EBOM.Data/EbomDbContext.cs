using Microsoft.EntityFrameworkCore;
using EBOM.Core.Entities;

namespace EBOM.Data;

public class EbomDbContext : DbContext
{
    public EbomDbContext(DbContextOptions<EbomDbContext> options) : base(options) { }

    public DbSet<Entity> Entities { get; set; }
    public DbSet<EntityValue> EntityValues { get; set; }
    public DbSet<EntityTemplateRevision> EntityTemplateRevisions { get; set; }
    public DbSet<EntityDependencyDefinition> EntityDependencyDefinitions { get; set; }
    public DbSet<EntityDataRevision> EntityDataRevisions { get; set; }
    public DbSet<MirrorEntity> MirrorEntities { get; set; }
    public DbSet<EntityDataType> EntityDataTypes { get; set; }
    public DbSet<UserMaster> UserMasters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EbomDbContext).Assembly);
        
        // Seed data
        SeedEntityDataTypes(modelBuilder);
        SeedDefaultUser(modelBuilder);
    }

    private void SeedEntityDataTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EntityDataType>().HasData(
            new EntityDataType { DataTypeID = 1, DataTypeName = "String", DataTypeDescription = "Text values", IsActive = true, CreatedBy = 1 },
            new EntityDataType { DataTypeID = 2, DataTypeName = "Integer", DataTypeDescription = "Whole numbers", IsActive = true, CreatedBy = 1 },
            new EntityDataType { DataTypeID = 3, DataTypeName = "Date", DataTypeDescription = "Date and time values", IsActive = true, CreatedBy = 1 },
            new EntityDataType { DataTypeID = 4, DataTypeName = "Boolean", DataTypeDescription = "True/False values", IsActive = true, CreatedBy = 1 },
            new EntityDataType { DataTypeID = 5, DataTypeName = "Range", DataTypeDescription = "Range values (e.g., 10:100)", IsActive = true, CreatedBy = 1 },
            new EntityDataType { DataTypeID = 6, DataTypeName = "RangeSet", DataTypeDescription = "Range with step (e.g., 10:100:10)", IsActive = true, CreatedBy = 1 }
        );
    }

    private void SeedDefaultUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserMaster>().HasData(
            new UserMaster { UserID = 1, UserName = "System", IsActive = true, CreatedBy = 1 }
        );
    }
}