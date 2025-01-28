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

    /// <summary>
    /// Конструктор, инициализирующий подключение к Redis.
    /// </summary>
    /// <param name="redis">Интерфейс подключения к Redis через IConnectionMultiplexer.</param>
    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase(); // Получение экземпляра базы данных Redis.
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
        // Сериализация данных в JSON-строку.
        var json = JsonSerializer.Serialize(value);
        // Сохранение данных в Redis с указанным временем жизни (если оно задано).
        await _database.StringSetAsync(key, json, expiry);
    }

    /// <summary>
    /// Извлекает данные из кэша Redis.
    /// </summary>
    /// <typeparam name="T">Тип данных, которые необходимо получить.</typeparam>
    /// <param name="key">Ключ, по которому извлекаются данные.</param>
    /// <returns>Данные, сохранённые в кэше, или значение по умолчанию, если данные отсутствуют.</returns>
    public async Task<T?> GetAsync<T>(string key)
    {
        // Получение данных из Redis по ключу.
        var json = await _database.StringGetAsync(key);

        // Проверяем, что json имеет значение и не пустой
        if (!json.HasValue || json.IsNullOrEmpty)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json!);
    }

    /// <summary>
    /// Удаляет данные из кэша Redis.
    /// </summary>
    /// <param name="key">Ключ, по которому нужно удалить данные.</param>
    /// <returns>Асинхронная задача без возвращаемого значения.</returns>
    public async Task RemoveAsync(string key)
    {
        // Удаление данных по ключу из Redis.
        await _database.KeyDeleteAsync(key);
    }
}
