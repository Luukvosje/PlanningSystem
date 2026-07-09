
namespace Planning.Application.Invites;

public sealed record CreateInviteRequest;

public sealed record InviteResponse(
    string Code,
    DateTime ExpiresAtUtc);

public sealed record AcceptInviteRequest(string Code);

public sealed record AcceptInviteResponse(
    Guid UserId,
    Guid OrganizationId);

public sealed record InvitePreviewResponse(
    string OrganizationName,
    DateTime ExpiresAtUtc,
    bool IsValid);
