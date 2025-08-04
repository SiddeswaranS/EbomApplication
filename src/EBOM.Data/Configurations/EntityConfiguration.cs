using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EBOM.Core.Entities;

namespace EBOM.Data.Configurations;

public class EntityConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("Entity");
        
        builder.HasKey(e => e.EntityID);
        
        builder.Property(e => e.EntityID)
            .ValueGeneratedOnAdd();
            
        builder.Property(e => e.EntityName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.EntityDisplayName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.EntityDescription)
            .HasMaxLength(500);
            
        builder.Property(e => e.EntityType)
            .IsRequired()
            .HasMaxLength(3);
            
        builder.HasIndex(e => e.EntityName)
            .IsUnique();
            
        builder.HasOne(e => e.DataType)
            .WithMany()
            .HasForeignKey(e => e.DataTypeID);
            
        builder.HasMany(e => e.EntityValues)
            .WithOne(v => v.Entity)
            .HasForeignKey(v => v.EntityId);
            
        builder.HasMany(e => e.TemplateRevisions)
            .WithOne(r => r.Entity)
            .HasForeignKey(r => r.EntityID);
    }
}