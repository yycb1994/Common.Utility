using MathNet.Numerics.Distributions;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utility.Helper
{
    public class CacheHelper
    {
        IDistributedCache cache;
        public CacheHelper(IDistributedCache cache)
        {
            this.cache = cache;
        }
        public bool Exists(string cacheKey)
        {
            var res = cache.Get(cacheKey);
            return res != null;
        }
        public T Get<T>(string cacheKey)
        {
            var res = cache.Get(cacheKey);
            return res == null ? default : JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(res));
        }
        public async Task<T> GetAsync<T>(string cacheKey)
        {
            var res = await cache.GetAsync(cacheKey);
            return res == null ? default : JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(res));
        }

        public object Get(Type type, string cacheKey)
        {
            var res = cache.Get(cacheKey);
            return res == null ? default : JsonConvert.DeserializeObject(Encoding.UTF8.GetString(res), type);
        }

        public async Task<object> GetAsync(Type type, string cacheKey)
        {
            var res = await cache.GetAsync(cacheKey);
            return res == null ? default : JsonConvert.DeserializeObject(Encoding.UTF8.GetString(res), type);
        }

        public string GetString(string cacheKey)
        {
            return cache.GetString(cacheKey);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public async Task<string> GetStringAsync(string cacheKey)
        {
            return await cache.GetStringAsync(cacheKey);
        }

        public void Remove(string key)
        {
            cache.Remove(key);
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task RemoveAsync(string key)
        {
            await cache.RemoveAsync(key);
        }

        public void Set<T>(string cacheKey, T value, TimeSpan? expire = null)
        {
            cache.Set(cacheKey, GetBytes(value),
                expire == null
                    ? new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6) }
                    : new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = expire });
        }

        public void SetPermanent<T>(string cacheKey, T value)
        {
            cache.Set(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
        }

        /// <summary>
        /// 增加对象缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task SetAsync<T>(string cacheKey, T value)
        {
            await cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)),
                new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6) });
        }

        /// <summary>
        /// 增加对象缓存,并设置过期时间
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        public async Task SetAsync<T>(string cacheKey, T value, TimeSpan expire)
        {
            await cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)),
                new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = expire });
        }

        public async Task SetPermanentAsync<T>(string cacheKey, T value)
        {
            await cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
        }

        public void SetString(string cacheKey, string value, TimeSpan? expire = null)
        {
            cache.SetString(cacheKey, value,
                expire == null
                    ? new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6) }
                    : new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = expire });
        }

        /// <summary>
        /// 增加字符串缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task SetStringAsync(string cacheKey, string value)
        {
            await cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6) });
        }

        /// <summary>
        /// 增加字符串缓存,并设置过期时间
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        public async Task SetStringAsync(string cacheKey, string value, TimeSpan expire)
        {
            await cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = expire });
        }
        private byte[] GetBytes<T>(T source)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(source));
        }
    }
}
