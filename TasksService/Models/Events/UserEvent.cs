namespace TasksService.Models.Events
{
    /// <summary>
    /// Модель события создания нового пользователя, используемая для передачи данных через RabbitMQ.
    /// </summary>
    public class UserEvent
    {
        /// <summary>
        /// Уникальный идентификатор пользователя.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Email-адрес пользователя.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Дата и время создания пользователя.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}