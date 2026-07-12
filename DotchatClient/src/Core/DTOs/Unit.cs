namespace DotchatClient.src.Core.DTOs;

/// <summary>
/// Represents a type with no meaningful value - used as a placeholder return type
/// for operations that succeed without producing a result (e.g. DELETE requests,
/// "mark as read" actions). Enables using Result&lt;Unit, TErrorType&gt; instead of
/// requiring a nullable value type, which Result's Match/Switch pattern isn't built for.
/// </summary>
public readonly record struct Unit
{
    /// <summary>
    /// The single instance of Unit. Since Unit carries no data, there's no need
    /// to ever construct a new one - reuse this static instance everywhere.
    /// </summary>
    public static readonly Unit Value = new();
}