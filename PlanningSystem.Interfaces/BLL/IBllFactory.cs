
using PlanningSystem.Interfaces.BLL;
using PlanningSystem.Interfaces.DAL;

namespace PlanningSystem
{
    public interface IBllFactory
    {
        IUserLogic UserLogic { get; }
        IOrganizationLogic OrganizationLogic { get; }
        IShiftLogic ShiftLogic { get; }
        IDalFactory DalFactory { get; set; }
    }
}
