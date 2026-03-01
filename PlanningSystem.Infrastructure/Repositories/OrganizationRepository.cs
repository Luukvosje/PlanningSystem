using Microsoft.EntityFrameworkCore;
using PlanningSystem.Application.Interfaces;
using PlanningSystem.Domain.Entities;
using PlanningSystem.Infrastructure.Persistence;

namespace PlanningSystem.Infrastructure.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly AppDbContext _context;

    public OrganizationRepository(AppDbContext context)
    {
        _context = context;
    }

    public Organization? GetById(int id)
    {
        return _context.Organizations
            .Include(o => o.OrganizationUserMaps)
            .ThenInclude(oum => oum.User)
            .FirstOrDefault(o => o.Id == id);
    }

    public List<Organization> GetByUserId(int userId)
    {
        return _context.OrganizationUserMaps
            .Where(oum => oum.UserId == userId)
            .Include(oum => oum.Organization)
            .Select(oum => oum.Organization)
            .ToList();
    }

    public OrganizationUserMap? GetUserMap(int organizationId, int userId)
    {
        return _context.OrganizationUserMaps
            .FirstOrDefault(oum => oum.OrganizationId == organizationId && oum.UserId == userId);
    }

    public void AddUserToOrganization(int organizationId, int userId, OrganizationRole role)
    {
        var existingMap = _context.OrganizationUserMaps
            .FirstOrDefault(oum => oum.OrganizationId == organizationId && oum.UserId == userId);

        if (existingMap != null)
        {
            existingMap.Role = role;
        }
        else
        {
            var map = new OrganizationUserMap
            {
                OrganizationId = organizationId,
                UserId = userId,
                Role = role
            };
            _context.OrganizationUserMaps.Add(map);
        }
    }

    public void RemoveUserFromOrganization(int organizationId, int userId)
    {
        var map = _context.OrganizationUserMaps
            .FirstOrDefault(oum => oum.OrganizationId == organizationId && oum.UserId == userId);

        if (map != null)
        {
            _context.OrganizationUserMaps.Remove(map);
        }
    }

    public List<User> GetOrganizationUsers(int organizationId)
    {
        return _context.OrganizationUserMaps
            .Where(oum => oum.OrganizationId == organizationId)
            .Include(oum => oum.User)
            .Select(oum => oum.User)
            .ToList();
    }

    public List<OrganizationUserMap> GetOrganizationUserMaps(int organizationId)
    {
        return _context.OrganizationUserMaps
            .Where(oum => oum.OrganizationId == organizationId)
            .ToList();
    }

    public Organization Create(Organization entity)
    {
        _context.Organizations.Add(entity);
        return entity;
    }

    public void Update(Organization entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(int id)
    {
        var entity = _context.Organizations.Find(id);
        if (entity != null)
        {
            _context.Organizations.Remove(entity);
        }
    }
}
