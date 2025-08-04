using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EBOM.Core.Entities;

namespace EBOM.Data.Configurations;

public class EntityDataTypeConfiguration : IEntityTypeConfiguration<EntityDataType>
{
    public void Configure(EntityTypeBuilder<EntityDataType> builder)
    {
        builder.ToTable("EntityDataType");
        
        builder.HasKey(e => e.DataTypeID);
        
        builder.Property(e => e.DataTypeID)
            .ValueGeneratedOnAdd();
            
        builder.Property(e => e.DataTypeName)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(e => e.DataTypeDescription)
            .HasMaxLength(255);
            
        builder.Property(e => e.DataTypeFormat)
            .HasMaxLength(100);
            
        builder.HasIndex(e => e.DataTypeName)
            .IsUnique();
    }
}