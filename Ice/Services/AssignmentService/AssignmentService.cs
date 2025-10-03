using Ice.Areas.Admin.Dtos.Req;
using Ice.Db;
using Ice.Db.Models;
using Ice.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ice.Services.AssignmentService;

public class AssignmentService(IceDbContext iceDbContext): IAssignmentService
{
    public async Task<IReadOnlyList<Assignments>> GetAllAssignmentsAsync(CancellationToken cancellationToken)
    {
        return await iceDbContext.Assignments
            .OrderBy(a => a.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public Task<Assignments?> GetAssignmentByIdAsync(long assignmentId, CancellationToken cancellationToken)
    {
        return iceDbContext.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken);
    }

    public async Task<IReadOnlyList<Assignments>> GetAssignmentsByGroupIdAsync(long groupId, CancellationToken cancellationToken)
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
    
    private async Task CreateInitialAssignmentsAsync(Assignments assignment, CancellationToken cancellationToken)
    {
        // 各StudentGroupsに対して、作成する課題を初期ステータスで追加
        var studentGroups = await iceDbContext.StudentGroups.ToListAsync(cancellationToken);
        
        // グループ内で最初の課題はNotStartedで追加、他はLockedで追加する場合
        foreach (var (studentGroup, index) in studentGroups.Select((value, i) => (value, i)))
        {
            var status = index == 0 ? AssignmentProgress.NotStarted : AssignmentProgress.InProgress;
            var studentGroupAssignment = new StudentGroupAssignmentsProgress
            {
                StudentGroupId = studentGroup.Id,
                AssignmentId = assignment.Id,
                Status = status
            };
            await iceDbContext.StudentGroupAssignmentsProgress.AddAsync(studentGroupAssignment, cancellationToken);
            await iceDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}