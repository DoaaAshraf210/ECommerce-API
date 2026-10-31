using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Storage;

public class LocalFileStorage : IFileStorage
{
    private const string RequestPath = "/uploads";

    private readonly string _root;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(
        IOptions<StorageSettings> options, IHostEnvironment env, ILogger<LocalFileStorage> logger)
    {
        _root = options.Value.GetRootPath(env.ContentRootPath);
        _logger = logger;
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(
        string folder, string extension, Stream content, CancellationToken ct = default)
    {
        var relative = $"{folder}/{Guid.NewGuid():N}{extension}";
        var fullPath = ResolvePath(relative);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var target = new FileStream(
            fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            bufferSize: 81920, useAsync: true);

        await content.CopyToAsync(target, ct);

        return $"{RequestPath}/{relative}";
    }

    public Task DeleteAsync(string url, CancellationToken ct = default)
    {
        if (!url.StartsWith(RequestPath + "/", StringComparison.Ordinal))
            return Task.CompletedTask;

        try
        {
            var fullPath = ResolvePath(url[(RequestPath.Length + 1)..]);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Could not delete stored file {Url}", url);
        }

        return Task.CompletedTask;
    }

    private string ResolvePath(string relative)
    {
        var full = Path.GetFullPath(Path.Combine(_root, relative));
        var check = Path.GetRelativePath(_root, full);

        if (check.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(check))
            throw new InvalidOperationException("Invalid storage path.");

        return full;
    }
}