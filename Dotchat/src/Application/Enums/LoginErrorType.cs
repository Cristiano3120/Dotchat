namespace DotchatServer.src.Application.Enums;

/// <summary>
/// Indicates what went wrong while trying to login on the DB-Level
/// </summary>
public enum LoginErrorType : byte
{
    WrongCredentials,
    DbException
}