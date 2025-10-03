using Ice.Areas.Admin.Dtos.Req;
using Ice.Db;
using Ice.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace Ice.Services.AdminUserService;

public class AdminUserService(IceDbContext iceDbContext): IAdminUserService
{
    public async Task<IReadOnlyList<AdminUsers>> GetAllAdminUsersAsync(CancellationToken cancellationToken)
    {
        return await iceDbContext.AdminUsers
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminUsers> AddAdminUserAsync(AddAdminUserDto request, CancellationToken cancellationToken)
    {
        var adminUser = new AdminUsers
        {
            FullName = request.FullName,
            TutorType = request.TutorType,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await iceDbContext.AdminUsers.AddAsync(adminUser, cancellationToken);
        await iceDbContext.SaveChangesAsync(cancellationToken);

        return adminUser;
    }

    public async Task DeleteAdminUserAsync(long adminUserId, CancellationToken cancellationToken)
    {
        var adminUser = await iceDbContext.AdminUsers
            .FirstOrDefaultAsync(u => u.Id == adminUserId, cancellationToken);

        if (adminUser == null)
        {
            throw new InvalidOperationException($"Admin user with ID {adminUserId} not found.");
        }

        iceDbContext.AdminUsers.Remove(adminUser);
        await iceDbContext.SaveChangesAsync(cancellationToken);
    }
}