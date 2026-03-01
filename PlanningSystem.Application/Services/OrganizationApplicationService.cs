using PlanningSystem.Application.Common;
using PlanningSystem.Application.DTOs.Requests;
using PlanningSystem.Application.Interfaces;
using PlanningSystem.Domain.Entities;
using PlanningSystem.Domain.Exceptions;
using PlanningSystem.Domain.Services;

namespace PlanningSystem.Application.Services;

public class OrganizationApplicationService : IOrganizationApplicationService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly OrganizationMembershipService _membershipService;
    private readonly OrganizationAuthorizationService _authorizationService;

    public OrganizationApplicationService(
        IOrganizationRepository organizationRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        OrganizationMembershipService membershipService,
        OrganizationAuthorizationService authorizationService)
    {
        _organizationRepository = organizationRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _membershipService = membershipService;
        _authorizationService = authorizationService;
    }

    public ResultObject<Organization> CreateOrganization(OrganizationCreateRequest request, int userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
                throw new ValidationException("Organization name is required");

            var user = _userRepository.GetById(userId);
            if (user == null)
                throw new EntityNotFoundException("User", userId);

            var organization = new Organization { Name = request.Name.Trim() };
            organization = _organizationRepository.Create(organization);
            _organizationRepository.AddUserToOrganization(organization.Id, userId, OrganizationRole.Admin);
            _unitOfWork.Commit();

            return new ResultObject<Organization> { Success = true, Data = organization };
        }
        catch (DomainException ex)
        {
            return new ResultObject<Organization> { Success = false, Message = ex.Message, Exception = ex };
        }
    }

    public ResultObject<Organization> UpdateOrganization(int organizationId, OrganizationUpdateRequest request, int userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
                throw new ValidationException("Organization name is required");

            var organization = _organizationRepository.GetById(organizationId);
            if (organization == null)
                throw new EntityNotFoundException("Organization", organizationId);

            var userMap = _organizationRepository.GetUserMap(organizationId, userId);
            _authorizationService.EnsureCanEditOrganization(userMap);

            organization.ChangeName(request.Name);
            _organizationRepository.Update(organization);
            _unitOfWork.Commit();

            return new ResultObject<Organization> { Success = true, Data = organization };
        }
        catch (DomainException ex)
        {
            return new ResultObject<Organization> { Success = false, Message = ex.Message, Exception = ex };
        }
    }

    public ResultObject<bool> DeleteOrganization(int organizationId, int userId)
    {
        try
        {
            var organization = _organizationRepository.GetById(organizationId);
            if (organization == null)
                throw new EntityNotFoundException("Organization", organizationId);

            var userMap = _organizationRepository.GetUserMap(organizationId, userId);
            _authorizationService.EnsureCanDeleteOrganization(userMap);

            _organizationRepository.Delete(organizationId);
            _unitOfWork.Commit();

            return new ResultObject<bool> { Success = true, Data = true };
        }
        catch (DomainException ex)
        {
            return new ResultObject<bool> { Success = false, Message = ex.Message, Exception = ex };
        }
    }

    public ResultObject<Organization> GetOrganization(int organizationId, int userId)
    {
        try
        {
            var organization = _organizationRepository.GetById(organizationId);
            if (organization == null)
                throw new EntityNotFoundException("Organization", organizationId);

            var userMap = _organizationRepository.GetUserMap(organizationId, userId);
            _authorizationService.EnsureHasAccess(userMap);

            return new ResultObject<Organization> { Success = true, Data = organization };
        }
        catch (DomainException ex)
        {
            return new ResultObject<Organization> { Success = false, Message = ex.Message, Exception = ex };
        }
    }

    public ResultObject<List<Organization>> GetUserOrganizations(int userId)
    {
        try
        {
            var organizations = _organizationRepository.GetByUserId(userId);
            return new ResultObject<List<Organization>> { Success = true, Data = organizations };
        }
        catch (DomainException ex)
        {
            return new ResultObject<List<Organization>> { Success = false, Message = ex.Message, Exception = ex };
        }
    }

    public ResultObject<bool> AddUserToOrganization(int organizationId, OrganizationAddUserRequest request, int userId)
    {
        try
        {
            var organization = _organizationRepository.GetById(organizationId);
            if (organization == null)
                throw new EntityNotFoundException("Organization", organizationId);

            var requestingUserMap = _organizationRepository.GetUserMap(organizationId, userId);
            _authorizationService.EnsureCanEditOrganization(requestingUserMap);

            var userToAdd = _userRepository.GetById(request.UserId);
            if (userToAdd == null)
                throw new EntityNotFoundException("User", request.UserId);

            var role = Enum.TryParse<OrganizationRole>(request.Role, true, out var r) ? r : OrganizationRole.Member;
            _organizationRepository.AddUserToOrganization(organizationId, request.UserId, role);
            _unitOfWork.Commit();

            return new ResultObject<bool> { Success = true, Data = true };
        }
        catch (DomainException ex)
        {
            return new ResultObject<bool> { Success = false, Message = ex.Message, Exception = ex };
        }
    }

    public ResultObject<bool> RemoveUserFromOrganization(int organizationId, int userIdToRemove, int requestingUserId)
    {
        try
        {
            var organization = _organizationRepository.GetById(organizationId);
            if (organization == null)
                throw new EntityNotFoundException("Organization", organizationId);

            var requestingUserMap = _organizationRepository.GetUserMap(organizationId, requestingUserId);
            _authorizationService.EnsureCanEditOrganization(requestingUserMap);

            var userToRemoveMap = _organizationRepository.GetUserMap(organizationId, userIdToRemove);
            if (userToRemoveMap != null)
            {
                var allMaps = _organizationRepository.GetOrganizationUserMaps(organizationId);
                _membershipService.ValidateRemoval(userToRemoveMap, allMaps);
            }

            _organizationRepository.RemoveUserFromOrganization(organizationId, userIdToRemove);
            _unitOfWork.Commit();

            return new ResultObject<bool> { Success = true, Data = true };
        }
        catch (DomainException ex)
        {
            return new ResultObject<bool> { Success = false, Message = ex.Message, Exception = ex };
        }
    }

    public ResultObject<List<User>> GetOrganizationUsers(int organizationId, int userId)
    {
        try
        {
            var organization = _organizationRepository.GetById(organizationId);
            if (organization == null)
                throw new EntityNotFoundException("Organization", organizationId);

            var userMap = _organizationRepository.GetUserMap(organizationId, userId);
            _authorizationService.EnsureHasAccess(userMap);

            var users = _organizationRepository.GetOrganizationUsers(organizationId);
            return new ResultObject<List<User>> { Success = true, Data = users };
        }
        catch (DomainException ex)
        {
            return new ResultObject<List<User>> { Success = false, Message = ex.Message, Exception = ex };
        }
    }
}
