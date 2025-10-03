using Ice.Areas.Admin.Dtos.Req;
using Ice.Areas.Admin.ViewModels.Assignment;
using Ice.Services.AssignmentService;
using Microsoft.AspNetCore.Mvc;

namespace Ice.Areas.Admin.Controllers;

[Area("Admin")]
[Route("[area]/assignments")]
public class AssignmentController(IAssignmentService assignmentService, ILogger<AssignmentController> logger) : Controller
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
        [FromForm] AddAssignmentDto request,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            return View("Add");
        }

        await assignmentService.CreateAssignmentAsync(request, cancellationToken);
        return RedirectToAction("Index");
    }

    [HttpPost("sortAssignments")]
    public async Task<IActionResult> SortAssignments(
        [FromForm] EditAssignmentOrderDto request,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("並び順が不正です。");
        }

        foreach (var assignmentOrder in request.Assignments)
        {
            var assignment = await assignmentService.GetAssignmentByIdAsync(assignmentOrder.Id, cancellationToken);
            if (assignment == null) continue;
            assignment.SortOrder = assignmentOrder.SortOrder;
            await assignmentService.EditAssignmentAsync(assignment, cancellationToken);
        }

        return RedirectToAction("Index");
    }
}