using PlanningSystem.Application.Common;
using PlanningSystem.Application.DTOs.Requests;
using PlanningSystem.Application.Interfaces;
using PlanningSystem.Domain.Entities;
using PlanningSystem.Domain.Exceptions;
using PlanningSystem.Domain.Services;
using PlanningSystem.Domain.ValueObjects;

namespace PlanningSystem.Application.Services;

public class ShiftApplicationService : IShiftApplicationService
{
    private readonly IShiftRepository _shiftRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ShiftSchedulingService _schedulingService;
    private readonly OrganizationAuthorizationService _authorizationService;

    public ShiftApplicationService(
        IShiftRepository shiftRepository,
        IOrganizationRepository organizationRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ShiftSchedulingService schedulingService,
        OrganizationAuthorizationService authorizationService)
    {
        _shiftRepository = shiftRepository;
        _organizationRepository = organizationRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _schedulingService = schedulingService;
        _authorizationService = authorizationService;
    }

    public ResultObject<Shift> CreateShift(ShiftCreateRequest request, int userId)
    {
        try
        {
            _ = new TimeRange(request.StartTime, request.EndTime);

            var organization = _organizationRepository.GetById(request.OrganizationId);
            if (organization == null)
                throw new EntityNotFoundException("Organization", request.OrganizationId);

            var userMap = _organizationRepository.GetUserMap(request.OrganizationId, userId);
            _authorizationService.EnsureHasAccess(userMap, "this organization");

            var worker = _userRepository.GetById(request.WorkerId);
            if (worker == null)
                throw new EntityNotFoundException("Worker", request.WorkerId);

            var workerMap = _organizationRepository.GetUserMap(request.OrganizationId, request.WorkerId);
            if (workerMap == null)
                throw new ValidationException("Worker is not a member of this organization");

            var existingShifts = _shiftRepository.GetByWorker(request.WorkerId);
            if (_schedulingService.HasOverlappingShift(existingShifts, request.StartTime, request.EndTime))
                throw new ValidationException("Worker already has a shift scheduled during this time");

            var shift = new Shift
            {
                Id = Guid.NewGuid(),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Location = request.Location,
                Status = ShiftStatus.IsValid(request.Status) ? request.Status!.Trim().ToLowerInvariant() : ShiftStatus.Scheduled,
                OrganizationId = request.OrganizationId,
                WorkerId = request.WorkerId
            };

            shift = _shiftRepository.Create(shift);
            _unitOfWork.Commit();

            return new ResultObject<Shift> { Success = true, Data = shift };
        }
        catch (DomainException ex)
        {
            return new ResultObject<Shift> { Success = false, Message = ex.Message, Exception = ex };
        }
    }

    public ResultObject<Shift> UpdateShift(Guid shiftId, ShiftUpdateRequest request, int userId)
    {
        try
        {
            var shift = _shiftRepository.GetById(shiftId);
            if (shift == null)
                throw new EntityNotFoundException("Shift", shiftId);

            var userMap = _organizationRepository.GetUserMap(shift.OrganizationId, userId);
            _authorizationService.EnsureCanManageShifts(userMap, userId, shift.WorkerId);

            if (request.StartTime.HasValue) shift.StartTime = request.StartTime.Value;
            if (request.EndTime.HasValue) shift.EndTime = request.EndTime.Value;
            if (shift.StartTime >= shift.EndTime)
                throw new ValidationException("Start time must be before end time");

            if (!string.IsNullOrWhiteSpace(request.Location))
                shift.Location = request.Location;

            if (request.WorkerId.HasValue)
            {
                var worker = _userRepository.GetById(request.WorkerId.Value);
                if (worker == null)
                    throw new EntityNotFoundException("Worker", request.WorkerId.Value);

                var workerMap = _organizationRepository.GetUserMap(shift.OrganizationId, request.WorkerId.Value);
                if (workerMap == null)
                    throw new ValidationException("Worker is not a member of this organization");

                shift.WorkerId = request.WorkerId.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                shift.SetStatus(request.Status);
            }

            var workerId = request.WorkerId ?? shift.WorkerId;
            var startTime = request.StartTime ?? shift.StartTime;
            var endTime = request.EndTime ?? shift.EndTime;

            var existingShifts = _shiftRepository.GetByWorker(workerId);
            if (_schedulingService.HasOverlappingShift(existingShifts, startTime, endTime, shiftId))
                throw new ValidationException("Worker already has a shift scheduled during this time");

            _shiftRepository.Update(shift);
            _unitOfWork.Commit();

            return new ResultObject<Shift> { Success = true, Data = shift };
        }
        catch (DomainException ex)
        {
            return new ResultObject<Shift> { Success = false, Message = ex.Message, Exception = ex };
        }
    }

