using Planning.Domain.Enums;

namespace Planning.Application.Planning;

/// <summary>Fields shared by the create and update requests, so one validator covers both.</summary>
public interface IPlanningRequestFields
{
    Guid AssignedUserId { get; }
    string Title { get; }
    string? Description { get; }
    string? Notes { get; }
    DateTime StartUtc { get; }
    DateTime EndUtc { get; }
    string? Color { get; }
    PlanningStatus Status { get; }
}

public sealed record CreatePlanningRequest(
    Guid AssignedUserId,
    Guid? CustomerId,
    string Title,
    string? Description,
    string? Notes,
    DateTime StartUtc,
    DateTime EndUtc,
    string? Color,
    PlanningStatus Status = PlanningStatus.Confirmed) : IPlanningRequestFields;

public sealed record UpdatePlanningRequest(
    Guid AssignedUserId,
    Guid? CustomerId,
    string Title,
    string? Description,
    string? Notes,
    DateTime StartUtc,
    DateTime EndUtc,
    PlanningStatus Status,
    string? Color) : IPlanningRequestFields;

public sealed record MovePlanningRequest(
    Guid AssignedUserId,
    Guid? CustomerId,
    DateTime StartUtc,
    DateTime EndUtc);

public sealed record DuplicatePlanningRequest(
    DateTime? StartUtc,
    Guid? AssignedUserId);

public sealed record PlanningListRequest(
    DateTime StartUtc,
    DateTime EndUtc,
    string? UserIds,
    string? CustomerIds,
    string? Statuses,
    string? Search,
    int Page = 1,
    int PageSize = 500);

public sealed record PlanningResponse(
    Guid Id,
    Guid OrganizationId,
    Guid AssignedUserId,
    string AssignedUserName,
    Guid? CustomerId,
    string? CustomerName,
    string Title,
    string? Description,
    string? Notes,
    DateTime StartUtc,
    DateTime EndUtc,
    PlanningStatus Status,
    string Color,
    bool HasOverlap,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record PlanningListResponse(
    IReadOnlyList<PlanningResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    DateTime RangeStartUtc,
    DateTime RangeEndUtc);
