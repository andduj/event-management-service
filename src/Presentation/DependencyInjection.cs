using EventManagement.Presentation.Extensions;
namespace EventManagement.Presentation
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddSwaggerDocumentation();

            return services;
        }
    }
}
