using EventManagement.Application.Interfaces;
using EventManagement.Application.Services;
using EventManagement.Data.Interfaces;
using EventManagement.Data.Repositories;

namespace EventManagement.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IEventRepository, InMemoryEventRepository>();

            services.AddAutoMapper(typeof(Program));

            return services;
        }
    }
}
