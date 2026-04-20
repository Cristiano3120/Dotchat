using DotchatServer.src.Core.Entities;

using Microsoft.EntityFrameworkCore;

namespace DotchatServer.src.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationUser> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.Entity<ApplicationUser>(x =>
        {
            _ = x.HasIndex(x => x.Email).IsUnique();
            _ = x.HasIndex(x => x.Username).IsUnique();
            _ = x.HasKey(x => x.Id);
        });
    }
}