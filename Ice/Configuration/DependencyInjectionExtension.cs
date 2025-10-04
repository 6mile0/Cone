using Ice.Services.AdminUserService;
using Ice.Services.AssignmentService;
using Ice.Services.AssignmentStudentGroupService;
using Ice.Services.NotificationService;
using Ice.Services.StudentGroupService;
using Ice.Services.TicketService;
using Lib.AspNetCore.ServerSentEvents;
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
        services.AddScoped<IStudentGroupService, StudentGroupService>();
        services.AddScoped<IAssignmentStudentGroupService, AssignmentStudentGroupService>();

        // Server-Sent Events
        services.AddServerSentEvents();
        services.AddSingleton<INotificationService, NotificationService>();

        // Register FlashMessage service
        services.AddFlashMessage();
        return services;
    }
}