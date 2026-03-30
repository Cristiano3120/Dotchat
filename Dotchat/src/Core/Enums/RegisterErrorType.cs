namespace DotchatServer.src.Core.Enums;

public enum RegisterErrorType : byte
{
    None,
    EmailTaken,
    UsernameTaken,
    DbUnavailable,
    Unknown
}