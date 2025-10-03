using System.ComponentModel.DataAnnotations;
using Ice.Db.Models;
using Ice.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ice.Areas.Student.ViewModels;

public class AddTicketViewModel
{
    [Required]
    public long StudentGroupId { get; set; }

    [MaxLength(200, ErrorMessage = "困っていること は200文字以内で入力してください。")]
    public string Title { get; set; } = string.Empty;

    public long AssignmentId { get; set; }

    public IEnumerable<SelectListItem> Assignments { get; set; }
}