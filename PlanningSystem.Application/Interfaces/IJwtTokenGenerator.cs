using PlanningSystem.Domain.Entities;

namespace PlanningSystem.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, string jwtKey);
}
