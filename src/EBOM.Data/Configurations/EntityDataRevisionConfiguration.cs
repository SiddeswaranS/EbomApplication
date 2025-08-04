using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EBOM.Core.Entities;

namespace EBOM.Data.Configurations;

public class EntityDataRevisionConfiguration : IEntityTypeConfiguration<EntityDataRevision>
{
    public void Configure(EntityTypeBuilder<EntityDataRevision> builder)
    {
        builder.ToTable("EntityDataRevision");
        
        builder.HasKey(e => e.DataRevisionId);
        
        builder.Property(e => e.DataRevisionId)
            .ValueGeneratedOnAdd();
            
        builder.Property(e => e.DataRevisionDescription)
            .HasMaxLength(500);
            
        builder.HasOne(e => e.TemplateRevision)
            .WithMany(t => t.DataRevisions)
            .HasForeignKey(e => e.TemplateRevisionId);
            
        builder.HasOne(e => e.Entity)
            .WithMany()
            .HasForeignKey(e => e.EntityId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}