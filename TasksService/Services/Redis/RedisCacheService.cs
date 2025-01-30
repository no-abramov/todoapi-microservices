using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

/// <summary>
/// Сервис для работы с Redis, предоставляющий методы кэширования, извлечения и удаления данных.
/// </summary>
public class RedisCacheService
{
    /// <summary>
    /// Экземпляр базы данных Redis для взаимодействия с кэшем.
    /// </summary>
    private readonly IDatabase _database;

    private readonly ILogger<RedisCacheService> _logger;

    /// <summary>
    /// Конструктор, инициализирующий подключение к Redis.
    /// </summary>
    /// <param name="redis">Интерфейс подключения к Redis через IConnectionMultiplexer.</param>
    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _database = redis.GetDatabase(); // Получение экземпляра базы данных Redis.
        _logger = logger;
    }

    /// <summary>
    /// Сохраняет данные в кэш Redis.
    /// </summary>
    /// <typeparam name="T">Тип данных, которые необходимо сохранить.</typeparam>
    /// <param name="key">Ключ, под которым будут храниться данные.</param>
    /// <param name="value">Данные для сохранения.</param>
    /// <param name="expiry">Время жизни записи в кэше (опционально).</param>
    /// <returns>Асинхронная задача без возвращаемого значения.</returns>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        string storedValue = value is string strValue ? strValue : JsonSerializer.Serialize(value);

        _logger.LogInformation("Setting key {Key} in Redis with value: {Value}", key, storedValue);

        bool result = await _database.StringSetAsync(key, storedValue, expiry);

        if (result)
        {
            _logger.LogInformation("Successfully stored key {Key}", key);
        }
        else
        {
            _logger.LogWarning("Failed to store key {Key}", key);
        }
    }

    /// <summary>
    /// Извлекает данные из кэша Redis.
    /// </summary>
    /// <typeparam name="T">Тип данных, которые необходимо получить.</typeparam>
    /// <param name="key">Ключ, по которому извлекаются данные.</param>
    /// <returns>Данные, сохранённые в кэше, или значение по умолчанию, если данные отсутствуют.</returns>
    public async Task<T?> GetAsync<T>(string key)
    {
        _logger.LogInformation("Retrieving key {Key} from Redis", key);
        var json = await _database.StringGetAsync(key);

        if (!json.HasValue || json.IsNullOrEmpty)
        {
            _logger.LogWarning("Key {Key} not found in Redis", key);
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json!);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize Redis value for key {Key}", key);
            return default;
        }
    }

    /// <summary>
    /// Удаляет данные из кэша Redis.
    /// </summary>
    /// <param name="key">Ключ, по которому нужно удалить данные.</param>
    /// <returns>Асинхронная задача без возвращаемого значения.</returns>
    public async Task RemoveAsync(string key)
    {
        _logger.LogInformation("Removing key {Key} from Redis", key);
        await _database.KeyDeleteAsync(key);
    }
}
