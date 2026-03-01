namespace PlanningSystem.Application.DTOs.Requests;

public class ShiftCreateRequest
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public required string Location { get; set; }
    public int OrganizationId { get; set; }
    public int WorkerId { get; set; }
    public string Status { get; set; } = "scheduled";
}

public class ShiftUpdateRequest
{
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Location { get; set; }
    public int? WorkerId { get; set; }
    public string? Status { get; set; }
}

public class ShiftStatusUpdateRequest
{
    public required string Status { get; set; }
}

public class ShiftFilterRequest
{
    public int? OrganizationId { get; set; }
    public int? WorkerId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
}
