﻿//using Finbourne.Lusid.Cache.Models.Exceptions;
//using Moq;
//using NLog;
//using Xunit;

//namespace Finbourne.Lusid.Cache.IntegrationTests
//{
//    public class MemoryCacheTests
//    {
//        [Fact]
//        public void CheckThatWeCanAddItemsNormally_ByAddingMaximumNumberOfItems_ShouldNotReturnAnyErrors()
//        {
//            // ARRANGE
//            var loggerMock = new Mock<ILogger>();
//            MemoryCache<object> memoryCache = new(3, loggerMock.Object);

//            var key1Object = new List<string>
//            {
//                "Test1",
//                "Test2"
//            };
//            var key2Object = new object();
//            var key3Object = "TestString";

//            // ACT
//            var key1AddResult = memoryCache.TryAddItem("Key1", key1Object);
//            var key2AddResult = memoryCache.TryAddItem("Key2", key2Object);
//            var key3AddResult = memoryCache.TryAddItem("Key3", key3Object);

//            // ASSERT
//            Assert.Equal(3, memoryCache.ItemCount);
//            loggerMock.Verify(x => x.Warn(It.IsAny<string>()), Times.Exactly(0));

//            var key1GetResult = memoryCache.TryGetItem("Key1", out var key1);
//            Assert.True(key1AddResult);
//            Assert.True(memoryCache.KeyExists("Key1"));
//            Assert.True(key1GetResult);
//            Assert.IsType<List<string>>(key1);

//            var key2GetResult = memoryCache.TryGetItem("Key2", out var key2);
//            Assert.True(key2AddResult);
//            Assert.True(memoryCache.KeyExists("Key2"));
//            Assert.True(key2GetResult);
//            Assert.IsType<object>(key2);

//            var key3GetResult = memoryCache.TryGetItem("Key3", out var key3);
//            Assert.True(key3AddResult);
//            Assert.True(memoryCache.KeyExists("Key3"));
//            Assert.True(key3GetResult);
//            Assert.IsType<string>(key3);

//        }

//        [Fact]
//        public void CheckThatOnlyOneInstanceOfItemIsAddres_ByAddingMultipleItemsOfSameKey_ExpectingOnlyOneItemToBeInCache()
//        {
//            // ARRANGE
//            var loggerMock = new Mock<ILogger>();
//            MemoryCache<object> memoryCache = new(3, loggerMock.Object);

//            var key1Object = new List<string>
//            {
//                "Test1",
//                "Test2"
//            };

//            // ACT
//            var key1Add1stTry = memoryCache.TryAddItem("Key1", key1Object);
//            var key1Add2ndTry = memoryCache.TryAddItem("Key1", key1Object);
//            var key1Add3rdTry = memoryCache.TryAddItem("Key1", key1Object);
//            var key1Add4thTry = memoryCache.TryAddItem("Key1", key1Object);

//            Assert.Equal(1, memoryCache.ItemCount);
//            loggerMock.Verify(x => x.Warn(It.IsAny<string>()), Times.Exactly(3));
//            Assert.True(key1Add1stTry);
//            Assert.True(key1Add2ndTry);
//            Assert.True(key1Add3rdTry);
//            Assert.True(key1Add4thTry);
//        }

//        [Fact]
//        public void CheckThatCacheCannotBeOverloaded_LoadMaximumItemsAndThenAddExtraOne_ShouldRemoveLeastUsedAndRaiseEvent()
//        {
//            var x = MemoryCacheSingleton<object>.Instance.ItemCount;

//            var loggerMock = new Mock<ILogger>();
//            MemoryCache<object> memoryCache = new(3, loggerMock.Object);

//            var cacheItem1KeyUsage = 5;
//            var cacheItem2KeyUsage = 1;
//            var cacheItem3KeyUsage = 10;

//            // Cache items
//            memoryCache.TryAddItem("CacheItem1Key", "CacheItem1");
//            memoryCache.TryAddItem("CacheItem2Key", "CacheItem2");
//            memoryCache.TryAddItem("CacheItem3Key", "CacheItem3");

//            // Increase usage count for cached items
//            for (int i = 0; i < cacheItem1KeyUsage; i++) 
//            {
//                memoryCache.TryGetItem("CacheItem1Key", out var cacheIem);
//            }

//            for (int i = 0; i < cacheItem2KeyUsage; i++)
//            {
//                memoryCache.TryGetItem("CacheItem2Key", out var cacheIem);
//            }

//            for (int i = 0; i < cacheItem3KeyUsage; i++)
//            {
//                memoryCache.TryGetItem("CacheItem3Key", out var cacheIem);
//            }

//            // Make sure that the event is raised and the correct data is passed back in the event args
//            var evt = Assert.Raises<CacheObjectRemovedEventArgs>(h => memoryCache.CacheObjectRemovedEvent += h,
//                                                                 h => memoryCache.CacheObjectRemovedEvent -= h,
//                                                                 () => memoryCache.TryAddItem("CacheItem4Key", "CacheItem4"));

//            Assert.NotNull(evt);
//            Assert.Equal(memoryCache, evt.Sender);
//            Assert.Equal(cacheItem2KeyUsage, evt.Arguments.ThresholdUsageCount);
//            Assert.Equal("CacheItem2Key", evt.Arguments.Key);

//            // Make sure that the cache contains the correct keys
//            Assert.True(memoryCache.KeyExists("CacheItem1Key"));
//            Assert.False(memoryCache.KeyExists("CacheItem2Key"));
//            Assert.True(memoryCache.KeyExists("CacheItem3Key"));
//            Assert.True(memoryCache.KeyExists("CacheItem4Key"));
//        }

//        // TEST : Singleton
//        // TEST : Check validation
//    }
//}