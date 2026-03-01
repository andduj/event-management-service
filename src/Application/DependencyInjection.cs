namespace EventManagement.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            //services.AddScoped<IOrderService, OrderService>();

            return services;
        }
    }
}
