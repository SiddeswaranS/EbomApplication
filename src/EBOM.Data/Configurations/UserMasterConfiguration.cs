using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EBOM.Core.Entities;

namespace EBOM.Data.Configurations;

public class UserMasterConfiguration : IEntityTypeConfiguration<UserMaster>
{
    public void Configure(EntityTypeBuilder<UserMaster> builder)
    {
        builder.ToTable("UserMaster");
        
        builder.HasKey(e => e.UserID);
        
        builder.Property(e => e.UserID)
            .ValueGeneratedOnAdd();
            
        builder.Property(e => e.UserName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.UserEmail)
            .HasMaxLength(255);
    }
}