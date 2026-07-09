using Planning.Domain.Enums;

namespace Planning.Application.Availability;

public sealed record WeekAvailabilityRequest(
    DateTime WeekStartUtc,
    Guid? UserId,
    string? UserIds);

public sealed record UpsertDayPartAvailabilityRequest(
    Guid UserId,
    DateOnly Date,
    DayPart DayPart,
    bool IsAvailable,
    string? Note);

public sealed record UpsertTimeBlockAvailabilityRequest(
    Guid UserId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? Note,
    Guid? Id);

public sealed record AvailabilityResponse(
    Guid Id,
    Guid UserId,
    DateOnly Date,
    AvailabilityType Type,
    DayPart? DayPart,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    bool IsAvailable,
    AvailabilitySource Source,
    Guid LastModifiedByUserId,
    string LastModifiedByName,
    string? Note,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record WeekAvailabilityResponse(
    IReadOnlyList<AvailabilityResponse> Items,
    DateOnly RangeStart,
    DateOnly RangeEnd);
