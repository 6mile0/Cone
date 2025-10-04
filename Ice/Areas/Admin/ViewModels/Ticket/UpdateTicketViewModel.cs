using System.ComponentModel.DataAnnotations;
using Ice.Enums;

namespace Ice.Areas.Admin.ViewModels.Ticket;

public class UpdateTicketViewModel
{
    [Required(ErrorMessage = "チケットIDは必須です")]
    public required long TicketId { get; init; }

    [Required(ErrorMessage = "班IDは必須です")]
    public required long StudentGroupId { get; init; }

    [Required(ErrorMessage = "概要は必須です")]
    [MaxLength(200)]
    public required string Title { get; init; }

    [MaxLength(2000, ErrorMessage = "備考は2000文字以内で入力してください")]
    public string? Remark { get; init; } = string.Empty;

    [Required(ErrorMessage = "ステータスは必須です")]
    public required TicketStatus Status { get; init; }
}