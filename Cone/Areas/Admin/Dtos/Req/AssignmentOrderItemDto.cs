using System.ComponentModel.DataAnnotations;

namespace Cone.Areas.Admin.Dtos.Req;

public class AssignmentOrderItemDto
{
    [Required]
    public required long Id { get; init; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "SortOrder must be greater than 0")]
    public required int SortOrder { get; init; }
}