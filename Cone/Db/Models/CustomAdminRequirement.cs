using System.Security.Claims;
using Cone.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Cone.Db.Models;

public class CustomAdminRequirement : IAuthorizationRequirement;

public class CustomAdminHandler(ConeDbContext ConeDbContext, ConeConfiguration ConeConfiguration) : AuthorizationHandler<CustomAdminRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CustomAdminRequirement requirement)
    {
        var email = context.User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
            return;
        
        // 緊急管理者メールアドレスリストに含まれているか確認
        var isEmergencyAdmin = ConeConfiguration.EmergencyAdminEmails.Contains(email);
        
        var isAdmin = await ConeDbContext.AdminUsers
            .Where(u => u.Email == email)
            .AnyAsync();

        if (isAdmin || isEmergencyAdmin)
            context.Succeed(requirement);
    }
}