using System.ComponentModel.DataAnnotations;
using Ice.Enums;

namespace Ice.Areas.Admin.Dtos.Req;

public class EditTicketDto
{
    [Required]
    [MaxLength(200)]
    public required string Title { get; init; }

    [MaxLength(2000)]
    public string? Remark { get; init; } = string.Empty;

    [Required]
    public required TicketStatus Status { get; init; }
}