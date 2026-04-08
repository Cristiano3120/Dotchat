using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DotchatServer.src.Infrastructure;

public sealed class DbContextWarmupUtility(IServiceProvider serviceProvider) : IWarmable
{
    public async Task WarmupAsync()
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            Console.WriteLine("DB Warmup start");

            _ = await dbContext.Users.AsNoTracking().AnyAsync(x => x.Username == "warmup");
            _ = await dbContext.Users.AsNoTracking().AnyAsync(x => x.Email == "warmup@warmup.com");

            Console.WriteLine("DB Warmup successful");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DB Warmup failed: {ex.Message}");
        }
    }
}