using System.ComponentModel.DataAnnotations;

namespace Ice.Areas.Admin.Dtos.Req;

public class EditAssignmentOrderDto
{
    [Required]
    public required List<AssignmentOrderItemDto> Assignments { get; init; }
}