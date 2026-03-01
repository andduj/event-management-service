using EventManagement.Application.Interfaces;
using EventManagement.Application.Services;

namespace EventManagement.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();

            return services;
        }
    }
}
