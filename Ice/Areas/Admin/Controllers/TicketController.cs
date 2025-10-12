using Ice.Areas.Admin.Dtos.Req;
using Ice.Areas.Admin.ViewModels.AdminUser;
using Ice.Areas.Admin.ViewModels.StudentGroup;
using Ice.Areas.Admin.ViewModels.Ticket;
using Ice.Enums;
using Ice.Exception;
using Ice.Services.AdminUserService;
using Ice.Services.TicketService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Ice.Areas.Admin.Controllers;

[Authorize(Policy = "Admin")]
[Area("admin")]
[Route("[area]/tickets")]
public class TicketController(ITicketService ticketService, IAdminUserService adminUserService, IFlashMessage flashMessage) : Controller
{
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var tickets = await ticketService.GetAllTicketsAsync(cancellationToken);
        var adminUsers = await adminUserService.GetAllAdminUsersAsync(cancellationToken);

        var ticketViewModels = tickets.Select(t => new TicketViewModel
        {
            Id = t.Id,
            Title = t.Title,
            Status = t.Status,
            AssignedTo = t.TicketAdminUser?.AdminUser,
            StudentGroup = new StudentGroupViewModel
            {
                Id = t.StudentGroup.Id, 
                GroupName = t.StudentGroup.GroupName,
                CreatedAt = t.StudentGroup.CreatedAt,
                UpdatedAt = t.StudentGroup.UpdatedAt,
                TicketCount = t.StudentGroup.Tickets?.Count ?? 0
            },
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        }).ToList();

        var adminUsersViewModels = adminUsers.Select(a => new AdminUserViewModel
        {
            Id = a.Id,
            FullName = a.FullName,
            TutorType = a.TutorType,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        }).ToList();

        return View("Index", new TicketViewModelList
        {
            Tickets = ticketViewModels,
            AdminUsers = adminUsersViewModels
        });
    }

    [Route("{id:long}")]
    [HttpGet]
    public async Task<IActionResult> Detail(long id, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetTicketByIdAsync(id, cancellationToken);

        if (ticket?.StudentGroup == null)
        {
            flashMessage.Danger("指定されたチケットは存在しません。");
            return RedirectToAction("Index", "Ticket", new { area = "admin" });
        }

        var assignedTo = ticket.TicketAdminUser != null ? new AdminUserViewModel
        {
            Id = ticket.TicketAdminUser.AdminUser.Id,
            FullName = ticket.TicketAdminUser.AdminUser.FullName,
            TutorType = Enum.Parse<TutorTypes>(ticket.TicketAdminUser.AdminUser.TutorType.ToString()),
            CreatedAt = ticket.TicketAdminUser.AdminUser.CreatedAt,
            UpdatedAt = ticket.TicketAdminUser.AdminUser.UpdatedAt
        } : null;

        var adminUsers = await adminUserService.GetAllAdminUsersAsync(cancellationToken);
        var adminUsersViewModels = adminUsers.Select(a => new AdminUserViewModel
        {
            Id = a.Id,
            FullName = a.FullName,
            TutorType = a.TutorType,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        }).ToList();

        var viewModel = new TicketDetailViewModel
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Status = GetTicketStatusText(ticket.Status),
            Remark = ticket.Remark,
            StudentGroupId = ticket.StudentGroupId,
            StudentGroupName = ticket.StudentGroup.GroupName,
            AssignedTo = assignedTo,
            AdminUsers = adminUsersViewModels,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt
        };

        return View("Detail", viewModel);
    }

    [Route("{id:long}/edit")]
    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetTicketByIdAsync(id, cancellationToken);

        if (ticket == null)
        {
            flashMessage.Danger("指定されたチケットは存在しません。");
            return RedirectToAction("Index", "Ticket", new { area = "admin" });
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

    [Route("{id:long}/edit")]
    [HttpPost]
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
                return RedirectToAction("Index", "Ticket", new { area = "admin" });
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

    [Route("{id:long}/assign")]
    [HttpPost]
    public async Task<IActionResult> AssignAdminUser(
        long id,
        [FromForm] long adminUserId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await ticketService.AssignTicketAsync(new AssignTicketReqDto
            {
                TicketId = id,
                AdminUserId = adminUserId
            }, cancellationToken);

            flashMessage.Info("担当者を変更しました。");
        }
        catch (EntityNotFoundException e)
        {
            flashMessage.Danger(e.Message);
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