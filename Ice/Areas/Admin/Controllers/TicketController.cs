using Ice.Areas.Admin.Dtos.Req;
using Ice.Areas.Admin.ViewModels.AdminUser;
using Ice.Areas.Admin.ViewModels.Ticket;
using Ice.Enums;
using Ice.Exception;
using Ice.Services.TicketService;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Ice.Areas.Admin.Controllers;

[Area("admin")]
[Route("[area]/tickets/{id:long}")]
public class TicketController(ITicketService ticketService, IFlashMessage flashMessage) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Detail(long id, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetTicketByIdAsync(id, cancellationToken);

        if (ticket?.StudentGroup == null)
        {
            flashMessage.Danger("指定されたチケットは存在しません。");
            // TODO: チケット一覧がないので、ひとまず学生グループ一覧にリダイレクト
            return RedirectToAction("Index", "StudentGroup", new { area = "admin" });
        }

        if (ticket.TicketAdminUser?.AdminUser == null)
        {
            flashMessage.Danger("チケットの担当者が設定されていません。");
            return RedirectToAction("Index", "StudentGroup", new { area = "admin" });
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

    [HttpGet("edit")]
    public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetTicketByIdAsync(id, cancellationToken);

        if (ticket == null)
        {
            flashMessage.Danger("指定されたチケットは存在しません。");
            return RedirectToAction("Index", "StudentGroup", new { area = "admin" });
        }

        return View("Edit", new UpdateTicketViewModel
        {
            TicketId = ticket.Id,
            StudentGroupId = ticket.StudentGroupId,
            Title = ticket.Title,
            Remark = ticket.Remark,
            Status = ticket.Status
        });
    }

    [HttpPost("edit")]
    public async Task<IActionResult> Edit(
        long id,
        [FromForm] UpdateTicketViewModel model,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            var ticket = await ticketService.GetTicketByIdAsync(id, cancellationToken);
            if (ticket == null)
            {
                flashMessage.Danger("指定されたチケットは存在しません。");
                return RedirectToAction("Index", "StudentGroup", new { area = "admin" });
            }

            return View("Edit", new UpdateTicketViewModel
            {
                TicketId = ticket.Id,
                StudentGroupId = ticket.StudentGroupId,
                Title = model.Title,
                Remark = model.Remark,
                Status = model.Status
            });
        }

        try
        {
            await ticketService.UpdateTicketAsync(new UpdateTicketReqDto
            {
                TicketId = id,
                Title = model.Title,
                Remark = model.Remark,
                Status = model.Status
            }, cancellationToken);
        }
        catch (EntityNotFoundException)
        {
            flashMessage.Danger("指定されたチケットは存在しません。");
            return View("Edit", model);
        }

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