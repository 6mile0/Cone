using Cone.Areas.Admin.ViewModels.AdminUser;

namespace Cone.Areas.Admin.ViewModels.Ticket;

public class TicketViewModelList
{
    public IReadOnlyList<TicketViewModel> Tickets { get; init; } = [];
    public IReadOnlyList<AdminUserViewModel> AdminUsers { get; init; } = [];
    public IReadOnlyList<TicketViewModel> InProgressTickets => Tickets.Where(t => t.Status == Enums.TicketStatus.InProgress).ToList();
    public IReadOnlyList<TicketViewModel> SolvedTickets => Tickets.Where(t => t.Status == Enums.TicketStatus.Resolved).ToList();
    public IReadOnlyList<TicketViewModel> PendingTickets => Tickets.Where(t => t.Status == Enums.TicketStatus.Pending).ToList();
}