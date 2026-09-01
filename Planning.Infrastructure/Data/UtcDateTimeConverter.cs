using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Planning.Infrastructure.Data;

/// <summary>
/// Forces every <see cref="DateTime"/> crossing the database boundary to <see cref="DateTimeKind.Utc"/>.
/// <para>
/// On write this is a hard requirement, not a nicety: Npgsql maps <see cref="DateTime"/> to
/// <c>timestamp with time zone</c> and throws when handed a value whose kind is not
/// <see cref="DateTimeKind.Utc"/>. Every column here is named <c>*Utc</c> and is stamped from
/// <see cref="DateTime.UtcNow"/>, so an <see cref="DateTimeKind.Unspecified"/> value is already UTC
/// and only needs labelling; a local-kind value is genuinely converted rather than mislabelled.
/// </para>
/// <para>
/// On read it guards the JSON contract. Without a kind, System.Text.Json serializes the timestamp
/// without a <c>Z</c>, and JavaScript parses a suffix-less timestamp as <em>local</em> time -
/// shifting every planning block by the viewer's UTC offset.
/// </para>
/// </summary>
public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            value => value.Kind == DateTimeKind.Local
                ? value.ToUniversalTime()
                : DateTime.SpecifyKind(value, DateTimeKind.Utc),
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
    {
    }
}

public sealed class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableUtcDateTimeConverter()
        : base(
            value => value.HasValue
                ? (value.Value.Kind == DateTimeKind.Local
                    ? value.Value.ToUniversalTime()
                    : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc))
                : value,
            value => value.HasValue
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                : value)
    {
    }
}
