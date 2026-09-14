using Microsoft.Extensions.Options;
using NSubstitute;
using Planning.Application.Common;
using Planning.Application.Invites;
using Planning.Application.Modules;
using Planning.Domain.Auth;
using Planning.Domain.Enums;
using Planning.Domain.Invites;
using Planning.Domain.Organizations;
using Planning.Domain.Users;

namespace Planning.Tests.Application;

/// <summary>
/// A targeted invite carries a member id from the request body, which makes it the one place in
/// the invite flow where a caller can name another tenant's record. These tests hold the tenant
/// rule and the link-once invariant in place.
/// </summary>
public class InviteServiceTests
{
    private static readonly Guid OwnOrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherOrganizationId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AdminUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly DateTime Now = new(2026, 9, 14, 9, 0, 0, DateTimeKind.Utc);

    private readonly IOrganizationInviteRepository _invites = Substitute.For<IOrganizationInviteRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ICurrentUserContext _context = Substitute.For<ICurrentUserContext>();

    private InviteService CreateService()
    {
        _context.IsAuthenticated.Returns(true);
        _context.HasOrganization.Returns(true);
        _context.OrganizationId.Returns(OwnOrganizationId);
        _context.UserId.Returns(AdminUserId);
        _context.Role.Returns(UserRole.Admin);

        return new InviteService(
            _invites,
            Substitute.For<IOrganizationRepository>(),
            _users,
            Substitute.For<IAccountRepository>(),
            _context,
            Substitute.For<IModuleService>(),
            Substitute.For<IEmailSender>(),
            Options.Create(new EmailOptions()));
    }

    private static User MemberOf(Guid organizationId) =>
        User.CreateWithoutAccount(organizationId, "Kevin", "Jansen", null, UserRole.Employee, Now);

    [Fact]
    public async Task Create_hides_a_member_from_another_organization()
    {
        var foreignMember = MemberOf(OtherOrganizationId);
        _users.GetByIdAsync(foreignMember.Id, Arg.Any<CancellationToken>()).Returns(foreignMember);

        var result = await CreateService().CreateAsync(new CreateInviteRequest(UserId: foreignMember.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(Failures.NotFound, result.ErrorCode);
        await _invites.DidNotReceive().AddAsync(Arg.Any<OrganizationInvite>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_refuses_to_invite_a_member_who_already_has_an_account()
    {
        var member = User.Create(Guid.NewGuid(), OwnOrganizationId, "Kevin", "Jansen", "kevin@acme.nl", UserRole.Employee, Now);
        _users.GetByIdAsync(member.Id, Arg.Any<CancellationToken>()).Returns(member);

        var result = await CreateService().CreateAsync(new CreateInviteRequest(UserId: member.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal("CONFLICT", result.ErrorCode);
    }

    [Fact]
    public async Task Create_targets_the_member_and_falls_back_to_their_email()
    {
        var member = User.CreateWithoutAccount(OwnOrganizationId, "Kevin", "Jansen", "Kevin@Acme.nl", UserRole.Employee, Now);
        _users.GetByIdAsync(member.Id, Arg.Any<CancellationToken>()).Returns(member);

        var result = await CreateService().CreateAsync(new CreateInviteRequest(UserId: member.Id));

        Assert.True(result.IsSuccess);
        await _invites.Received(1).AddAsync(
            Arg.Is<OrganizationInvite>(invite => invite.UserId == member.Id && invite.OrganizationId == OwnOrganizationId),
            Arg.Any<CancellationToken>());
    }
}
