using System.ComponentModel.DataAnnotations;

namespace Cone.Areas.Student.Dtos.Req;

public class AddTicketDto
{
    [Required]
    public long StudentGroupId { get; init; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;
    
    [Required]
    public long AssignmentId { get; init; }
}