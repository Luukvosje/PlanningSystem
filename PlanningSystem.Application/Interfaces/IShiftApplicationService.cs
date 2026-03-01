using PlanningSystem.Application.Common;
using PlanningSystem.Application.DTOs.Requests;
using PlanningSystem.Domain.Entities;

namespace PlanningSystem.Application.Interfaces;

public interface IShiftApplicationService
{
    ResultObject<Shift> CreateShift(ShiftCreateRequest request, int userId);
    ResultObject<Shift> UpdateShift(Guid shiftId, ShiftUpdateRequest request, int userId);
    ResultObject<bool> DeleteShift(Guid shiftId, int userId);
    ResultObject<Shift> GetShift(Guid shiftId, int userId);
    ResultObject<List<Shift>> GetShifts(ShiftFilterRequest? filter, int userId);
    ResultObject<Shift> UpdateShiftStatus(Guid shiftId, ShiftStatusUpdateRequest request, int userId);
}
