using Cone.Areas.Admin.Dtos.Req;
using Cone.Db;
using Cone.Db.Models;
using Cone.Exception;
using Microsoft.EntityFrameworkCore;

namespace Cone.Services.AdminUserService;

public class AdminUserService(ConeDbContext ConeDbContext): IAdminUserService
{
    public async Task<IReadOnlyList<AdminUsers>> GetAllAdminUsersAsync(CancellationToken cancellationToken)
    {
        return await ConeDbContext.AdminUsers
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

        await ConeDbContext.AdminUsers.AddAsync(adminUser, cancellationToken);
        await ConeDbContext.SaveChangesAsync(cancellationToken);

        return adminUser;
    }

    public async Task DeleteAdminUserAsync(long adminUserId, CancellationToken cancellationToken)
    {
        var adminUser = await ConeDbContext.AdminUsers
            .FirstOrDefaultAsync(u => u.Id == adminUserId, cancellationToken);

        if (adminUser == null)
        {
            throw new EntityNotFoundException($"Admin user with ID {adminUserId} not found.");
        }

        ConeDbContext.AdminUsers.Remove(adminUser);
        await ConeDbContext.SaveChangesAsync(cancellationToken);
    }
}