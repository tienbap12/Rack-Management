using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.Application.Commons.Helpers
{
    /// <summary>
    /// Extension methods for IMemoryCache with proper size setting
    /// </summary>
    public static class CacheExtensions
    {
        /// <summary>
        /// Gets an item from the cache or creates it using the specified factory
        /// </summary>
        public static async Task<TItem> GetOrCreateAsync<TItem>(
            this IMemoryCache cache,
            string key,
            Func<ICacheEntry, Task<TItem>> factory,
            TimeSpan expiration,
            CacheItemPriority priority = CacheItemPriority.Normal,
            CancellationToken cancellationToken = default)
        {
            if (cache.TryGetValue(key, out TItem cachedItem))
            {
                return cachedItem;
            }

            using var entry = cache.CreateEntry(key);
            entry.AbsoluteExpirationRelativeToNow = expiration;
            entry.Priority = priority;

            var item = await factory(entry);
            entry.Value = item;

            return item;
        }

        /// <summary>
        /// Gets an item from the cache or creates it using the specified factory with size
        /// </summary>
        public static TItem GetOrCreate<TItem>(
            this IMemoryCache cache,
            string key,
            Func<ICacheEntry, TItem> factory,
            TimeSpan expiration,
            CacheItemPriority priority = CacheItemPriority.Normal,
            int size = 1)
        {
            if (cache.TryGetValue(key, out TItem cachedItem))
            {
                return cachedItem;
            }

            using var entry = cache.CreateEntry(key);
            entry.AbsoluteExpirationRelativeToNow = expiration;
            entry.Priority = priority;
            entry.Size = size;

            var item = factory(entry);
            entry.Value = item;

            return item;
        }
    }
}