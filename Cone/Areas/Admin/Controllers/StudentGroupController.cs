using System.Collections.Immutable;
using Cone.Areas.Admin.Dtos.Req;
using Cone.Areas.Admin.ViewModels.Assignment;
using Cone.Areas.Admin.ViewModels.StudentGroup;
using Cone.Areas.Admin.ViewModels.Ticket;
using Cone.Enums;
using Cone.Services.AssignmentService;
using Cone.Services.StudentGroupService;
using Cone.Services.TicketService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Cone.Areas.Admin.Controllers;

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
                StudentGroup = new StudentGroupViewModel
                {
                    Id = group.Id,
                    GroupName = group.GroupName,
                    CreatedAt = group.CreatedAt,
                    UpdatedAt = group.UpdatedAt,
                    TicketCount = group.Tickets?.Count ?? 0
                },
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

    [HttpPost("{studentGroupId:long}/bulk-update-assignment-status")]
    public async Task<IActionResult> BulkUpdateAssignmentStatus(
        long studentGroupId,
        [FromForm] List<long> assignmentIds,
        [FromForm] string status,
        CancellationToken cancellationToken
    )
    {
        if (assignmentIds.Count == 0)
        {
            flashMessage.Warning("課題を選択してください。");
            return RedirectToAction("Detail", new { id = studentGroupId });
        }

        if (!Enum.TryParse<AssignmentProgress>(status, out var assignmentStatus))
        {
            flashMessage.Danger("無効なステータスです。");
            return RedirectToAction("Detail", new { id = studentGroupId });
        }

        foreach (var assignmentId in assignmentIds)
        {
            await studentGroupService.UpdateAssignmentProgressAsync(new UpdateAssignmentProgressDto
            {
                StudentGroupId = studentGroupId,
                AssignmentId = assignmentId,
                Status = assignmentStatus
            }, cancellationToken);
        }

        var statusText = assignmentStatus switch
        {
            AssignmentProgress.NotStarted => "未着手",
            AssignmentProgress.InProgress => "進行中",
            AssignmentProgress.Completed => "完了",
            _ => "不明"
        };

        flashMessage.Info($"{assignmentIds.Count}件の課題のステータスを「{statusText}」に更新しました。");
        return RedirectToAction("Detail", new { id = studentGroupId });
    }
}