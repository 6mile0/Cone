using Ice.Areas.Admin.Dtos.Req;
using Ice.Areas.Admin.ViewModels.AdminUser;
using Ice.Enums;
using Ice.Services.AdminUserService;
using Microsoft.AspNetCore.Mvc;

namespace Ice.Areas.Admin.Controllers;

[Area("admin")]
[Route("[area]/users")]
public class AdminUserController(IAdminUserService adminUserService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var users = await adminUserService.GetAllAdminUsersAsync(cancellationToken);
        var userViewModels = users.Select(u => new AdminUserViewModel
        {
            Id = u.Id,
            FullName = u.FullName,
            TutorType = Enum.Parse<TutorTypes>(u.TutorType.ToString()),
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        }).ToList();

        return View("Index", new AdminUserListViewModel
        {
            AdminUsers = userViewModels
        });
    }

    [HttpGet("add")]
    public IActionResult AddUser()
    {
        return View("Add");
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddUser(
        [FromForm] AddAdminUserViewModel model,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            return View("Add");
        }

        await adminUserService.AddAdminUserAsync(new AddAdminUserDto
        {
            FullName = model.FullName,
            TutorType = model.TutorType
        }, cancellationToken);
        
        return RedirectToAction("Index");
    }

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteUser(
        [FromForm] long adminUserId,
        CancellationToken cancellationToken
    )
    {
        await adminUserService.DeleteAdminUserAsync(adminUserId, cancellationToken);
        return RedirectToAction("Index");
    }
}