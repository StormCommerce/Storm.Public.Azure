
using System;
using Enferno.Public.Caching;
using Microsoft.ApplicationServer.Caching;

namespace Enferno.Public.Azure.Caching
{
    // TODO: add transient fault handling. See BlobStorage.cs
    public class AzureCache : BaseCache
    {       
        private readonly DataCache cache;

        public AzureCache(string name) : base(name)
        {
            var cacheFactory = new DataCacheFactory();
            cache = cacheFactory.GetCache(name);
        }

        public AzureCache(string name, int durationMinutes): this(name)
        {
            DurationMinutes = durationMinutes;
        }

        public override void FlushTag(string name)
        {
            cache.RemoveRegion(name);
        }

        protected override void AddItem(string key, object cached, TimeSpan duration)
        {
            AddItem(key, cached, duration);
        }

        protected override void AddItem(string key, object cached, TimeSpan duration, string tagName)
        {
            if (!string.IsNullOrWhiteSpace(tagName))
            {
                // TODO: How does the regions get removed when they are not used. This implmentation requires a flushtag call.
                cache.CreateRegion(tagName);                
            }
            cache.Put(key, cached, duration, tagName);
        }

        protected override object GetItem(string key)
        {
            return cache.Get(key);
        }

        protected override void RemoveItem(string key)
        {
            cache.Remove(key);
        }
    }
}
