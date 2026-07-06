namespace EventManagement.Events.Application.Options
{
    /// <summary>
    /// Параметры проверки JWT-токена.
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Имя секции в конфигурации.
        /// </summary>
        public const string SectionName = "Jwt";

        /// <summary>
        /// Секретный ключ для проверки подписи токена.
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
    }
}
