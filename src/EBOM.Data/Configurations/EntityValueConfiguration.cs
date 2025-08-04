using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EBOM.Core.Entities;
using System.Security.Cryptography;
using System.Text;

namespace EBOM.Data.Configurations;

public class EntityValueConfiguration : IEntityTypeConfiguration<EntityValue>
{
    public void Configure(EntityTypeBuilder<EntityValue> builder)
    {
        builder.ToTable("EntityValue");
        
        builder.HasKey(e => e.EntityValueId);
        
        builder.Property(e => e.EntityValueId)
            .ValueGeneratedOnAdd();
            
        builder.Property(e => e.EntityObjValue)
            .IsRequired();
            
        builder.Property(e => e.EntityObjValueHash)
            .HasComputedColumnSql("CAST(HASHBYTES('SHA2_256', EntityObjValue) AS VARBINARY(32))", stored: true);
            
        builder.HasIndex(e => e.EntityObjValueHash)
            .HasDatabaseName("IX_EntityValue_Hash");
            
        builder.HasIndex(e => e.EntityId)
            .HasDatabaseName("IX_EntityValue_EntityId");
            
        builder.HasOne(e => e.Entity)
            .WithMany(e => e.EntityValues)
            .HasForeignKey(e => e.EntityId);
    }
}