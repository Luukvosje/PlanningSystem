namespace Planning.Application.Common;

public sealed record OrganizationLogoFile(Stream Content, string ContentType);

public interface IOrganizationLogoStorage
{
    OrganizationLogoFile? Get(Guid organizationId);

    Task SaveAsync(Guid organizationId, Stream content, string contentType, CancellationToken cancellationToken = default);

    string? GetPublicUrl(Guid organizationId);
}
