using Moq;
using NLog;
using Xunit;

namespace Finbourne.Lusid.Cache.UnitTests
{
    public class MemoryCacheTests
    {

        private readonly ILogger _loggerMock;

        public MemoryCacheTests()
        {
            _loggerMock = new Mock<ILogger>().Object;
        }

        #region TryAddItem Method Tests

        [Fact]
        public void ValidateTryAddItem_ByNotRunningInitialiseMethod_ShouldRaiseException()
        {
            var memoryCacheMock = MemoryCache<object>.Instance;
            var e = Assert.Throws<ArgumentNullException>(() =>
            {
                memoryCacheMock.TryAddItem(null, new object());
            });
            Assert.Equal("The cache object has not been setup.  Please run Initialise method (Parameter '_cache')", e.Message);

        }

        [Fact]
        public void ValidateTryAddItem_ByPassingInNullKey_ShouldRaiseException()
        {
            var memoryCacheMock = MemoryCache<object>.Instance;
            memoryCacheMock.Initialise(3, _loggerMock);
            var e = Assert.Throws<ArgumentNullException>(() =>
            {
                memoryCacheMock.TryAddItem(null, new object());
            });
            Assert.Equal("The cache key must have a value (Parameter 'key')", e.Message);
        }

        [Fact]
        public void ValidateTryAddItem_ByPassingInZeroThreashold_ShouldRaiseException()
        {
            var memoryCacheMock = MemoryCache<object>.Instance;
            memoryCacheMock.Initialise(0, _loggerMock);
            var e = Assert.Throws<ArgumentException>(() =>
            {
                memoryCacheMock.TryAddItem("key", new object());
            });
            Assert.Equal("Threshold usage count must be greater than 0.  Currently set to 0 (Parameter '_usageThresholdCount')", e.Message);
        }

        [Fact]
        public void ValidateTryAddItem_ByAddingTheSameKeyTwice_ShouldRaiseException()
        {
            // ARRANGE
            var loggerMock = new Mock<ILogger>();
            var memoryCacheMock = MemoryCache<object>.Instance;

            // ACT
            memoryCacheMock.Initialise(1, loggerMock.Object);
            var initialLoad = memoryCacheMock.TryAddItem("key", new object());
            var duplicateLoad = memoryCacheMock.TryAddItem("key", new object()); ;

            // ASSERT
            Assert.True(initialLoad);
            Assert.True(duplicateLoad);
            loggerMock.Verify(x => x.Warn(It.IsAny<string>()), Times.Exactly(1));
            Assert.Equal(1, memoryCacheMock.ItemCount);
        }

        [Fact]
        public void CallTryAddItem_AddNewUniqueItem_NewCacheCreated()
        {
            // ARRANGE
            var loggerMock = new Mock<ILogger>();
            var memoryCacheMock = MemoryCache<object>.Instance;

            // ACT
            memoryCacheMock.Initialise(1, loggerMock.Object);
            var initialLoad = memoryCacheMock.TryAddItem("key", new object());

            // ASSERT
            Assert.True(initialLoad);
            Assert.Equal(1, memoryCacheMock.ItemCount);
        }

        #endregion TryAddItem Method Tests

        #region TryGetItem Method Tests
        [Fact]
        public void ValidateTryGetItem_ByNotRunningInitialiseMethod_ShouldRaiseException()
        {
            // ARRANGE
            var memoryCacheMock = MemoryCache<object>.Instance;

            // ACT & ASSERT
            var e = Assert.Throws<ArgumentNullException>(() =>
            {
                memoryCacheMock.TryGetItem(null, out var cacheObject);
            });
            Assert.Equal("The cache object has not been setup.  Please run Initialise method (Parameter '_cache')", e.Message);

        }

        [Fact]
        public void ValidateTryGetItem_ByPassingInNullKey_ShouldRaiseException()
        {
            // ARRANGE
            var memoryCacheMock = MemoryCache<object>.Instance;

            // ACT & ASSERT
            memoryCacheMock.Initialise(3, _loggerMock);
            var e = Assert.Throws<ArgumentNullException>(() =>
            {
                memoryCacheMock.TryGetItem(null, out var item);
            });
            Assert.Equal("The cache key must have a value (Parameter 'key')", e.Message);
        }

        [Fact]
        public void CallTryGetItem_AddNewItemAndRetrieveIt_ShouldWorkWithNoErrors()
        {
            // ARRANGE
            const string ItemToCache = "TestCacheItem";
            var memoryCacheMock = MemoryCache<object>.Instance;

            // ACT
            memoryCacheMock.Initialise(3, _loggerMock);
            var itemAdded = memoryCacheMock.TryAddItem("key", ItemToCache);
            var itemFound = memoryCacheMock.TryGetItem("key", out var item);

            // ASSERT
            Assert.True(itemAdded);
            Assert.True(itemFound);
            Assert.Equal(ItemToCache, item);
        }

        [Fact]
        public void CallTryGetItem_AddNewItemAndRetrieveItemThatIsNotCached_ShouldReturnFalse()
        {
            // ARRANGE
            const string ItemToCache = "TestCacheItem";
            var memoryCacheMock = MemoryCache<object>.Instance;

            // ACT
            memoryCacheMock.Initialise(3, _loggerMock);
            var itemAdded = memoryCacheMock.TryAddItem("key", ItemToCache);
            var itemFound = memoryCacheMock.TryGetItem("keyNotFound", out var item);

            // ASSERT
            Assert.True(itemAdded);
            Assert.False(itemFound);
            Assert.Null(item);
        }

        #endregion TryGetItem Method Tests

        #region ItemCount Property Tests

