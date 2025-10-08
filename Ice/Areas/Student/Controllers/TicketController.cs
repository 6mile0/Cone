using Ice.Areas.Student.Dtos.Req;
using Ice.Areas.Student.ViewModels;
using Ice.Services.AssignmentService;
using Ice.Services.StudentGroupService;
using Ice.Services.TicketService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vereyon.Web;

namespace Ice.Areas.Student.Controllers;

[Authorize(Policy = "AllowedEmailDomain")]
[Area("student")]
[Route("tickets/add")]
public class TicketController(ITicketService ticketService, IAssignmentService assignmentService, IStudentGroupService studentGroupService, IFlashMessage flashMessage) : Controller
{
    [HttpGet]
    public async Task<IActionResult> AddTicket(
        [FromQuery] long? studentGroupId,
        CancellationToken cancellationToken
    )
    {
        // 学生グループが存在するか確認
        var studentGroup = await studentGroupService.GetStudentGroupByIdAsync(studentGroupId, cancellationToken);
        if (studentGroup == null)
        {
            flashMessage.Danger("指定されたグループIDが存在しません。もう一度班選択を行うか、QRコードを読み込み直してください。");
            return RedirectToAction("Index", "Top");
        }

        var assignments = await assignmentService.GetReleasedAssignmentByGroupId(studentGroup.Id, cancellationToken);
        if (!assignments.Any())
        {
            flashMessage.Danger("現在対応可能な課題が存在しません。お近くのTA/SAを呼んでください。");
            return RedirectToAction("Index", "Top");
        }
        
        var isAbleAddTicket = await ticketService.IsAbleAddTicketAsync(studentGroup.Id, cancellationToken);
            
        var enumerableAssignments = assignments
            .OrderBy(a => a.SortOrder)
            .Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            })
            .ToList();
        
        return View("Add", new AddTicketViewModel
        {
            StudentGroupId = studentGroup.Id,
            Assignments = enumerableAssignments,
            IsAbleAddTicket = isAbleAddTicket != null,
            StaffName = isAbleAddTicket?.TicketAdminUser?.AdminUser?.FullName
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddTicket(
        [FromForm] AddTicketViewModel model,
        CancellationToken cancellationToken
    )
    {
        var isAbleAddTicket = await ticketService.IsAbleAddTicketAsync(model.StudentGroupId, cancellationToken);
        
        if (isAbleAddTicket != null)
        {
            flashMessage.Danger("現在対応中のチケットが存在するため、新しいチケットを追加できません。対応が完了するまでお待ちください。");
            return RedirectToAction("AddTicket", new { studentGroupId = model.StudentGroupId });
        }
        
        if (!ModelState.IsValid)
        {
            var assignments = await assignmentService.GetAllAssignmentsAsync(cancellationToken);
            var enumerableAssignments = assignments
                .OrderBy(a => a.SortOrder)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                })
                .ToList();
            
            return View("Add", new AddTicketViewModel
            {
                StudentGroupId = model.StudentGroupId,
                AssignmentId = model.AssignmentId,
                Title = model.Title,
                Assignments = enumerableAssignments
            });
        }
        
        var request = new AddTicketDto
        {
            StudentGroupId = model.StudentGroupId,
            AssignmentId = model.AssignmentId,
            Title = model.Title,
        };

        await ticketService.CreateTicketAsync(request, cancellationToken);
        return RedirectToAction("AddTicket", "Ticket", new { studentGroupId = request.StudentGroupId });
    }
}