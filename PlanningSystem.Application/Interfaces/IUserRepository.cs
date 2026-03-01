using PlanningSystem.Domain.Entities;

namespace PlanningSystem.Application.Interfaces;

public interface IUserRepository
{
    User? GetByEmail(string email);
    User? GetById(int id);
    User Create(User entity);
}
