using System.Text.Json;

namespace EventManagement.Bookings.Infrastructure.Kafka
{
    /// <summary>
    /// Сериализация сообщений Kafka в JSON.
    /// </summary>
    internal static class KafkaJsonSerializer
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Десериализует JSON в объект указанного типа.
        /// </summary>
        public static T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, SerializerOptions);
        }

        /// <summary>
        /// Сериализует объект в JSON.
        /// </summary>
        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, SerializerOptions);
        }
    }
}