    public ResultObject<bool> DeleteShift(Guid shiftId, int userId)
    {
        try
        {
            var shift = _shiftRepository.GetById(shiftId);
            if (shift == null)
                throw new EntityNotFoundException("Shift", shiftId);

            var userMap = _organizationRepository.GetUserMap(shift.OrganizationId, userId);
            _authorizationService.EnsureCanManageShifts(userMap, userId, shift.WorkerId);

            _shiftRepository.Delete(shiftId);
            _unitOfWork.Commit();

            return new ResultObject<bool> { Success = true, Data = true };
        }
        catch (DomainException ex)
        {
            return new ResultObject<bool> { Success = false, Message = ex.Message, Exception = ex };
        }
    }

    public ResultObject<Shift> GetShift(Guid shiftId, int userId)
    {
        try
        {
            var shift = _shiftRepository.GetById(shiftId);
            if (shift == null)
                throw new EntityNotFoundException("Shift", shiftId);

            var userMap = _organizationRepository.GetUserMap(shift.OrganizationId, userId);
            _authorizationService.EnsureHasAccess(userMap, "this shift");

            return new ResultObject<Shift> { Success = true, Data = shift };
        }
        catch (DomainException ex)
        {
            return new ResultObject<Shift> { Success = false, Message = ex.Message, Exception = ex };
        }
    }

    public ResultObject<List<Shift>> GetShifts(ShiftFilterRequest? filter, int userId)
    {
        try
        {
            List<Shift> shifts;

            if (filter == null)
            {
                var userOrganizations = _organizationRepository.GetByUserId(userId);
                var organizationIds = userOrganizations.Select(o => o.Id).ToList();
                shifts = new List<Shift>();
                foreach (var orgId in organizationIds)
                {
                    var orgShifts = _shiftRepository.GetByOrganization(orgId);
                    shifts.AddRange(orgShifts);
                }
            }
            else
            {
                if (filter.OrganizationId.HasValue)
                {
                    var userMap = _organizationRepository.GetUserMap(filter.OrganizationId.Value, userId);
                    _authorizationService.EnsureHasAccess(userMap, "this organization");
                }
                shifts = _shiftRepository.GetFiltered(filter);
            }

            return new ResultObject<List<Shift>> { Success = true, Data = shifts.OrderBy(s => s.StartTime).ToList() };
        }
        catch (DomainException ex)
        {
            return new ResultObject<List<Shift>> { Success = false, Message = ex.Message, Exception = ex };
        }
    }

    public ResultObject<Shift> UpdateShiftStatus(Guid shiftId, ShiftStatusUpdateRequest request, int userId)
    {
        try
        {
            var shift = _shiftRepository.GetById(shiftId);
            if (shift == null)
                throw new EntityNotFoundException("Shift", shiftId);

            var userMap = _organizationRepository.GetUserMap(shift.OrganizationId, userId);
            _authorizationService.EnsureCanManageShifts(userMap, userId, shift.WorkerId);

            shift.SetStatus(request.Status);
            _shiftRepository.Update(shift);
            _unitOfWork.Commit();

            return new ResultObject<Shift> { Success = true, Data = shift };
        }
        catch (DomainException ex)
        {
            return new ResultObject<Shift> { Success = false, Message = ex.Message, Exception = ex };
        }
    }
}
