using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

/// <summary>
/// Structural equality comparer for JSON-converted list properties. Without this, EF Core
/// falls back to reference equality for the list, which makes its model-vs-snapshot change
/// detection unstable (a "pending model changes" false positive on every startup).
/// </summary>
internal sealed class JsonListValueComparer<TItem> : ValueComparer<List<TItem>>
{
    public JsonListValueComparer()
        : base(
            (left, right) => (left ?? new List<TItem>()).SequenceEqual(right ?? new List<TItem>()),
            list => list.Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
            list => list.ToList())
    {
    }
}
