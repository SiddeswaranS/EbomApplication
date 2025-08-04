using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EBOM.Core.Entities;

namespace EBOM.Data.Configurations;

public class EntityTemplateRevisionConfiguration : IEntityTypeConfiguration<EntityTemplateRevision>
{
    public void Configure(EntityTypeBuilder<EntityTemplateRevision> builder)
    {
        builder.ToTable("EntityTemplateRevision");
        
        builder.HasKey(e => e.TemplateRevisionID);
        
        builder.Property(e => e.TemplateRevisionID)
            .ValueGeneratedOnAdd();
            
        builder.Property(e => e.TemplateRevisionDescription)
            .HasMaxLength(500);
            
        builder.HasOne(e => e.Entity)
            .WithMany(e => e.TemplateRevisions)
            .HasForeignKey(e => e.EntityID);
            
        builder.HasMany(e => e.DependencyDefinitions)
            .WithOne(d => d.TemplateRevision)
            .HasForeignKey(d => d.TemplateRevisionID);
            
        builder.HasMany(e => e.DataRevisions)
            .WithOne(d => d.TemplateRevision)
            .HasForeignKey(d => d.TemplateRevisionId);
    }
}