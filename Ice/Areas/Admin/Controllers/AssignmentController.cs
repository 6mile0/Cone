using Microsoft.AspNetCore.Mvc;

namespace Ice.Areas.Admin.Controllers;

[Area("Admin")]
[Route("[area]/assignments")]
public class AssignmentController: Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}