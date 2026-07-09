using Planning.Application.Common;
using Planning.Domain.Availability;
using Planning.Domain.Enums;
using Planning.Domain.Users;

namespace Planning.Application.Availability;

public class AvailabilityService : IAvailabilityService
{
    private readonly IEmployeeAvailabilityRepository _availabilityRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public AvailabilityService(
        IEmployeeAvailabilityRepository availabilityRepository,
        IUserRepository userRepository,
        ICurrentUserContext currentUserContext)
    {
        _availabilityRepository = availabilityRepository;
        _userRepository = userRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<Result<WeekAvailabilityResponse>> GetWeekAsync(
        WeekAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result<WeekAvailabilityResponse>.Failure("Organization context is required.", "NO_ORGANIZATION");
        }

        var organizationId = _currentUserContext.OrganizationId!.Value;
        var targetUserIds = await ResolveTargetUserIdsAsync(request, organizationId, cancellationToken);
        if (targetUserIds is null)
        {
            return Result<WeekAvailabilityResponse>.Failure("You are not allowed to view this availability.", "FORBIDDEN");
        }

        var (rangeStart, rangeEnd) = GetWeekRange(request.WeekStartUtc);
        var items = await _availabilityRepository.GetByUsersAndDateRangeAsync(
            organizationId,
            targetUserIds,
            rangeStart,
            rangeEnd,
            cancellationToken);

        var usersById = await LoadUsersByIdAsync(organizationId, cancellationToken);
        var responses = AvailabilityCombiner.CombineAdjacent(
            items.Select(x => AvailabilityMapper.ToResponse(x, usersById)));
            
        return Result<WeekAvailabilityResponse>.Success(new WeekAvailabilityResponse(
            responses,
            rangeStart,
            rangeEnd));
    }

    public async Task<Result<AvailabilityResponse?>> UpsertDayPartAsync(
        UpsertDayPartAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization || _currentUserContext.UserId is null)
        {
            return Result<AvailabilityResponse>.Failure("Organization context is required.", "NO_ORGANIZATION");
        }

        var organizationId = _currentUserContext.OrganizationId!.Value;

        if (!await CanManageForUserAsync(request.UserId, organizationId, cancellationToken))
        {
            return Result<AvailabilityResponse>.Failure("You are not allowed to update this availability.", "FORBIDDEN");
        }

        if (!await UserExistsInOrganizationAsync(request.UserId, organizationId, cancellationToken))
        {
            return Result<AvailabilityResponse>.Failure("User not found.", "NOT_FOUND");
        }

        var utcNow = DateTime.UtcNow;
        var source = ResolveSource(request.UserId);
        var existing = await _availabilityRepository.GetDayPartAsync(
            organizationId,
            request.UserId,
            request.Date,
            request.DayPart,
            cancellationToken);

        if (request.IsAvailable)
        {
            if (existing is not null)
            {
                await _availabilityRepository.DeleteAsync(existing, cancellationToken);
            }

            return Result<AvailabilityResponse?>.Success(null);
        }

        try
        {
            if (existing is null)
            {
                var created = EmployeeAvailability.CreateDayPart(
                    organizationId,
                    request.UserId,
                    request.Date,
                    request.DayPart,
                    request.IsAvailable,
                    source,
                    _currentUserContext.UserId.Value,
                    request.Note,
                    utcNow);

                await _availabilityRepository.AddAsync(created, cancellationToken);
                return await BuildSingleResponseAsync(created, organizationId, cancellationToken);
            }

            existing.UpdateDayPart(
                request.IsAvailable,
                source,
                _currentUserContext.UserId.Value,
                request.Note,
                utcNow);

            await _availabilityRepository.UpdateAsync(existing, cancellationToken);
            return await BuildSingleResponseAsync(existing, organizationId, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return Result<AvailabilityResponse>.Failure(ex.Message, "VALIDATION_ERROR");
        }
    }

    public async Task<Result<AvailabilityResponse>> UpsertTimeBlockAsync(
        UpsertTimeBlockAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization || _currentUserContext.UserId is null)
        {
            return Result<AvailabilityResponse>.Failure("Organization context is required.", "NO_ORGANIZATION");
        }

        var organizationId = _currentUserContext.OrganizationId!.Value;

        if (!await CanManageForUserAsync(request.UserId, organizationId, cancellationToken))
        {
            return Result<AvailabilityResponse>.Failure("You are not allowed to update this availability.", "FORBIDDEN");
        }

        if (!await UserExistsInOrganizationAsync(request.UserId, organizationId, cancellationToken))
        {
            return Result<AvailabilityResponse>.Failure("User not found.", "NOT_FOUND");
        }

        var utcNow = DateTime.UtcNow;
        var source = ResolveSource(request.UserId);

