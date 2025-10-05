using System.Security.Claims;
using Ice.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Ice.Db.Models;

public class CustomAdminRequirement : IAuthorizationRequirement;

public class CustomAdminHandler(IceDbContext iceDbContext, IceConfiguration iceConfiguration) : AuthorizationHandler<CustomAdminRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CustomAdminRequirement requirement)
    {
        var email = context.User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
            return;
        
        var isEmergencyAdmin = iceConfiguration.AllowedEmailEndPrefixes
            .Any(prefix => email.EndsWith(prefix, StringComparison.OrdinalIgnoreCase));
        
        var isAdmin = await iceDbContext.AdminUsers
            .Where(u => u.Email == email)
            .AnyAsync();

        if (isAdmin || isEmergencyAdmin)
            context.Succeed(requirement);
    }
}