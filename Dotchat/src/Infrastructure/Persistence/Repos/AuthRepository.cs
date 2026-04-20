using DotchatServer.src.Application.Enums;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Interfaces.Security;
using DotchatServer.src.Core.Entities;

using DotchatShared.src.Enums;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Serilog;

namespace DotchatServer.src.Infrastructure.Persistence.Repos;

public sealed class AuthRepository(
    [FromKeyedServices(HashingAlgorithm.Argon2)] IHashingService hashingService,
    AppDbContext dbContext) : IAuthRepository
{
    private readonly DbSet<ApplicationUser> _users = dbContext.Users;

    /// <summary>
    /// Asynchronously creates a new user in the database and returns the result of the operation.
    /// </summary>
    /// <remarks>If the username or email already exists in the database, the method returns a corresponding
    /// result. If the database is unavailable or an unexpected error occurs, the method returns an appropriate error
    /// result. The operation does not throw exceptions for these cases but instead returns a result value describing
    /// the outcome.</remarks>
    /// <param name="applicationUser">The user entity to be created. Must contain valid and unique username and email information.</param>
    /// <returns>A value indicating the result of the user creation operation. Returns a specific result if the username or email
    /// is already taken, if the database is unavailable, or if an unknown error occurs.</returns>
    public async Task<RegisterErrorType> CreateUserAsync(ApplicationUser applicationUser)
    {
        try
        {
            bool usernameTaken = await _users.AnyAsync(x => x.Username == applicationUser.Username);
            if (usernameTaken)
            {
                Log.Information("Username {Username} was already taken", applicationUser.Username);
                return RegisterErrorType.UsernameTaken;
            }

            bool emailTaken = await _users.AnyAsync(x => x.Email == applicationUser.Email);
            if (emailTaken)
            {
                Log.Information("Email {Email} was already taken", applicationUser.Email);
                return RegisterErrorType.EmailTaken;
            }

            // Hash the password after confirming the username and email are unique to avoid unnecessary hashing work in case of duplicates.
            applicationUser = applicationUser with
            {
                PasswordHash = hashingService.Hash(applicationUser.PasswordHash)
            };

            _ = await _users.AddAsync(applicationUser);
            if (await dbContext.SaveChangesAsync() > 0)
            {
                Log.Information("User added successfully: {@User}", applicationUser);
                return RegisterErrorType.None;
            }

            Log.Error("Failed to create user for unknown reasons. No exceptions were thrown, but the database operation did not succeed.");
            return RegisterErrorType.Unknown;
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
            Log.Error(ex, "Database error occurred while creating user.");
            return RegisterErrorType.DbUnavailable;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred while creating user.");
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