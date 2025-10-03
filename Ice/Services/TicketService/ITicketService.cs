using Ice.Areas.Student.Dtos.Req;
using Ice.Areas.Student.Dtos.Res;
using Ice.Db.Models;

namespace Ice.Services.TicketService;

public interface ITicketService
{
    /// <summary>
    /// Gets all ticket, 
    /// </summary>
    Task<IReadOnlyList<Tickets>> GetAllTicketsAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Create a new ticket.
    /// </summary>
    Task<AddTicketResDto> CreateTicketAsync(AddTicketDto addTicketDto, CancellationToken cancellationToken);
    
    /// <summary>
    /// Update an existing ticket.
    /// </summary>
    Task<Tickets> UpdateTicketAsync(Tickets ticket, CancellationToken cancellationToken);
    
    /// <summary>
    /// Delete a ticket by its ID.
    /// </summary>
    Task DeleteTicketAsync(long ticketId, CancellationToken cancellationToken);
}