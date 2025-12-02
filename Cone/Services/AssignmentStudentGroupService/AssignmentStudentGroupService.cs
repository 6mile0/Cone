using Cone.Db;
using Cone.Db.Models;
using Cone.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cone.Services.AssignmentStudentGroupService;

public class AssignmentStudentGroupService(ConeDbContext ConeDbContext): IAssignmentStudentGroupService
{
    public async Task<IReadOnlyList<StudentGroups>> GetNotStartedStudentGroupsAsync(long assignmentId, CancellationToken cancellationToken)
    {
        var notStartedGroups = await ConeDbContext.StudentGroupAssignmentsProgress
            .Where(p => p.AssignmentId == assignmentId && p.Status == AssignmentProgress.NotStarted)
            .Include(p => p.StudentGroup)
            .Select(p => p.StudentGroup!)
            .ToListAsync(cancellationToken);

        return notStartedGroups;
    }

    public async Task<IReadOnlyList<StudentGroups>> GetInProgressStudentGroupsAsync(long assignmentId, CancellationToken cancellationToken)
    {
        var inProgressGroups = await ConeDbContext.StudentGroupAssignmentsProgress
            .Where(p => p.AssignmentId == assignmentId && p.Status == AssignmentProgress.InProgress)
            .Include(p => p.StudentGroup)
            .Select(p => p.StudentGroup!)
            .ToListAsync(cancellationToken);

        return inProgressGroups;
    }

    public async Task<IReadOnlyList<StudentGroups>> GetCompletedStudentGroupsAsync(long assignmentId, CancellationToken cancellationToken)
    {
        var completedGroups = await ConeDbContext.StudentGroupAssignmentsProgress
            .Where(p => p.AssignmentId == assignmentId && p.Status == AssignmentProgress.Completed)
            .Include(p => p.StudentGroup)
            .Select(p => p.StudentGroup!)
            .ToListAsync(cancellationToken);

        return completedGroups;
    }

    public async Task UpdateBulkStatusAsync(long assignmentId, List<long> studentGroupIds, AssignmentProgress newStatus, CancellationToken cancellationToken)
    {
        var progressRecords = await ConeDbContext.StudentGroupAssignmentsProgress
            .Where(p => p.AssignmentId == assignmentId && studentGroupIds.Contains(p.StudentGroupId))
            .ToListAsync(cancellationToken);

        foreach (var record in progressRecords)
        {
            record.Status = newStatus;
            record.UpdatedAt = DateTime.UtcNow;
        }

        await ConeDbContext.SaveChangesAsync(cancellationToken);
    }
}