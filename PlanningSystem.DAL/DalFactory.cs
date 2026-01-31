using PlanningSystem.DAL.Repositories;
using PlanningSystem.Interfaces.DAL;
using System.Runtime;

namespace PlanningSystem.DAL
{
    public class DalFactory : IDalFactory
    {
        private AppSettings _settings;

        public IUserRepository UserRepository => new UserRepository(_settings);
        public IOrganizationRepository OrganizationRepository => new OrganizationRepository(_settings);
        public IShiftRepository ShiftRepository => new ShiftRepository(_settings);

        public DalFactory(IAppSettings settings)
        {
            _settings = new AppSettings(settings);

            try
            {
                this.UserRepository.ReloadCachedUsers();
            }
            catch (Exception ex)
            {
                // Ignore cache reload errors on startup
            }
        }
    }
}
