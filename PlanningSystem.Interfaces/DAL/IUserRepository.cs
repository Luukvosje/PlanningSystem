using PlanningSystem.Models.Models;

namespace PlanningSystem.Interfaces.DAL
{
    public interface IUserRepository : IGenericGuidRepository<User>
    {
        User GetUserByEmail(string email);
        void ReloadCachedUsers();
    }
}
