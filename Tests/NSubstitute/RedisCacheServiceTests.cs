using System.Text.Json;
using NSubstitute;
using StackExchange.Redis;
using Tests.Models;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests.NSubstitute
{
    public class RedisCacheServiceTests
    {
        private readonly RedisCacheService _cacheService;
        private readonly IDatabase _mockDatabase;
        private readonly ITestOutputHelper _output;

        public RedisCacheServiceTests(ITestOutputHelper output)
        {
            _output = output;
            _output.WriteLine("--- Запуск тестов NSubstitute ->");

            var mockMultiplexer = Substitute.For<IConnectionMultiplexer>();
            _mockDatabase = Substitute.For<IDatabase>();

            mockMultiplexer.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(_mockDatabase);

            // Добавляем NullLogger для логирования
            var logger = new NullLogger<RedisCacheService>();

            _cacheService = new RedisCacheService(mockMultiplexer, logger);
        }

        [Fact]
        public async Task SetAsync_ShouldStoreComplexObjectInMockCache()
        {
            var key = "test:object";
            var testObject = new TestObject { Name = "Alice", Age = 30 };
            var serializedValue = JsonSerializer.Serialize(testObject);

            _mockDatabase.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan?>(), Arg.Any<When>(), Arg.Any<CommandFlags>())
                         .Returns(true);

            _mockDatabase.StringGetAsync(key, Arg.Any<CommandFlags>())
                         .Returns(serializedValue);

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

            _mockDatabase.StringGetAsync(key, Arg.Any<CommandFlags>()).Returns(serializedValue);

            _output.WriteLine($"Чтение из Redis: {serializedValue}");

            var result = await _cacheService.GetAsync<string>(key);

            Assert.NotNull(result);
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public async Task RemoveAsync_ShouldDeleteKeyFromCache()
        {
            var key = "test:key";
            _mockDatabase.KeyDeleteAsync(key, Arg.Any<CommandFlags>()).Returns(true);

            _output.WriteLine($"Удаление ключа {key} из Redis");

            await _cacheService.RemoveAsync(key);

            await _mockDatabase.Received(1).KeyDeleteAsync(key, Arg.Any<CommandFlags>());
        }
    }
}
