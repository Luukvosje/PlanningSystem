using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Planning.Infrastructure.Data;

internal static class JsonColumnOptions
{
    internal static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };
}

internal sealed class JsonColumnConverter<T>(T defaultValue) : ValueConverter<T, string>(
    value => JsonSerializer.Serialize(value, JsonColumnOptions.SerializerOptions),
    value => JsonSerializer.Deserialize<T>(value, JsonColumnOptions.SerializerOptions) ?? defaultValue)
    where T : notnull;
