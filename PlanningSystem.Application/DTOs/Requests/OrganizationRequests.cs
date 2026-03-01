namespace PlanningSystem.Application.DTOs.Requests;

public class OrganizationCreateRequest
{
    public required string Name { get; set; }
}

public class OrganizationUpdateRequest
{
    public required string Name { get; set; }
}

public class OrganizationAddUserRequest
{
    public int UserId { get; set; }
    public string Role { get; set; } = "Member";
}
