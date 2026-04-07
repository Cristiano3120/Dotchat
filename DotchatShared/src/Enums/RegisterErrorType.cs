namespace DotchatShared.src.Enums;

public enum RegisterErrorType : byte
{
    None,
    EmailTaken,
    UsernameTaken,
    DbUnavailable,
    Unknown
}