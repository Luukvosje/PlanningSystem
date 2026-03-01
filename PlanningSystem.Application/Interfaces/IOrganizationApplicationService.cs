using PlanningSystem.Application.Common;
using PlanningSystem.Application.DTOs.Requests;
using PlanningSystem.Domain.Entities;

namespace PlanningSystem.Application.Interfaces;

public interface IOrganizationApplicationService
{
    ResultObject<Organization> CreateOrganization(OrganizationCreateRequest request, int userId);
    ResultObject<Organization> UpdateOrganization(int organizationId, OrganizationUpdateRequest request, int userId);
    ResultObject<bool> DeleteOrganization(int organizationId, int userId);
    ResultObject<Organization> GetOrganization(int organizationId, int userId);
    ResultObject<List<Organization>> GetUserOrganizations(int userId);
    ResultObject<bool> AddUserToOrganization(int organizationId, OrganizationAddUserRequest request, int userId);
    ResultObject<bool> RemoveUserFromOrganization(int organizationId, int userIdToRemove, int requestingUserId);
    ResultObject<List<User>> GetOrganizationUsers(int organizationId, int userId);
}
