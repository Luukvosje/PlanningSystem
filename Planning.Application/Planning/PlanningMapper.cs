using Planning.Domain.Planning;

namespace Planning.Application.Planning;

internal static class PlanningMapper
{
    public static PlanningResponse ToResponse(
        PlanningRecord planningRecord,
        string? assignedUserName,
        string? customerName,
        bool hasOverlap) =>
        new(
            planningRecord.Id,
            planningRecord.OrganizationId,
            planningRecord.AssignedUserId,
            assignedUserName,
            planningRecord.CustomerId,
            customerName,
            planningRecord.Title,
            planningRecord.Description,
            planningRecord.Notes,
            planningRecord.StartUtc,
            planningRecord.EndUtc,
            planningRecord.Status,
            planningRecord.Color,
            hasOverlap,
            planningRecord.CreatedAtUtc,
            planningRecord.UpdatedAtUtc);
}