        try
        {
            if (request.Id.HasValue)
            {
                var existing = await _availabilityRepository.GetByIdAsync(request.Id.Value, cancellationToken);
                if (existing is null || existing.OrganizationId != organizationId || existing.Type != AvailabilityType.TimeBlock)
                {
                    return Result<AvailabilityResponse>.Failure("Availability entry not found.", "NOT_FOUND");
                }

                if (!await CanManageForUserAsync(existing.UserId, organizationId, cancellationToken))
                {
                    return Result<AvailabilityResponse>.Failure("You are not allowed to update this availability.", "FORBIDDEN");
                }

                existing.UpdateTimeBlock(
                    request.StartTime,
                    request.EndTime,
                    source,
                    _currentUserContext.UserId.Value,
                    request.Note,
                    utcNow);

                await _availabilityRepository.UpdateAsync(existing, cancellationToken);
                return await BuildSingleResponseAsync(existing, organizationId, cancellationToken);
            }

            var created = EmployeeAvailability.CreateTimeBlock(
                organizationId,
                request.UserId,
                request.Date,
                request.StartTime,
                request.EndTime,
                source,
                _currentUserContext.UserId.Value,
                request.Note,
                utcNow);

            await _availabilityRepository.AddAsync(created, cancellationToken);
            return await BuildSingleResponseAsync(created, organizationId, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return Result<AvailabilityResponse>.Failure(ex.Message, "VALIDATION_ERROR");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.HasOrganization)
        {
            return Result.Failure("Organization context is required.", "NO_ORGANIZATION");
        }

        var organizationId = _currentUserContext.OrganizationId!.Value;
        var existing = await _availabilityRepository.GetByIdAsync(id, cancellationToken);

        if (existing is null || existing.OrganizationId != organizationId)
        {
            return Result.Failure("Availability entry not found.", "NOT_FOUND");
        }

        if (!await CanManageForUserAsync(existing.UserId, organizationId, cancellationToken))
        {
            return Result.Failure("You are not allowed to delete this availability.", "FORBIDDEN");
        }

        await _availabilityRepository.DeleteAsync(existing, cancellationToken);
        return Result.Success();
    }

    private async Task<Result<AvailabilityResponse>> BuildSingleResponseAsync(
        EmployeeAvailability availability,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var usersById = await LoadUsersByIdAsync(organizationId, cancellationToken);
        return Result<AvailabilityResponse>.Success(AvailabilityMapper.ToResponse(availability, usersById));
    }

    private async Task<IReadOnlyDictionary<Guid, User>> LoadUsersByIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetByOrganizationIdAsync(organizationId, cancellationToken);
        return users.ToDictionary(x => x.Id);
    }

    private async Task<IReadOnlyList<Guid>?> ResolveTargetUserIdsAsync(
        WeekAvailabilityRequest request,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.UserIds))
        {
            var parsed = request.UserIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(Guid.Parse)
                .ToList();

            foreach (var userId in parsed)
            {
                if (!await CanManageForUserAsync(userId, organizationId, cancellationToken))
                {
                    return null;
                }
            }

            return parsed;
        }

        if (request.UserId.HasValue)
        {
            if (!await CanManageForUserAsync(request.UserId.Value, organizationId, cancellationToken))
            {
                return null;
            }

            return [request.UserId.Value];
        }

        if (CanManageAnyUser())
        {
            var users = await _userRepository.GetByOrganizationIdAsync(organizationId, cancellationToken);
            return users.Where(x => x.IsActive).Select(x => x.Id).ToList();
        }

        if (_currentUserContext.UserId is null)
        {
            return null;
        }

        return [_currentUserContext.UserId.Value];
    }

    private async Task<bool> CanManageForUserAsync(
        Guid targetUserId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        if (_currentUserContext.UserId == targetUserId)
        {
            return true;
        }

        if (!CanManageAnyUser())
        {
            return false;
        }

        return await UserExistsInOrganizationAsync(targetUserId, organizationId, cancellationToken);
    }

    private bool CanManageAnyUser() =>
        _currentUserContext.Role is UserRole.Owner or UserRole.Admin or UserRole.Planner;

    private AvailabilitySource ResolveSource(Guid targetUserId) =>
        _currentUserContext.UserId == targetUserId
            ? AvailabilitySource.Employee
            : AvailabilitySource.Manager;

    private async Task<bool> UserExistsInOrganizationAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return user is not null && user.OrganizationId == organizationId;
    }

    private static (DateOnly RangeStart, DateOnly RangeEnd) GetWeekRange(DateTime weekStartUtc)
    {
        var start = DateOnly.FromDateTime(weekStartUtc.Date);
        return (start, start.AddDays(6));
    }
}
