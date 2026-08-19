using NSubstitute;
using Planning.Application.Common;
using Planning.Application.Planning;
using Planning.Domain.Customers;
using Planning.Domain.Enums;
using Planning.Domain.Planning;
using Planning.Domain.Users;

namespace Planning.Tests.Application;

public class PlanningServiceTests
{
    private static readonly Guid OwnOrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherOrganizationId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    private static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly DateTime Now = new(2026, 8, 18, 10, 0, 0, DateTimeKind.Utc);

    private readonly IPlanningRecordRepository _records = Substitute.For<IPlanningRecordRepository>();
    private readonly ICustomerRepository _customers = Substitute.For<ICustomerRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ICurrentUserContext _context = Substitute.For<ICurrentUserContext>();

    public PlanningServiceTests()
    {
        _context.HasOrganization.Returns(true);
        _context.OrganizationId.Returns(OwnOrganizationId);
        _context.UserId.Returns(UserId);
        _context.Role.Returns(UserRole.Owner);

        _users.GetByOrganizationIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _customers.GetByOrganizationIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns([]);
    }

    private PlanningService CreateService() =>
        new(_records, _customers, _users, _context);

    private static PlanningRecord Record(
        Guid organizationId,
        DateTime? startUtc = null,
        DateTime? endUtc = null,
        Guid? assignedUserId = null,
        PlanningStatus status = PlanningStatus.Confirmed) =>
        PlanningRecord.Create(
            organizationId,
            customerId: null,
            assignedUserId ?? UserId,
            "Onderhoud",
            description: null,
            notes: null,
            startUtc ?? Now,
            endUtc ?? Now.AddHours(1),
            color: null,
            Now,
            status);

    // ---- Tenant isolation -------------------------------------------------
    // Another organization's record must read as NOT_FOUND, never FORBIDDEN:
    // confirming existence would leak that the record is real. See CLAUDE.md.

    [Fact]
    public async Task GetById_hides_a_record_from_another_organization()
    {
        var foreign = Record(OtherOrganizationId);
        _records.GetByIdAsync(foreign.Id, Arg.Any<CancellationToken>()).Returns(foreign);

        var result = await CreateService().GetByIdAsync(foreign.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task Delete_refuses_a_record_from_another_organization()
    {
        var foreign = Record(OtherOrganizationId);
        _records.GetByIdAsync(foreign.Id, Arg.Any<CancellationToken>()).Returns(foreign);

        var result = await CreateService().DeleteAsync(foreign.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("NOT_FOUND", result.ErrorCode);
        await _records.DidNotReceive().DeleteAsync(Arg.Any<PlanningRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Confirm_refuses_a_record_from_another_organization()
    {
        var foreign = Record(OtherOrganizationId, status: PlanningStatus.Planned);
        _records.GetByIdAsync(foreign.Id, Arg.Any<CancellationToken>()).Returns(foreign);

        var result = await CreateService().ConfirmAsync(foreign.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("NOT_FOUND", result.ErrorCode);
        await _records.DidNotReceive().UpdateAsync(Arg.Any<PlanningRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_returns_a_record_of_the_own_organization()
    {
        var own = Record(OwnOrganizationId);
        _records.GetByIdAsync(own.Id, Arg.Any<CancellationToken>()).Returns(own);

        var result = await CreateService().GetByIdAsync(own.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(own.Id, result.Value!.Id);
    }

    // ---- Overlap ----------------------------------------------------------

    private async Task<IReadOnlyList<PlanningResponse>> ListAsync(params PlanningRecord[] records)
    {
        _records.GetByOrganizationAndRangeAsync(
                Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(),
                Arg.Any<IReadOnlyList<Guid>?>(), Arg.Any<IReadOnlyList<Guid>?>(),
                Arg.Any<IReadOnlyList<PlanningStatus>?>(), Arg.Any<string?>(),
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<PlanningRecord>)records, records.Length));

        var result = await CreateService().GetListAsync(new PlanningListRequest(
            Now.AddDays(-1),
            Now.AddDays(1),
            UserIds: null,
            CustomerIds: null,
            Statuses: null,
            Search: null));

        Assert.True(result.IsSuccess);
        return result.Value!.Items;
    }

    [Fact]
    public async Task Overlapping_records_for_the_same_user_are_both_flagged()
    {
        var first = Record(OwnOrganizationId, Now, Now.AddHours(2));
        var second = Record(OwnOrganizationId, Now.AddHours(1), Now.AddHours(3));

        var items = await ListAsync(first, second);

        Assert.All(items, item => Assert.True(item.HasOverlap));
    }

    [Fact]
    public async Task Touching_records_do_not_count_as_overlap()
    {
        var first = Record(OwnOrganizationId, Now, Now.AddHours(1));
        var second = Record(OwnOrganizationId, Now.AddHours(1), Now.AddHours(2));

        var items = await ListAsync(first, second);

        Assert.All(items, item => Assert.False(item.HasOverlap));
    }

    [Fact]
    public async Task Overlapping_records_of_different_users_are_not_flagged()
    {
        var first = Record(OwnOrganizationId, Now, Now.AddHours(2));
        var second = Record(OwnOrganizationId, Now.AddHours(1), Now.AddHours(3),
            assignedUserId: Guid.Parse("33333333-3333-3333-3333-333333333333"));

        var items = await ListAsync(first, second);

        Assert.All(items, item => Assert.False(item.HasOverlap));
    }

    [Fact]
    public async Task A_cancelled_record_does_not_cause_an_overlap()
    {
        var active = Record(OwnOrganizationId, Now, Now.AddHours(2));
        var cancelled = Record(OwnOrganizationId, Now.AddHours(1), Now.AddHours(3),
            status: PlanningStatus.Cancelled);

        var items = await ListAsync(active, cancelled);

        Assert.All(items, item => Assert.False(item.HasOverlap));
    }
}
