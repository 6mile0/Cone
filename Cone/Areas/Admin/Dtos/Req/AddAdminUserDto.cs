using System.ComponentModel.DataAnnotations;
using Cone.Enums;

namespace Cone.Areas.Admin.Dtos.Req;

public class AddAdminUserDto
{
    [Required]
    public required string FullName { get; init; }

    [Required]
    [EnumDataType(typeof(TutorTypes))]
    public required TutorTypes TutorType { get; init; }
    
    [Required]
    [EmailAddress]
    public required string Email { get; init; }
}