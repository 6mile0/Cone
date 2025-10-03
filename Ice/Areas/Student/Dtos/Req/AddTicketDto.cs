using System.ComponentModel.DataAnnotations;

namespace Ice.Areas.Student.Dtos.Req;

public class AddTicketDto
{
    [Required]
    public long StudentGroupId { get; init; }
    
    public string Title { get; init; } = string.Empty;
    
    public long AssignmentId { get; init; }
}