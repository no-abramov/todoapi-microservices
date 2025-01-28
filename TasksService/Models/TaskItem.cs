using System.ComponentModel.DataAnnotations;

namespace TasksService.Models
{
    /// <summary>
    /// Представляет задачу пользователя (TODO).
    /// </summary>
    public class TaskItem
    {
        /// <summary>
        /// Уникальный идентификатор задачи.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Дата создания задачи.
        /// Устанавливается автоматически при создании.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Заголовок задачи.
        /// </summary>
        [Required]
        public string? Title { get; set; }

        /// <summary>
        /// Описание задачи.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Статус завершения задачи.
        /// True, если задача выполнена; False, если нет.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Уникальный идентификатор пользователя.
        /// </summary>
        [Required]
        public int UserId { get; set; }
    }

}