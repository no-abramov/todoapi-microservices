using System.Text.Json;
using Moq;
using StackExchange.Redis;
using Tests.Models;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests.Moq
{
    public class RedisCacheServiceTests
    {
        private readonly RedisCacheService _cacheService;
        private readonly Mock<IDatabase> _mockDatabase;
        private readonly ITestOutputHelper _output;

        public RedisCacheServiceTests(ITestOutputHelper output)
        {
            _output = output;
            _output.WriteLine("--- Запуск тестов Moq ->");

            var mockMultiplexer = new Mock<IConnectionMultiplexer>();
            _mockDatabase = new Mock<IDatabase>();

            mockMultiplexer.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                           .Returns(_mockDatabase.Object);

            // Подключаем NullLogger, чтобы RedisCacheService не требовал настройки логирования
            var logger = new NullLogger<RedisCacheService>();

            _cacheService = new RedisCacheService(mockMultiplexer.Object, logger);
        }

        [Fact]
        public async Task SetAsync_ShouldStoreComplexObjectInCache()
        {
            var key = "test:object";
            var testObject = new TestObject { Name = "Alice", Age = 30 };
            var serializedValue = JsonSerializer.Serialize(testObject);

            _mockDatabase.Setup(db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            _mockDatabase.Setup(db => db.StringGetAsync(key, It.IsAny<CommandFlags>()))
                .ReturnsAsync(serializedValue);

            _output.WriteLine($"Сохранение объекта в Redis: {serializedValue}");

            await _cacheService.SetAsync(key, testObject);
            var cachedValue = await _cacheService.GetAsync<TestObject>(key);

            Assert.NotNull(cachedValue);
            Assert.Equal(testObject.Name, cachedValue.Name);
            Assert.Equal(testObject.Age, cachedValue.Age);
        }

        [Fact]
        public async Task GetAsync_ShouldRetrieveDataFromCache()
        {
            var key = "test:key";
            var expectedValue = "Hello, Redis!";
            var serializedValue = JsonSerializer.Serialize(expectedValue);

            _mockDatabase.Setup(db => db.StringGetAsync(key, It.IsAny<CommandFlags>()))
                         .ReturnsAsync(serializedValue);

            _output.WriteLine($"Чтение из Redis: {serializedValue}");

            var result = await _cacheService.GetAsync<string>(key);

            Assert.NotNull(result);
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public async Task RemoveAsync_ShouldDeleteKeyFromCache()
        {
            var key = "test:key";
            _mockDatabase.Setup(db => db.KeyDeleteAsync(key, It.IsAny<CommandFlags>()))
                         .ReturnsAsync(true);

            _output.WriteLine($"Удаление ключа {key} из Redis");

            await _cacheService.RemoveAsync(key);

            _mockDatabase.Verify(db => db.KeyDeleteAsync(key, It.IsAny<CommandFlags>()), Times.Once);
        }
    }
}