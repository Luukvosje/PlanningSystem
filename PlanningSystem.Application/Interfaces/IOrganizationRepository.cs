using PlanningSystem.Domain.Entities;

namespace PlanningSystem.Application.Interfaces;

public interface IOrganizationRepository
{
    Organization? GetById(int id);
    List<Organization> GetByUserId(int userId);
    OrganizationUserMap? GetUserMap(int organizationId, int userId);
    void AddUserToOrganization(int organizationId, int userId, OrganizationRole role);
    void RemoveUserFromOrganization(int organizationId, int userId);
    List<User> GetOrganizationUsers(int organizationId);
    List<OrganizationUserMap> GetOrganizationUserMaps(int organizationId);
    Organization Create(Organization entity);
    void Update(Organization entity);
    void Delete(int id);
}
