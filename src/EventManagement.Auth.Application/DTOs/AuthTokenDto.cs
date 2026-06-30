namespace EventManagement.Auth.Application.DTOs
{
    /// <summary>
    /// Ответ с JWT-токеном после успешного входа.
    /// </summary>
    public class AuthTokenDto
    {
        /// <summary>
        /// JWT-токен доступа.
        /// </summary>
        public required string Token { get; set; }
    }
}
