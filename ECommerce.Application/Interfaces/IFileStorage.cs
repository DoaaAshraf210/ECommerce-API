namespace ECommerce.Application.Interfaces;

public interface IFileStorage
{
    Task<string> SaveAsync(string folder, string extension, Stream content, CancellationToken ct = default);

    Task DeleteAsync(string url, CancellationToken ct = default);
}