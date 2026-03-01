using PlanningSystem.Domain.Entities;
using PlanningSystem.Domain.Exceptions;

namespace PlanningSystem.Domain.Services;

public class OrganizationAuthorizationService
{
    /// <summary>
    /// Checks if user can edit organization (Admin or Manager).
    /// </summary>
    public void EnsureCanEditOrganization(OrganizationUserMap? userMap)
    {
        if (userMap == null || (userMap.Role != OrganizationRole.Admin && userMap.Role != OrganizationRole.Manager))
            throw new UnauthorizedException("You do not have permission to update this organization");
    }

    /// <summary>
    /// Checks if user can manage shifts (Admin, Manager, or the worker themselves).
    /// </summary>
    public void EnsureCanManageShifts(OrganizationUserMap? userMap, int userId, int shiftWorkerId)
    {
        if (userMap == null ||
            (userMap.Role != OrganizationRole.Admin && userMap.Role != OrganizationRole.Manager && userId != shiftWorkerId))
            throw new UnauthorizedException("You do not have permission to update this shift");
    }

    /// <summary>
    /// Checks if user has access to organization (any role).
    /// </summary>
    public void EnsureHasAccess(OrganizationUserMap? userMap, string resourceName = "this organization")
    {
        if (userMap == null)
            throw new UnauthorizedException($"You do not have access to {resourceName}");
    }

    /// <summary>
    /// Checks if user can delete organization (Admin only).
    /// </summary>
    public void EnsureCanDeleteOrganization(OrganizationUserMap? userMap)
    {
        if (userMap == null || userMap.Role != OrganizationRole.Admin)
            throw new UnauthorizedException("Only organization administrators can delete organizations");
    }
}
