using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Core.Entities;
using DotchatServer.src.Infrastructure.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotchatServer.src.Infrastructure.Persistence.EntityConfigurations;

public sealed class RefreshTokenInfoConfiguration : IEntityTypeConfiguration<RefreshTokenInfo>
{
    public void Configure(EntityTypeBuilder<RefreshTokenInfo> builder)
    {
        _ = builder.HasKey(x => x.Id);

        // Internal record identifier — just a Guid, not a Snowflake, since this
        // row has no meaning outside the DB (nobody references "RefreshToken #X"
        // anywhere in the API/client).
        _ = builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // References ApplicationUser.Id, which is a Snowflake — needs the converter.
        _ = builder.Property(x => x.UserId)
            .HasConversion<SnowflakeValueConverter>()
            .IsRequired();

        _ = builder.HasOne<ApplicationUser>()
        .WithMany() 
        .HasForeignKey(x => x.UserId)
        .OnDelete(DeleteBehavior.Cascade);

        // Client-generated device identifier, not a domain entity — stays Guid.
        _ = builder.Property(x => x.DeviceId)
            .IsRequired();

        _ = builder.Property(x => x.TokenHash)
            .IsRequired();

        _ = builder.Property(x => x.DeviceName)
            .HasMaxLength(30);

        _ = builder.Property(x => x.Platform);

        _ = builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        _ = builder.Property(x => x.LastUsedAt)
            .HasDefaultValueSql("now()");

        _ = builder.Property(x => x.ExpiresAt)
            .IsRequired();

        // Primary lookup path for the refresh endpoint: find by token hash.
        _ = builder.HasIndex(x => x.TokenHash);

        // One active token entry per user+device; re-login on the same device
        // upserts this row instead of accumulating duplicates.
        _ = builder.HasIndex(x => new { x.UserId, x.DeviceId })
            .IsUnique();
    }
} 