using Microsoft.AspNetCore.Mvc;

namespace Ice.Areas.Admin.Controllers;

[Area("admin")]
[Route("[area]/student-groups")]
public class StudentGroupController(ILogger<StudentGroupController> logger) : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    
    
}