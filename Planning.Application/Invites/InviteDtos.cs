
namespace Planning.Application.Invites;

/// <summary>
/// Email is optional: sharing the code by hand still works, and leaving it empty keeps the
/// existing flow unchanged.
/// </summary>
public sealed record CreateInviteRequest(string? Email = null);

public sealed record InviteResponse(
    string Code,
    DateTime ExpiresAtUtc,
    bool EmailSent);

public sealed record AcceptInviteRequest(string Code);

public sealed record AcceptInviteResponse(
    Guid UserId,
    Guid OrganizationId);

public sealed record InvitePreviewResponse(
    string OrganizationName,
    DateTime ExpiresAtUtc,
    bool IsValid);
