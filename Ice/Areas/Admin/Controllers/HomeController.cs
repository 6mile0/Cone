using Microsoft.AspNetCore.Mvc;

namespace Ice.Areas.Admin.Controllers;

[Area("admin")]
[Route("[area]")]
public class HomeController(ILogger<HomeController> logger) : Controller
{
    public IActionResult Index()
    {
        logger.LogInformation("Index");
        return View();
    }
}