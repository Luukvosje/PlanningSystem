using PlanningSystem.Domain.Exceptions;

namespace PlanningSystem.Domain.ValueObjects;

public readonly struct ShiftStatus
{
    public const string Scheduled = "scheduled";
    public const string Finished = "finished";
    public const string Missed = "missed";

    public string Value { get; }

    public ShiftStatus(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException("Shift status is required");

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized != Scheduled && normalized != Finished && normalized != Missed)
            throw new ValidationException("Invalid status. Must be: scheduled, finished, or missed");

        Value = normalized;
    }

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var normalized = value.Trim().ToLowerInvariant();
        return normalized == Scheduled || normalized == Finished || normalized == Missed;
    }

    public override string ToString() => Value;
}
