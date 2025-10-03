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
            .OrderBy(a => a.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<Assignments?> GetAssignmentByIdAsync(long assignmentId, CancellationToken cancellationToken)
    {
        return await iceDbContext.Assignments
            .Include(a => a.StudentGroupAssignmentsProgress)
            .Include(a => a.TicketAssignments)
            .FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken);
    }

    public async Task<IReadOnlyList<Assignments>> GetAssignmentsByStudentGroupIdAsync(long studentGroupId, CancellationToken cancellationToken)
    {
        return await iceDbContext.Assignments
            .Include(a => a.TicketAssignments)
            .Include(a => a.StudentGroupAssignmentsProgress)
            .Where(a => a.StudentGroupAssignmentsProgress.StudentGroupId == studentGroupId)
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