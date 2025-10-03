using Ice.Services.AdminUserService;
using Ice.Services.AssignmentService;
using Ice.Services.TicketService;
using Vereyon.Web;

namespace Ice.Configuration;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddIceServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register custom services here
        // services.AddScoped<IMyService, MyServiceImplementation>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddFlashMessage();
        return services;
    }
}