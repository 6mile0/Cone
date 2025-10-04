using System.ComponentModel.DataAnnotations;
using Ice.Enums;

namespace Ice.Areas.Admin.Dtos.Req;

public class UpdateTicketReqDto
{
    public required long TicketId { get; init; }
    
    public required string Title { get; init; }
    
    public string? Remark { get; init; }
    
    [Required]
    public required TicketStatus Status { get; init; }
}