using FluentValidation;

namespace Planning.Application.Common;

/// <summary>
/// Filters arrive on the query string as comma-separated lists ("a,b,c"). Parsing them with
/// <c>Guid.Parse</c> / <c>Enum.Parse</c> turns a typo in the URL into an unhandled exception, so
/// both the validator and the service go through the <c>TryParse</c> helpers below - the validator
/// to reject the request with a 400, the service so a value that slipped past cannot throw.
/// </summary>
public static class CsvList
{
    public static IReadOnlyList<string> Split(string value) =>
        value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static bool TryParseGuids(string? value, out IReadOnlyList<Guid>? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var parsed = new List<Guid>();

        foreach (var part in Split(value))
        {
            if (!Guid.TryParse(part, out var id))
            {
                return false;
            }

            parsed.Add(id);
        }

        result = parsed;
        return true;
    }

    public static bool TryParseEnums<TEnum>(string? value, out IReadOnlyList<TEnum>? result)
        where TEnum : struct, Enum
    {
        result = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var parsed = new List<TEnum>();

        foreach (var part in Split(value))
        {
            // TryParse also accepts undefined numeric values ("99"), so check the range too.
            if (!Enum.TryParse<TEnum>(part, ignoreCase: true, out var item) || !Enum.IsDefined(item))
            {
                return false;
            }

            parsed.Add(item);
        }

        result = parsed;
        return true;
    }

    public static IReadOnlyList<Guid>? ParseGuidsOrNull(string? value) =>
        TryParseGuids(value, out var result) ? result : null;

    public static IReadOnlyList<TEnum>? ParseEnumsOrNull<TEnum>(string? value)
        where TEnum : struct, Enum =>
        TryParseEnums<TEnum>(value, out var result) ? result : null;
}

public static class CsvListRules
{
    public static IRuleBuilderOptions<T, string?> MustBeGuidList<T>(
        this IRuleBuilder<T, string?> rule) =>
        rule.Must(value => CsvList.TryParseGuids(value, out _))
            .WithMessage("'{PropertyName}' must be a comma-separated list of ids.");

    public static IRuleBuilderOptions<T, string?> MustBeEnumList<T, TEnum>(
        this IRuleBuilder<T, string?> rule)
        where TEnum : struct, Enum =>
        rule.Must(value => CsvList.TryParseEnums<TEnum>(value, out _))
            .WithMessage($"'{{PropertyName}}' must be a comma-separated list of {typeof(TEnum).Name} values.");
}
