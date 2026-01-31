using PlanningSystem.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PlanningSystem.Interfaces.DAL
{
    public interface IDalFactory
    {
        IUserRepository UserRepository { get; }
        IOrganizationRepository OrganizationRepository { get; }
        IShiftRepository ShiftRepository { get; }
    }
}
