using Ice.Areas.Student.ViewModels.Top;
using Ice.Services.StudentGroupService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ice.Areas.Student.Controllers;

[Authorize(Policy = "AllowedEmailDomain")]
[Area("Student")]
[Route("/")]
public class TopController(IStudentGroupService studentGroupService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var studentGroups = await studentGroupService.GetAllStudentGroupsAsync(cancellationToken);

        var viewModel = new SeatMapViewModel
        {
            StudentGroups = studentGroups.Select(sg => new StudentGroupSeatViewModel
            {
                Id = sg.Id,
                GroupName = sg.GroupName,
                TicketCount = sg.Tickets?.Count ?? 0
            }).ToList()
        };

        return View("Index", viewModel);
    }
}