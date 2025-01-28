using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;

using System.Text;
using System.Text.Json;

using RabbitMQ.Client;

using UsersService.Data;
using UsersService.Models;

namespace UsersService.Controllers
{
    /// <summary>
    /// Контроллер для управления пользователями и их взаимодействия с TasksService.
    /// </summary>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class UsersController : ControllerBase
    {
        #region Fields

        private readonly UsersDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IModel _channel;

        #endregion

        #region Constructor

        /// <summary>
        /// Конструктор для внедрения зависимостей.
        /// </summary>
        /// <param name="context">Контекст базы данных для работы с пользователями.</param>
        /// <param name="httpClientFactory">Фабрика для создания HTTP-клиентов.</param>
        /// <param name="model">Канал RabbitMQ для публикации сообщений.</param>
        public UsersController(UsersDbContext context, IHttpClientFactory httpClientFactory, IModel model)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _channel = model;
        }

        #endregion

        #region CRUD

        /// <summary>
        /// Получение задач пользователя через TasksService.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Список задач пользователя вместе с его данными.</returns>
        [HttpGet("{userId}/tasks")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GetUserTasks(int userId)
        {
            // Проверяем, существует ли пользователь в базе данных.
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound($"User with ID {userId} not found.");

            // Создаем HTTP-клиент и отправляем запрос к TasksService.
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"http://tasks-service:8080/api/v1/tasks/user/{userId}");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error fetching tasks.");

            // Получаем данные задач.
            var tasks = await response.Content.ReadAsStringAsync();

            // Возвращаем объединенные данные пользователя и задач.
            return Ok(new
            {
                User = user,
                Tasks = tasks
            });
        }

        /// <summary>
        /// Регистрация нового пользователя.
        /// </summary>
        /// <param name="user">Данные пользователя для регистрации.</param>
        /// <returns>Результат операции.</returns>
        [HttpPost("register")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            // Проверяем, существует ли пользователь с таким же email
            if (_context.Users.Any(u => u.Email == user.Email))
                return BadRequest("User with this email already exists.");

            // Хэшируем пароль перед сохранением
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            // Сохраняем пользователя в базе данных
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Отправляем сообщение в RabbitMQ
            var queueName = "users_queue";
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var message = JsonSerializer.Serialize(new
            {
                Id = user.Id,
                Email = user.Email,
                CreatedAt = DateTime.UtcNow
            });

            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);

            return Ok("User registered and message published successfully.");
        }

        /// <summary>
        /// Авторизация пользователя.
        /// </summary>
        /// <param name="request">Данные для входа (email и пароль).</param>
        /// <returns>Результат операции: успешный вход или ошибка.</returns>
        [HttpPost("login")]
        [MapToApiVersion("1.0")]
        public IActionResult Login([FromBody] UserLoginRequest request)
        {
            try
            {
                // Проверяем наличие email и пароля.
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest("Email and Password are required.");
                }

                // Ищем пользователя в базе данных по email.
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == request.Email);
                if (existingUser == null)
                {
                    return Unauthorized("Invalid email or password.");
                }

                // Проверяем, соответствует ли хэш пароля.
                if (!BCrypt.Net.BCrypt.Verify(request.Password, existingUser.PasswordHash))
                {
                    return Unauthorized("Invalid email or password.");
                }

                // Успешный вход.
                return Ok("Login successful.");
            }
            catch (Exception ex)
            {
                // Обработка непредвиденных ошибок.
                return StatusCode(500, $"An error occurred during login: {ex.Message}");
            }
        }

        #endregion
    }
}