        [Fact]
        public void CallItemCount_WithoutInitialisingCache_ShouldRaiseException()
        {
            // ARRANGE
            var memoryCacheMock = MemoryCache<object>.Instance;

            // ACT & ASSERT
            var e = Assert.Throws<ArgumentNullException>(() =>
            {
                var count = memoryCacheMock.ItemCount;
            });
            Assert.Equal("The cache object has not been setup.  Please run Initialise method (Parameter '_cache')", e.Message);
        }

        [Fact]
        public void CallItemCount_InitialisingCacheAndAddingTwoItems_ShouldReturnTwo()
        {
            // ARRANGE
            var memoryCacheMock = MemoryCache<object>.Instance;

            // ACT
            memoryCacheMock.Initialise(3, _loggerMock);
            memoryCacheMock.TryAddItem("Key1", new object());
            memoryCacheMock.TryAddItem("Key2", new object());
            var count = memoryCacheMock.ItemCount;

            // ASSERT
            Assert.Equal(2, count);
        }

        #endregion ItemCount Property Tests

        #region KeyExists Method Tests

        [Fact]
        public void CallKeyExists_WithoutInitialisingCache_ShouldRaiseException()
        {
            // ARRANGE
            var memoryCacheMock = MemoryCache<object>.Instance;

            // ACT & ASSERT
            var e = Assert.Throws<ArgumentNullException>(() =>
            {
                var count = memoryCacheMock.KeyExists("Key1");
            });
            Assert.Equal("The cache object has not been setup.  Please run Initialise method (Parameter '_cache')", e.Message);
        }

        [Fact]
        public void CallKeyExists_WithNullKey_ShouldRaiseException()
        {
            // ARRANGE
            var memoryCacheMock = MemoryCache<object>.Instance;

            // ACT
            memoryCacheMock.Initialise(3, _loggerMock);
            memoryCacheMock.TryAddItem("Key1", new object());

            // ASSERT
            var e = Assert.Throws<ArgumentNullException>(() =>
            {
                var count = memoryCacheMock.KeyExists(null);
            });
            Assert.Equal("The cache key must have a value (Parameter 'key')", e.Message);
        }

        [Fact]
        public void CallKeyExists_WithValidKey_ShouldReturnTrue()
        {
            // ARRANGE
            var memoryCacheMock = MemoryCache<object>.Instance;

            // ACT
            memoryCacheMock.Initialise(3, _loggerMock);
            memoryCacheMock.TryAddItem("Key1", new object());
            var found = memoryCacheMock.KeyExists("Key1");

            // ASSERT
            Assert.True(found);
        }

        [Fact]
        public void CallKeyExists_WithInvalidKey_ShouldReturnTrue()
        {
            // ARRANGE
            var memoryCacheMock = MemoryCache<object>.Instance;

            // ACT
            memoryCacheMock.Initialise(3, _loggerMock);
            memoryCacheMock.TryAddItem("Key1", new object());
            var found = memoryCacheMock.KeyExists("Key2");

            // ASSERT
            Assert.False(found);
        }

        #endregion KeyExists Method Tests

        #region UsageCount Method Tests

        [Fact]
        public void CallUsageCount_WithoutInitialisingCache_ShouldRaiseException()
        {
            // ARRANGE
            var memoryCacheMock = MemoryCache<object>.Instance;

            // ACT & ASSERT
            var e = Assert.Throws<ArgumentNullException>(() =>
            {
                var count = memoryCacheMock.UsageCount("Key1");
            });
            Assert.Equal("The cache object has not been setup.  Please run Initialise method (Parameter '_cache')", e.Message);
        }

        [Fact]
        public void CallUsageCount_WithNullKey_ShouldRaiseException()
        {
            // ARRANGE
            var memoryCacheMock = MemoryCache<object>.Instance;

            // ACT
            memoryCacheMock.Initialise(3, _loggerMock);
            memoryCacheMock.TryAddItem("Key1", new object());

            // ASSERT
            var e = Assert.Throws<ArgumentNullException>(() =>
            {
                var count = memoryCacheMock.UsageCount(null);
            });
            Assert.Equal("The cache key must have a value (Parameter 'key')", e.Message);
        }

        [Fact]
        public void CallUsageCount_WithValidKey_ShouldReturnFive()
        {
            // ARRANGE
            var memoryCacheMock = MemoryCache<object>.Instance;

            // ACT
            memoryCacheMock.Initialise(3, _loggerMock);
            memoryCacheMock.TryAddItem("Key1", new object());
            memoryCacheMock.TryGetItem("Key1", out _);
            memoryCacheMock.TryGetItem("Key1", out _);
            memoryCacheMock.TryGetItem("Key1", out _);
            memoryCacheMock.TryGetItem("Key1", out _);
            memoryCacheMock.TryGetItem("Key1", out _);
            var usage = memoryCacheMock.UsageCount("Key1");

            // ASSERT
            Assert.Equal(5, usage);
        }

        [Fact]
        public void CallUsageCount_WithInvalidKey_ShouldReturnMinusOne()
        {
            // ARRANGE
            var memoryCacheMock = MemoryCache<object>.Instance;

            // ACT
            memoryCacheMock.Initialise(3, _loggerMock);
            memoryCacheMock.TryAddItem("Key1", new object());
            memoryCacheMock.TryGetItem("Key1", out _);
            memoryCacheMock.TryGetItem("Key1", out _);
            var usageFound = memoryCacheMock.UsageCount("Key1");
            var usageNotFound = memoryCacheMock.UsageCount("Key2");

            // ASSERT
            Assert.Equal(2, usageFound);
            Assert.Equal(-1, usageNotFound);
        }

        #endregion UsageCount Method Tests

    }
}
