using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Identity.Infrastructure.Persistence.Entities;

namespace Modules.Identity.Infrastructure.Persistence.Configurations;

public class I18nResourceConfiguration : IEntityTypeConfiguration<I18nResource>
{
    public void Configure(EntityTypeBuilder<I18nResource> builder)
    {
        builder.ToTable("I18nResources");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasDefaultValueSql("NEWID()");
        
        builder.Property(x => x.TenantId)
            .IsRequired(false);
        
        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(x => x.Lang)
            .IsRequired()
            .HasMaxLength(10);
        
        builder.Property(x => x.Value)
            .IsRequired();
        
        builder.Property(x => x.VersionNo)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");
        
        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");
        
        builder.HasIndex(x => new { x.TenantId, x.Key, x.Lang })
            .IsUnique()
            .HasDatabaseName("IX_I18nResources_TenantId_Key_Lang");
        
        builder.HasIndex(x => x.Lang)
            .HasDatabaseName("IX_I18nResources_Lang");
        
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("IX_I18nResources_TenantId");
    }
}
