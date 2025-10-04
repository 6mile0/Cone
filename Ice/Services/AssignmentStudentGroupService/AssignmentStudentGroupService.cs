using Ice.Db;
using Ice.Db.Models;
using Ice.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ice.Services.AssignmentStudentGroupService;

public class AssignmentStudentGroupService(IceDbContext iceDbContext): IAssignmentStudentGroupService
{
    public async Task<IReadOnlyList<StudentGroups>> GetNotStartedStudentGroupsAsync(long assignmentId, CancellationToken cancellationToken)
    {
        var notStartedGroups = await iceDbContext.StudentGroupAssignmentsProgress
            .Where(p => p.AssignmentId == assignmentId && p.Status == AssignmentProgress.NotStarted)
            .Include(p => p.StudentGroup)
            .Select(p => p.StudentGroup!)
            .ToListAsync(cancellationToken);

        return notStartedGroups;
    }

    public async Task<IReadOnlyList<StudentGroups>> GetInProgressStudentGroupsAsync(long assignmentId, CancellationToken cancellationToken)
    {
        var inProgressGroups = await iceDbContext.StudentGroupAssignmentsProgress
            .Where(p => p.AssignmentId == assignmentId && p.Status == AssignmentProgress.InProgress)
            .Include(p => p.StudentGroup)
            .Select(p => p.StudentGroup!)
            .ToListAsync(cancellationToken);

        return inProgressGroups;
    }

    public async Task<IReadOnlyList<StudentGroups>> GetCompletedStudentGroupsAsync(long assignmentId, CancellationToken cancellationToken)
    {
        var completedGroups = await iceDbContext.StudentGroupAssignmentsProgress
            .Where(p => p.AssignmentId == assignmentId && p.Status == AssignmentProgress.Completed)
            .Include(p => p.StudentGroup)
            .Select(p => p.StudentGroup!)
            .ToListAsync(cancellationToken);

        return completedGroups;
    }
}