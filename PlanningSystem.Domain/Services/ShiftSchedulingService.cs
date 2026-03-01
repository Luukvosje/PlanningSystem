using PlanningSystem.Domain.Entities;

namespace PlanningSystem.Domain.Services;

public class ShiftSchedulingService
{
    /// <summary>
    /// Returns true if there is an overlapping shift (excluding missed shifts and optionally a specific shift).
    /// </summary>
    public bool HasOverlappingShift(
        IEnumerable<Shift> existingShifts,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeShiftId = null)
    {
        return existingShifts.Any(s =>
            (excludeShiftId == null || s.Id != excludeShiftId.Value) &&
            s.Status != "missed" &&
            s.StartTime < endTime &&
            s.EndTime > startTime);
    }
}
