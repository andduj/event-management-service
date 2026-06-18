namespace EventManagement.Bookings.Application.Options
{
    /// <summary>
    /// Параметры подписи и срока жизни JWT-токена.
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Имя секции в конфигурации.
        /// </summary>
        public const string SectionName = "Jwt";

        /// <summary>
        /// Секретный ключ для подписи токена.
        /// </summary>
        public string Secret { get; set; } = null!;

        /// <summary>
        /// Издатель токена.
        /// </summary>
        public string Issuer { get; set; } = null!;

        /// <summary>
        /// Аудитория токена.
        /// </summary>
        public string Audience { get; set; } = null!;

        /// <summary>
        /// Время жизни токена в минутах.
        /// </summary>
        public int LifetimeMinutes { get; set; }
    }
}
