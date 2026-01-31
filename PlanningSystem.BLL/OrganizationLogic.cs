using PlanningSystem.Interfaces.BLL;
using PlanningSystem.Models;
using PlanningSystem.Models.Models;
using PlanningSystem.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanningSystem.BLL
{
    public class OrganizationLogic : BaseLogic, IOrganizationLogic
    {
        public OrganizationLogic(BllFactory bllFactory) : base(bllFactory) { }

        public ResultObject<Organization> CreateOrganization(OrganizationCreateRequest request, int userId)
        {
            return SafeExecute(() =>
            {
                if (string.IsNullOrWhiteSpace(request?.Name))
                    throw new Exception("Organization name is required");

                var user = DalFactory.UserRepository.GetSingle(userId);
                if (user == null)
                    throw new Exception("User not found");

                var organization = new Organization
                {
                    Name = request.Name.Trim()
                };

                organization = DalFactory.OrganizationRepository.Create(organization);

                // Add creator as admin
                DalFactory.OrganizationRepository.AddUserToOrganization(organization.Id, userId, OrganizationRole.Admin);

                return organization;
            });
        }

        public ResultObject<Organization> UpdateOrganization(int organizationId, OrganizationUpdateRequest request, int userId)
        {
            return SafeExecute(() =>
            {
                if (string.IsNullOrWhiteSpace(request?.Name))
                    throw new Exception("Organization name is required");

                var organization = DalFactory.OrganizationRepository.GetById(organizationId);
                if (organization == null)
                    throw new Exception("Organization not found");

                // Check if user has permission (Admin or Manager)
                var userMap = DalFactory.OrganizationRepository.GetUserMap(organizationId, userId);
                if (userMap == null || (userMap.Role != OrganizationRole.Admin && userMap.Role != OrganizationRole.Manager))
                    throw new Exception("You do not have permission to update this organization");

                organization.Name = request.Name.Trim();
                DalFactory.OrganizationRepository.Save(organization);

                return organization;
            });
        }

        public ResultObject<bool> DeleteOrganization(int organizationId, int userId)
        {
            return SafeExecute(() =>
            {
                var organization = DalFactory.OrganizationRepository.GetById(organizationId);
                if (organization == null)
                    throw new Exception("Organization not found");

                // Check if user is admin
                var userMap = DalFactory.OrganizationRepository.GetUserMap(organizationId, userId);
                if (userMap == null || userMap.Role != OrganizationRole.Admin)
                    throw new Exception("Only organization administrators can delete organizations");

                DalFactory.OrganizationRepository.Delete(organizationId);
                return true;
            });
        }

        public ResultObject<Organization> GetOrganization(int organizationId, int userId)
        {
            return SafeExecute(() =>
            {
                var organization = DalFactory.OrganizationRepository.GetById(organizationId);
                if (organization == null)
                    throw new Exception("Organization not found");

                // Check if user has access
                var userMap = DalFactory.OrganizationRepository.GetUserMap(organizationId, userId);
                if (userMap == null)
                    throw new Exception("You do not have access to this organization");

                return organization;
            });
        }

        public ResultObject<List<Organization>> GetUserOrganizations(int userId)
        {
            return SafeExecute(() =>
            {
                var organizations = DalFactory.OrganizationRepository.GetByUserId(userId);
                return organizations;
            });
        }

        public ResultObject<bool> AddUserToOrganization(int organizationId, OrganizationAddUserRequest request, int userId)
        {
            return SafeExecute(() =>
            {
                var organization = DalFactory.OrganizationRepository.GetById(organizationId);
                if (organization == null)
                    throw new Exception("Organization not found");

                // Check if requesting user has permission (Admin or Manager)
                var requestingUserMap = DalFactory.OrganizationRepository.GetUserMap(organizationId, userId);
                if (requestingUserMap == null || (requestingUserMap.Role != OrganizationRole.Admin && requestingUserMap.Role != OrganizationRole.Manager))
                    throw new Exception("You do not have permission to add users to this organization");

                var userToAdd = DalFactory.UserRepository.GetSingle(request.UserId);
                if (userToAdd == null)
                    throw new Exception("User to add not found");

                if (!Enum.TryParse<OrganizationRole>(request.Role, true, out var role))
                    role = OrganizationRole.Member;

                DalFactory.OrganizationRepository.AddUserToOrganization(organizationId, request.UserId, role);
                return true;
            });
        }

        public ResultObject<bool> RemoveUserFromOrganization(int organizationId, int userIdToRemove, int requestingUserId)
        {
            return SafeExecute(() =>
            {
                var organization = DalFactory.OrganizationRepository.GetById(organizationId);
                if (organization == null)
                    throw new Exception("Organization not found");

                // Check if requesting user has permission (Admin or Manager)
                var requestingUserMap = DalFactory.OrganizationRepository.GetUserMap(organizationId, requestingUserId);
                if (requestingUserMap == null || (requestingUserMap.Role != OrganizationRole.Admin && requestingUserMap.Role != OrganizationRole.Manager))
                    throw new Exception("You do not have permission to remove users from this organization");

                // Prevent removing the last admin
                var userToRemoveMap = DalFactory.OrganizationRepository.GetUserMap(organizationId, userIdToRemove);
                if (userToRemoveMap?.Role == OrganizationRole.Admin)
                {
                    var adminCount = DalFactory.OrganizationRepository.GetOrganizationUsers(organizationId)
                        .Count(u => DalFactory.OrganizationRepository.GetUserMap(organizationId, u.Id)?.Role == OrganizationRole.Admin);
                    if (adminCount <= 1)
                        throw new Exception("Cannot remove the last administrator from the organization");
                }

                DalFactory.OrganizationRepository.RemoveUserFromOrganization(organizationId, userIdToRemove);
                return true;
            });
        }

        public ResultObject<List<User>> GetOrganizationUsers(int organizationId, int userId)
        {
            return SafeExecute(() =>
            {
                var organization = DalFactory.OrganizationRepository.GetById(organizationId);
                if (organization == null)
                    throw new Exception("Organization not found");

                // Check if user has access
                var userMap = DalFactory.OrganizationRepository.GetUserMap(organizationId, userId);
                if (userMap == null)
                    throw new Exception("You do not have access to this organization");

                var users = DalFactory.OrganizationRepository.GetOrganizationUsers(organizationId);
                return users;
            });
        }
    }
}
