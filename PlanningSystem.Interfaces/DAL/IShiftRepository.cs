using PlanningSystem.Models.Models;
using PlanningSystem.Models.Request;
using System;
using System.Collections.Generic;

namespace PlanningSystem.Interfaces.DAL
{
    public interface IShiftRepository : IGenericRepository<Shift>
    {
        Shift? GetById(Guid id);
        List<Shift> GetByOrganization(int organizationId);
        List<Shift> GetByWorker(int workerId);
        List<Shift> GetFiltered(ShiftFilterRequest filter);
        List<Shift> GetByDateRange(DateTime startDate, DateTime endDate);
        void Delete(Guid id);
    }
}
