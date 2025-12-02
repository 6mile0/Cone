using Cone.Areas.Admin.Dtos.Req;
using Cone.Areas.Admin.ViewModels.AdminUser;
using Cone.Configuration;
using Cone.Enums;
using Cone.Exception;
using Cone.Services.AdminUserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace Cone.Areas.Admin.Controllers;

[Authorize(Policy = "Admin")]
[Area("admin")]
[Route("[area]/users")]
public class AdminUserController(IAdminUserService adminUserService, IFlashMessage flashMessage, ConeConfiguration coneConfiguration) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var users = await adminUserService.GetAllAdminUsersAsync(cancellationToken);
        var userViewModels = users.Select(u => new AdminUserViewModel
        {
            Id = u.Id,
            FullName = u.FullName,
            TutorType = Enum.Parse<TutorTypes>(u.TutorType.ToString()),
            IsAbsent = u.IsAbsent,
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
        if (!coneConfiguration.AllowedEmailEndPrefixes.Contains(emailDomain))
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

    [HttpPost("toggle-absent")]
    public async Task<IActionResult> ToggleAbsent(
        [FromForm] long adminUserId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            // すべての管理者を取得
            var allUsers = await adminUserService.GetAllAdminUsersAsync(cancellationToken);

            // 対象の管理者を検索
            var targetUser = allUsers.FirstOrDefault(u => u.Id == adminUserId);
            if (targetUser == null)
            {
                flashMessage.Danger("指定された管理ユーザーが見つかりません。");
                return RedirectToAction("Index");
            }

            // 出講中からお休みモードにする場合、出講中の管理者が2人以上いるか確認
            if (!targetUser.IsAbsent)
            {
                var activeUsersCount = allUsers.Count(u => !u.IsAbsent);

                if (activeUsersCount < 2)
                {
                    flashMessage.Danger("最低1人は出講中である必要があるため、お休みモードに設定できません。");
                    return RedirectToAction("Index");
                }
            }

            var updatedUser = await adminUserService.ToggleAbsentStatusAsync(adminUserId, cancellationToken);

            var statusMessage = updatedUser.IsAbsent ? "お休みモードに設定しました。" : "出講中に設定しました。";
            flashMessage.Info(statusMessage);
            return RedirectToAction("Index");
        }
        catch (EntityNotFoundException)
        {
            flashMessage.Danger("指定された管理ユーザーが見つかりません。");
            return RedirectToAction("Index");
        }
    }
}