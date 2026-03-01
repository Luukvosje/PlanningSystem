using PlanningSystem.Domain.Entities;
using PlanningSystem.Domain.Exceptions;

namespace PlanningSystem.Domain.Services;

public class OrganizationMembershipService
{
    /// <summary>
    /// Validates that the last administrator cannot be removed from an organization.
    /// </summary>
    public void ValidateRemoval(
        OrganizationUserMap userToRemoveMap,
        IEnumerable<OrganizationUserMap> allOrganizationMaps)
    {
        if (userToRemoveMap.Role != OrganizationRole.Admin)
            return;

        var adminCount = allOrganizationMaps.Count(m => m.Role == OrganizationRole.Admin);
        if (adminCount <= 1)
            throw new ValidationException("Cannot remove the last administrator from the organization");
    }
}
