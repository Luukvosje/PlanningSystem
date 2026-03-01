using PlanningSystem.Domain.Entities;

namespace PlanningSystem.Application.DTOs.Requests;

public class UserAuthResponse
{
    public string Token { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}
