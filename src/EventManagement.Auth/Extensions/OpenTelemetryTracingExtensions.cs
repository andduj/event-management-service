using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

namespace EventManagement.Auth.Extensions
{
    /// <summary>
    /// Регистрация OpenTelemetry tracing.
    /// </summary>
    public static class OpenTelemetryTracingExtensions
    {
        public const string ServiceName = "auth-api";

        /// <summary>
        /// Подключает сбор трейсов (ASP.NET Core, HttpClient, EF Core) и экспорт в Jaeger через OTLP.
        /// </summary>
        public static IServiceCollection AddOpenTelemetryTracing(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var otlpEndpoint = configuration["Otlp:Endpoint"] ?? "http://localhost:4317";

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName: ServiceName))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint)));

            return services;
        }
    }
}
