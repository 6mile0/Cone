using Ice.Areas.Admin.Dtos.Req;
using Ice.Areas.Admin.ViewModels.Assignment;
using Ice.Areas.Admin.ViewModels.StudentGroup;
using Ice.Exception;
using Ice.Services.AssignmentService;
using Ice.Services.AssignmentStudentGroupService;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Ice.Areas.Admin.Controllers;

[Area("admin")]
[Route("[area]/assignments")]
public class AssignmentController(
    IAssignmentService assignmentService,
    IAssignmentStudentGroupService assignmentStudentGroupService,
    IFlashMessage flashMessage) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var assignments = await assignmentService.GetAllAssignmentsAsync(cancellationToken);
        var assignmentViewModels = assignments.Select(a => new AssignmentViewModel
        {
            Id = a.Id,
            Description = a.Description,
            Name = a.Name,
            SortOrder = a.SortOrder,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            InProgressGroupCount = a.StudentGroupAssignmentsProgress?
                .Count(p => p.Status == Ice.Enums.AssignmentProgress.InProgress) ?? 0,
            CompletedGroupCount = a.StudentGroupAssignmentsProgress?
                .Count(p => p.Status == Ice.Enums.AssignmentProgress.Completed) ?? 0
        }).ToList();

        return View("Index", new AssignmentListViewModel
        {
            Assignments = assignmentViewModels
        });
    }

    // 追加
    [HttpGet("add")]
    public IActionResult AddAssignment()
    {
        return View("Add");
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddAssignment(
        [FromForm] AddAssignmentViewModel model,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            flashMessage.Danger("入力に誤りがあります。");
            return View("Add");
        }

        await assignmentService.CreateAssignmentAsync(new AddAssignmentDto
        {
            Name = model.Name,
            Description = model.Description
        }, cancellationToken);
        
        flashMessage.Info("新しい課題を追加しました。");
        return RedirectToAction("Index");
    }

    [HttpPost("sortAssignments")]
    public async Task<IActionResult> SortAssignments(
        [FromForm] EditAssignmentOrderViewModel model,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            flashMessage.Danger("並び順が不正です。");
            return RedirectToAction("Index");
        }

        foreach (var assignmentOrder in model.Assignments)
        {
            var assignment = await assignmentService.GetAssignmentByIdAsync(assignmentOrder.Id, cancellationToken);
            if (assignment == null) continue;
            assignment.SortOrder = assignmentOrder.SortOrder;
            await assignmentService.EditAssignmentAsync(assignment, cancellationToken);
        }

        flashMessage.Info("課題の並び順を更新しました。");
        return RedirectToAction("Index");
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Detail(long id, CancellationToken cancellationToken)
    {
        var assignment = await assignmentService.GetAssignmentByIdAsync(id, cancellationToken);

        if (assignment == null)
        {
            flashMessage.Danger("指定された課題が見つかりません。");
            return RedirectToAction("Index");
        }

        var assignedGroups = await assignmentService.GetAssignedStudentGroupsAsync(id, cancellationToken);
        var unassignedGroups = await assignmentService.GetUnassignedStudentGroupsAsync(id, cancellationToken);

        var notStartedGroups =
            await assignmentStudentGroupService.GetNotStartedStudentGroupsAsync(id, cancellationToken);
        var inProgressGroups =
            await assignmentStudentGroupService.GetInProgressStudentGroupsAsync(id, cancellationToken);
        var completedGroups = await assignmentStudentGroupService.GetCompletedStudentGroupsAsync(id, cancellationToken);


        // ステータスごとにグループを分類
        var viewModel = new AssignmentDetailViewModel
        {
            Id = assignment.Id,
            Name = assignment.Name,
            Description = assignment.Description,
            SortOrder = assignment.SortOrder,
            CreatedAt = assignment.CreatedAt,
            UpdatedAt = assignment.UpdatedAt,
            AssignedStudentGroups = assignedGroups.Select(g => new StudentGroupViewModel
            {
                Id = g.Id,
                GroupName = g.GroupName,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt
            }).ToList(),
            UnassignedStudentGroups = unassignedGroups.Select(g => new StudentGroupViewModel
            {
                Id = g.Id,
                GroupName = g.GroupName,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt
            }).ToList(),
            NotStartedGroups = notStartedGroups.Select(g => new StudentGroupViewModel
            {
                Id = g.Id,
                GroupName = g.GroupName,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt
            }).ToList(),
            InProgressGroups = inProgressGroups.Select(g => new StudentGroupViewModel
            {
                Id = g.Id,
                GroupName = g.GroupName,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt
            }).ToList(),
            CompletedGroups = completedGroups.Select(g => new StudentGroupViewModel
            {
                Id = g.Id,
                GroupName = g.GroupName,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt
            }).ToList()
        };

        return View("Detail", viewModel);
    }

    [HttpGet("{id:long}/edit")]
    public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
    {
        var assignment = await assignmentService.GetAssignmentByIdAsync(id, cancellationToken);

        if (assignment == null)
        {
            flashMessage.Danger("指定された課題が見つかりません。");
            return RedirectToAction("Index");
        }

        return View("Edit", new UpdateAssignmentViewModel
        {
            AssignmentId = assignment.Id,
            Name = assignment.Name,
            Description = assignment.Description
        });
    }

    [HttpPost("{id:long}/edit")]
    public async Task<IActionResult> Edit(
        long id,
        [FromForm] UpdateAssignmentViewModel model,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            flashMessage.Danger("入力に誤りがあります。");
            return View("Edit", model);
        }

        var assignment = await assignmentService.GetAssignmentByIdAsync(id, cancellationToken);
        if (assignment == null)
        {
            flashMessage.Danger("指定された課題が見つかりません。");
            return RedirectToAction("Index");
        }

        assignment.Name = model.Name;
        assignment.Description = model.Description;
        await assignmentService.EditAssignmentAsync(assignment, cancellationToken);

        flashMessage.Info("課題情報を更新しました。");
        return RedirectToAction("Detail", new { id });
    }

    [HttpPost("{assignmentId:long}/assign")]
    public async Task<IActionResult> AssignStudentGroups(
        long assignmentId,
        [FromForm] List<long> studentGroupIds,
        CancellationToken cancellationToken
    )
    {
        var assignment = await assignmentService.GetAssignmentByIdAsync(assignmentId, cancellationToken);
        if (assignment == null)
        {
            return NotFound();
        }

        if (studentGroupIds.Count == 0)
        {
            flashMessage.Warning("学生グループを選択してください。");
            return RedirectToAction("Detail", new { id = assignmentId });
        }

        try
        {
            await assignmentService.AssignToStudentGroupsAsync(assignmentId, studentGroupIds, cancellationToken);
        }
        catch (EntityNotFoundException)
        {
            flashMessage.Danger("割り当てに失敗しました。存在しない学生グループが含まれている可能性があります。");
            return RedirectToAction("Detail", new { id = assignmentId });
        }

        flashMessage.Info("学生グループに課題を割り当てました。");
        return RedirectToAction("Detail", new { id = assignmentId });
    }

    [HttpPost("{assignmentId:long}/unassign/{studentGroupId:long}")]
    public async Task<IActionResult> UnassignStudentGroup(
        long assignmentId,
        long studentGroupId,
        CancellationToken cancellationToken
    )
    {
        var assignment = await assignmentService.GetAssignmentByIdAsync(assignmentId, cancellationToken);
        if (assignment == null)
        {
            flashMessage.Danger("指定された課題が見つかりません。");
            return RedirectToAction("Index");
        }

        await assignmentService.UnassignFromStudentGroupAsync(assignmentId, studentGroupId, cancellationToken);
        flashMessage.Info("学生グループから課題の割り当てを解除しました。");
        return RedirectToAction("Detail", new { id = assignmentId });
    }
}