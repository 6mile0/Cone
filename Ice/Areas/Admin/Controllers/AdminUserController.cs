using Ice.Areas.Admin.Dtos.Req;
using Ice.Areas.Admin.ViewModels.AdminUser;
using Ice.Enums;
using Ice.Services.AdminUserService;
using Microsoft.AspNetCore.Mvc;

namespace Ice.Areas.Admin.Controllers;

[Area("admin")]
[Route("[area]/users")]
public class AdminUserController(IAdminUserService adminUserService): Controller
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
        [FromForm] AddAdminUserDto request,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
        {
            return View("Add");
        }

        await adminUserService.AddAdminUserAsync(request, cancellationToken);
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

    [HttpGet("api/list")]
    public async Task<IActionResult> GetAdminUsersList(CancellationToken cancellationToken)
    {
        var users = await adminUserService.GetAllAdminUsersAsync(cancellationToken);
        var userList = users.Select(u => new
        {
            id = u.Id,
            fullName = u.FullName
        }).ToList();

        return Json(userList);
    }
}