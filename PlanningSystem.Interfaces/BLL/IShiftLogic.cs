using PlanningSystem.Models;
using PlanningSystem.Models.Models;
using PlanningSystem.Models.Request;
using System;
using System.Collections.Generic;

namespace PlanningSystem.Interfaces.BLL
{
    public interface IShiftLogic
    {
        ResultObject<Shift> CreateShift(ShiftCreateRequest request, int userId);
        ResultObject<Shift> UpdateShift(Guid shiftId, ShiftUpdateRequest request, int userId);
        ResultObject<bool> DeleteShift(Guid shiftId, int userId);
        ResultObject<Shift> GetShift(Guid shiftId, int userId);
        ResultObject<List<Shift>> GetShifts(ShiftFilterRequest filter, int userId);
        ResultObject<Shift> UpdateShiftStatus(Guid shiftId, ShiftStatusUpdateRequest request, int userId);
    }
}
