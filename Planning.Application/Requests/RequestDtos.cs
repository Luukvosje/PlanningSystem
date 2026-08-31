using Planning.Domain.Enums;

namespace Planning.Application.Requests;

/// <summary>
/// One row in the request inbox, flat on purpose: the kinds do not share a shape, so the feed
/// carries every field a kind might fill and leaves the rest null rather than nesting a payload
/// per kind that the client would have to switch on twice.
/// </summary>
public sealed record RequestResponse(
    Guid Id,
    RequestKind Kind,
    Guid EmployeeId,
    string EmployeeFirstName,
    string EmployeeLastName,
    ApprovalStatus Status,
    DateOnly? Date,
    Weekday? Weekday,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? Reason,
    DateTime CreatedAtUtc,
    DateTime? DecidedAtUtc);

public sealed record RequestListResponse(IReadOnlyList<RequestResponse> Items);

/// <summary>
/// Points at one request. The kind travels with the id because a request is not one table: today
/// every kind is an availability rule, a shift swap will be its own entity.
/// </summary>
public sealed record RequestRef(RequestKind Kind, Guid Id);

public sealed record DecideRequestsRequest(IReadOnlyList<RequestRef> Requests);

/// <summary>
/// Counts rather than a per-item result: the caller listed these a moment ago, so the only
/// interesting answer is how many were still pending.
/// </summary>
public sealed record DecideRequestsResponse(int Decided, int Skipped);
