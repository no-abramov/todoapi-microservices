using RabbitMQ.Client.Events;
using RabbitMQ.Client;

using System.Text;
using System.Text.Json;

using TasksService.Models.Events;
using TasksService.Data;
using TasksService.Models;

namespace TasksService.Service
{
    /// <summary>
    /// Сервис потребителя сообщений из RabbitMQ, который обрабатывает события создания новых пользователей.
    /// </summary>
    public class RabbitMqUserConsumer : BackgroundService
    {
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Конструктор, принимающий соединение RabbitMQ и сервис-провайдер для работы с зависимостями.
        /// </summary>
        /// <param name="connection">Соединение с RabbitMQ</param>
        /// <param name="serviceProvider">Сервис-провайдер для доступа к контексту базы данных</param>
        public RabbitMqUserConsumer(IConnection connection, IServiceProvider serviceProvider)
        {
            _channel = connection.CreateModel();
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Запускает обработчик сообщений из очереди RabbitMQ.
        /// </summary>
        /// <param name="stoppingToken">Токен отмены для завершения фоновой задачи</param>
        /// <returns>Task</returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queueName = "users_queue";

            // Объявляем очередь в RabbitMQ, если она не была создана ранее.
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Десериализуем сообщение в объект UserEvent
                var userEvent = JsonSerializer.Deserialize<UserEvent>(message);

                // Проверяем, успешно ли было десериализовано сообщение
                if (userEvent != null)
                {
                    Console.WriteLine($"————— New user created: {userEvent.Email} (ID: {userEvent.Id})");

                    // Вызываем метод для создания задачи для нового пользователя
                    await CreateInitialTaskForUser(userEvent);
                }
            };

            // Подписываемся на очередь и начинаем обработку сообщений
            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Создает стартовую задачу для нового пользователя.
        /// </summary>
        /// <param name="userEvent">Событие с данными о новом пользователе</param>
        /// <returns>Task</returns>
        private async Task CreateInitialTaskForUser(UserEvent userEvent)
        {
            // Создаем новый скоуп для работы с контекстом базы данных
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();

            // Создаем новую задачу для пользователя
            var taskItem = new TaskItem
            {
                UserId = userEvent.Id,
                Title = "Моя первая задача",
                Description = "Задание №1: успешно выполнить свою первую задачу",
                CreatedDate = DateTime.UtcNow
            };

            // Добавляем задачу в базу данных
            dbContext.Tasks.Add(taskItem);

            Console.WriteLine($"————— New task created (Data: {taskItem.Title}, {taskItem.Description} — {taskItem.CreatedDate.ToShortDateString()})");

            // Сохраняем изменения в базе данных
            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Освобождает ресурсы при остановке сервиса.
        /// </summary>
        public override void Dispose()
        {
            _channel.Close();
            _channel.Dispose();
            base.Dispose();
        }
    }
}