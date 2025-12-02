using System.ComponentModel.DataAnnotations;
using Cone.Enums;

namespace Cone.Areas.Admin.Dtos.Req;

public class UpdateTicketReqDto
{
    public required long TicketId { get; init; }
    
    public required string Title { get; init; }
    
    public string? Remark { get; init; } = string.Empty;
    
    [Required]
    public required TicketStatus Status { get; init; }
}