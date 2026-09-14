namespace Planning.Domain.Common;

/// <summary>
/// The colour rules shared by everything that carries one — a planning record and the customer
/// it defaults to. Kept in one place so the two cannot drift apart on what a valid colour is.
/// </summary>
public static class HexColor
{
    public const string Default = "#6366F1";

    /// <summary>
    /// Trims and upper-cases the colour, falling back to <see cref="Default"/> when none is
    /// given. Validation happens on the normalized value, so surrounding whitespace is tolerated.
    /// </summary>
    public static string Normalize(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return Default;
        }

        var normalized = color.Trim().ToUpperInvariant();

        if (!IsValid(normalized))
        {
            throw new ArgumentException("Color must be a valid hex color (e.g. #6366F1).", nameof(color));
        }

        return normalized;
    }

    private static bool IsValid(string color) =>
        color.Length == 7 && color[0] == '#' && color[1..].All(Uri.IsHexDigit);
}
