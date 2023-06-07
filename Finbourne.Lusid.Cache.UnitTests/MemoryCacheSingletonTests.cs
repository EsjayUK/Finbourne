using Finbourne.Lusid.Cache.CacheStrategy;
using Moq;
using NLog;
using Xunit;

namespace Finbourne.Lusid.Cache.UnitTests
{
    public class MemoryCacheSingletonTests
    {
        private readonly ILogger _loggerMock;

        public MemoryCacheSingletonTests() 
        {
            _loggerMock = new Mock<ILogger>().Object;   
        }

        #region TryAddItem Method Tests

        [Fact]
        public void ValidateTryAddItem_ByNotRunningInitialiseMethod_ShouldRaiseException()
        {

            var memoryCacheMock = MemoryCacheSingleton<object>.Instance;
            var e = Assert.Throws<ArgumentNullException>(() => 
            { 
                memoryCacheMock.TryAddItem(null, new object());
            });
            Assert.Equal("The cache object has not been setup.  Please run Initialise method (Parameter '_cache')", e.Message);

        }

        [Fact]
        public void ValidateTryAddItem_ByPassingInNullKey_ShouldRaiseException()
        {
            var memoryCacheMock = MemoryCacheSingleton<object>.Instance;
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
            var memoryCacheMock = MemoryCacheSingleton<object>.Instance;
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
            var memoryCacheMock = MemoryCacheSingleton<object>.Instance;
            
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
            var memoryCacheMock = MemoryCacheSingleton<object>.Instance;

            // ACT
            memoryCacheMock.Initialise(1, loggerMock.Object);
            var initialLoad = memoryCacheMock.TryAddItem("key", new object());

            // ASSERT
            Assert.True(initialLoad);
            Assert.Equal(1, memoryCacheMock.ItemCount);
        }

        #endregion TryAddItem Method Tests

    }
}