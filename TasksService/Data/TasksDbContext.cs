using Microsoft.EntityFrameworkCore;
using TasksService.Models;

namespace TasksService.Data
{
    /// <summary>
    /// Контекст базы данных для работы с задачами.
    /// </summary>
    public class TasksDbContext : DbContext
    {
        /// <summary>
        /// Конструктор для инициализации контекста задач.
        /// </summary>
        /// <param name="options">Параметры конфигурации DbContext.</param>
        public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options) { }

        /// <summary>
        /// Набор задач в базе данных.
        /// </summary>
        public DbSet<TaskItem> Tasks { get; set; }

        /// <summary>
        /// Конфигурация сущностей модели.
        /// </summary>
        /// <param name="modelBuilder">Построитель модели.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Указание первичного ключа для задачи
            modelBuilder.Entity<TaskItem>()
                .HasKey(t => t.Id);

            // Установка значения по умолчанию для CreatedDate
            modelBuilder.Entity<TaskItem>()
                .Property(t => t.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}