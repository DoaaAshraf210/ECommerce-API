namespace ECommerce.Application.Common.Models;

public sealed record UploadedFile(long Length, Func<Stream> OpenRead);