using Ice.Areas.Admin.Dtos.Req;
using Ice.Db;
using Ice.Db.Models;
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

    public Task<Assignments> CreateAssignmentAsync(AddAssignmentDto addAssignmentDto, CancellationToken cancellationToken)
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

        return iceDbContext.Assignments.AddAsync(assignment, cancellationToken)
            .AsTask()
            .ContinueWith(t =>
            {
                iceDbContext.SaveChangesAsync(cancellationToken).Wait(cancellationToken);
                return t.Result.Entity;
            }, cancellationToken);
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
}