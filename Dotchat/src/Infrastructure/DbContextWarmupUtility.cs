using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DotchatServer.src.Infrastructure;

/// <summary>
/// This utility is designed to "warm up" the database context by executing simple queries. This can help reduce the latency of the first real database operations after the application starts, as it allows the Entity Framework to initialize and cache necessary metadata and connections.
/// </summary>
/// <param name="serviceProvider"></param>
internal sealed class DbContextWarmupUtility(IServiceProvider serviceProvider) : IWarmable
{
    public async Task WarmupAsync()
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            Log.Information("DB Warmup start");

            _ = await dbContext.Users.Take(1).FirstOrDefaultAsync();
            _ = await dbContext.RefreshTokens.Take(1).FirstOrDefaultAsync();

            Log.Information("DB Warmup successful");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DB Warmup failed:");
        }
    }
}