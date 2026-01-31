using Microsoft.EntityFrameworkCore;
using PlanningSystem.Interfaces.DAL;
using PlanningSystem.Models.Models;
using PlanningSystem.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanningSystem.DAL.Repositories
{
    internal class ShiftRepository : GenericRepository<Shift>, IShiftRepository
    {
        public ShiftRepository(AppSettings settings) : base(settings)
        {
        }

        public Shift? GetById(Guid id)
        {
            using (var context = GetContext())
            {
                return context.Shifts
                    .Include(s => s.Organization)
                    .Include(s => s.Worker)
                    .FirstOrDefault(s => s.Id == id);
            }
        }

        public List<Shift> GetByOrganization(int organizationId)
        {
            using (var context = GetContext())
            {
                return context.Shifts
                    .Where(s => s.OrganizationId == organizationId)
                    .Include(s => s.Organization)
                    .Include(s => s.Worker)
                    .OrderBy(s => s.StartTime)
                    .ToList();
            }
        }

        public List<Shift> GetByWorker(int workerId)
        {
            using (var context = GetContext())
            {
                return context.Shifts
                    .Where(s => s.WorkerId == workerId)
                    .Include(s => s.Organization)
                    .Include(s => s.Worker)
                    .OrderBy(s => s.StartTime)
                    .ToList();
            }
        }

        public List<Shift> GetFiltered(ShiftFilterRequest filter)
        {
            using (var context = GetContext())
            {
                var query = context.Shifts
                    .Include(s => s.Organization)
                    .Include(s => s.Worker)
                    .AsQueryable();

                if (filter.OrganizationId.HasValue)
                {
                    query = query.Where(s => s.OrganizationId == filter.OrganizationId.Value);
                }

                if (filter.WorkerId.HasValue)
                {
                    query = query.Where(s => s.WorkerId == filter.WorkerId.Value);
                }

                if (filter.StartDate.HasValue)
                {
                    query = query.Where(s => s.StartTime >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(s => s.EndTime <= filter.EndDate.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.Status))
                {
                    query = query.Where(s => s.Status == filter.Status);
                }

                return query.OrderBy(s => s.StartTime).ToList();
            }
        }

        public List<Shift> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            using (var context = GetContext())
            {
                return context.Shifts
                    .Where(s => s.StartTime >= startDate && s.EndTime <= endDate)
                    .Include(s => s.Organization)
                    .Include(s => s.Worker)
                    .OrderBy(s => s.StartTime)
                    .ToList();
            }
        }

        public override Shift Create(Shift entity)
        {
            using (var context = GetContext())
            {
                var set = context.Set<Shift>();
                set.Add(entity);
                context.SaveChanges();
                return entity;
            }
        }

        public override void Save(Shift entity)
        {
            using (var context = GetContext())
            {
                context.Entry(entity).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            using (var context = GetContext())
            {
                var shift = context.Shifts.Find(id);
                if (shift != null)
                {
                    context.Shifts.Remove(shift);
                    context.SaveChanges();
                }
            }
        }
    }
}
