using System.ComponentModel.DataAnnotations;
using Cone.Areas.Admin.Dtos.Req;

namespace Cone.Areas.Admin.ViewModels.Assignment;

public class EditAssignmentOrderViewModel
{
    [Required(ErrorMessage = "課題の並び順を指定してください")]
    public required List<AssignmentOrderItemDto> Assignments { get; init; }
}