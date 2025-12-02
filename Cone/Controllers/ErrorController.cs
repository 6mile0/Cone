using Cone.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cone.Controllers;

[AllowAnonymous]
[Route("error")]
public class ErrorController : Controller
{
    [Route("{statusCode:int}")]
    public IActionResult Error(int statusCode)
    {
        return statusCode switch
        {
            404 => View("Error",
                new ErrorViewModel
                {
                    StatusCode = 404,
                    Message = "ページが見つかりません",
                    DetailedMessage = "お探しのページは存在しないか、移動または削除された可能性があります。",
                    RequestId = HttpContext.TraceIdentifier
                }),
            403 => View("Error",
                new ErrorViewModel
                {
                    StatusCode = 403,
                    Message = "アクセスが拒否されました",
                    DetailedMessage = "このページを表示する権限がありません。",
                    RequestId = HttpContext.TraceIdentifier
                }),
            500 => View("Error",
                new ErrorViewModel
                {
                    StatusCode = 500,
                    Message = "サーバーエラー",
                    DetailedMessage = "サーバーで予期しないエラーが発生しました。",
                    Suggestion = "しばらく時間をおいてから再度お試しください。問題が続く場合は管理者にお問い合わせください。",
                    RequestId = HttpContext.TraceIdentifier
                }),
            _ => View("Error",
                new ErrorViewModel
                {
                    StatusCode = statusCode,
                    Message = "エラーが発生しました",
                    DetailedMessage = "予期しないエラーが発生しました。時間を空けてから再度お試しください。",
                    RequestId = HttpContext.TraceIdentifier
                })
        };
    }
}