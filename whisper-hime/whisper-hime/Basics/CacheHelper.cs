using Microsoft.Extensions.Caching.Memory;
using Sisters.WudiLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace whisper_hime
{
    public class CacheHelper
    {
        public static object CacatDate(string key, string value, HttpApiClient api, Sisters.WudiLib.Posts.Message e, IMemoryCache cache)
        {
            object result = cache.Set(key, value,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddDays(60000)));
            return result;
        }

        public static bool boolDate(string key, IMemoryCache cache)
        {

            try
            {
                object now = cache.Get(key);
                if (now == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public static void RemoveCache(string key, IMemoryCache cache)
        {
            cache.Remove(key);
        }
    }
}
