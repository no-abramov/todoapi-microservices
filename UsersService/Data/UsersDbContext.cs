using Microsoft.EntityFrameworkCore;
using UsersService.Models;

namespace UsersService.Data
{
    /// <summary>
    /// Контекст базы данных для работы с пользователями.
    /// </summary>
    public class UsersDbContext : DbContext
    {
        /// <summary>
        /// Набор данных (таблица) для хранения и управления данными пользователей.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Конструктор, позволяющий передавать настройки подключения через параметры.
        /// </summary>
        /// <param name="options">Настройки подключения к базе данных.</param>
        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }
    }
}