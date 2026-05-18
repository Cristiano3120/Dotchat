using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Core.Entities;

using Microsoft.EntityFrameworkCore;

namespace DotchatServer.src.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<RefreshTokenInfo> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.Entity<ApplicationUser>(x =>
        {
            _ = x.HasIndex(x => x.Email).IsUnique();
            _ = x.HasIndex(x => x.Username).IsUnique();
            _ = x.HasKey(x => x.Id);
        });

        _ = modelBuilder.Entity<RefreshTokenInfo>(x =>
            {
                _ = x.HasKey(x => x.Id);

                _ = x.Property(x => x.TokenHash)
                     .IsRequired();

                _ = x.Property(x => x.ExpiresAt)
                     .IsRequired();

                _ = x.Property(x => x.CreatedAt)
                     .IsRequired()
                     .HasDefaultValueSql("now()");

                _ = x.HasIndex(x => x.TokenHash)
                     .IsUnique();

                _ = x.HasIndex(x => x.UserId);

                _ = x.HasOne<ApplicationUser>()
                     .WithMany()
                     .HasForeignKey(x => x.UserId)
                     .OnDelete(DeleteBehavior.Cascade);
            });
    }
}