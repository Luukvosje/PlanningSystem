using Microsoft.EntityFrameworkCore;
using PlanningSystem.Application.DTOs.Requests;
using PlanningSystem.Application.Interfaces;
using PlanningSystem.Domain.Entities;
using PlanningSystem.Infrastructure.Persistence;

namespace PlanningSystem.Infrastructure.Repositories;

public class ShiftRepository : IShiftRepository
{
    private readonly AppDbContext _context;

    public ShiftRepository(AppDbContext context)
    {
        _context = context;
    }

    public Shift? GetById(Guid id)
    {
        return _context.Shifts
            .Include(s => s.Organization)
            .Include(s => s.Worker)
            .FirstOrDefault(s => s.Id == id);
    }

    public List<Shift> GetByOrganization(int organizationId)
    {
        return _context.Shifts
            .Where(s => s.OrganizationId == organizationId)
            .Include(s => s.Organization)
            .Include(s => s.Worker)
            .OrderBy(s => s.StartTime)
            .ToList();
    }

    public List<Shift> GetByWorker(int workerId)
    {
        return _context.Shifts
            .Where(s => s.WorkerId == workerId)
            .Include(s => s.Organization)
            .Include(s => s.Worker)
            .OrderBy(s => s.StartTime)
            .ToList();
    }

    public List<Shift> GetFiltered(ShiftFilterRequest filter)
    {
        var query = _context.Shifts
            .Include(s => s.Organization)
            .Include(s => s.Worker)
            .AsQueryable();

        if (filter.OrganizationId.HasValue)
            query = query.Where(s => s.OrganizationId == filter.OrganizationId.Value);

        if (filter.WorkerId.HasValue)
            query = query.Where(s => s.WorkerId == filter.WorkerId.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(s => s.StartTime >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(s => s.EndTime <= filter.EndDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(s => s.Status == filter.Status);

        return query.OrderBy(s => s.StartTime).ToList();
    }

    public Shift Create(Shift entity)
    {
        _context.Shifts.Add(entity);
        return entity;
    }

    public void Update(Shift entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(Guid id)
    {
        var shift = _context.Shifts.Find(id);
        if (shift != null)
        {
            _context.Shifts.Remove(shift);
        }
    }
}
