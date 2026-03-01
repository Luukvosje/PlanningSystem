namespace PlanningSystem.Application.DTOs.Requests;

public class UserAddRequest
{
    public Guid Guid { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Name { get; set; }
}

public class UserAuthRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
