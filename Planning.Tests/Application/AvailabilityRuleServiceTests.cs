using NSubstitute;
using Planning.Application.Availability;
using Planning.Application.Common;
using Planning.Domain.Availability;
using Planning.Domain.Common;
using Planning.Domain.Enums;
using Planning.Domain.Planning;
using Planning.Domain.Users;

namespace Planning.Tests.Application;

/// <summary>
/// Availability rules carry a free-text <c>Reason</c> ("Vakantie", "Ziek"), so who may read
/// whose rules is a privacy boundary, not a convenience.
/// </summary>
public class AvailabilityRuleServiceTests
{
    private static readonly Guid OrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SelfId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ColleagueId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly DateTime Now = new(2026, 8, 18, 10, 0, 0, DateTimeKind.Utc);

    private readonly IAvailabilityRuleRepository _rules = Substitute.For<IAvailabilityRuleRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPlanningRecordRepository _records = Substitute.For<IPlanningRecordRepository>();
    private readonly ICurrentUserContext _context = Substitute.For<ICurrentUserContext>();

    public AvailabilityRuleServiceTests()
    {
        _context.HasOrganization.Returns(true);
        _context.OrganizationId.Returns(OrganizationId);
        _context.UserId.Returns(SelfId);

        _rules.GetForPlanningAsync(
                Arg.Any<Guid>(), Arg.Any<IReadOnlyList<Guid>>(),
                Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Creating and updating a dated rule looks for a shift that clashes with it; without a stub
        // the substitute hands back a null list and every one of those calls fails on it.
        _records.GetByOrganizationAndRangeAsync(
                Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(),
                Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<IReadOnlyList<Guid>?>(),
                Arg.Any<IReadOnlyList<PlanningStatus>?>(), Arg.Any<string?>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<PlanningRecord>)[], 0));

