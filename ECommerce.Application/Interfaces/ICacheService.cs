using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Interfaces
{
    public interface ICacheService
    {
        Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken ct = default);

        Task RemoveByTagAsync(string tag, CancellationToken ct = default);
    }
}
