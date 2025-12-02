using Cone.Areas.Admin.Dtos.Req;
using Cone.Db;
using Cone.Db.Models;
using Cone.Exception;
using Microsoft.EntityFrameworkCore;

namespace Cone.Services.StudentGroupService;

public class StudentGroupService(ConeDbContext coneDbContext) : IStudentGroupService
{
    public async Task<IReadOnlyList<StudentGroups>> GetAllStudentGroupsAsync(CancellationToken cancellationToken)
    {
        return await coneDbContext.StudentGroups
            .Include(sg => sg.Tickets)
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentGroups?> GetStudentGroupByIdAsync(long? groupId, CancellationToken cancellationToken)
    {
        return await coneDbContext.StudentGroups
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

        coneDbContext.StudentGroups.Add(studentGroup);
        await coneDbContext.SaveChangesAsync(cancellationToken);

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
        var group = await coneDbContext.StudentGroups
                        .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken)
                    ?? throw new EntityNotFoundException($"Student group with ID {groupId} not found.");

        return group;
    }

    public async Task UpdateAssignmentProgressAsync(UpdateAssignmentProgressDto updateAssignmentProgressDto, CancellationToken cancellationToken)
    {
        var progress = await coneDbContext.StudentGroupAssignmentsProgress
            .FirstOrDefaultAsync(p => p.StudentGroupId == updateAssignmentProgressDto.StudentGroupId
                && p.AssignmentId == updateAssignmentProgressDto.AssignmentId, cancellationToken);

        if (progress == null)
        {
            throw new EntityNotFoundException($"Assignment progress not found for student group {updateAssignmentProgressDto.StudentGroupId} and assignment {updateAssignmentProgressDto.AssignmentId}.");
        }

        progress.Status = updateAssignmentProgressDto.Status;
        progress.UpdatedAt = DateTimeOffset.UtcNow;

        await coneDbContext.SaveChangesAsync(cancellationToken);
    }
}