        _users.GetByIdAsync(SelfId, Arg.Any<CancellationToken>())
            .Returns(MakeUser(SelfId, UserRole.Employee));
        _users.GetByIdAsync(ColleagueId, Arg.Any<CancellationToken>())
            .Returns(MakeUser(ColleagueId, UserRole.Employee));
        _users.GetByOrganizationIdAsync(OrganizationId, Arg.Any<CancellationToken>())
            .Returns([MakeUser(SelfId, UserRole.Employee), MakeUser(ColleagueId, UserRole.Employee)]);
    }

    /// <summary>Builds a user with a known id; <c>Id</c> has a protected setter.</summary>
    private static User MakeUser(Guid id, UserRole role)
    {
        var user = User.Create(Guid.NewGuid(), OrganizationId, "Jan", "Jansen", $"{id}@acme.nl", role, Now);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!
            .GetSetMethod(nonPublic: true)!
            .Invoke(user, [id]);
        return user;
    }

    private AvailabilityRuleService CreateService() =>
        new(_rules, _users, _records, _context);

    private Task<Result<PlanningAvailabilityResponse>> GetForPlanning(string? employeeIds) =>
        CreateService().GetForPlanningAsync(new PlanningAvailabilityRequest(
            new DateOnly(2026, 8, 17),
            new DateOnly(2026, 8, 23),
            employeeIds));

    private IReadOnlyList<Guid> CapturedEmployeeIds()
    {
        var call = _rules.ReceivedCalls()
            .Single(c => c.GetMethodInfo().Name == nameof(IAvailabilityRuleRepository.GetForPlanningAsync));
        return (IReadOnlyList<Guid>)call.GetArguments()[1]!;
    }

    [Fact]
    public async Task An_employee_without_a_filter_only_sees_their_own_availability()
    {
        _context.Role.Returns(UserRole.Employee);

        var result = await GetForPlanning(null);

        Assert.True(result.IsSuccess);
        Assert.Equal([SelfId], CapturedEmployeeIds());
    }

    [Fact]
    public async Task An_employee_cannot_ask_for_a_colleagues_availability()
    {
        _context.Role.Returns(UserRole.Employee);

        var result = await GetForPlanning(ColleagueId.ToString());

        Assert.False(result.IsSuccess);
        Assert.Equal("FORBIDDEN", result.ErrorCode);
    }

    [Fact]
    public async Task An_employee_may_ask_for_their_own_availability()
    {
        _context.Role.Returns(UserRole.Employee);

        var result = await GetForPlanning(SelfId.ToString());

        Assert.True(result.IsSuccess);
        Assert.Equal([SelfId], CapturedEmployeeIds());
    }

    [Theory]
    [InlineData(UserRole.Owner)]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Planner)]
    public async Task A_planner_without_a_filter_sees_the_whole_team(UserRole role)
    {
        _context.Role.Returns(role);

        var result = await GetForPlanning(null);

        Assert.True(result.IsSuccess);
        Assert.Equal([SelfId, ColleagueId], CapturedEmployeeIds());
    }

    [Fact]
    public async Task A_planner_may_ask_for_a_colleagues_availability()
    {
        _context.Role.Returns(UserRole.Planner);

        var result = await GetForPlanning(ColleagueId.ToString());

        Assert.True(result.IsSuccess);
        Assert.Equal([ColleagueId], CapturedEmployeeIds());
    }

    // ---- Managing rules ---------------------------------------------------
    // Recording, changing and removing your own absence is all self-service. Changing one hands it
    // back to whoever has to approve it: the approval was about what the rule used to say.

    private static AvailabilityRule OwnRule() => RuleFor(SelfId);

    private static AvailabilityRule ColleagueRule() => RuleFor(ColleagueId);

    private static AvailabilityRule RuleFor(Guid employeeId) =>
        AvailabilityRule.CreateOneTime(
            OrganizationId, employeeId, new DateOnly(2026, 8, 19),
            new TimeOnly(9, 0), new TimeOnly(17, 0),
            AvailabilityRuleStatus.Unavailable, "Vakantie", ApprovalStatus.Approved, Now);

    private static User MustRequestUser(Guid id)
    {
        var user = MakeUser(id, UserRole.Employee);
        user.SetRequiresApproval(true, Now);
        return user;
    }

    private static UpdateAvailabilityRuleRequest UpdateRequest() =>
        new(null, new DateOnly(2026, 8, 20), new TimeOnly(10, 0), new TimeOnly(16, 0),
            AvailabilityRuleStatus.Unavailable, "Vakantie");

    private CreateAvailabilityRuleRequest CreateRequest(Guid employeeId) =>
        new(employeeId, AvailabilityRuleType.OneTime, null, new DateOnly(2026, 8, 19),
            new TimeOnly(9, 0), new TimeOnly(17, 0), AvailabilityRuleStatus.Unavailable, "Vakantie");

    [Fact]
    public async Task An_employee_may_record_their_own_absence()
    {
        _context.Role.Returns(UserRole.Employee);
        _records.GetByOrganizationAndRangeAsync(
                Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(),
                Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<IReadOnlyList<Guid>?>(),
                Arg.Any<IReadOnlyList<PlanningStatus>?>(), Arg.Any<string?>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<PlanningRecord>)[], 0));

        var result = await CreateService().CreateAsync(CreateRequest(SelfId));

        Assert.True(result.IsSuccess);
        await _rules.Received(1).AddAsync(Arg.Any<AvailabilityRule>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_employee_may_not_record_an_absence_for_a_colleague()
    {
        _context.Role.Returns(UserRole.Employee);

        var result = await CreateService().CreateAsync(CreateRequest(ColleagueId));

        Assert.False(result.IsSuccess);
        Assert.Equal("FORBIDDEN", result.ErrorCode);
        await _rules.DidNotReceive().AddAsync(Arg.Any<AvailabilityRule>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_employee_may_delete_their_own_rule()
    {
        _context.Role.Returns(UserRole.Employee);
        var rule = OwnRule();
        _rules.GetByIdAsync(rule.Id, Arg.Any<CancellationToken>()).Returns(rule);

        var result = await CreateService().DeleteAsync(rule.Id);

        Assert.True(result.IsSuccess);
        await _rules.Received(1).DeleteAsync(rule, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_employee_may_not_delete_a_rule_of_a_colleague()
    {
        _context.Role.Returns(UserRole.Employee);
        var rule = ColleagueRule();
        _rules.GetByIdAsync(rule.Id, Arg.Any<CancellationToken>()).Returns(rule);

        var result = await CreateService().DeleteAsync(rule.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("FORBIDDEN", result.ErrorCode);
        await _rules.DidNotReceive().DeleteAsync(Arg.Any<AvailabilityRule>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Changing_an_approved_rule_puts_it_back_in_review_when_the_member_has_to_request()
    {
        _context.Role.Returns(UserRole.Employee);
        _users.GetByIdAsync(SelfId, Arg.Any<CancellationToken>()).Returns(MustRequestUser(SelfId));
        var rule = OwnRule();
        _rules.GetByIdAsync(rule.Id, Arg.Any<CancellationToken>()).Returns(rule);

        var result = await CreateService().UpdateAsync(rule.Id, UpdateRequest());

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.Error}");
        Assert.Equal(ApprovalStatus.Pending, result.Value!.ApprovalStatus);
        await _rules.Received(1).UpdateAsync(rule, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Changing_your_own_rule_stays_in_force_when_you_do_not_have_to_request()
    {
        _context.Role.Returns(UserRole.Employee);
        var rule = OwnRule();
        _rules.GetByIdAsync(rule.Id, Arg.Any<CancellationToken>()).Returns(rule);

        var result = await CreateService().UpdateAsync(rule.Id, UpdateRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal(ApprovalStatus.Approved, result.Value!.ApprovalStatus);
    }

    [Fact]
    public async Task An_employee_may_not_change_a_rule_of_a_colleague()
    {
        _context.Role.Returns(UserRole.Employee);
        var rule = ColleagueRule();
        _rules.GetByIdAsync(rule.Id, Arg.Any<CancellationToken>()).Returns(rule);

        var result = await CreateService().UpdateAsync(rule.Id, UpdateRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal("FORBIDDEN", result.ErrorCode);
        await _rules.DidNotReceive().UpdateAsync(Arg.Any<AvailabilityRule>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_planner_may_delete_a_rule()
    {
        _context.Role.Returns(UserRole.Planner);
        var rule = OwnRule();
        _rules.GetByIdAsync(rule.Id, Arg.Any<CancellationToken>()).Returns(rule);

        var result = await CreateService().DeleteAsync(rule.Id);

        Assert.True(result.IsSuccess);
        await _rules.Received(1).DeleteAsync(rule, Arg.Any<CancellationToken>());
    }
}
