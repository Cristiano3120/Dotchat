using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Core.Entities;
using DotchatShared.src.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DotchatServer.src.Infrastructure.Persistence.Repos;

internal sealed class AuthRepository(AppDbContext dbContext) : IAuthRepository
{
    private readonly DbSet<ApplicationUser> _users = dbContext.Users;
    private readonly DbSet<RefreshTokenInfo> _refreshTokens = dbContext.RefreshTokens;

    public async Task<ApplicationUser?> FindUserByEmailAsync(string email)
    => await _users.FirstOrDefaultAsync(x => x.Email == email);

    public async Task UpsertRefreshTokenAsync(RefreshTokenInfo refreshTokenInfo)
    {
        int updatedRows = await _refreshTokens
            .Where(x => x.UserId == refreshTokenInfo.UserId && x.DeviceId == refreshTokenInfo.DeviceId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.TokenHash, refreshTokenInfo.TokenHash)
                .SetProperty(x => x.ExpiresAt, refreshTokenInfo.ExpiresAt)
                .SetProperty(x => x.LastUsedAt, DateTime.UtcNow)
                .SetProperty(x => x.RevokedAt, (DateTime?)null));

        if (updatedRows == 0)
        {
            _ = await _refreshTokens.AddAsync(refreshTokenInfo);
            _ = await dbContext.SaveChangesAsync();
        }
    }

    public async Task<bool> CheckIfEmailExistsAsync(string email)
        => await _users.AnyAsync(x => x.Email == email);

    public async Task<bool> CheckIfUsernameExistsAsync(string username)
        => await _users.AnyAsync(x => x.Username == username);

    public async Task<bool> ConfirmEmailAsync(Snowflake userId)
    {
        int affectedRows = await _users.Where(x => x.Id == userId).ExecuteUpdateAsync(s => s.SetProperty(e => e.EmailConfirmed, true));
        if (affectedRows > 0)
        {
            Log.Information("Email confirmed successfully for user {UserId}", userId);
            return true;
        }

        Log.Error("Failed to confirm email for user {UserId}. No rows were affected, which either means the user does not exist or the email is already confirmed.", userId);
        return false;
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(Snowflake userId) 
        => await _users.FirstOrDefaultAsync(x => x.Id == userId);

    public async Task RegisterUserAsync(ApplicationUser user, RefreshTokenInfo tokenInfo)
    {
        _ = await _users.AddAsync(user);
        _ = await _refreshTokens.AddAsync(tokenInfo);
        _ = await dbContext.SaveChangesAsync();
    }
}