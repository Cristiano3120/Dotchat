using DotchatServer.src.Application.DTOs;
using DotchatServer.src.Application.Enums;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Interfaces.Security;
using DotchatServer.src.Core.Entities;

using DotchatShared.src.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

using Serilog;

namespace DotchatServer.src.Infrastructure.Persistence.Repos;

public sealed class AuthRepository(
    [FromKeyedServices(HashingAlgorithm.Argon2)] IHashingService hashingService,
    AppDbContext dbContext) : IAuthRepository
{
    private readonly DbSet<ApplicationUser> _users = dbContext.Users;

    public async Task<bool> ConfirmEmailAsync(long userId)
    {
        try
        {
            int affectedRows = await _users.Where(x => x.Id == userId).ExecuteUpdateAsync(s => s.SetProperty(e => e.EmailConfirmed, true));
            if (affectedRows > 0)
            {
                Log.Information("Email confirmed successfully for user {UserId}", userId);
                return true;
            }

            Log.Error("Failed to confirm email for user {UserId}. No rows were affected, which likely means the user does not exist.", userId);
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while confirming email for user {UserId}", userId);
            return false;
        }
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(long userId)
    {
        try
        {//TODO: Consider returning a more specific error type or using a Result<T> pattern to distinguish between "user not found" and actual errors.
            return await _users.FirstOrDefaultAsync(x => x.Id == userId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while retrieving user by ID {UserId}", userId);
            return null;
        }
    }

    private async Task<RegisterErrorType> CreateUserAsync(ApplicationUser applicationUser, string password)
    {
        bool usernameTaken = await _users.AnyAsync(x => x.Username == applicationUser.Username);
        if (usernameTaken)
        {
            return RegisterErrorType.UsernameTaken;
        }

        bool emailTaken = await _users.AnyAsync(x => x.Email == applicationUser.Email);
        if (emailTaken)
        {
            return RegisterErrorType.EmailTaken;
        }

        applicationUser = applicationUser with
        {
            PasswordHash = hashingService.Hash(password)
        };

        _ = _users.Add(applicationUser);
        return RegisterErrorType.None;
    }

    private void StoreRefreshToken(RefreshTokenInfo refreshTokenInfo)
        => _ = dbContext.RefreshTokens.Add(refreshTokenInfo);

    public async Task<RegisterErrorType> CompleteRegistrationAsync(ApplicationUser applicationUser, RefreshTokenInfo refreshTokenInfo, string userPassword)
    {
        await using IDbContextTransaction tx = await dbContext.Database.BeginTransactionAsync();
        try
        {
            RegisterErrorType result = await CreateUserAsync(applicationUser, userPassword);
            if (result != RegisterErrorType.None)
            {
                await tx.RollbackAsync();
                return result;
            }

            StoreRefreshToken(refreshTokenInfo);

            _ = await dbContext.SaveChangesAsync();
            await tx.CommitAsync();

            return result;
        }
        catch (DbUpdateException dbUpdateEx)
        {
            if (dbUpdateEx.InnerException is PostgresException pgEx
                && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                string? fieldName = GetFieldNameFromConstraint(pgEx.ConstraintName);
                Log.Error(dbUpdateEx, "Unique constraint violation occurred while creating user. Constraint: {ConstraintName}, Parsed Field: {FieldName}", pgEx.ConstraintName, fieldName);

                return fieldName switch
                {
                    nameof(ApplicationUser.Username) => RegisterErrorType.UsernameTaken,
                    nameof(ApplicationUser.Email) => RegisterErrorType.EmailTaken,
                    _ => RegisterErrorType.Unknown,
                };
            }

            Log.Error(dbUpdateEx, "An unexpected error occurred while creating user.");
            return RegisterErrorType.Unknown;
        }
        catch (NpgsqlException ex)
        {
            await tx.RollbackAsync();
            Log.Error(ex, "DB error during registration for {Username}", applicationUser.Username);
            return RegisterErrorType.DbUnavailable;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            Log.Error(ex, "Unexpected error during registration for {Username}", applicationUser.Username);
            return RegisterErrorType.Unknown;
        }
    }


    /// <summary>
    /// Extracts a PascalCase field name from a PostgreSQL constraint name.
    /// Expects constraints following the convention: prefix_table_fieldname
    /// e.g. "ix_users_email" → "Email", "ix_users_username" → "Username"
    /// </summary>
    /// <param name="constraint">
    /// The constraint name from <see cref="PostgresException.ConstraintName"/>.
    /// </param>
    /// <returns>
    /// PascalCase field name if the constraint name is valid and parseable;
    /// otherwise <see langword="null"/>.
    /// </returns>
    private static string? GetFieldNameFromConstraint(string? constraint)
    {
        if (string.IsNullOrWhiteSpace(constraint))
        {
            return null;
        }

        string[] parts = constraint.Split('_', StringSplitOptions.RemoveEmptyEntries);
        string fieldName = parts[^1];

        if (parts.Length < 2)
        {
            return null;
        }

        return char.ToUpper(fieldName[0]) + fieldName[1..];
    }
}