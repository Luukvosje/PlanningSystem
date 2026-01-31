using Microsoft.EntityFrameworkCore;
using PlanningSystem.Interfaces.DAL;
using PlanningSystem.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanningSystem.DAL.Repositories
{
    internal class OrganizationRepository : GenericRepository<Organization>, IOrganizationRepository
    {
        public OrganizationRepository(AppSettings settings) : base(settings)
        {
        }

        public Organization? GetById(int id)
        {
            using (var context = GetContext())
            {
                return context.Organizations
                    .Include(o => o.OrganizationUserMaps)
                    .ThenInclude(oum => oum.User)
                    .FirstOrDefault(o => o.Id == id);
            }
        }

        public List<Organization> GetByUserId(int userId)
        {
            using (var context = GetContext())
            {
                return context.OrganizationUserMaps
                    .Where(oum => oum.UserId == userId)
                    .Include(oum => oum.Organization)
                    .Select(oum => oum.Organization)
                    .ToList();
            }
        }

        public OrganizationUserMap? GetUserMap(int organizationId, int userId)
        {
            using (var context = GetContext())
            {
                return context.OrganizationUserMaps
                    .FirstOrDefault(oum => oum.OrganizationId == organizationId && oum.UserId == userId);
            }
        }

        public void AddUserToOrganization(int organizationId, int userId, OrganizationRole role)
        {
            using (var context = GetContext())
            {
                var existingMap = context.OrganizationUserMaps
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
                    context.OrganizationUserMaps.Add(map);
                }

                context.SaveChanges();
            }
        }

        public void RemoveUserFromOrganization(int organizationId, int userId)
        {
            using (var context = GetContext())
            {
                var map = context.OrganizationUserMaps
                    .FirstOrDefault(oum => oum.OrganizationId == organizationId && oum.UserId == userId);

                if (map != null)
                {
                    context.OrganizationUserMaps.Remove(map);
                    context.SaveChanges();
                }
            }
        }

        public List<User> GetOrganizationUsers(int organizationId)
        {
            using (var context = GetContext())
            {
                return context.OrganizationUserMaps
                    .Where(oum => oum.OrganizationId == organizationId)
                    .Include(oum => oum.User)
                    .Select(oum => oum.User)
                    .ToList();
            }
        }
    }
}
