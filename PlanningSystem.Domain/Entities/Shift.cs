using PlanningSystem.Domain.Exceptions;
using PlanningSystem.Domain.ValueObjects;

namespace PlanningSystem.Domain.Entities;

public class Shift
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = ShiftStatus.Scheduled;

    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public int WorkerId { get; set; }
    public User Worker { get; set; } = null!;

    public void Reschedule(DateTime startTime, DateTime endTime)
    {
        var timeRange = new TimeRange(startTime, endTime);
        StartTime = timeRange.StartTime;
        EndTime = timeRange.EndTime;
    }

    public void MarkFinished()
    {
        Status = ShiftStatus.Finished;
    }

    public void MarkMissed()
    {
        Status = ShiftStatus.Missed;
    }

    public void SetStatus(string status)
    {
        var shiftStatus = new ShiftStatus(status);
        Status = shiftStatus.Value;
    }
}
