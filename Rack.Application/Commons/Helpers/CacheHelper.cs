using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Rack.Application.Commons.Helpers
{
    public static class CacheHelper
    {
        /// <summary>
        /// Gets or creates a cached value using the specified factory method
        /// </summary>
        public static async Task<T> GetOrCreateAsync<T>(
            this IMemoryCache cache,
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? expiration = null,
            CacheItemPriority priority = CacheItemPriority.Normal,
            CancellationToken cancellationToken = default)
        {
            if (cache.TryGetValue(key, out T? cachedValue))
            {
                return cachedValue!;
            }

            var value = await factory(cancellationToken);

            var options = new MemoryCacheEntryOptions
            {
                Priority = priority
            };

            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration;
            }

            // Set cache entry size based on the type of value
            if (value is ICollection<object> collection)
            {
                options.Size = collection.Count;
            }
            else
            {
                options.Size = 1;
            }

            cache.Set(key, value, options);
            return value;
        }

        /// <summary>
        /// Gets or creates a cached value using the specified factory method (synchronous version)
        /// </summary>
        public static T GetOrCreate<T>(
            this IMemoryCache cache,
            string key,
            Func<T> factory,
            TimeSpan? expiration = null,
            CacheItemPriority priority = CacheItemPriority.Normal)
        {
            if (cache.TryGetValue(key, out T? cachedValue))
            {
                return cachedValue!;
            }

            var value = factory();

            var options = new MemoryCacheEntryOptions
            {
                Priority = priority
            };

            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration;
            }

            // Set cache entry size based on the type of value
            if (value is ICollection<object> collection)
            {
                options.Size = collection.Count;
            }
            else
            {
                options.Size = 1;
            }

            cache.Set(key, value, options);
            return value;
        }

        /// <summary>
        /// Creates and returns a cache key using the specified parts
        /// </summary>
        public static string CreateCacheKey(string prefix, params object[] parts)
        {
            return $"{prefix}_{string.Join("_", parts)}";
        }
    }
}