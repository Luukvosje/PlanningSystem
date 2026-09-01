using Planning.Application.Common;

namespace Planning.Application.Auth;

public interface IPasswordResetService
{
    /// <summary>
    /// Always succeeds, whether or not the address belongs to an account. Reporting "unknown
    /// email" here would turn this endpoint into a way to find out who has an account.
    /// </summary>
    Task<Result> RequestResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    Task<Result> ResetAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
