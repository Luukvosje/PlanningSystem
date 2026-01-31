using PlanningSystem.BLL;
using PlanningSystem.Interfaces.BLL;
using PlanningSystem.Interfaces.DAL;
using PlanningSystem.Models.Models;

namespace PlanningSystem
{
    public class BllFactory : IBllFactory
    {   
        public IDalFactory DalFactory { get; set; }

        public BllFactory(IDalFactory dalFactory)
        {
            DalFactory = dalFactory;
        }

        public IUserLogic UserLogic => new UserLogic(this);
        public IOrganizationLogic OrganizationLogic => new OrganizationLogic(this);
        public IShiftLogic ShiftLogic => new ShiftLogic(this);
    }
}
