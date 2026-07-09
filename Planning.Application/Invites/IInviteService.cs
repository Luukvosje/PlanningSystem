using Planning.Application.Common;

namespace Planning.Application.Invites;

public interface IInviteService
{
    Task<Result<InviteResponse>> CreateAsync(CreateInviteRequest request, CancellationToken cancellationToken = default);
    Task<Result<AcceptInviteResponse>> AcceptAsync(AcceptInviteRequest request, CancellationToken cancellationToken = default);
    Task<Result<InvitePreviewResponse>> GetPreviewAsync(string code, CancellationToken cancellationToken = default);
}
