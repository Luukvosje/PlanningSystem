using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Planning.Infrastructure.Data;

/// <summary>
/// Stamps <see cref="DateTimeKind.Utc"/> on every <see cref="DateTime"/> read from the database.
/// <para>
/// SQL Server's <c>datetime2</c> carries no time zone, so EF materializes
/// <see cref="DateTimeKind.Unspecified"/>. System.Text.Json then serializes it without a <c>Z</c>,
/// and JavaScript parses a suffix-less timestamp as <em>local</em> time - shifting every planning
/// block by the viewer's UTC offset. The frontend used to patch this up client-side by appending
/// the missing <c>Z</c>; this fixes it at the source instead.
/// </para>
/// <para>
/// Values are only converted on the way out. On write, an already-UTC or unspecified value is
/// stored as-is (all columns are named <c>*Utc</c> and the domain stamps them from
/// <see cref="DateTime.UtcNow"/>); a local-kind value is converted first rather than silently
/// stored in the wrong zone.
/// </para>
/// </summary>
public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            value => value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value,
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
    {
    }
}

public sealed class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableUtcDateTimeConverter()
        : base(
            value => value.HasValue && value.Value.Kind == DateTimeKind.Local
                ? value.Value.ToUniversalTime()
                : value,
            value => value.HasValue
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                : value)
    {
    }
}
