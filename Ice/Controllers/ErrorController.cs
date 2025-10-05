using Ice.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ice.Controllers;

[AllowAnonymous]
[Route("[controller]")]
public class ErrorController : Controller
{
    [Route("{statusCode:int}")]
    public IActionResult Error(int statusCode)
    {
        return View(
            new ErrorViewModel
            {
                StatusCode = statusCode,
                RequestId = HttpContext.TraceIdentifier
            }
        );
    }
}