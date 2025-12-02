using ClosedXML.Excel;
using Cone.Areas.Admin.Dtos.Req;
using Cone.Db;
using Cone.Db.Models;
using Cone.Enums;
using Cone.Exception;
using Microsoft.EntityFrameworkCore;

namespace Cone.Services.AssignmentService;

public class AssignmentService(ConeDbContext coneDbContext) : IAssignmentService
{
    public async Task<IReadOnlyList<Assignments>> GetAllAssignmentsAsync(CancellationToken cancellationToken)
    {
        return await coneDbContext.Assignments
            .Include(a => a.StudentGroupAssignmentsProgress)
            .OrderBy(a => a.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<Assignments?> GetAssignmentByIdAsync(long assignmentId, CancellationToken cancellationToken)
    {
        return await coneDbContext.Assignments
            .Include(a => a.StudentGroupAssignmentsProgress)!
            .ThenInclude(p => p.StudentGroup)
            .Include(a => a.TicketAssignments)
            .FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken);
    }

    public async Task<IReadOnlyList<Assignments>> GetAssignmentsByStudentGroupIdAsync(long studentGroupId,
        CancellationToken cancellationToken)
    {
        return await coneDbContext.Assignments
            .Include(a => a.TicketAssignments)
            .Include(a => a.StudentGroupAssignmentsProgress)
            .Where(a => a.StudentGroupAssignmentsProgress != null &&
                        a.StudentGroupAssignmentsProgress.Any(p => p.StudentGroupId == studentGroupId))
            .OrderBy(a => a.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Assignments>> GetReleasedAssignmentByGroupId(long groupId,
        CancellationToken cancellationToken)
    {
        // グループごとに解放されている課題を取得
        return await coneDbContext.Assignments
            .Include(a => a.TicketAssignments)
            .Include(a => a.StudentGroupAssignmentsProgress)
            .Where(a => a.StudentGroupAssignmentsProgress != null &&
                        a.StudentGroupAssignmentsProgress.Any(p =>
                            p.StudentGroupId == groupId && p.Status != AssignmentProgress.NotStarted))
            .OrderBy(a => a.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<Assignments> CreateAssignmentAsync(AddAssignmentDto addAssignmentDto,
        CancellationToken cancellationToken)
    {
        var transaction = await coneDbContext.Database.BeginTransactionAsync(cancellationToken);

        // 学生グループが存在しない場合はエラー
        var studentGroupCount = await coneDbContext.StudentGroups.CountAsync(cancellationToken);
        if (studentGroupCount == 0)
        {
            throw new StudentGroupNotFoundException("学生グループが1つも存在しません。先に学生グループを作成してください。");
        }

        // 最大値のSortOrderを取得して+1する
        var maxSortOrder = coneDbContext.Assignments
            .Max(a => (int?)a.SortOrder) ?? 0;

        var assignment = new Assignments
        {
            Name = addAssignmentDto.Name,
            Description = addAssignmentDto.Description,
            SortOrder = maxSortOrder + 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await coneDbContext.Assignments.AddAsync(assignment, cancellationToken);
        await coneDbContext.SaveChangesAsync(cancellationToken);

        await InitialAssignmentsAsync(assignment, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return assignment;
    }

    public async Task<Assignments> EditAssignmentAsync(Assignments assignment, CancellationToken cancellationToken)
    {
        var existingAssignment = await coneDbContext.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignment.Id, cancellationToken);

        if (existingAssignment == null)
        {
            throw new InvalidOperationException($"Assignment with ID {assignment.Id} not found.");
        }

        existingAssignment.Name = assignment.Name;
        existingAssignment.Description = assignment.Description;
        existingAssignment.SortOrder = assignment.SortOrder;
        existingAssignment.UpdatedAt = DateTimeOffset.UtcNow;

        await coneDbContext.SaveChangesAsync(cancellationToken);

        return existingAssignment;
    }

    public Task DeleteAssignmentAsync(long assignmentId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task AssignToStudentGroupsAsync(long assignmentId, List<long> studentGroupIds,
        CancellationToken cancellationToken)
    {
        var assignment = await coneDbContext.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken);

        if (assignment == null)
        {
            throw new EntityNotFoundException($"Assignment with ID {assignmentId} not found.");
        }

        foreach (var studentGroupId in studentGroupIds)
        {
            var existingProgress = await coneDbContext.StudentGroupAssignmentsProgress
                .FirstOrDefaultAsync(p => p.AssignmentId == assignmentId && p.StudentGroupId == studentGroupId,
                    cancellationToken);

            if (existingProgress != null) continue;

            var progress = new StudentGroupAssignmentsProgress
            {
                AssignmentId = assignmentId,
                StudentGroupId = studentGroupId,
                Status = AssignmentProgress.NotStarted,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await coneDbContext.StudentGroupAssignmentsProgress.AddAsync(progress, cancellationToken);
            await coneDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UnassignFromStudentGroupAsync(long assignmentId, long studentGroupId,
        CancellationToken cancellationToken)
    {
        var progress = await coneDbContext.StudentGroupAssignmentsProgress
            .FirstOrDefaultAsync(p => p.AssignmentId == assignmentId && p.StudentGroupId == studentGroupId,
                cancellationToken);

        if (progress != null)
        {
            coneDbContext.StudentGroupAssignmentsProgress.Remove(progress);
            await coneDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<StudentGroups>> GetAssignedStudentGroupsAsync(long assignmentId,
        CancellationToken cancellationToken)
    {
        return await coneDbContext.StudentGroupAssignmentsProgress
            .Where(p => p.AssignmentId == assignmentId)
            .Include(p => p.StudentGroup)
            .Select(p => p.StudentGroup!)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentGroups>> GetUnassignedStudentGroupsAsync(long assignmentId,
        CancellationToken cancellationToken)
    {
        var allGroups = await coneDbContext.StudentGroups.ToListAsync(cancellationToken);
        var assignedGroupIds = await coneDbContext.StudentGroupAssignmentsProgress
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
        var studentGroups = await coneDbContext.StudentGroups.ToListAsync(cancellationToken);

        foreach (var progress in studentGroups.Select(group =>
                     new StudentGroupAssignmentsProgress
                     {
                         StudentGroupId = group.Id,
                         AssignmentId = assignment.Id,
                         Status = AssignmentProgress.NotStarted,
                         CreatedAt = DateTimeOffset.UtcNow,
                         UpdatedAt = DateTimeOffset.UtcNow
                     }))
        {
            coneDbContext.StudentGroupAssignmentsProgress.Add(progress);
            await coneDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<byte[]> ExportAssignmentProgressToExcelAsync(CancellationToken cancellationToken)
{
    var assignments = await coneDbContext.Assignments
        .OrderBy(a => a.SortOrder)
        .ToListAsync(cancellationToken);

    var studentGroups = await coneDbContext.StudentGroups
        .OrderBy(g => g.GroupName)
        .ToListAsync(cancellationToken);

    var progressData = await coneDbContext.StudentGroupAssignmentsProgress
        .ToListAsync(cancellationToken);

    using var workbook = new XLWorkbook();
    var worksheet = workbook.Worksheets.Add("課題進捗状況");

    worksheet.Style.Font.FontName = "Yu Gothic";

    worksheet.Cell(1, 1).Value = "課題チェック";
    worksheet.Cell(1, 1).Style.Font.Bold = true;
    worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;

    for (var i = 0; i < assignments.Count; i++)
    {
        var cell = worksheet.Cell(1, i + 2);
        cell.Value = assignments[i].Name;
        cell.Style.Font.Bold = true;
        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    for (var rowIndex = 0; rowIndex < studentGroups.Count; rowIndex++)
    {
        var group = studentGroups[rowIndex];
        var excelRow = rowIndex + 2;

        worksheet.Cell(excelRow, 1).Value = group.GroupName;

        for (var colIndex = 0; colIndex < assignments.Count; colIndex++)
        {
            var assignment = assignments[colIndex];
            var excelCol = colIndex + 2;

            var progress = progressData.FirstOrDefault(p =>
                p.StudentGroupId == group.Id && p.AssignmentId == assignment.Id);

            var statusDisplay = progress?.Status switch
            {
                AssignmentProgress.Completed => "○",
                AssignmentProgress.InProgress => "進行中",
                _ => "-"
            };

            var cell = worksheet.Cell(excelRow, excelCol);
            cell.Value = statusDisplay;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            cell.Style.Fill.BackgroundColor = progress?.Status switch
            {
                AssignmentProgress.Completed => XLColor.LightGreen,
                AssignmentProgress.InProgress => XLColor.LightYellow,
                _ => cell.Style.Fill.BackgroundColor
            };
        }
    }

    worksheet.Columns().AdjustToContents();

    foreach (var col in worksheet.ColumnsUsed())
    {
        if (col.Width < 12.0)
        {
            col.Width = 12.0;
        }
    }

    using var stream = new MemoryStream();
    workbook.SaveAs(stream);
    return stream.ToArray();
}
}