using Ice.Db.Models;
using Ice.Enums;

namespace Ice.Areas.Admin.ViewModels.Ticket;

public class TicketViewModel
{
    public required long Id { get; init; }

    public required string Title { get; init; }

    public required TicketStatus Status { get; init; }
    
    public required AdminUsers? AssignedTo { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
    
    public required DateTimeOffset UpdatedAt { get; init; }
}
