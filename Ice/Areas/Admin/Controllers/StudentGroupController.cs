using System.Collections.Immutable;
using Ice.Areas.Admin.Dtos.Req;
using Ice.Areas.Admin.ViewModels.Assignment;
using Ice.Areas.Admin.ViewModels.StudentGroup;
using Ice.Areas.Admin.ViewModels.Ticket;
using Ice.Enums;
using Ice.Services.AssignmentService;
using Ice.Services.StudentGroupService;
using Ice.Services.TicketService;
using Microsoft.AspNetCore.Mvc;

namespace Ice.Areas.Admin.Controllers;

[Area("admin")]
[Route("[area]/student-groups")]
public class StudentGroupController(IStudentGroupService studentGroupService, IAssignmentService assignmentService, ITicketService ticketService) : Controller
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
        [FromForm] AddStudentGroupViewModel model,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            return View("Add");
        }

        await studentGroupService.CreateStudentGroupAsync(new AddStudentGroupDto
        {
            GroupName = model.GroupName
        }, cancellationToken);
        return RedirectToAction("Index");
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Detail(long id, CancellationToken cancellationToken)
    {
        var group = await studentGroupService.GetStudentGroupByIdAsync(id, cancellationToken);
        
        if (group == null)
        {
            return NotFound();
        }
        
        var assignments = await assignmentService.GetAssignmentsByStudentGroupIdAsync(group.Id, cancellationToken);
        var tickets = await ticketService.GetTicketsByStudentGroupIdAsync(group.Id, cancellationToken);

        var assignmentProgress = assignments.Select(a => new AssignmentProgressViewModel
        {
            AssignmentId = a.Id,
            AssignmentName = a.Name,
            Status = GetAssignmentProgressText(a.StudentGroupAssignmentsProgress.Status)
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
            TicketStatus.InProgress => "対応中",
            TicketStatus.Resolved => "解決済み",
            TicketStatus.Pending => "トラブルなどで保留中",
            _ => "不明"
        };
    }
}