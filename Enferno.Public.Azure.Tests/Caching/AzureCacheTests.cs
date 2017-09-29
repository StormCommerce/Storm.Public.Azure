using System;
using Enferno.Public.Azure.Caching;
using Microsoft.ApplicationServer.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enferno.Public.Azure.Tests.Caching
{
    [TestClass]
    public class AzureCacheTests
    {
        private const string ItemToCache = "cached item";
        private const string CacheKey = "key1";
        private static AzureCache _cache;

        [TestInitialize]
        public void Initialize()
        {
            _cache = new AzureCache("default");
        }

        [TestCleanup]
        public void CleanUp()
        {
            _cache.Remove(CacheKey);
        }

        [TestMethod]
        [Ignore]
        public void CanAddItemToCache()
        {
            _cache.Add(CacheKey, ItemToCache, 600);
            string itemFetchedFromCache;
            var cacheHit = _cache.TryGet(CacheKey, out itemFetchedFromCache);
            Assert.IsTrue(cacheHit, "Did not get a hit in cache for key {0}", CacheKey);
            Assert.AreEqual(ItemToCache, itemFetchedFromCache, "Fetched item <{0}> is not equal to cached item <{1}>",
                itemFetchedFromCache, ItemToCache);
        }

        [TestMethod]
        [Ignore]
        public void CanAddItemWithKeyAlreadyExistingInCache()
        {
            const string newItemToCache = "new cached item";
            _cache.Add(CacheKey, ItemToCache, 600);
            try
            {
                _cache.Add(CacheKey, newItemToCache,600);
            }
            catch (DataCacheException)
            {
                Assert.Fail("Could not add an already existing key to the cache.");
            }

            string itemFetchedFromCache;
            var cacheHit = _cache.TryGet(CacheKey, out itemFetchedFromCache);
            Assert.IsTrue(cacheHit, "Did not get a hit in cache for key {0}", CacheKey);
            Assert.AreNotEqual(ItemToCache, itemFetchedFromCache, "Item in cache should not be the same as the old item for the given key.");
            Assert.AreEqual(newItemToCache, itemFetchedFromCache, "Fetched item <{0}> is not equal to cached item <{1}>",
                itemFetchedFromCache, ItemToCache);
        }

        [TestMethod]
        [Ignore]
        public void CanRemoveItemFromCache()
        {
            _cache.Add(CacheKey, ItemToCache, 600);
            string itemFetchedFromCache;
            var cacheHitBeforeRemoval = _cache.TryGet(CacheKey, out itemFetchedFromCache);
            _cache.Remove(CacheKey);
            var cacheHitAfterRemoval = _cache.TryGet(CacheKey, out itemFetchedFromCache);

            Assert.IsTrue(cacheHitBeforeRemoval, "Could not find item in cache when it should have been there");
            Assert.AreEqual(false, cacheHitAfterRemoval, "Item still in cache after removal");
        }

        [TestMethod]
        [Ignore]
        public void CanAskForItemNotPresentInCache()
        {
            string itemFetchedFromCache;
            var cacheHit = _cache.TryGet(CacheKey, out itemFetchedFromCache);

            Assert.IsFalse(cacheHit, "Got hit in cache although we shouldn't have.");
            Assert.AreEqual(null, itemFetchedFromCache, "Non existing item in cache should be null");
        }
    }
}
