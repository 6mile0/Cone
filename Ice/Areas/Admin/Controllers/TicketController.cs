using Microsoft.AspNetCore.Mvc;

namespace Ice.Areas.Admin.Controllers;

[Area("admin")]
[Route("[area]/tickets")]
public class TicketController: Controller
{
    public IActionResult Index()
    {
        return View();
    }
}