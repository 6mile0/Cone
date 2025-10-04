using Ice.Areas.Admin.Dtos.Req;
using Ice.Db;
using Ice.Db.Models;
using Ice.Exception;
using Microsoft.EntityFrameworkCore;

namespace Ice.Services.StudentGroupService;

public class StudentGroupService(IceDbContext iceDbContext) : IStudentGroupService
{
    public async Task<IReadOnlyList<StudentGroups>> GetAllStudentGroupsAsync(CancellationToken cancellationToken)
    {
        return await iceDbContext.StudentGroups.ToListAsync(cancellationToken);
    }

    public async Task<StudentGroups?> GetStudentGroupByIdAsync(long? groupId, CancellationToken cancellationToken)
    {
        return await iceDbContext.StudentGroups
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);
    }

    public async Task<StudentGroups> CreateStudentGroupAsync(AddStudentGroupDto addStudentGroupDto, CancellationToken cancellationToken)
    {
        var studentGroup = new StudentGroups
        {
            GroupName = addStudentGroupDto.GroupName,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        iceDbContext.StudentGroups.Add(studentGroup);
        await iceDbContext.SaveChangesAsync(cancellationToken);

        return studentGroup;
    }

    public Task<StudentGroups> EditStudentGroupAsync(StudentGroups studentGroup, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteStudentGroupAsync(long groupId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<StudentGroups> GetStudentGroupDetailAsync(long groupId, CancellationToken cancellationToken)
    {
        var group = await iceDbContext.StudentGroups
                        .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken)
                    ?? throw new EntityNotFoundException($"Student group with ID {groupId} not found.");

        return group;
    }
}