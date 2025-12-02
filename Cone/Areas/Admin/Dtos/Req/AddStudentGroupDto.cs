using System.ComponentModel.DataAnnotations;

namespace Cone.Areas.Admin.Dtos.Req;

public class AddStudentGroupDto
{
    [Required]
    public required string GroupName { get; init; }
}