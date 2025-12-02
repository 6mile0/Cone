using Cone.Areas.Admin.Dtos.Req;
using Cone.Db;
using Cone.Db.Models;
using Cone.Exception;
using Microsoft.EntityFrameworkCore;

namespace Cone.Services.AdminUserService;

public class AdminUserService(ConeDbContext coneDbContext): IAdminUserService
{
    public async Task<IReadOnlyList<AdminUsers>> GetAllAdminUsersAsync(CancellationToken cancellationToken)
    {
        return await coneDbContext.AdminUsers
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminUsers> AddAdminUserAsync(AddAdminUserDto request, CancellationToken cancellationToken)
    {
        var adminUser = new AdminUsers
        {
            FullName = request.FullName,
            TutorType = request.TutorType,
            Email = request.Email,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await coneDbContext.AdminUsers.AddAsync(adminUser, cancellationToken);
        await coneDbContext.SaveChangesAsync(cancellationToken);

        return adminUser;
    }

    public async Task DeleteAdminUserAsync(long adminUserId, CancellationToken cancellationToken)
    {
        var adminUser = await coneDbContext.AdminUsers
            .FirstOrDefaultAsync(u => u.Id == adminUserId, cancellationToken);

        if (adminUser == null)
        {
            throw new EntityNotFoundException($"Admin user with ID {adminUserId} not found.");
        }

        coneDbContext.AdminUsers.Remove(adminUser);
        await coneDbContext.SaveChangesAsync(cancellationToken);
    }
}