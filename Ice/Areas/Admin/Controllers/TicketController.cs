using Ice.Areas.Admin.Dtos.Req;
using Ice.Areas.Admin.ViewModels.AdminUser;
using Ice.Areas.Admin.ViewModels.Ticket;
using Ice.Enums;
using Ice.Services.TicketService;
using Microsoft.AspNetCore.Mvc;

namespace Ice.Areas.Admin.Controllers;

[Area("admin")]
[Route("[area]/tickets")]
public class TicketController(ITicketService ticketService) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Detail(long id, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetTicketByIdAsync(id, cancellationToken);

        if (ticket == null)
        {
            return NotFound();
        }

        var viewModel = new TicketDetailViewModel
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Status = GetTicketStatusText(ticket.Status),
            Remark = ticket.Remark,
            StudentGroupId = ticket.StudentGroupId,
            StudentGroupName = ticket.StudentGroup.GroupName,
            AssignedTo = new AdminUserViewModel
            {
                Id = ticket.TicketAdminUser.AdminUser.Id,
                FullName = ticket.TicketAdminUser.AdminUser.FullName,
                TutorType = Enum.Parse<TutorTypes>(ticket.TicketAdminUser.AdminUser.TutorType.ToString()),
                CreatedAt = ticket.TicketAdminUser.AdminUser.CreatedAt,
                UpdatedAt = ticket.TicketAdminUser.AdminUser.UpdatedAt
            },
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt
        };

        return View("Detail", viewModel);
    }

    [HttpGet("{id:long}/edit")]
    public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetTicketByIdAsync(id, cancellationToken);

        if (ticket == null)
        {
            return NotFound();
        }

        ViewData["TicketId"] = ticket.Id;
        ViewData["StudentGroupId"] = ticket.StudentGroupId;
        return View("Edit", new UpdateTicketViewModel
        {
            Title = ticket.Title,
            Remark = ticket.Remark,
            Status = ticket.Status
        });
    }

    [HttpPost("{id:long}/edit")]
    public async Task<IActionResult> Edit(
        long id,
        [FromForm] UpdateTicketViewModel model,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            var ticket = await ticketService.GetTicketByIdAsync(id, cancellationToken);
            if (ticket == null) return View("Edit", model);

            ViewData["TicketId"] = ticket.Id;
            ViewData["StudentGroupId"] = ticket.StudentGroupId;

            return View("Edit", model);
        }

        await ticketService.UpdateTicketAsync(new UpdateTicketReqDto
        {
            TicketId = id,
            Title = model.Title,
            Remark = model.Remark,
            Status = model.Status
        }, cancellationToken);
        
        return RedirectToAction("Detail", new { id });
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