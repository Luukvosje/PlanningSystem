using PlanningSystem.Domain.Exceptions;

namespace PlanningSystem.Domain.ValueObjects;

public readonly struct TimeRange
{
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }

    public TimeRange(DateTime startTime, DateTime endTime)
    {
        if (startTime >= endTime)
            throw new ValidationException("Start time must be before end time");

        StartTime = startTime;
        EndTime = endTime;
    }
}
