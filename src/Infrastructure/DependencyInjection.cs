using EventManagement.Data.Interfaces;
using EventManagement.Data.Repositories;

namespace EventManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IEventRepository, InMemoryEventRepository>();

            return services;
        }
    }
}
