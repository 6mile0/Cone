using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ice.Areas.Student.ViewModels;

public class AddTicketViewModel
{
    [Required]
    public long StudentGroupId { get; init; }

    [MaxLength(200, ErrorMessage = "困っていること は200文字以内で入力してください。")]
    public string Title { get; init; } = string.Empty;

    public long AssignmentId { get; init; }

    public IEnumerable<SelectListItem> Assignments { get; init; }
    
    public bool IsAbleAddTicket { get; init; } = true;
    
    public string? StaffName { get; init; } = string.Empty;
}