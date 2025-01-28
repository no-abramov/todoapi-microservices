using System.ComponentModel.DataAnnotations;

namespace UsersService.Models
{
    /// <summary>
    /// Модель пользователя, представляющая сущность User в системе.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Уникальный идентификатор пользователя (Primary Key).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Имя пользователя. Поле обязательно для заполнения.
        /// </summary>
        [Required]
        public string? Username { get; set; }

        /// <summary>
        /// Email пользователя. Поле обязательно для заполнения.
        /// Включает валидацию формата email.
        /// </summary>
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// Хэш пароля пользователя. Поле обязательно для заполнения.
        /// </summary>
        [Required]
        public string? PasswordHash { get; set; }
    }
}