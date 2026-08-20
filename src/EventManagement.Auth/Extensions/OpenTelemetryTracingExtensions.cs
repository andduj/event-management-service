using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

namespace EventManagement.Auth.Extensions
{
    /// <summary>
    /// Регистрация OpenTelemetry tracing и metrics.
    /// </summary>
    public static class OpenTelemetryTracingExtensions
    {
        public const string ServiceName = "auth-api";

        /// <summary>
        /// Подключает трейсы (OTLP → Jaeger) и метрики (экспорт Prometheus на /metrics).
        /// </summary>
        public static IServiceCollection AddOpenTelemetryTracing(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var otlpEndpoint = configuration["Otlp:Endpoint"] ?? "http://localhost:4317";
            var serviceName = configuration["Otlp:ServiceName"] ?? ServiceName;

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName: serviceName))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint)))
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter());

            return services;
        }
    }
}
