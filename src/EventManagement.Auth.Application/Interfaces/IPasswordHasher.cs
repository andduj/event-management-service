namespace EventManagement.Auth.Application.Interfaces
{
    /// <summary>
    /// Компонент хеширования и проверки паролей.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Вычисляет хеш пароля.
        /// </summary>
        /// <param name="password">Пароль в открытом виде.</param>
        /// <returns>Хеш пароля.</returns>
        string Hash(string password);

        /// <summary>
        /// Проверяет соответствие пароля сохранённому хешу.
        /// </summary>
        /// <param name="password">Пароль в открытом виде.</param>
        /// <param name="passwordHash">Сохранённый хеш.</param>
        /// <returns><c>true</c>, если пароль совпадает с хешом.</returns>
        bool Verify(string password, string passwordHash);
    }
}
