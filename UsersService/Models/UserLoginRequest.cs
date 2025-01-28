namespace UsersService.Models
{
    /// <summary>
    /// Модель запроса для авторизации пользователя.
    /// </summary>
    public class UserLoginRequest
    {
        /// <summary>
        /// Email пользователя, предоставляемый для входа в систему.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Пароль пользователя, предоставляемый для входа в систему.
        /// </summary>
        public string? Password { get; set; }
    }
}