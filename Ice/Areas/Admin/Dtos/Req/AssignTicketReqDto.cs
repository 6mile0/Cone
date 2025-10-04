using System.ComponentModel.DataAnnotations;

namespace Ice.Areas.Admin.Dtos.Req;

public class AssignTicketReqDto
{
    [Required]
    public required long TicketId { get; init; }

    [Required]
    public required long AdminUserId { get; init; }
}