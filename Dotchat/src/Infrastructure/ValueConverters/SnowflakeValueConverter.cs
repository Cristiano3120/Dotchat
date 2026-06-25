using DotchatShared.src.DTOs;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DotchatServer.src.Infrastructure.ValueConverters;

public sealed class SnowflakeValueConverter : ValueConverter<Snowflake, long>
{
    public SnowflakeValueConverter() : base(
        snowflake => snowflake.Value,
        value => new Snowflake(value))
    { }
}