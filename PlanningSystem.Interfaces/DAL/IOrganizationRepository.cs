using PlanningSystem.Models.Models;

namespace PlanningSystem.Interfaces.DAL
{
    public interface IOrganizationRepository : IGenericRepository<Organization>
    {
        Organization? GetById(int id);
        List<Organization> GetByUserId(int userId);
        OrganizationUserMap? GetUserMap(int organizationId, int userId);
        void AddUserToOrganization(int organizationId, int userId, OrganizationRole role);
        void RemoveUserFromOrganization(int organizationId, int userId);
        List<User> GetOrganizationUsers(int organizationId);
    }
}
