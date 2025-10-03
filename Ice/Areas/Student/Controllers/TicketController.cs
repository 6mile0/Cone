using Ice.Areas.Student.Dtos.Req;
using Ice.Areas.Student.ViewModels;
using Ice.Services.AssignmentService;
using Ice.Services.TicketService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vereyon.Web;

namespace Ice.Areas.Student.Controllers;

[Area("student")]
[Route("tickets/add")]
public class TicketController(ITicketService ticketService, IAssignmentService assignmentService, IFlashMessage flashMessage) : Controller
{
    [HttpGet]
    public async Task<IActionResult> AddTicket(
        [FromQuery] long? studentGroupId,
        CancellationToken cancellationToken
    )
    {
        if (studentGroupId is null or 0)
        {
            return BadRequest("学生のグループIDが存在しません。もう一度テーブル選択を行うか、QRコードを読み込み直してください。");
        }

        var assignments = await assignmentService.GetAllAssignmentsAsync(cancellationToken);
        var isAbleAddTicket = await ticketService.IsAbleAddTicketAsync(studentGroupId.Value, cancellationToken);
            
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
            StudentGroupId = studentGroupId.Value,
            Assignments = enumerableAssignments,
            IsAbleAddTicket = isAbleAddTicket != null,
            StaffName = isAbleAddTicket?.TicketAdminUser?.AdminUser?.FullName
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddTicket(
        [FromForm] AddTicketDto request,
        CancellationToken cancellationToken
    )
    {
        var isAbleAddTicket = await ticketService.IsAbleAddTicketAsync(request.StudentGroupId, cancellationToken);
        
        if (isAbleAddTicket != null)
        {
            flashMessage.Danger("現在対応中のチケットが存在するため、新しいチケットを追加できません。対応が完了するまでお待ちください。");
            return RedirectToAction("AddTicket", new { studentGroupId = request.StudentGroupId });
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
                StudentGroupId = request.StudentGroupId,
                AssignmentId = request.AssignmentId,
                Title = request.Title,
                Assignments = enumerableAssignments
            });
        }

        await ticketService.CreateTicketAsync(request, cancellationToken);
        return RedirectToAction("AddTicket", "Ticket", new { studentGroupId = request.StudentGroupId });
    }
}