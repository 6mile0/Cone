using Ice.Services.AssignmentService;

namespace Ice.Configuration;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddIceServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register custom services here
        // services.AddScoped<IMyService, MyServiceImplementation>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        return services;
    }
}