using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EBOM.Core.Entities;

namespace EBOM.Data.Configurations;

public class EntityDependencyDefinitionConfiguration : IEntityTypeConfiguration<EntityDependencyDefinition>
{
    public void Configure(EntityTypeBuilder<EntityDependencyDefinition> builder)
    {
        builder.ToTable("EntityDependencyDefinition");
        
        builder.HasKey(e => e.EntityDependencyID);
        
        builder.Property(e => e.EntityDependencyID)
            .ValueGeneratedOnAdd();
            
        builder.HasOne(e => e.TemplateRevision)
            .WithMany(t => t.DependencyDefinitions)
            .HasForeignKey(e => e.TemplateRevisionID);
            
        builder.HasOne(e => e.Entity)
            .WithMany()
            .HasForeignKey(e => e.EntityID)
            .OnDelete(DeleteBehavior.NoAction);
            
        builder.HasOne(e => e.DependentEntity)
            .WithMany()
            .HasForeignKey(e => e.DependentEntityID)
            .OnDelete(DeleteBehavior.NoAction);
    }
}