using DotchatServer.src.Core.Entities;
using DotchatServer.src.Infrastructure.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotchatServer.src.Infrastructure.Persistence.EntityConfigurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        _ = builder.HasIndex(x => x.Email).IsUnique();
        _ = builder.HasIndex(x => x.Username).IsUnique();
        _ = builder.Property(x => x.Id).HasConversion<SnowflakeValueConverter>();
        _ = builder.HasKey(x => x.Id);
    }
}