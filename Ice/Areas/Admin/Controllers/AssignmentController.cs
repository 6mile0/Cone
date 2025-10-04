using Ice.Areas.Admin.Dtos.Req;
using Ice.Areas.Admin.ViewModels.Assignment;
using Ice.Areas.Admin.ViewModels.StudentGroup;
using Ice.Services.AssignmentService;
using Ice.Services.StudentGroupService;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Ice.Areas.Admin.Controllers;

[Area("admin")]
[Route("[area]/assignments")]
public class AssignmentController(IAssignmentService assignmentService, IStudentGroupService studentGroupService, IFlashMessage flashMessage) : Controller
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
            UpdatedAt = a.UpdatedAt
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
            return View("Add");
        }

        await assignmentService.CreateAssignmentAsync(new AddAssignmentDto
        {
            Name = model.Name,
            Description = model.Description
        }, cancellationToken);
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
            return BadRequest("並び順が不正です。");
        }

        foreach (var assignmentOrder in model.Assignments)
        {
            var assignment = await assignmentService.GetAssignmentByIdAsync(assignmentOrder.Id, cancellationToken);
            if (assignment == null) continue;
            assignment.SortOrder = assignmentOrder.SortOrder;
            await assignmentService.EditAssignmentAsync(assignment, cancellationToken);
        }

        return RedirectToAction("Index");
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Detail(long id, CancellationToken cancellationToken)
    {
        var assignment = await assignmentService.GetAssignmentByIdAsync(id, cancellationToken);

        if (assignment == null)
        {
            return NotFound();
        }

        var assignedGroups = await assignmentService.GetAssignedStudentGroupsAsync(id, cancellationToken);
        var allGroups = await studentGroupService.GetAllStudentGroupsAsync(cancellationToken);
        var unassignedGroups = allGroups.Where(g => assignedGroups.All(ag => ag.Id != g.Id)).ToList();

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
            return NotFound();
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
            return View("Edit", model);
        }

        var assignment = await assignmentService.GetAssignmentByIdAsync(id, cancellationToken);
        if (assignment == null)
        {
            return NotFound();
        }

        assignment.Name = model.Name;
        assignment.Description = model.Description;
        await assignmentService.EditAssignmentAsync(assignment, cancellationToken);

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

        await assignmentService.AssignToStudentGroupsAsync(assignmentId, studentGroupIds, cancellationToken);
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
            return NotFound();
        }

        await assignmentService.UnassignFromStudentGroupAsync(assignmentId, studentGroupId, cancellationToken);
        flashMessage.Info("学生グループから課題の割り当てを解除しました。");
        return RedirectToAction("Detail", new { id = assignmentId });
    }
}