using System.ComponentModel.DataAnnotations;

namespace Cone.Areas.Admin.Dtos.Req;

public class AddAssignmentDto
{
    [Required]
    [MaxLength(200)]
    public required string Name { get; init; }
    
    [Required]
    public required string Description { get; init; }
}