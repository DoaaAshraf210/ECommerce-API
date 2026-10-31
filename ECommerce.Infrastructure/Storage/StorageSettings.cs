namespace ECommerce.Infrastructure.Storage;

public class StorageSettings
{
    public string UploadsPath { get; set; } = "uploads";

    public string GetRootPath(string contentRootPath)
        => Path.GetFullPath(Path.IsPathRooted(UploadsPath)
            ? UploadsPath
            : Path.Combine(contentRootPath, UploadsPath));
}