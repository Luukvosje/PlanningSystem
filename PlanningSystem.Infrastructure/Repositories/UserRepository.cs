using Microsoft.EntityFrameworkCore;
using PlanningSystem.Application.Interfaces;
using PlanningSystem.Domain.Entities;
using PlanningSystem.Infrastructure.Persistence;

namespace PlanningSystem.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public User? GetByEmail(string email)
    {
        return _context.Users
            .FirstOrDefault(u => u.Email != null && u.Email.Trim().ToLower() == email.Trim().ToLower());
    }

    public User? GetById(int id)
    {
        return _context.Users.Find(id);
    }

    public User Create(User entity)
    {
        _context.Users.Add(entity);
        return entity;
    }
}
