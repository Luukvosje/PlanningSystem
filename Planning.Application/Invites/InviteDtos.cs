
namespace Planning.Application.Invites;

/// <summary>
/// Email is optional: sharing the code by hand still works, and leaving it empty keeps the
/// existing flow unchanged. UserId targets a member who was added without a login; accepting
/// then links the account to that member instead of creating a new one. When it is set and Email
/// is not, the member's own e-mail is used for the mail.
/// </summary>
public sealed record CreateInviteRequest(string? Email = null, Guid? UserId = null);

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
