using Planning.Application.Common;

namespace Planning.Infrastructure.Storage;

public sealed class OrganizationLogoStorage : IOrganizationLogoStorage
{
    private static readonly Dictionary<string, string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/png"] = ".png",
        ["image/jpeg"] = ".jpg",
        ["image/webp"] = ".webp",
        ["image/gif"] = ".gif",
    };

    private readonly string _directoryPath;

    public OrganizationLogoStorage(string directoryPath)
    {
        _directoryPath = directoryPath;
        Directory.CreateDirectory(_directoryPath);
    }

    public OrganizationLogoFile? Get(Guid organizationId)
    {
        var filePath = FindExistingFile(organizationId);

        if (filePath is null)
        {
            return null;
        }

        var contentType = GetContentTypeFromExtension(Path.GetExtension(filePath));
        var stream = File.OpenRead(filePath);
        return new OrganizationLogoFile(stream, contentType);
    }

    public async Task SaveAsync(
        Guid organizationId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (!AllowedContentTypes.TryGetValue(contentType, out var extension))
        {
            throw new ArgumentException("Unsupported image type.", nameof(contentType));
        }

        DeleteExistingFiles(organizationId);

        var filePath = Path.Combine(_directoryPath, $"{organizationId}{extension}");

        await using var fileStream = File.Create(filePath);
        await content.CopyToAsync(fileStream, cancellationToken);
    }

    public string? GetPublicUrl(Guid organizationId)
    {
        var filePath = FindExistingFile(organizationId);

        if (filePath is null)
        {
            return null;
        }

        return $"/img/logos/{Path.GetFileName(filePath)}";
    }

    private string? FindExistingFile(Guid organizationId)
    {
        var prefix = organizationId.ToString();
        return Directory
            .EnumerateFiles(_directoryPath, $"{prefix}.*")
            .FirstOrDefault();
    }

    private void DeleteExistingFiles(Guid organizationId)
    {
        foreach (var filePath in Directory.EnumerateFiles(_directoryPath, $"{organizationId}.*"))
        {
            File.Delete(filePath);
        }
    }

    private static string GetContentTypeFromExtension(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream",
        };
}
