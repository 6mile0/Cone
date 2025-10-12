using System.Collections.Immutable;
using Ice.Areas.Admin.Dtos.Req;
using Ice.Areas.Admin.ViewModels.Assignment;
using Ice.Areas.Admin.ViewModels.StudentGroup;
using Ice.Areas.Admin.ViewModels.Ticket;
using Ice.Enums;
using Ice.Services.AssignmentService;
using Ice.Services.StudentGroupService;
using Ice.Services.TicketService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Ice.Areas.Admin.Controllers;

[Authorize(Policy = "Admin")]
[Area("admin")]
[Route("[area]/student-groups")]
public class StudentGroupController(IStudentGroupService studentGroupService, IAssignmentService assignmentService, ITicketService ticketService, IFlashMessage flashMessage) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var studentGroups = await studentGroupService.GetAllStudentGroupsAsync(cancellationToken);
        var studentGroupList = studentGroups.Select(sg => new StudentGroupViewModel
        {
            Id = sg.Id,
            GroupName = sg.GroupName,
            CreatedAt = sg.CreatedAt,
            UpdatedAt = sg.UpdatedAt,
            TicketCount = sg.Tickets?.Count ?? 0
        }).ToImmutableList();

        return View("Index", new StudentGroupListViewModel
        {
            StudentGroupList = studentGroupList
        });
    }

    [HttpGet("add")]
    public IActionResult AddStudentGroup()
    {
        return View("Add");
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddStudentGroup(
        [FromForm] AddStudentGroupViewModel model,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            flashMessage.Danger("入力に誤りがあります。");
            return View("Add");
        }

        await studentGroupService.CreateStudentGroupAsync(new AddStudentGroupDto
        {
            GroupName = model.GroupName
        }, cancellationToken);
        
        flashMessage.Info("学生グループを追加しました。");
        return RedirectToAction("Index");
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Detail(long id, CancellationToken cancellationToken)
    {
        var group = await studentGroupService.GetStudentGroupByIdAsync(id, cancellationToken);

        if (group == null)
        {
            flashMessage.Danger("学生グループが見つかりません。");
            return RedirectToAction("Index");
        }

        var assignments = await assignmentService.GetAssignmentsByStudentGroupIdAsync(group.Id, cancellationToken);
        var tickets = await ticketService.GetTicketsByStudentGroupIdAsync(group.Id, cancellationToken);

        var assignmentProgress = assignments.Select(a =>
        {
            var progress = a.StudentGroupAssignmentsProgress?.FirstOrDefault(p => p.StudentGroupId == group.Id);
            return new AssignmentProgressViewModel
            {
                AssignmentId = a.Id,
                AssignmentName = a.Name,
                Status = progress?.Status ?? AssignmentProgress.NotStarted,
            };
        }).ToImmutableList();

        var viewModel = new StudentGroupDetailViewModel
        {
            Id = group.Id,
            GroupName = group.GroupName,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt,
            AssignmentProgress = assignmentProgress,
            Tickets = tickets.Select(t => new TicketViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                AssignedTo = t.TicketAdminUser?.AdminUser,
                UpdatedAt = t.UpdatedAt
            }).ToImmutableList()
        };

        return View("Detail", viewModel);
    }

    [HttpPost("{studentGroupId:long}/assignments/{assignmentId:long}/update-status")]
    public async Task<IActionResult> UpdateAssignmentStatus(
        long studentGroupId,
        long assignmentId,
        [FromForm] string status,
        CancellationToken cancellationToken
    )
    {
        if (!Enum.TryParse<AssignmentProgress>(status, out var assignmentStatus))
        {
            flashMessage.Danger("無効なステータスです。");
            return RedirectToAction("Detail", new { id = studentGroupId });
        }

        await studentGroupService.UpdateAssignmentProgressAsync(new UpdateAssignmentProgressDto
        {
            StudentGroupId = studentGroupId,
            AssignmentId = assignmentId,
            Status = assignmentStatus
        }, cancellationToken);

        flashMessage.Info("課題のステータスを更新しました。");
        return RedirectToAction("Detail", new { id = studentGroupId });
    }
}