using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cone.Areas.Admin.Controllers;

[Authorize(Policy = "Admin")]
[Area("admin")]
[Route("/notifications")]
public class NotificationController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}