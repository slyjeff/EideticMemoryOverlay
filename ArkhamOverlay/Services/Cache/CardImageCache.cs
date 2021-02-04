using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;


namespace ArkhamOverlay.Services.Cache {
    public static class CardImageCache {
        private static readonly CacheItemPolicy DEFAULT_POLICY = new CacheItemPolicy();

        public static bool SaveTocache(string cacheKey, object savedItem) {
            return MemoryCache.Default.Add(cacheKey, savedItem, DEFAULT_POLICY);
        }

        public static T GetFromCache<T>(string cacheKey) where T : class {
            return MemoryCache.Default[cacheKey] as T;
        }

        public static void RemoveFromCache(string cacheKey) {
            MemoryCache.Default.Remove(cacheKey);
        }

        public static bool IsIncache(string cacheKey) {
            return MemoryCache.Default[cacheKey] != null;
        }

    }
}
