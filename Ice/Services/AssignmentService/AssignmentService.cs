using Ice.Areas.Admin.Dtos.Req;
using Ice.Db;
using Ice.Db.Models;
using Ice.Enums;
using Ice.Exception;
using Microsoft.EntityFrameworkCore;

namespace Ice.Services.AssignmentService;

public class AssignmentService(IceDbContext iceDbContext): IAssignmentService
{
    public async Task<IReadOnlyList<Assignments>> GetAllAssignmentsAsync(CancellationToken cancellationToken)
    {
        return await iceDbContext.Assignments
            .Include(a => a.StudentGroupAssignmentsProgress)
            .OrderBy(a => a.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<Assignments?> GetAssignmentByIdAsync(long assignmentId, CancellationToken cancellationToken)
    {
        return await iceDbContext.Assignments
            .Include(a => a.StudentGroupAssignmentsProgress)!
            .ThenInclude(p => p.StudentGroup)
            .Include(a => a.TicketAssignments)
            .FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken);
    }

    public async Task<IReadOnlyList<Assignments>> GetAssignmentsByStudentGroupIdAsync(long studentGroupId, CancellationToken cancellationToken)
    {
        return await iceDbContext.Assignments
            .Include(a => a.TicketAssignments)
            .Include(a => a.StudentGroupAssignmentsProgress)
            .Where(a => a.StudentGroupAssignmentsProgress != null && a.StudentGroupAssignmentsProgress.Any(p => p.StudentGroupId == studentGroupId))
            .OrderBy(a => a.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Assignments>> GetReleasedAssignmentByGroupId(long groupId, CancellationToken cancellationToken)
    {
        // グループごとに解放されている課題を取得
        var assignmentIds = iceDbContext.StudentGroupAssignmentsProgress
            .Where(sgap => sgap.StudentGroupId == groupId);
        
        return await iceDbContext.Assignments
            .Where(a => assignmentIds.Any(sgap => sgap.AssignmentId == a.Id))
            .OrderBy(a => a.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<Assignments> CreateAssignmentAsync(AddAssignmentDto addAssignmentDto, CancellationToken cancellationToken)
    {
        var transaction = await iceDbContext.Database.BeginTransactionAsync(cancellationToken);
        
        // 学生グループが存在しない場合はエラー
        var studentGroupCount = await iceDbContext.StudentGroups.CountAsync(cancellationToken);
        if (studentGroupCount == 0)
        {
            throw new StudentGroupNotFoundException("学生グループが1つも存在しません。先に学生グループを作成してください。");
        }
        
        // 最大値のSortOrderを取得して+1する
        var maxSortOrder = iceDbContext.Assignments
            .Max(a => (int?)a.SortOrder) ?? 0;

        var assignment = new Assignments
        {
            Name = addAssignmentDto.Name,
            Description = addAssignmentDto.Description,
            SortOrder = maxSortOrder + 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        await iceDbContext.Assignments.AddAsync(assignment, cancellationToken);
        await iceDbContext.SaveChangesAsync(cancellationToken);

        await InitialAssignmentsAsync(assignment, cancellationToken);
        
        await transaction.CommitAsync(cancellationToken);

        return assignment;
    }

    public async Task<Assignments> EditAssignmentAsync(Assignments assignment, CancellationToken cancellationToken)
    {
        var existingAssignment = await iceDbContext.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignment.Id, cancellationToken);

        if (existingAssignment == null)
        {
            throw new InvalidOperationException($"Assignment with ID {assignment.Id} not found.");
        }

        existingAssignment.Name = assignment.Name;
        existingAssignment.Description = assignment.Description;
        existingAssignment.SortOrder = assignment.SortOrder;
        existingAssignment.UpdatedAt = DateTimeOffset.UtcNow;

        await iceDbContext.SaveChangesAsync(cancellationToken);

        return existingAssignment;
    }

    public Task DeleteAssignmentAsync(long assignmentId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task AssignToStudentGroupsAsync(long assignmentId, List<long> studentGroupIds, CancellationToken cancellationToken)
    {
        var assignment = await iceDbContext.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken);

        if (assignment == null)
        {
            throw new EntityNotFoundException($"Assignment with ID {assignmentId} not found.");
        }

        foreach (var studentGroupId in studentGroupIds)
        {
            var existingProgress = await iceDbContext.StudentGroupAssignmentsProgress
                .FirstOrDefaultAsync(p => p.AssignmentId == assignmentId && p.StudentGroupId == studentGroupId, cancellationToken);

            if (existingProgress == null)
            {
                var progress = new StudentGroupAssignmentsProgress
                {
                    AssignmentId = assignmentId,
                    StudentGroupId = studentGroupId,
                    Status = AssignmentProgress.NotStarted,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                iceDbContext.StudentGroupAssignmentsProgress.Add(progress);
                await iceDbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public async Task UnassignFromStudentGroupAsync(long assignmentId, long studentGroupId, CancellationToken cancellationToken)
    {
        var progress = await iceDbContext.StudentGroupAssignmentsProgress
            .FirstOrDefaultAsync(p => p.AssignmentId == assignmentId && p.StudentGroupId == studentGroupId, cancellationToken);

        if (progress != null)
        {
            iceDbContext.StudentGroupAssignmentsProgress.Remove(progress);
            await iceDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<StudentGroups>> GetAssignedStudentGroupsAsync(long assignmentId, CancellationToken cancellationToken)
    {
        return await iceDbContext.StudentGroupAssignmentsProgress
            .Where(p => p.AssignmentId == assignmentId)
            .Include(p => p.StudentGroup)
            .Select(p => p.StudentGroup!)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentGroups>> GetUnassignedStudentGroupsAsync(long assignmentId, CancellationToken cancellationToken)
    {
        var allGroups = await iceDbContext.StudentGroups.ToListAsync(cancellationToken);
        var assignedGroupIds = await iceDbContext.StudentGroupAssignmentsProgress
            .Where(p => p.AssignmentId == assignmentId)
            .Select(p => p.StudentGroupId)
            .ToListAsync(cancellationToken);

        var unassignedGroups = allGroups
            .Where(g => !assignedGroupIds.Contains(g.Id))
            .ToList();

        return unassignedGroups;
    }

    private async Task InitialAssignmentsAsync(Assignments assignment, CancellationToken cancellationToken)
    {
        // 各StudentGroupsに対して、作成する課題を初期ステータスで追加
        var studentGroups = await iceDbContext.StudentGroups.ToListAsync(cancellationToken);

        foreach (var progress in studentGroups.Select(group => new StudentGroupAssignmentsProgress
                 {
                     StudentGroupId = group.Id,
                     AssignmentId = assignment.Id,
                     Status = AssignmentProgress.NotStarted,
                     CreatedAt = DateTimeOffset.UtcNow,
                     UpdatedAt = DateTimeOffset.UtcNow
                 }))
        {
            iceDbContext.StudentGroupAssignmentsProgress.Add(progress);
            await iceDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}