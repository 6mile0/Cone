using System.Collections.Immutable;
using Ice.Areas.Admin.Dtos.Req;
using Ice.Areas.Admin.ViewModels.StudentGroup;
using Ice.Enums;
using Ice.Services.StudentGroupService;
using Microsoft.AspNetCore.Mvc;

namespace Ice.Areas.Admin.Controllers;

[Area("admin")]
[Route("[area]/student-groups")]
public class StudentGroupController(IStudentGroupService studentGroupService) : Controller
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
            UpdatedAt = sg.UpdatedAt
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
        [FromForm] AddStudentGroupDto request,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            return View("Add");
        }

        await studentGroupService.CreateStudentGroupAsync(request, cancellationToken);
        return RedirectToAction("Index");
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Detail(long id, CancellationToken cancellationToken)
    {
        var (group, assignments, tickets) = await studentGroupService.GetStudentGroupDetailAsync(id, cancellationToken);

        var viewModel = new StudentGroupDetailViewModel
        {
            Id = group.Id,
            GroupName = group.GroupName,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt,
            AssignmentProgress = assignments.Select(a => new AssignmentProgressViewModel
            {
                AssignmentId = a.AssignmentId,
                AssignmentName = a.Assignment?.Name ?? "不明",
                Status = GetAssignmentProgressText(a.Status)
            }).ToImmutableList(),
            Tickets = tickets.Select(t => new TicketViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Status = GetTicketStatusText(t.Status),
                CreatedAt = t.CreatedAt,
                AssignedTo = t.TicketAdminUser?.AdminUser
            }).ToImmutableList()
        };

        return View("Detail", viewModel);
    }

    private static string GetAssignmentProgressText(AssignmentProgress status)
    {
        return status switch
        {
            AssignmentProgress.NotStarted => "未着手",
            AssignmentProgress.InProgress => "進行中",
            AssignmentProgress.Completed => "完了",
            _ => "不明"
        };
    }

    private static string GetTicketStatusText(TicketStatus status)
    {
        return status switch
        {
            TicketStatus.Open => "未対応",
            TicketStatus.InProgress => "対応中",
            TicketStatus.Resolved => "解決済み",
            TicketStatus.Closed => "クローズ",
            _ => "不明"
        };
    }
}