namespace Finbourne.Caching.Test
{
    // TODO: This is intended to be a minimal set of tests for the purposes of this exercise.
    // We test some basic examples using CacheManager<int, string> only.
    // Many more scenarios could be tested, as required!

    [TestClass]
    public class CacheManagerTests
    {
        [TestMethod]
        public void AddOrUpdate_ReturnsSameValue()
        {
            var cacheManager = new CacheManager<int, string>();

            cacheManager.AddOrUpdate(1, "1");
            Assert.IsTrue(cacheManager.TryGetValue(1, out var result));
            Assert.AreEqual("1", result);
        }

        [TestMethod]
        public void AddOrUpdate_UpdatesValue()
        {
            var cacheManager = new CacheManager<int, string>();

            cacheManager.AddOrUpdate(1, "1"); 
            cacheManager.AddOrUpdate(1, "2");
            Assert.IsTrue(cacheManager.TryGetValue(1, out var result));
            Assert.AreEqual("2", result);
        }

        [TestMethod]
        public void AddOrUpdate_Capacity1_OldestEvicted()
        {
            var cacheManager = new CacheManager<int, string>(1);

            cacheManager.AddOrUpdate(1, "");
            cacheManager.AddOrUpdate(2, "");
            Assert.IsFalse(cacheManager.TryGetValue(1, out _));     // 1 was evicted when we add 2
            Assert.IsTrue(cacheManager.TryGetValue(2, out _));      // Should still have 2
        }

        [TestMethod]
        public void AddOrUpdate_Capacity2_OldestEvicted()
        {
            var cacheManager = new CacheManager<int, string>(2);

            cacheManager.AddOrUpdate(1, "");
            cacheManager.AddOrUpdate(2, "");
            cacheManager.AddOrUpdate(3, "");
            Assert.IsFalse(cacheManager.TryGetValue(1, out _));     // 1 was evicted when we add 3
            Assert.IsTrue(cacheManager.TryGetValue(2, out _));      // Should still have 2
            Assert.IsTrue(cacheManager.TryGetValue(3, out _));      // Should still have 3
        }

        [TestMethod]
        public void AddOrUpdate_Capacity2_OldestUpdateEvicted()
        {
            var cacheManager = new CacheManager<int, string>(2);

            cacheManager.AddOrUpdate(1, "");
            cacheManager.AddOrUpdate(2, "");
            Assert.IsTrue(cacheManager.TryGetValue(1, out _));      // 1 was accessed ...
            cacheManager.AddOrUpdate(3, "");                       // ... so when we do this then 2 should be evicted, not 1
            Assert.IsTrue(cacheManager.TryGetValue(1, out _));      // Should still have 1
            Assert.IsFalse(cacheManager.TryGetValue(2, out _));     // 1 was evicted when we add 3
            Assert.IsTrue(cacheManager.TryGetValue(3, out _));      // Should still have 3
        }

        [TestMethod]
        public void AddOrUpdate_Remove_GetReturnsNull()
        {
            var cacheManager = new CacheManager<int, string>();

            cacheManager.AddOrUpdate(1, "");
            cacheManager.Remove(1);
            Assert.IsFalse(cacheManager.TryGetValue(1, out _));
        }
    }
}