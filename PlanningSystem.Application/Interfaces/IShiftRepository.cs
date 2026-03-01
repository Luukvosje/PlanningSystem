using PlanningSystem.Application.DTOs.Requests;
using PlanningSystem.Domain.Entities;

namespace PlanningSystem.Application.Interfaces;

public interface IShiftRepository
{
    Shift? GetById(Guid id);
    List<Shift> GetByOrganization(int organizationId);
    List<Shift> GetByWorker(int workerId);
    List<Shift> GetFiltered(ShiftFilterRequest filter);
    Shift Create(Shift entity);
    void Update(Shift entity);
    void Delete(Guid id);
}
