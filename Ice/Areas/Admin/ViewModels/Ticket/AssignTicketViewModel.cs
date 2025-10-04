using System.ComponentModel.DataAnnotations;
using Ice.Areas.Admin.ViewModels.AdminUser;

namespace Ice.Areas.Admin.ViewModels.Ticket;

public class AssignTicketViewModel
{
    public required long TicketId { get; init; }

    public required long StudentGroupId { get; init; }

    public string Title { get; init; } = string.Empty;
    
    public string StudentGroupName { get; init; } = string.Empty;

    [Required(ErrorMessage = "担当者は必須です")]
    public long AdminUserId { get; init; }
    
    public IEnumerable<AdminUserViewModel> AdminUsers { get; init; }　= [];
}