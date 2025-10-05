using Ice.Areas.Admin.Dtos.Req;
using Ice.Areas.Admin.ViewModels.AdminUser;
using Ice.Configuration;
using Ice.Enums;
using Ice.Exception;
using Ice.Services.AdminUserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Ice.Areas.Admin.Controllers;

[Authorize(Policy = "Admin")]
[Area("admin")]
[Route("[area]/users")]
public class AdminUserController(IAdminUserService adminUserService, IFlashMessage flashMessage, IceConfiguration iceConfiguration) : Controller
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
        // 特定のメールアドレスドメインのみ許可
        var emailDomain = model.Email.Split('@').Last();
        if (!iceConfiguration.AllowedEmailEndPrefixes.Contains(emailDomain))
        {
            flashMessage.Danger("許可されていないメールアドレスのドメインです。");
            return View("Add");
        }
        
        if (!ModelState.IsValid)
        {
            return View("Add");
        }

        await adminUserService.AddAdminUserAsync(new AddAdminUserDto
        {
            FullName = model.FullName,
            TutorType = model.TutorType,
            Email = model.Email
        }, cancellationToken);

        flashMessage.Info("管理ユーザーを追加しました。");
        return RedirectToAction("Index");
    }

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteUser(
        [FromForm] long adminUserId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await adminUserService.DeleteAdminUserAsync(adminUserId, cancellationToken);
            
            flashMessage.Info("管理ユーザーを削除しました。");
            return RedirectToAction("Index");
        }
        catch (EntityNotFoundException)
        {
            flashMessage.Danger("指定された管理ユーザーが見つかりません。");
            return RedirectToAction("Index");
        }
    }
}