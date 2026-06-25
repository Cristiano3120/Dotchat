namespace DotchatShared.src.DTOs;

/// <summary>
/// Represets a unique ID
/// </summary>
/// <param name="Value"></param>
public readonly record struct Snowflake(long Value)
{
    public static implicit operator Snowflake(long value) => new(value);
    public static implicit operator long(Snowflake snowflake) => snowflake.Value;

    public override string ToString() => Value.ToString();
}