using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EBOM.Core.Entities;

namespace EBOM.Data.Configurations;

public class MirrorEntityConfiguration : IEntityTypeConfiguration<MirrorEntity>
{
    public void Configure(EntityTypeBuilder<MirrorEntity> builder)
    {
        builder.ToTable("MirrorEntity");
        
        builder.HasKey(e => new { e.EntityId, e.MirrorEntityId });
        
        builder.HasOne(e => e.Entity)
            .WithMany(e => e.MirrorEntities)
            .HasForeignKey(e => e.EntityId)
            .OnDelete(DeleteBehavior.NoAction);
            
        builder.HasOne(e => e.MirrorEntityNavigation)
            .WithMany(e => e.MirroredByEntities)
            .HasForeignKey(e => e.MirrorEntityId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}