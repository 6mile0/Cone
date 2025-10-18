using Ice.Db;
using Ice.Db.Models;
using Ice.Services.AdminUserService;
using Ice.Services.AssignmentService;
using Ice.Services.AssignmentStudentGroupService;
using Ice.Services.NotificationService;
using Ice.Services.StudentGroupService;
using Ice.Services.TicketService;
using Lib.AspNetCore.ServerSentEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
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

        // Background Services
        services.AddHostedService<StaffStatusBroadcastService>();

        // Custom Admin AdminRequirement
        services.AddScoped<IAuthorizationHandler, CustomAdminHandler>();

        // Register FlashMessage service
        services.AddFlashMessage();
        
        // Configuration settings
        var allowedEmailEndPrefixes = configuration.GetSection("AllowedEmailEndPrefixes").Get<List<string>>() ?? [];
        var emergencyAdminEmails = configuration.GetSection("EmergencyAdminGoogleAccounts").Get<List<string>>() ?? [];
        services.AddSingleton(new IceConfiguration
        {
            EmergencyAdminEmails = emergencyAdminEmails,
            AllowedEmailEndPrefixes = allowedEmailEndPrefixes
        });
        
        // Add Data Protection
        services.AddDataProtection()
            .PersistKeysToDbContext<IceDbContext>()
            .SetApplicationName("IceApp");
        
        return services;
    }
}